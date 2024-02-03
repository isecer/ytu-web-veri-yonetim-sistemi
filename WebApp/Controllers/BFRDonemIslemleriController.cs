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
    [Authorize(Roles = RoleNames.BFRDonemIslemleri)]
    public class BfrDonemIslemleriController : Controller
    {
        // GET: FRDonemIslemleri
        private readonly VysDBEntities db = new VysDBEntities();
        public ActionResult Index()
        {
            return Index(new FmBfrDonemleri() { PageSize = 15 });
        }
        [HttpPost]
        public ActionResult Index(FmBfrDonemleri model)
        {
            var nowDate = DateTime.Now.Date;
            var q = from s in db.BFRDonemleris
                    join k in db.Kullanicilars on s.IslemYapanID equals k.KullaniciID
                    select new FrBfrDonemleri
                    {
                        BFRDonemID = s.BFRDonemID,
                        DonemYilAdi = s.Yil + " Yılı Dönemi",
                        Yil = s.Yil,
                        BaslangicTarihi = s.BaslangicTarihi,
                        BitisTarihi = s.BitisTarihi,
                        IsAktif = s.IsAktif,
                        IslemYapanID = s.IslemYapanID,
                        IslemYapan = k.KullaniciAdi,
                        IslemTarihi = s.IslemTarihi,
                        IslemYapanIP = s.IslemYapanIP,
                        AktifSurec = (s.BaslangicTarihi <= nowDate && s.BitisTarihi >= nowDate)
                    };
            if (model.IsAktif.HasValue) q = q.Where(p => p.IsAktif == model.IsAktif);
            model.RowCount = q.Count();
            q = !model.Sort.IsNullOrWhiteSpace() ? q.OrderBy(model.Sort) : q.OrderByDescending(t => t.Yil).ThenByDescending(t => t.BaslangicTarihi);
            model.Data = q.Skip(model.StartRowIndex).Take(model.PageSize).ToList();
            model.CountIngfos =  new MIndexBilgi() { Toplam = model.RowCount, Pasif = q.Count(p => !p.IsAktif),Aktif = q.Count(p=>p.IsAktif)}; 
            ViewBag.IsAktif = new SelectList(ComboData.GetCmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            return View(model);
        }
        public ActionResult Kayit(int? id)
        {
            var mmMessage = new MmMessage();
            ViewBag.MmMessage = mmMessage;
            var model = new KmBfrDonemleri
            {
                IsAktif = true
            };
            if (id > 0)
            {
                var data = db.BFRDonemleris.FirstOrDefault(p => p.BFRDonemID == id);

                if (data != null)
                {
                    model.BFRDonemID = data.BFRDonemID;
                    model.Yil = data.Yil;
                    model.BaslangicTarihi = data.BaslangicTarihi;
                    model.BitisTarihi = data.BitisTarihi;
                    model.IsAktif = data.IsAktif;
                    model.IslemTarihi = DateTime.Now;
                    model.IslemYapanID = data.IslemYapanID;
                    model.IslemYapanIP = data.IslemYapanIP;
                }

            }
            else
            {
                model.Yil = DateTime.Now.Year;
            }
            ViewBag.IsAktif = new SelectList(ComboData.GetCmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            ViewBag.Yil = new SelectList(ComboData.CmbSurecKayitYillari(), "Value", "Caption", model.Yil);

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = RoleNames.BFRDonemIslemleriKayitYetkisi)]
        public ActionResult Kayit(KmBfrDonemleri kModel, List<int> bfrFormId)
        {
            var mmMessage = new MmMessage();
            bfrFormId = bfrFormId ?? new List<int>();
            #region Kontrol  
            if (kModel.Yil <= 0)
            {
                mmMessage.Messages.Add("Dönem Yılını Giriniz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Yil" });

            }
            else if (db.BFRDonemleris.Any(p => p.BFRDonemID != kModel.BFRDonemID && p.Yil == kModel.Yil))
            {
                mmMessage.Messages.Add("Seçtiğiniz Faaliyet Rapor Dönem Yılı Daha Önceden Kayıt Edilmiştir. Tekrar Kayıt Edilemez.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Yil" });
            }
            else
            {
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Yil" });
            }
            if (!kModel.BaslangicTarihi.HasValue || !kModel.BitisTarihi.HasValue)
            {
                if (!kModel.BaslangicTarihi.HasValue)
                {
                    mmMessage.Messages.Add("Başlangıç Tarihi Seçiniz.");
                    mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BaslangicTarihi" });

                }
                if (!kModel.BitisTarihi.HasValue)
                {
                    mmMessage.Messages.Add("Bitiş Tarihi Seçiniz.");
                    mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BitisTarihi" });

                }

            }
            else if (kModel.BaslangicTarihi >= kModel.BitisTarihi)
            {
                mmMessage.Messages.Add("Başlangıç Tarihi Bitiş Tarihinden Büyük Yada Eşit Olamaz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BitisTarihi" });
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BaslangicTarihi" });
            }
            else
            {
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "BaslangicTarihi" });
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "BitisTarihi" });
            }
            if (bfrFormId.Count == 0)
            {
                mmMessage.Messages.Add("En az bir Form seçilmelidir.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BFRFormID" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "BFRFormID" });
            #endregion

            if (mmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                kModel.Yil = kModel.Yil;
                BFRDonemleri table;
                if (kModel.BFRDonemID <= 0)
                {
                    table = db.BFRDonemleris.Add(new BFRDonemleri
                    {

                        Yil = kModel.Yil,
                        BaslangicTarihi = kModel.BaslangicTarihi.Value,
                        BitisTarihi = kModel.BitisTarihi.Value,
                        IsAktif = kModel.IsAktif,
                        IslemTarihi = kModel.IslemTarihi,
                        IslemYapanID = kModel.IslemYapanID,
                        IslemYapanIP = kModel.IslemYapanIP

                    });
                }
                else
                {
                    table = db.BFRDonemleris.First(p => p.BFRDonemID == kModel.BFRDonemID);
                    table.Yil = kModel.Yil;
                    table.BaslangicTarihi = kModel.BaslangicTarihi.Value;
                    table.BitisTarihi = kModel.BitisTarihi.Value;
                    table.IsAktif = kModel.IsAktif;
                    table.IslemTarihi = DateTime.Now;
                    table.IslemYapanID = kModel.IslemYapanID;
                    table.IslemYapanIP = kModel.IslemYapanIP;


                }
                var donemFormlari = db.BFRDonemlerForms.Where(p => p.BFRDonemID == table.BFRDonemID).ToList();
                var formlar = db.BFRFormlars.Where(p => bfrFormId.Contains(p.BFRFormID)).ToList();
                var eklenecek = formlar.Where(p => donemFormlari.All(a => a.BFRFormID != p.BFRFormID)).ToList();
                var silinecekler = donemFormlari.Where(p => !bfrFormId.Contains(p.BFRFormID)).ToList();
                var guncellenecekler = donemFormlari.Where(p => bfrFormId.Contains(p.BFRFormID)).ToList();
                foreach (var item in guncellenecekler)
                {
                    var form = formlar.First(p => p.BFRFormID == item.BFRFormID);
                    item.IsAktif = form.IsAktif;
                    item.IslemTarihi = DateTime.Now;
                    item.IslemYapanIP = UserIdentity.Ip;
                    item.IslemYapanID = UserIdentity.Current.Id;
                    db.BFRDonemlerFormBirims.RemoveRange(item.BFRDonemlerFormBirims);
                    foreach (var itemB in form.BFRFormlarBirims)
                        item.BFRDonemlerFormBirims.Add(new BFRDonemlerFormBirim { BirimID = itemB.BirimID, BFRFormID = itemB.BFRFormID });
                }

                foreach (var item in eklenecek)
                {
                    table.BFRDonemlerForms.Add(new BFRDonemlerForm
                    {
                        BFRFormID = item.BFRFormID,
                        IsAktif = item.IsAktif,
                        IslemTarihi = DateTime.Now,
                        IslemYapanIP = UserIdentity.Ip,
                        IslemYapanID = UserIdentity.Current.Id,
                        BFRDonemlerFormBirims = item.BFRFormlarBirims.Select(s => new BFRDonemlerFormBirim { BirimID = s.BirimID, BFRFormID = s.BFRFormID }).ToList()

                    });
                }
                foreach (var item in silinecekler)
                    item.IsAktif = false;

                db.SaveChanges();

                return RedirectToAction("Index");
            }

            MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, mmMessage.Messages.ToArray());
            ViewBag.SecilenlerMd = bfrFormId;
            ViewBag.MmMessage = mmMessage;
            ViewBag.IsAktif = new SelectList(ComboData.GetCmbAktifPasifData(), "Value", "Caption", kModel.IsAktif);
            ViewBag.Yil = new SelectList(ComboData.CmbSurecKayitYillari(), "Value", "Caption", kModel.Yil);
            return View(kModel);
        }

        [Authorize(Roles = RoleNames.BFRDonemIslemleriKayitYetkisi)]
        public ActionResult Sil(int id)
        {
            var mmMessage = new MmMessage();

            var kayit = db.BFRDonemleris.FirstOrDefault(p => p.BFRDonemID == id);

            string message;
            if (kayit != null)
            {
                try
                {
                    message = "'" + kayit.Yil + "' Yılı Faaliyet Rapor Dönemi Silindi!";
                    db.BFRDonemleris.Remove(kayit);
                    db.SaveChanges();
                    mmMessage.Title = "Uyarı";
                    mmMessage.Messages.Add(message);
                    mmMessage.MessageType = Msgtype.Success;
                    mmMessage.IsSuccess = true;
                }
                catch (Exception ex)
                {
                    message = "'" + kayit.Yil + "' Yılı Faaliyet Rapor Dönemi Silinirken Bir Hata Oluştu! </br> Hata:" + ex.ToExceptionMessage();
                    SistemBilgilendirmeBus.SistemBilgisiKaydet(message, "BFRDonemIslemleri/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                    mmMessage.Title = "Hata";
                    mmMessage.Messages.Add(message);
                    mmMessage.MessageType = Msgtype.Error;
                    mmMessage.IsSuccess = false;
                }
            }
            else
            {
                message = "Silmek İstediğiniz Faaliyet Rapor Dönemi Sistemde Bulunamadı!";
                mmMessage.Title = "Hata";
                mmMessage.Messages.Add(message);
                mmMessage.MessageType = Msgtype.Error;
                mmMessage.IsSuccess = true;
            }
            var strView = ViewRenderHelper.RenderPartialView("Ajax", "getMessage", mmMessage);
            return Json(new { mmMessage.IsSuccess, Messages = strView }, "application/json", JsonRequestBehavior.AllowGet);
        }
    }
}