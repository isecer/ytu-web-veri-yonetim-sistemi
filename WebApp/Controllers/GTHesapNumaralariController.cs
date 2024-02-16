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
    [Authorize(Roles = RoleNames.GTHesapNumaralari)]
    public class GtHesapNumaralariController : Controller
    {
        // GET: BTKurumsalKodlar
        private VysDBEntities db = new VysDBEntities();
        public ActionResult Index()
        {

            return Index(new FmGTHesapNumaralari { PageSize = 30 });
        }
        [HttpPost]
        public ActionResult Index(FmGTHesapNumaralari model)
        {
            var q = (from s in db.GTHesapNumaralaris
                     join kul in db.Kullanicilars on s.IslemYapanID equals kul.KullaniciID
                     select new FrGTHesapNumaralari
                     {
                         GTHesapNoID = s.GTHesapNoID,
                         HesapNo = s.HesapNo,
                         HesapNoAdi = s.HesapNoAdi,
                         IsAktif = s.IsAktif,
                         IslemTarihi = s.IslemTarihi,
                         IslemYapan = kul.KullaniciAdi,
                         IslemYapanID = s.IslemYapanID,
                         IslemYapanIP = s.IslemYapanIP,
                     }).AsQueryable();

            if (!model.HesapNo.IsNullOrWhiteSpace()) q = q.Where(p => p.HesapNo.StartsWith(model.HesapNo));
            if (!model.HesapNoAdi.IsNullOrWhiteSpace()) q = q.Where(p => p.HesapNoAdi.Contains(model.HesapNoAdi));
            if (model.IsAktif.HasValue) q = q.Where(p => p.IsAktif == model.IsAktif);

            model.RowCount = q.Count();
            model.AktifCount = q.Count(p => p.IsAktif);
            q = !model.Sort.IsNullOrWhiteSpace() ? q.OrderBy(model.Sort) : q.OrderBy(o => o.HesapNoAdi); 



            model.Data = q.Skip(model.PagingStartRowIndex).Take(model.PageSize).ToList(); ;

            ViewBag.IsAktif = new SelectList(ComboData.GetCmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            return View(model);
        }
        public ActionResult Kayit(int? id)
        {
            var MmMessage = new MmMessage();
            ViewBag.MmMessage = MmMessage;
            var model = new GTHesapNumaralari();
            model.IsAktif = true;
            if (id.HasValue)
            {
                var data = db.GTHesapNumaralaris.Where(p => p.GTHesapNoID == id).FirstOrDefault();
                if (data != null)
                {
                    model = data;
                }
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.GTHesapNumaralariKayit)]
        public ActionResult Kayit(GTHesapNumaralari kModel)
        {
            var MmMessage = new MmMessage();
            #region Kontrol
            if (kModel.HesapNo.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Hesap Numarası bilgisi boş bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "HesapNo" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "HesapNo" });
            if (kModel.HesapNoAdi.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Hesap Adı boş bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "HesapNoAdi" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "HesapNoAdi" });



            if (MmMessage.Messages.Count == 0)
            {
                if (db.GTHesapNumaralaris.Any(a => a.HesapNo == kModel.HesapNo && a.GTHesapNoID != kModel.GTHesapNoID))
                {
                    MmMessage.Messages.Add("Girilen Hesap Numarası bilgisi daha önceden girilen bir Hesap Numarası ile çakışmaktadır.");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "HesapNo" });
                }
                else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "HesapNo" });

            }

            #endregion

            if (MmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                var Table = new GTHesapNumaralari();

                if (kModel.GTHesapNoID <= 0)
                {
                    kModel.IsAktif = true;
                    Table = db.GTHesapNumaralaris.Add(kModel);
                }
                else
                {
                    Table = db.GTHesapNumaralaris.Where(p => p.GTHesapNoID == kModel.GTHesapNoID).First();
                    Table.HesapNoAdi = kModel.HesapNoAdi;
                    Table.HesapNo = kModel.HesapNo;
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

        [Authorize(Roles = RoleNames.GTHesapNumaralariKayit)]
        public ActionResult Sil(int id)
        {
            var kayit = db.GTHesapNumaralaris.Where(p => p.GTHesapNoID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {

                try
                {
                    message = "'" + kayit.HesapNoAdi + " (" + kayit.GTHesapNoID + ")' isimli Hesap Numarası silindi!";
                    db.GTHesapNumaralaris.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.HesapNoAdi + " (" + kayit.GTHesapNoID + ")' isimli Hesap Numarası silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    SistemBilgilendirmeBus.SistemBilgisiKaydet(message, "GTHesapNumaralari/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
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