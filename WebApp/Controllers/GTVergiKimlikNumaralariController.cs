using BiskaUtil;
using Database;
using System;
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
    [Authorize(Roles = RoleNames.GTVergiKimlikNumaralari)]
    public class GtVergiKimlikNumaralariController : Controller
    {
        private VysDBEntities db = new VysDBEntities();
        public ActionResult Index()
        {

            return Index(new FmGTVergiKimlikNumaralari { PageSize = 30 });
        }
        [HttpPost]
        public ActionResult Index(FmGTVergiKimlikNumaralari model)
        {
            var q = (from s in db.GTVergiKimlikNumaralaris
                     join kul in db.Kullanicilars on s.IslemYapanID equals kul.KullaniciID
                     select new FrGTVergiKimlikNumaralari
                     {
                         GTVergiKimlikNoID = s.GTVergiKimlikNoID,
                         VergiKimlikNo = s.VergiKimlikNo,
                         AdSoyad = s.AdSoyad,
                         IsAktif = s.IsAktif,
                         IslemTarihi = s.IslemTarihi,
                         IslemYapan = kul.KullaniciAdi,
                         IslemYapanID = s.IslemYapanID,
                         IslemYapanIP = s.IslemYapanIP,
                     }).AsQueryable();

            if (!model.VergiKimlikNo.IsNullOrWhiteSpace()) q = q.Where(p => p.VergiKimlikNo == model.VergiKimlikNo);
            if (!model.AdSoyad.IsNullOrWhiteSpace()) q = q.Where(p => p.AdSoyad.Contains(model.AdSoyad));
            if (model.IsAktif.HasValue) q = q.Where(p => p.IsAktif == model.IsAktif);

            model.RowCount = q.Count();
            model.AktifCount = q.Count(p => p.IsAktif);
            q = !model.Sort.IsNullOrWhiteSpace() ? q.OrderBy(model.Sort) : q.OrderBy(o => o.AdSoyad); 

            model.Data = q.Skip(model.PagingStartRowIndex).Take(model.PageSize).ToList(); ;

            ViewBag.IsAktif = new SelectList(ComboData.GetCmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            return View(model);
        }
        public ActionResult Kayit(int? id)
        {
            var MmMessage = new MmMessage();
            ViewBag.MmMessage = MmMessage;
            var model = new GTVergiKimlikNumaralari();
            model.IsAktif = true;
            if (id.HasValue)
            {
                var data = db.GTVergiKimlikNumaralaris.Where(p => p.GTVergiKimlikNoID == id).FirstOrDefault();
                if (data != null)
                {
                    model = data;
                }
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.GTVergiKimlikNumaralariKayit)]
        public ActionResult Kayit(GTVergiKimlikNumaralari kModel)
        {
            var MmMessage = new MmMessage();
            #region Kontrol
            if (kModel.VergiKimlikNo.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Vergi Kimlik No bilgisi boş bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "VergiKimlikNo" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "VergiKimlikNo" });
            if (kModel.AdSoyad.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Ad Soyad boş bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "AdSoyad" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "AdSoyad" });



            if (MmMessage.Messages.Count == 0)
            {
                if (db.GTVergiKimlikNumaralaris.Any(a => a.VergiKimlikNo == kModel.VergiKimlikNo && a.GTVergiKimlikNoID != kModel.GTVergiKimlikNoID))
                {
                    MmMessage.Messages.Add("Girilen Vergi Kimlik Numarası bilgisi daha önceden girilen bir Vergi Kimlik Numarası ile çakışmaktadır.");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "VergiKimlikNo" });
                }
                else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "VergiKimlikNo" });

            }

            #endregion

            if (MmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                var Table = new GTVergiKimlikNumaralari();

                if (kModel.GTVergiKimlikNoID <= 0)
                {
                    kModel.IsAktif = true;
                    Table = db.GTVergiKimlikNumaralaris.Add(kModel);
                }
                else
                {
                    Table = db.GTVergiKimlikNumaralaris.Where(p => p.GTVergiKimlikNoID == kModel.GTVergiKimlikNoID).First();
                    Table.AdSoyad = kModel.AdSoyad;
                    Table.VergiKimlikNo = kModel.VergiKimlikNo;
                    Table.IsAktif = kModel.IsAktif;
                    Table.IslemTarihi = kModel.IslemTarihi;
                    Table.IslemYapanID = kModel.IslemYapanID;
                    Table.IslemYapanIP = kModel.IslemYapanIP;
                }
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, MmMessage.Messages.ToArray());
            }
            ViewBag.MmMessage = MmMessage;
            return View(kModel);
        }

        [Authorize(Roles = RoleNames.GTVergiKimlikNumaralariKayit)]
        public ActionResult Sil(int id)
        {
            var kayit = db.GTVergiKimlikNumaralaris.Where(p => p.GTVergiKimlikNoID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {

                try
                {
                    message = "'" + kayit.AdSoyad + " (" + kayit.VergiKimlikNo + ")' isimli Vergi Kimlik Numarası silindi!";
                    db.GTVergiKimlikNumaralaris.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.AdSoyad + " (" + kayit.VergiKimlikNo + ")' isimli Vergi Kimlik Numarası silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    SistemBilgilendirmeBus.SistemBilgisiKaydet(message, "GTVergiKimlikNumaralari/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Hesap Numarası sistemde bulunamadı!";
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