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

namespace WebApp.Controllers
{
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [Authorize(Roles = RoleNames.GTBirimler)]
    public class GtBirimlerController : Controller
    {
        // GET: BTKurumsalKodlar
        private VysDBEntities db = new VysDBEntities();
        public ActionResult Index()
        {

            return Index(new FmGTBirimler { PageSize = 30 });
        }
        [HttpPost]
        public ActionResult Index(FmGTBirimler model)
        {
            var q = (from s in db.GTBirimlers
                     join kul in db.Kullanicilars on s.IslemYapanID equals kul.KullaniciID
                     select new FrGTBirimler
                     {
                         GTBirimID = s.GTBirimID,
                         BirimKod = s.BirimKod,
                         BirimAdi = s.BirimAdi,
                         IsAktif = s.IsAktif,
                         IslemTarihi = s.IslemTarihi,
                         IslemYapan = kul.KullaniciAdi,
                         IslemYapanID = s.IslemYapanID,
                         IslemYapanIP = s.IslemYapanIP,
                         GTHesapKodlariList = s.GTBirimHesapKodlaris.Select(s2 => s2.GTHesapKodlari).ToList(),
                         GTHesapNumaralariList = s.GTBirimHesapNumaralaris.Select(s2 => s2.GTHesapNumaralari).ToList()
                     }).AsQueryable();

            model.RowCount = q.Count();
            model.AktifCount = q.Count(p => p.IsAktif);
            q = !model.Sort.IsNullOrWhiteSpace() ? q.OrderBy(model.Sort) : q.OrderBy(o => o.BirimAdi); 



            model.Data = q.Skip(model.PagingStartRowIndex).Take(model.PageSize).ToList(); ;

            return View(model);
        }
        public ActionResult Kayit(int? id)
        {
            var MmMessage = new MmMessage();
            ViewBag.MmMessage = MmMessage;
            var model = new GTBirimler();
            model.IsAktif = true;
            if (id.HasValue)
            {
                var data = db.GTBirimlers.Where(p => p.GTBirimID == id).FirstOrDefault();
                if (data != null)
                {
                    model = data;
                }
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.GTBirimlerKayit)]
        public ActionResult Kayit(GTBirimler kModel, List<int> GTHesapNoID, List<int> GTHesapKodID)
        {
            GTHesapKodID = GTHesapKodID ?? new List<int>();
            GTHesapNoID = GTHesapNoID ?? new List<int>();
            var MmMessage = new MmMessage();
            #region Kontrol
            if (kModel.BirimKod.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Birim Kodu bilgisi boş bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BirimKod" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "BirimKod" });
            if (kModel.BirimAdi.IsNullOrWhiteSpace())
            {
                MmMessage.Messages.Add("Birim Adı boş bırakılamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BirimAdi" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "BirimAdi" });
            if (GTHesapNoID.Count == 0)
            {

                MmMessage.Messages.Add("Birime bağlı en az bir Hesap Numarası seçmeniz gerekmektedir.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "GTHesapNoID" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "GTHesapNoID" });
            if (GTHesapKodID.Count == 0)
            {

                MmMessage.Messages.Add("Birime bağlı en az bir Hesap Kodu seçmeniz gerekmektedir.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "GTHesapKodID" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "GTHesapKodID" });


            if (MmMessage.Messages.Count == 0)
            {
                if (db.GTBirimlers.Any(a => a.BirimKod == kModel.BirimKod && a.GTBirimID != kModel.GTBirimID))
                {
                    MmMessage.Messages.Add("Girilen Birim Kod bilgisi daha önceden girilen bir Birim Kod ile çakışmaktadır.");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BirimKod" });
                }
                else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "BirimKod" });

            }

            #endregion

            if (MmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                var Table = new GTBirimler();

                if (kModel.GTBirimID <= 0)
                {
                    kModel.IsAktif = true;
                    Table = db.GTBirimlers.Add(kModel);

                }
                else
                {
                    Table = db.GTBirimlers.Where(p => p.GTBirimID == kModel.GTBirimID).First();
                    Table.BirimAdi = kModel.BirimAdi;
                    Table.BirimKod = kModel.BirimKod;
                    Table.IsAktif = kModel.IsAktif;
                    Table.IslemTarihi = kModel.IslemTarihi;
                    Table.IslemYapanID = kModel.IslemYapanID;
                    Table.IslemYapanIP = kModel.IslemYapanIP;
                    db.GTBirimHesapNumaralaris.RemoveRange(Table.GTBirimHesapNumaralaris);
                    db.GTBirimHesapKodlaris.RemoveRange(Table.GTBirimHesapKodlaris);
                }
                foreach (var item in GTHesapNoID)
                    Table.GTBirimHesapNumaralaris.Add(new GTBirimHesapNumaralari { GTHesapNoID = item });
                foreach (var item in GTHesapKodID)
                    Table.GTBirimHesapKodlaris.Add(new GTBirimHesapKodlari { GTHesapKodID = item });
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, MmMessage.Messages.ToArray());
            }
            ViewBag.MmMessage = MmMessage;
            ViewBag.SecilenlerHK = GTHesapKodID;
            ViewBag.SecilenlerHN = GTHesapNoID;
            return View(kModel);
        }

        [Authorize(Roles = RoleNames.GTBirimlerKayit)]
        public ActionResult Sil(int id)
        {
            var kayit = db.GTBirimlers.Where(p => p.GTBirimID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {

                try
                {
                    message = "'" + kayit.BirimAdi + " (" + kayit.BirimKod + ")' isimli Birim silindi!";
                    db.GTBirimlers.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.BirimAdi + " (" + kayit.BirimKod + ")' isimli Birim silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    SistemBilgilendirmeBus.SistemBilgisiKaydet(message, "GTBirimler/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Birim sistemde bulunamadı!";
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