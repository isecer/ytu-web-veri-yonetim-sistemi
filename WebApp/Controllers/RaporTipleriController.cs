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
    [Authorize(Roles = RoleNames.RaporTipleri)]
    public class RaporTipleriController : Controller
    {
        // GET: RaporTipleri
        private readonly VysDBEntities db = new VysDBEntities();

        public ActionResult Index()
        {

            return Index(new FmRaporTipleri { PageSize = 50, IsAktif = true });
        }
        [HttpPost]
        public ActionResult Index(FmRaporTipleri model)
        {


            var q = (from s in db.RaporTipleris
                     join kul in db.Kullanicilars on s.IslemYapanID equals kul.KullaniciID
                     select new FrRaporTipleri
                     {
                         RaporTipID = s.RaporTipID,
                         RaporTipAdi = s.RaporTipAdi,
                         Aciklama = s.Aciklama,
                         IsAktif = s.IsAktif,
                         IslemTarihi = s.IslemTarihi,
                         IslemYapan = kul.KullaniciAdi,
                         IslemYapanID = s.IslemYapanID,
                         IslemYapanIP = s.IslemYapanIP,
                         RaporMaddeleri = db.Vw_MaddelerTree.Where(p => s.RaporTipleriSecilenMaddelers.Any(a => a.MaddeID == p.MaddeID)).OrderBy(o => o.MaddeTreeAdi).ToList()
                     }).AsQueryable();

            if (!model.RaporTipAdi.IsNullOrWhiteSpace()) q = q.Where(p => p.RaporTipAdi.Contains(model.RaporTipAdi) || p.Aciklama.Contains(model.RaporTipAdi));
            if (model.IsAktif.HasValue) q = q.Where(p => p.IsAktif == model.IsAktif.Value);
            model.RowCount = q.Count();
            q = !model.Sort.IsNullOrWhiteSpace() ? q.OrderBy(model.Sort) : q.OrderBy(o => o.RaporTipAdi);
            model.CountIngfos =  new MIndexBilgi() { Toplam = model.RowCount, Pasif = q.Count(p => !p.IsAktif),Aktif = q.Count(p=>p.IsAktif)};
            
            
           
            model.Data = q.Skip(model.PagingStartRowIndex).Take(model.PageSize).ToList();;
            
            ViewBag.IsAktif = new SelectList(ComboData.GetCmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            return View(model);
        }

        public ActionResult Kayit(int? id)
        {
            var mmMessage = new MmMessage();
            ViewBag.MmMessage = mmMessage;
            var model = new RaporTipleri
            {
                IsAktif = true
            };
            if (!id.HasValue) return View(model);
            var data = db.RaporTipleris.FirstOrDefault(p => p.RaporTipID == id);
            if (data != null)
            {
                model = data;
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.RaporTipleriKayit)]
        public ActionResult Kayit(RaporTipleri kModel, List<int> maddeId)
        {
            maddeId = maddeId ?? new List<int>();
            var mmMessage = new MmMessage();
            #region Kontrol

            if (kModel.RaporTipAdi.IsNullOrWhiteSpace())
            {
                mmMessage.Messages.Add("Rapor Tip Adı boş bırakılamaz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "RaporTipAdi" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "RaporTipAdi" });
            if (kModel.Aciklama.IsNullOrWhiteSpace())
            {
                mmMessage.Messages.Add("Açıklama boş bırakılamaz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Aciklama" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Aciklama" });


            if (maddeId.Count == 0)
            {
                mmMessage.Messages.Add("En az bir Gösterge (Madde) Seçilmelidir.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "MaddeID" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "MaddeID" });
            #endregion

            if (mmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;


                RaporTipleri raporTipi;

                if (kModel.RaporTipID <= 0)
                {
                    raporTipi = db.RaporTipleris.Add(kModel);
                }
                else
                {
                    raporTipi = db.RaporTipleris.First(p => p.RaporTipID == kModel.RaporTipID);
                    raporTipi.RaporTipAdi = kModel.RaporTipAdi;
                    raporTipi.Aciklama = kModel.Aciklama;
                    raporTipi.IsAktif = kModel.IsAktif;
                    raporTipi.IslemTarihi = kModel.IslemTarihi;
                    raporTipi.IslemYapanID = kModel.IslemYapanID;
                    raporTipi.IslemYapanIP = kModel.IslemYapanIP;
                }  
                #region AddMaddeler 
                var rmVarolanlar = raporTipi.RaporTipleriSecilenMaddelers.Where(p => maddeId.Contains(p.MaddeID)).ToList();
                var rmSilinenler = raporTipi.RaporTipleriSecilenMaddelers.Where(p => !maddeId.Contains(p.MaddeID)).ToList();
                if (rmSilinenler.Any()) db.RaporTipleriSecilenMaddelers.RemoveRange(rmSilinenler);

                var rmEklenecekler = maddeId.Where(p => rmVarolanlar.Select(s => s.MaddeID).All(a => a != p)).ToList();
                foreach (var item in rmEklenecekler)
                    raporTipi.RaporTipleriSecilenMaddelers.Add(new RaporTipleriSecilenMaddeler { MaddeID = item });
                #endregion 
                db.SaveChanges();

                return RedirectToAction("Index");
            } 
            MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, mmMessage.Messages.ToArray());
            ViewBag.SecilenlerMd = maddeId;
            ViewBag.MmMessage = mmMessage;
            return View(kModel);
        }





        [Authorize(Roles = RoleNames.RaporTipleriKayit)]
        public ActionResult Sil(int id)
        {
            var kayit = db.RaporTipleris.FirstOrDefault(p => p.RaporTipID == id);
            string message;
            var success = true;
            if (kayit != null)
            {

                try
                {
                    message = "'" + kayit.RaporTipAdi + "' isimli Rapor Tipi silindi!";
                    db.RaporTipleris.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.RaporTipAdi + "' isimli Rapor Tipi silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    SistemBilgilendirmeBus.SistemBilgisiKaydet(message, "RaporTipleri/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Rapor Tipi sistemde bulunamadı!";
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