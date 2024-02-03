using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebApp.Models;
using BiskaUtil;
using Database;
using WebApp.Business;
using WebApp.Utilities.Extensions;
using WebApp.Utilities.Helpers;
using WebApp.Utilities.MenuAndRoles;
using WebApp.Utilities.MessageBox;
using WebApp.Utilities.SystemData;

namespace WebApp.Controllers
{
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [Authorize(Roles = RoleNames.Birimler)]
    public class BirimlerController : Controller
    {
        private readonly VysDBEntities db = new VysDBEntities();
        public ActionResult Index()
        {
            return Index(new FmBirimler { PageSize = 15 });
        }
        [HttpPost]
        public ActionResult Index(FmBirimler model)
        {

            var brmTree = db.sp_BirimAgaci().ToList();
            var mdTree = db.sp_MaddeAgaci().ToList();
            var brms = db.Birimlers.ToList();
            var q = (from s in brms
                     join bt in brmTree on s.BirimID equals bt.BirimID
                     join kul in db.Kullanicilars on s.IslemYapanID equals kul.KullaniciID
                     select new FrBirimler
                     {
                         BirimID = s.BirimID,
                         BirimKod = s.BirimKod,
                         UstBirimID = s.UstBirimID,
                         BirimAdi = s.BirimAdi,
                         BirimTreeAdi = bt.BirimTreeAdi,
                         Adres = s.Adres,
                         IsAktif = s.IsAktif,
                         IslemTarihi = s.IslemTarihi,
                         IslemYapan = kul.KullaniciAdi,
                         IslemYapanID = s.IslemYapanID,
                         IslemYapanIP = s.IslemYapanIP,
                         IsMaddeEklenebilir=s.IsMaddeEklenebilir,
                         BirimMaddeleri = (from s2 in s.BirimMaddeleris
                                           join md in mdTree on s2.MaddeID equals md.MaddeID
                                           select new FrMaddeler
                                           {
                                               MaddeID = s2.MaddeID,
                                               MaddeTreeAdi=md.MaddeTreeAdi,
                                               MaddeAdi = md.MaddeAdi,
                                               MaddeKod = md.MaddeKod
                                           }).OrderBy(o=>o.MaddeAdi).ToList()
                     }).AsQueryable();
            if (model.BirimKod.IsNullOrWhiteSpace() == false) q = q.Where(p => p.BirimKod == model.BirimKod);
            if (!model.BirimAdi.IsNullOrWhiteSpace()) q = q.Where(p => p.BirimTreeAdi.ToLower().Contains(model.BirimAdi.ToLower())); if (model.EslestirmeDurum.HasValue)
            {
                q = model.EslestirmeDurum.Value ? q.Where(p => p.BirimMaddeleri.Count > 0) : q.Where(p => p.BirimMaddeleri.Count == 0);
            }
            if (model.IsAktif.HasValue) q = q.Where(p => p.IsAktif == model.IsAktif.Value);
            if (model.IsMaddeEklenebilir.HasValue) q = q.Where(p => p.IsMaddeEklenebilir == model.IsMaddeEklenebilir.Value);
            model.RowCount = q.Count();
            q = !model.Sort.IsNullOrWhiteSpace() ? q.OrderBy(model.Sort) : q.OrderBy(o => o.UstBirimID.HasValue).ThenBy(o => o.BirimTreeAdi);
            model.CountIngfos =  new MIndexBilgi() { Toplam = model.RowCount, Pasif = q.Count(p => !p.IsAktif),Aktif = q.Count(p=>p.IsAktif)};
            
            
           
            model.Data = q.Skip(model.PagingStartRowIndex).Take(model.PageSize).ToList();;
            
            ViewBag.IsAktif = new SelectList(ComboData.GetCmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            ViewBag.IsMaddeEklenebilir = new SelectList(ComboData.GetCmbEvetHayirData(), "Value", "Caption", model.IsMaddeEklenebilir);
            ViewBag.EslestirmeDurum = new SelectList(ComboData.CmbEslesenEslesmeyenData(), "Value", "Caption", model.EslestirmeDurum);

            return View(model);
        }
        public ActionResult Kayit(int? id)
        {
            var mmMessage = new MmMessage();
            ViewBag.MmMessage = mmMessage;
            var model = new Birimler
            {
                IsAktif = true
            };
            if (id.HasValue)
            {
                var data = db.Birimlers.FirstOrDefault(p => p.BirimID == id);
                if (data != null)
                {
                    model = data;
                }
            }

            ViewBag.UstBirimID = new SelectList(BirimlerBus.CmbBirimlerTree(true, model.BirimID), "Value", "Caption", model.UstBirimID);
            ViewBag.Secilenler = null;
            return View(model);
        }
        [HttpPost]
        public ActionResult Kayit(Birimler kModel, List<int> maddeId)
        {
            maddeId = maddeId ?? new List<int>();
            var mmMessage = new MmMessage();
            #region Kontrol


            if (kModel.BirimKod.IsNullOrWhiteSpace())
            { 
                mmMessage.Messages.Add("Birim Kodunu Giriniz");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BirimKod"});
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "BirimKod" });
            if (kModel.BirimAdi.IsNullOrWhiteSpace())
            { 
                mmMessage.Messages.Add("Birim Adı Boş bırakılamaz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BirimAdi"});
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "BirimAdi" });

            if (!kModel.BirimKisaAdi.IsNullOrWhiteSpace() && kModel.BirimKisaAdi.Length > 10)
            { 
                mmMessage.Messages.Add("Birim Kısa Adı 10 karakterden daha uzun olamaz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BirimKisaAdi"  });
            }

            #endregion
            if (mmMessage.Messages.Count == 0)
            {
                kModel.BirimKod = kModel.BirimKod.ReplaceSpecialCharacter().Trim();
                if (db.Birimlers.Any(a => a.BirimID != kModel.BirimID && a.BirimKod == kModel.BirimKod))
                { 
                    mmMessage.Messages.Add("Girdiğiniz Birime Kodu başka bir birime aittir tekrar kullanamazsınız");
                    mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BirimKod"  });
                }
            }
            if (mmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                Birimler table;
                if (kModel.BirimID <= 0)
                {
                    kModel.IsAktif = true;
                    table = db.Birimlers.Add(kModel);
                }
                else
                {
                    table = db.Birimlers.First(p => p.BirimID == kModel.BirimID);
                    table.BirimKod = kModel.BirimKod;
                    table.BirimAdi = kModel.BirimAdi.Trim();
                    table.BirimKisaAdi = kModel.BirimKisaAdi.ToStrObjEmptString().Trim();
                    table.UstBirimID = kModel.UstBirimID;
                    table.Adres = kModel.Adres;
                    table.IsMaddeEklenebilir = kModel.IsMaddeEklenebilir;
                    table.IsAktif = kModel.IsAktif;
                    table.IslemTarihi = kModel.IslemTarihi;
                    table.IslemYapanID = kModel.IslemYapanID;
                    table.IslemYapanIP = kModel.IslemYapanIP;
                }
                #region MaddelerSet
                var birimMaddeleri = db.BirimMaddeleris.Where(p => p.BirimID == kModel.BirimID).ToList();
                var varolanlar = birimMaddeleri.Where(p => p.BirimID == kModel.BirimID && maddeId.Contains(p.MaddeID)).ToList();
                var silinenler = birimMaddeleri.Where(p => p.BirimID == kModel.BirimID && !maddeId.Contains(p.MaddeID)).ToList();
                db.BirimMaddeleris.RemoveRange(silinenler);
                var eklenecekler = maddeId.Where(p => varolanlar.Select(s => s.MaddeID).All(a => a != p)).ToList();
                foreach (var item in eklenecekler)
                    table.BirimMaddeleris.Add(new BirimMaddeleri { MaddeID = item });
                #endregion
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, mmMessage.Messages.ToArray());
            }
            ViewBag.Secilenler = maddeId;
            ViewBag.UstBirimID = new SelectList(BirimlerBus.CmbBirimlerTree(), "Value", "Caption", kModel.UstBirimID);
            ViewBag.MmMessage = mmMessage;
            return View(kModel);
        }
 


        public ActionResult Sil(int id)
        {
            var kayit = db.Birimlers.FirstOrDefault(p => p.BirimID == id);
            string message;
            var success = true;
            if (kayit != null)
            {

                try
                {
                    message = "'" + kayit.BirimAdi + "' İsimli Birim Silindi!";
                    db.Birimlers.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.BirimAdi + "' İsimli Birim Silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    SistemBilgilendirmeBus.SistemBilgisiKaydet(message, "Birimler/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Birim sistemde bulunamadı!";
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
