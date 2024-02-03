using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;
using BiskaUtil;
using System.Net.Mail;
using System.IO;
using System.Net.Mime;
using Database;
using WebApp.Business;
using WebApp.Utilities.Extensions;
using WebApp.Utilities.Helpers;
using WebApp.Utilities.MenuAndRoles;
using WebApp.Utilities.MessageBox;

namespace WebApp.Controllers
{
    [OutputCache(NoStore = false, Duration = 0, VaryByParam = "*")]
    [Authorize(Roles = RoleNames.MailIslemleri)]
    public class MailIslemleriController : Controller
    {
        private readonly VysDBEntities db = new VysDBEntities();
        public ActionResult Index()
        {
            return Index(new FmMailGonderme() { PageSize = 15 });
        }
        [HttpPost]
        public ActionResult Index(FmMailGonderme model)
        {
            var q = from s in db.GonderilenMaillers
                    join k in db.Kullanicilars on s.IslemYapanID equals k.KullaniciID
                    where s.Silindi == false  
                    select new
                    {
                        s.GonderilenMailID,
                        s.Tarih, 
                        s.Konu,
                        s.Aciklama,
                        s.AciklamaHtml,
                        MailGonderen = k.Ad + " " + k.Soyad,
                        EkSayisi = s.GonderilenMailEkleris.Count,
                        KisiSayisi = s.GonderilenMailKullanicilars.Count,
                        s.Gonderildi,
                        s.HataMesaji
                    };

            if (!model.Konu.IsNullOrWhiteSpace()) q = q.Where(p => p.Konu.Contains(model.Konu));
            if (!model.Aciklama.IsNullOrWhiteSpace()) q = q.Where(p => p.Aciklama.Contains(model.Aciklama));
            if (!model.MailGonderen.IsNullOrWhiteSpace()) q = q.Where(p => p.MailGonderen.Contains(model.MailGonderen));
            if (model.Tarih.HasValue)
            {
                var trih = model.Tarih.Value.TodateToShortDate();
                q = q.Where(p => p.Tarih == trih);

            }
            model.RowCount = q.Count();
            if (!model.Sort.IsNullOrWhiteSpace()) q = q.OrderBy(model.Sort);
            else q = q.OrderByDescending(o => o.Tarih);
            model.Data = q.Skip(model.StartRowIndex).Take(model.PageSize).Select(s => new FrMailGonderme
            {
                GonderilenMailID = s.GonderilenMailID,
                Tarih = s.Tarih,
                Konu = s.Konu,
                Aciklama = s.Aciklama,
                AciklamaHtml = s.AciklamaHtml,
                MailGonderen = s.MailGonderen,
                KisiSayisi = s.KisiSayisi,
                EkSayisi = s.EkSayisi,
                Gonderildi = s.Gonderildi,
                HataMesaji = s.HataMesaji

            }).ToList();
            return View(model);
        }
        public ActionResult MailDetay(int gonderilenMailId)
        {

            var data = (from s in db.GonderilenMaillers
                        join k in db.Kullanicilars on s.IslemYapanID equals k.KullaniciID
                        where s.GonderilenMailID == gonderilenMailId
                        select new FrMailGonderme
                        {
                            GonderilenMailID = s.GonderilenMailID,
                            Tarih = s.Tarih,
                            Konu = s.Konu,
                            Aciklama = s.Aciklama,
                            AciklamaHtml = s.AciklamaHtml,
                            MailGonderen = k.Ad + " " + k.Soyad,
                            IslemYapanIP = s.IslemYapanIP,
                            EkSayisi = s.GonderilenMailEkleris.Count,
                            KisiSayisi = s.GonderilenMailKullanicilars.Count,
                            GonderilenMailEkleris = s.GonderilenMailEkleris.ToList()

                        }).First();
            var dataK = (from s in db.GonderilenMailKullanicilars
                         orderby s.Kullanicilar.Ad, s.Kullanicilar.Soyad
                         where s.GonderilenMailID == gonderilenMailId
                         select new MailKullaniciBilgi
                         {
                             AdSoyad = s.Kullanicilar.Ad + " " + s.Kullanicilar.Soyad,
                             Email = s.Email
                         }).ToList();
            ViewBag.DataK = dataK;
            return View(data);
        }

        public ActionResult Gonder(int? id, List<int> kullaniciId)
        {
            var model = new GonderilenMailler();
            var mmMessage = new MmMessage();
            ViewBag.MmMessage = mmMessage;

            var dataK = (from s in db.GonderilenMailKullanicilars
                         orderby s.Kullanicilar.Ad, s.Kullanicilar.Soyad
                         select new MailKullaniciBilgi
                         {
                             AdSoyad = s.Kullanicilar.Ad + " " + s.Kullanicilar.Soyad,
                             Email = s.Email
                         }).ToList();



            ViewBag.Kullanicilar = dataK;
            ViewBag.SelectedTab = 1;
            ViewBag.Alici = "";
            var eList = new List<ComboModelInt>();
            kullaniciId = kullaniciId ?? new List<int>();
            db.Kullanicilars.Where(p => kullaniciId.Contains(p.KullaniciID)).ToList().ForEach((k) => { eList.Add(new ComboModelInt { Value = k.KullaniciID, Caption = k.EMail }); });

            ViewBag.MailSablonlariID = new SelectList(MailIslemleriBus.CmbMailSablonlari(true, false), "Value", "Caption");
            ViewBag.EmailList = eList;
            return View(model);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Gonder(GonderilenMailler kModel, List<string> dosyaEkiAdi, List<HttpPostedFileBase> dosyaEki, List<int> kullaniciIDs, string alici = "")
        {
            var mmMessage = new MmMessage();
            dosyaEki = dosyaEki ?? new List<HttpPostedFileBase>();
            dosyaEkiAdi = dosyaEkiAdi ?? new List<string>();
            kullaniciIDs = kullaniciIDs ?? new List<int>();
            var secilenAlicilar = new List<string>();
            if (alici.IsNullOrWhiteSpace() == false) alici.Split(',').ToList().ForEach((itm) => { secilenAlicilar.Add(itm); });
            var qDosyaEkAdi = dosyaEkiAdi.Select((s, inx) => new { s, inx }).ToList();
            var qDosyaEki = dosyaEki.Select((s, inx) => new { s, inx }).ToList();
            var qDosyalar = (from dek in qDosyaEkAdi
                             join de in qDosyaEki on dek.inx equals de.inx
                             select new
                             {
                                 dek.inx,
                                 DosyaEkAdi = dek.s,
                                 Dosya = de.s,
                                 mDosyaAdi = dek.s.Replace(".", "") + "." + de.s.FileName.Split('.').Last(),
                                 DosyaYolu = "/MailDosyalari/" + dek.s + "_" + Guid.NewGuid().ToString().Substring(0, 4) + "." + de.s.FileName.Split('.').Last()
                             }).ToList();

            #region Kontrol
            kModel.Tarih = DateTime.Now;

            if (kModel.Konu.IsNullOrWhiteSpace())
            { 
                mmMessage.Messages.Add("Konu Giriniz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Konu" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Konu" });

            if (kModel.Aciklama.IsNullOrWhiteSpace() && kModel.AciklamaHtml.IsNullOrWhiteSpace())
            { 
                mmMessage.Messages.Add("İçerik Giriniz.");
            }


            #endregion
            if (mmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now; 
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                kModel.Aciklama = kModel.Aciklama ?? "";
                var eklenen = db.GonderilenMaillers.Add(kModel);

                foreach (var item in qDosyalar)
                {
                    item.Dosya.SaveAs(Server.MapPath("~" + item.DosyaYolu));
                    db.GonderilenMailEkleris.Add(new GonderilenMailEkleri
                    {

                        GonderilenMailID = eklenen.GonderilenMailID,
                        EkAdi = item.DosyaEkAdi,
                        EkDosyaYolu = item.DosyaYolu
                    });
                }
                var mailList = new List<GonderilenMailKullanicilar>();

                if (secilenAlicilar.Count > 0)
                {
                    var qscIDs = secilenAlicilar.Where(p => p.IsNumber()).Select(s => s.ToInt().Value).ToList();
                    var qscMails = secilenAlicilar.Where(p => p.IsNumber() == false).ToList();
                    var dataqx = (from s in db.Kullanicilars
                                  where qscIDs.Contains(s.KullaniciID)
                                  select new
                                  {
                                      Email = s.EMail,
                                      eklenen.GonderilenMailID,
                                      s.KullaniciID
                                  }).ToList();
                    mailList.AddRange(dataqx.Select(item => new GonderilenMailKullanicilar { Email = item.Email, GonderilenMailID = item.GonderilenMailID, KullaniciID = item.KullaniciID }));

                    mailList.AddRange(qscMails.Select(item => new GonderilenMailKullanicilar { Email = item, GonderilenMailID = eklenen.GonderilenMailID, KullaniciID = null }));
                }
                mailList = db.GonderilenMailKullanicilars.AddRange(mailList).ToList();
                db.SaveChanges();
                var attach = new List<Attachment>();

                foreach (var item in qDosyalar)
                {

                    attach.Add(new Attachment(new MemoryStream(System.IO.File.ReadAllBytes(Server.MapPath("~" + item.DosyaYolu))), item.mDosyaAdi, MediaTypeNames.Application.Octet));
                }
                MailManager.SendMail(eklenen.GonderilenMailID, kModel.Konu, kModel.AciklamaHtml, mailList.Select(s => s.Email).ToList(), attach);
                return RedirectToAction("Index");
            }

            MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, mmMessage.Messages.ToArray());
            ViewBag.MmMessage = mmMessage;


            var qKullanicilar = from k in db.Kullanicilars
                                join bi in db.Birimlers on k.BirimID equals bi.BirimID
                                where k.EMail.Contains("@")
                                orderby k.Ad, k.Soyad
                                select new MailKullaniciBilgi
                                {
                                    KullaniciID = k.KullaniciID,
                                    AdSoyad = k.Ad + " " + k.Soyad,
                                    BirimAdi = bi.BirimAdi,
                                    Email = k.EMail

                                };
            var kul = qKullanicilar.ToList();
            foreach (var item in kul.Where(item => kullaniciIDs.Contains(item.KullaniciID)))
            {
                item.Checked = true;
            }

            ViewBag.Kullanicilar = kul;
            ViewBag.Alici = alici;
            ViewBag.MailSablonlariID = new SelectList(MailIslemleriBus.CmbMailSablonlari(true, false), "Value", "Caption");
            return View(kModel);
        }

         
        public ActionResult TekrarGonder(int id)
        {
            var gonderilenMail = db.GonderilenMaillers.FirstOrDefault(p => p.GonderilenMailID == id);
            var gonderilenMailKullanicilars = gonderilenMail.GonderilenMailKullanicilars.ToList();
            var gonderilenMailEkleris = gonderilenMail.GonderilenMailEkleris.ToList();
            var attachments = gonderilenMailEkleris.Select(item => new Attachment(new MemoryStream(System.IO.File.ReadAllBytes(Server.MapPath("~" + item.EkDosyaYolu))), item.EkAdi, MediaTypeNames.Application.Octet)).ToList();
            MailManager.SendMail(gonderilenMail.GonderilenMailID, gonderilenMail.Konu, gonderilenMail.AciklamaHtml, gonderilenMailKullanicilars.Select(s => s.Email).ToList(), attachments);
            return true.ToJsonResult();
        }



        public ActionResult Sil(int id)
        {
            var kayit = db.GonderilenMaillers.FirstOrDefault(p => p.GonderilenMailID == id);
            var message = "";
            var success = true;
            if (kayit != null)
            {
                try
                {
                    //var dosyalar = kayit.GonderilenMailEkleris.ToList();
                    //foreach (var item in dosyalar)
                    //{
                    //    if (success == true)
                    //    {
                    //        try
                    //        {
                    //            System.IO.File.Delete(Server.MapPath("~" + item.EkDosyaYolu));
                    //        }
                    //        catch (Exception exM)
                    //        {
                    //            message = exM.ToExceptionMessage().Replace("\r\n", "<br/>");
                    //            success = false;
                    //        }
                    //    }
                    //}
                    if (message == "")
                    {
                        kayit.Silindi = true;

                        //db.GonderilenMaillers.Remove(kayit);
                        db.SaveChanges();
                        message = "'" + kayit.Konu + "' konulu email Silindi!";
                    }

                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.Konu + "' Konulu Mail Silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    SistemBilgilendirmeBus.SistemBilgisiKaydet(message, "MailGonder/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz mail bilgisi sistemde bulunamadı!";
            }
            return Json(new { success, message }, "application/json", JsonRequestBehavior.AllowGet);
        }

    }
}
