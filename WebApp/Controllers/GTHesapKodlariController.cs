using BiskaUtil;
using Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebApp.Business;
using WebApp.Models;
using WebApp.Utilities.Extensions;
using WebApp.Utilities.Helpers;
using WebApp.Utilities.MenuAndRoles;
using WebApp.Utilities.MessageBox;
using WebApp.Utilities.SystemData;

namespace WebApp.Controllers
{
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [Authorize(Roles = RoleNames.GTHesapKodlari)]
    public class GtHesapKodlariController : Controller
    {
        // GET: BTKurumsalKodlar
        private VysDBEntities db = new VysDBEntities();
        public ActionResult Index()
        {

            return Index(new FmGTHesapKodlari { PageSize = 30 });
        }
        [HttpPost]
        public ActionResult Index(FmGTHesapKodlari model)
        {
            var q = (from s in db.GTHesapKodlaris
                     join kul in db.Kullanicilars on s.IslemYapanID equals kul.KullaniciID
                     select new FrGTHesapKodlari
                     {
                         GTHesapKodID = s.GTHesapKodID,
                         HesapKod = s.HesapKod,
                         HesapKodAdi = s.HesapKodAdi,
                         IsAktif = s.IsAktif,
                         IslemTarihi = s.IslemTarihi,
                         IslemYapan = kul.KullaniciAdi,
                         IslemYapanID = s.IslemYapanID,
                         IslemYapanIP = s.IslemYapanIP,
                         GTHesapKodlariGelirNitelikleris = s.GTHesapKodlariGelirNitelikleris
                     }).AsQueryable();

            if (!model.HesapKod.IsNullOrWhiteSpace()) q = q.Where(p => p.HesapKod.StartsWith(model.HesapKod));
            if (!model.HesapKodAdi.IsNullOrWhiteSpace()) q = q.Where(p => p.HesapKodAdi.Contains(model.HesapKodAdi));
            if (model.IsAktif.HasValue) q = q.Where(p => p.IsAktif == model.IsAktif);


            model.RowCount = q.Count();
            model.AktifCount = q.Count(p => p.IsAktif);
            q = !model.Sort.IsNullOrWhiteSpace() ? q.OrderBy(model.Sort) : q.OrderBy(o => o.HesapKodAdi); 



            model.Data = q.Skip(model.PagingStartRowIndex).Take(model.PageSize).ToList(); ;

            ViewBag.IsAktif = new SelectList(ComboData.GetCmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            return View(model);
        }
        public ActionResult Kayit(int? id)
        {
            var MmMessage = new MmMessage();
            ViewBag.MmMessage = MmMessage;
            var model = new GTHesapKodlari();
            model.IsAktif = true;
            if (id.HasValue)
            {
                var data = db.GTHesapKodlaris.Where(p => p.GTHesapKodID == id).FirstOrDefault();
                if (data != null)
                {
                    model = data;
                }
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.GTHesapKodlariKayit)]
        public ActionResult Kayit(GTHesapKodlari kModel, List<int> GTHesapKodlariGelirNiteligiID, List<string> GelirNiteligiAdi)
        {
            var MmMessage = new MmMessage();
            GelirNiteligiAdi = GelirNiteligiAdi ?? new List<string>();
            GTHesapKodlariGelirNiteligiID = GTHesapKodlariGelirNiteligiID ?? new List<int>();
            #region Kontrol
            if (kModel.HesapKod.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Hesap Kodu bilgisi boş bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "HesapKod" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "HesapKod" });
            if (kModel.HesapKodAdi.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Hesap Adı boş bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "HesapKodAdi" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "HesapKodAdi" });


            var qID = GTHesapKodlariGelirNiteligiID.Select((s, inx) => new { s, inx }).ToList();
            var qAd = GelirNiteligiAdi.Select((s, inx) => new { s, inx }).ToList();
            var qGelirNiteliks = (from dek in qID
                                  join de in qAd on dek.inx equals de.inx
                                  select new GTHesapKodlariGelirNitelikleri
                                  {
                                      GTHesapKodlariGelirNiteligiID = dek.s,
                                      GelirNiteligiAdi = de.s
                                  }).ToList();
            if (qGelirNiteliks.Count == 0)
            {
                string msg = "Kayıt işlemini yapabilmeniz için en az bir Gelir Niteliği eklemeniz gerekmektedir!";
                MmMessage.Messages.Add(msg);
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "GelirNiteligiAdi_" });
            }




            if (MmMessage.Messages.Count == 0)
            {
                if (db.GTHesapKodlaris.Any(a => a.HesapKod == kModel.HesapKod && a.GTHesapKodID != kModel.GTHesapKodID))
                {
                    MmMessage.Messages.Add("Girilen Hesap Kod bilgisi daha önceden girilen bir Hesap Kod ile çakışmaktadır.");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "HesapKod" });
                }
                else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "HesapKod" });

            }

            #endregion

            if (MmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                var Table = new GTHesapKodlari();

                if (kModel.GTHesapKodID <= 0)
                {
                    kModel.IsAktif = true;
                    Table = db.GTHesapKodlaris.Add(kModel);
                    Table.GTHesapKodlariGelirNitelikleris = qGelirNiteliks;
                }
                else
                {
                    Table = db.GTHesapKodlaris.Where(p => p.GTHesapKodID == kModel.GTHesapKodID).First();
                    Table.HesapKodAdi = kModel.HesapKodAdi;
                    Table.HesapKod = kModel.HesapKod;
                    Table.IsAktif = kModel.IsAktif;
                    Table.IslemTarihi = kModel.IslemTarihi;
                    Table.IslemYapanID = kModel.IslemYapanID;
                    Table.IslemYapanIP = kModel.IslemYapanIP;

                    var VarolanGNT = qGelirNiteliks.Where(p => Table.GTHesapKodlariGelirNitelikleris.Any(a => a.GTHesapKodlariGelirNiteligiID == p.GTHesapKodlariGelirNiteligiID)).ToList();
                    var SilinecekGNT = Table.GTHesapKodlariGelirNitelikleris.Where(p => !qGelirNiteliks.Any(a => a.GTHesapKodlariGelirNiteligiID == p.GTHesapKodlariGelirNiteligiID)).ToList();
                    var EklenecekGNT = qGelirNiteliks.Where(p => p.GTHesapKodlariGelirNiteligiID == 0).ToList();

                    db.GTHesapKodlariGelirNitelikleris.RemoveRange(SilinecekGNT);
                    foreach (var item in EklenecekGNT)
                        Table.GTHesapKodlariGelirNitelikleris.Add(item);

                    foreach (var item in VarolanGNT)
                    {
                        var GNT = Table.GTHesapKodlariGelirNitelikleris.Where(p => p.GTHesapKodlariGelirNiteligiID == item.GTHesapKodlariGelirNiteligiID).First();
                        GNT.GelirNiteligiAdi = item.GelirNiteligiAdi;
                    }

                }
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                kModel.GTHesapKodlariGelirNitelikleris = qGelirNiteliks;
                MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, MmMessage.Messages.ToArray());
            }
            ViewBag.MmMessage = MmMessage;
            return View(kModel);
        }

        [Authorize(Roles = RoleNames.GTHesapKodlariKayit)]
        public ActionResult Sil(int id)
        {
            var kayit = db.GTHesapKodlaris.Where(p => p.GTHesapKodID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {

                try
                {
                    message = "'" + kayit.HesapKodAdi + " (" + kayit.GTHesapKodID + ")' isimli Hesap Kodu silindi!";
                    db.GTHesapKodlaris.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.HesapKodAdi + " (" + kayit.GTHesapKodID + ")' isimli Hesap Kodu silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    SistemBilgilendirmeBus.SistemBilgisiKaydet(message, "GTHesapKodlari/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Hesap Kodu sistemde bulunamadı!";
            }
            return Json(new { success = success, message = message }, "application/json", JsonRequestBehavior.AllowGet);
        }
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}