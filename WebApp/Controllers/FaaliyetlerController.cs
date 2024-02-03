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
    [Authorize(Roles = RoleNames.Faaliyetler)]
    public class FaaliyetlerController : Controller
    {
        private readonly VysDBEntities db = new VysDBEntities();
        // GET: Maddeler
        public ActionResult Index()
        {

            return Index(new FmFaaliyetler { PageSize = 50, IsAktif = true });
        }
        [HttpPost]
        public ActionResult Index(FmFaaliyetler model)
        {


            var q = (from s in db.Faaliyetlers
                     join kul in db.Kullanicilars on s.IslemYapanID equals kul.KullaniciID
                     select new FrFaaliyetler
                     {
                         FaaliyetID = s.FaaliyetID,
                         FaaliyetKod = s.FaaliyetKod,
                         FaaliyetAdi = s.FaaliyetAdi,
                         TakipGostergesi = s.TakipGostergesi,
                         IsAktif = s.IsAktif,
                         IslemTarihi = s.IslemTarihi,
                         IslemYapan = kul.KullaniciAdi,
                         IslemYapanID = s.IslemYapanID,
                         IslemYapanIP = s.IslemYapanIP,
                         FaaliyetAylari = s.FaaliyetlerAys.Select(s2 => s2.Aylar).OrderBy(o => o.AyID).ToList(),
                         FaaliyetMaddeleri = db.Vw_MaddelerTree.Where(p => s.FaaliyetlerMaddes.Any(a => a.MaddeID == p.MaddeID)).OrderBy(o => o.MaddeTreeAdi).ToList(),
                         FaaliyetKaynaklari = s.FaaliyetlerKaynaks.Select(s2 => s2.Kaynaklar).OrderBy(o => o.KaynakID).ToList(),
                     }).AsQueryable();

            if (model.FaaliyetKod.IsNullOrWhiteSpace() == false) q = q.Where(p => p.FaaliyetKod == model.FaaliyetKod);
            if (!model.FaaliyetAdi.IsNullOrWhiteSpace()) q = q.Where(p => p.FaaliyetAdi.Contains(model.FaaliyetAdi) || p.TakipGostergesi.Contains(model.FaaliyetAdi));
            if (model.IsAktif.HasValue) q = q.Where(p => p.IsAktif == model.IsAktif.Value);
            model.RowCount = q.Count();
            q = !model.Sort.IsNullOrWhiteSpace() ? q.OrderBy(model.Sort) : q.OrderBy(o => o.FaaliyetAdi);
            model.CountIngfos =  new MIndexBilgi() { Toplam = model.RowCount, Pasif = q.Count(p => !p.IsAktif),Aktif = q.Count(p=>p.IsAktif)};
            
            
           
            model.Data = q.Skip(model.PagingStartRowIndex).Take(model.PageSize).ToList();;
            
            ViewBag.IsAktif = new SelectList(ComboData.GetCmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            return View(model);
        }

        public ActionResult Kayit(int? id)
        {
            var mmMessage = new MmMessage();
            ViewBag.MmMessage = mmMessage;
            var model = new Faaliyetler
            {
                IsAktif = true
            };
            if (id.HasValue)
            {
                var data = db.Faaliyetlers.FirstOrDefault(p => p.FaaliyetID == id);
                if (data != null)
                {
                    model = data;
                }
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.FaaliyetlerKayit)]
        public ActionResult Kayit(Faaliyetler kModel, List<int> kaynakId, List<int> ayId, List<int> maddeId)
        {
            kaynakId = kaynakId ?? new List<int>();
            ayId = ayId ?? new List<int>();
            maddeId = maddeId ?? new List<int>();
            var mmMessage = new MmMessage();
            #region Kontrol

            if (kModel.FaaliyetKod.IsNullOrWhiteSpace())
            {
                mmMessage.Messages.Add("Faaliyet Kodunu giriniz");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "FaaliyetKod" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "FaaliyetKod" });
            if (kModel.FaaliyetAdi.IsNullOrWhiteSpace())
            {
                mmMessage.Messages.Add("Faaliyet Adı boş bırakılamaz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "FaaliyetAdi" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "FaaliyetAdi" });

            if (ayId.Count == 0)
            {
                mmMessage.Messages.Add("En az bir planlanan gerçekleşme zamanı (Ay) Seçilmelidir.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "AyID" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "AyID" });
            if (kaynakId.Count == 0)
            {
                mmMessage.Messages.Add("En az bir Kaynak Seçilmelidir.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "KaynakID" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "KaynakID" });
            if (maddeId.Count == 0)
            {
                mmMessage.Messages.Add("En az bir Gösterge (Madde) Seçilmelidir.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "MaddeID" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "MaddeID" });
            #endregion
            if (mmMessage.Messages.Count == 0)
            {

                if (db.Faaliyetlers.Any(a => a.FaaliyetID != kModel.FaaliyetID && a.FaaliyetKod == kModel.FaaliyetKod.Trim()))
                {
                    mmMessage.Messages.Add("Girdiğiniz Faaliyet Kodu başka bir maddeye aittir tekrar kullanamazsınız");
                    mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "FaaliyetKod" });
                }

            }
            if (mmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;


                Faaliyetler table;

                if (kModel.FaaliyetID <= 0)
                {
                    table = db.Faaliyetlers.Add(kModel);
                }
                else
                {
                    table = db.Faaliyetlers.First(p => p.FaaliyetID == kModel.FaaliyetID);
                    table.FaaliyetKod = kModel.FaaliyetKod;
                    table.FaaliyetAdi = kModel.FaaliyetAdi;
                    table.TakipGostergesi = kModel.TakipGostergesi;
                    table.IsAktif = kModel.IsAktif;
                    table.IslemTarihi = kModel.IslemTarihi;
                    table.IslemYapanID = kModel.IslemYapanID;
                    table.IslemYapanIP = kModel.IslemYapanIP;
                }




                #region AddKaynaklar 
                var fkVarolanlar = table.FaaliyetlerKaynaks.Where(p => kaynakId.Contains(p.KaynakID)).ToList();
                var fkSilinenler = table.FaaliyetlerKaynaks.Where(p => !kaynakId.Contains(p.KaynakID)).ToList();
                if (fkSilinenler.Any()) db.FaaliyetlerKaynaks.RemoveRange(fkSilinenler);
                var fkEklenecekler = kaynakId.Where(p => fkVarolanlar.Select(s => s.KaynakID).All(a => a != p)).ToList();
                foreach (var item in fkEklenecekler)
                    table.FaaliyetlerKaynaks.Add(new FaaliyetlerKaynak { KaynakID = item });
                #endregion
                #region AddAylar 
                var faVarolanlar = table.FaaliyetlerAys.Where(p => ayId.Contains(p.AyID)).ToList();
                var faSilinenler = table.FaaliyetlerAys.Where(p => !ayId.Contains(p.AyID)).ToList();
                if (faSilinenler.Any()) db.FaaliyetlerAys.RemoveRange(faSilinenler);
                var faEklenecekler = ayId.Where(p => faVarolanlar.Select(s => s.AyID).All(a => a != p)).ToList();
                foreach (var item in faEklenecekler)
                    table.FaaliyetlerAys.Add(new FaaliyetlerAy { AyID = item });
                #endregion 
                #region AddMaddeler

                var fmVarolanlar = table.FaaliyetlerMaddes.Where(p => maddeId.Contains(p.MaddeID)).ToList();
                var fmSilinenler = table.FaaliyetlerMaddes.Where(p => !maddeId.Contains(p.MaddeID)).ToList();
                if (fmSilinenler.Any()) db.FaaliyetlerMaddes.RemoveRange(fmSilinenler);

                var fmEklenecekler = maddeId.Where(p => fmVarolanlar.Select(s => s.MaddeID).All(a => a != p)).ToList();
                foreach (var item in fmEklenecekler)
                    table.FaaliyetlerMaddes.Add(new FaaliyetlerMadde { MaddeID = item });
                #endregion





                db.SaveChanges();

                return RedirectToAction("Index");
            } 
            MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, mmMessage.Messages.ToArray());
            ViewBag.SecilenlerKy = kaynakId;
            ViewBag.SecilenlerAy = ayId;
            ViewBag.SecilenlerMd = maddeId;
            ViewBag.MmMessage = mmMessage;
            return View(kModel);
        }





        [Authorize(Roles = RoleNames.FaaliyetlerKayit)]
        public ActionResult Sil(int id)
        {
            var kayit = db.Faaliyetlers.FirstOrDefault(p => p.FaaliyetID == id);
            string message;
            var success = true;
            if (kayit != null)
            {

                try
                {
                    message = "'" + kayit.FaaliyetAdi + "' isimli Faaliyet silindi!";
                    db.Faaliyetlers.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.FaaliyetAdi + "' isimli Faaliyet silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    SistemBilgilendirmeBus.SistemBilgisiKaydet(message, "Faaliyetler/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Faaliyet sistemde bulunamadı!";
            }

            return new { success, message }.ToJsonResult();
        }
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}