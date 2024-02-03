using BiskaUtil;
using Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
    [Authorize(Roles = RoleNames.Maddeler)]
    public class MaddeTurleriController : Controller
    {
        private VysDBEntities db = new VysDBEntities();
        public ActionResult Index()
        {

            return Index(new FmMaddeTurleri { PageSize = 10, IsAktif = true });
        }
        [HttpPost]
        public ActionResult Index(FmMaddeTurleri model)
        {
            var q = (from s in db.MaddeTurleris
                     join kul in db.Kullanicilars on s.IslemYapanID equals kul.KullaniciID
                     select new FrMaddeTurleri
                     {
                         MaddeTurID = s.MaddeTurID,
                         MaddeTurAdi = s.MaddeTurAdi,
                         IsPlanlananDegerOlacak = s.IsPlanlananDegerOlacak,
                         IsPlanlananDegerOlacakGelecekYil = s.IsPlanlananDegerOlacakGelecekYil, 
                         IsAktif = s.IsAktif,
                         IslemTarihi = s.IslemTarihi,
                         IslemYapan = kul.KullaniciAdi,
                         IslemYapanID = s.IslemYapanID,
                         IslemYapanIP = s.IslemYapanIP,
                     }).AsQueryable();

            if (!model.MaddeTurAdi.IsNullOrWhiteSpace()) q = q.Where(p => p.MaddeTurAdi.Contains(model.MaddeTurAdi));
            if (model.IsPlanlananDegerOlacak.HasValue) q = q.Where(p => p.IsPlanlananDegerOlacak == model.IsPlanlananDegerOlacak);
            if (model.IsPlanlananDegerOlacakGelecekYil.HasValue) q = q.Where(p => p.IsPlanlananDegerOlacakGelecekYil == model.IsPlanlananDegerOlacakGelecekYil); 
            if (model.IsAktif.HasValue) q = q.Where(p => p.IsAktif == model.IsAktif);
            model.RowCount = q.Count();
            q = !model.Sort.IsNullOrWhiteSpace() ? q.OrderBy(model.Sort) : q.OrderBy(o => o.MaddeTurAdi);
            model.CountIngfos = new MIndexBilgi() { Toplam = model.RowCount, Pasif = q.Count(p => !p.IsAktif), Aktif = q.Count(p => p.IsAktif) };
            
           
            model.Data = q.Skip(model.PagingStartRowIndex).Take(model.PageSize).ToList();;
            ViewBag.IsPlanlananDegerOlacak = new SelectList(ComboData.GetCmbEvetHayirData(), "Value", "Caption", model.IsPlanlananDegerOlacak);
            ViewBag.IsPlanlananDegerOlacakGelecekYil = new SelectList(ComboData.GetCmbEvetHayirData(), "Value", "Caption", model.IsPlanlananDegerOlacakGelecekYil); 
            ViewBag.IsAktif = new SelectList(ComboData.GetCmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            return View(model);
        }
        public ActionResult Kayit(int? id)
        {
            var mmMessage = new MmMessage();
            ViewBag.MmMessage = mmMessage;
            var model = new MaddeTurleri
            {
                IsAktif = true
            };
            if (!id.HasValue) return View(model);
            var data = db.MaddeTurleris.FirstOrDefault(p => p.MaddeTurID == id);
            if (data != null)
            {
                model = data;
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Kayit(MaddeTurleri kModel)
        {
            var mmMessage = new MmMessage();
            #region Kontrol

            if (kModel.MaddeTurAdi.IsNullOrWhiteSpace())
            {
                mmMessage.Messages.Add("Madde Tür Adı boş bırakılamaz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "MaddeTurAdi" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "MaddeTurAdi" });


            #endregion

            if (mmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                MaddeTurleri maddeTuru;

                if (kModel.MaddeTurID <= 0)
                {
                    maddeTuru = db.MaddeTurleris.Add(kModel);
                }
                else
                {
                    maddeTuru = db.MaddeTurleris.First(p => p.MaddeTurID == kModel.MaddeTurID); 
                    maddeTuru.IsPlanlananDegerOlacak = kModel.IsPlanlananDegerOlacak;
                    maddeTuru.IsPlanlananDegerOlacakGelecekYil = kModel.IsPlanlananDegerOlacakGelecekYil;
                    maddeTuru.MaddeTurAdi = kModel.MaddeTurAdi;
                    maddeTuru.IsAktif = kModel.IsAktif;
                    maddeTuru.IslemTarihi = kModel.IslemTarihi;
                    maddeTuru.IslemYapanID = kModel.IslemYapanID;
                    maddeTuru.IslemYapanIP = kModel.IslemYapanIP;
                }
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, mmMessage.Messages.ToArray());
            ViewBag.MmMessage = mmMessage;
            return View(kModel);
        }


        public ActionResult Sil(int id)
        {
            var kayit = db.MaddeTurleris.FirstOrDefault(p => p.MaddeTurID == id);
            string message;
            var success = true;
            if (kayit != null)
            {

                try
                {
                    message = "'" + kayit.MaddeTurAdi + "' isimli Madde Türü silindi!";
                    db.MaddeTurleris.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.MaddeTurAdi + "' isimli Madde Türü silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    SistemBilgilendirmeBus.SistemBilgisiKaydet(message, "MaddeTurleri/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Madde Türü sistemde bulunamadı!";
            }
            return Json(new { success, message }, "application/json", JsonRequestBehavior.AllowGet);
        }
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}