using BiskaUtil;
using Database;
using System;
using System.Linq;
using System.Web.Mvc;
using WebApp.Models;
using WebApp.Utilities.Extensions;

namespace WebApp.Controllers
{
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class HomeController : Controller
    {
        private readonly VysDBEntities db = new VysDBEntities();

        public ActionResult Index(string mesajGroupId)
        {

            #region duyurular 
            try
            {
                var duyuruList = (from s in db.Duyurulars
                                  join k in db.Kullanicilars on s.IslemYapanID equals k.KullaniciID
                                  where s.IsAktif && s.Tarih <= DateTime.Now && (!s.YayinSonTarih.HasValue || s.YayinSonTarih.Value >= DateTime.Now) && s.AnaSayfadaGozuksun
                                  select new FrDuyurular
                                  {
                                      DuyuruID = s.DuyuruID,
                                      Baslik = s.Baslik,
                                      Aciklama = s.Aciklama,
                                      AciklamaHtml = s.AciklamaHtml,
                                      Tarih = s.Tarih,
                                      DuyuruYapan = k.Ad + " " + k.Soyad,
                                      IslemYapanIP = s.IslemYapanIP,
                                      EkSayisi = s.DuyuruEkleris.Count,
                                      DuyuruEkleris = s.DuyuruEkleris,
                                      AnaSayfadaGozuksun = s.AnaSayfadaGozuksun,
                                      AnaSayfaPopupAc = s.AnaSayfaPopupAc,
                                      YayinSonTarih = s.YayinSonTarih
                                  }).OrderByDescending(o => o.Tarih).ToList();
                ViewBag.Duyurular = duyuruList;
            }
            catch
            {
                ViewBag.Duyurular = Array.Empty<Duyurular>();
            }

            //YeniDersKontrol();
            #endregion

            var secilenMesaj = db.Mesajlars.FirstOrDefault(p => p.GroupID == mesajGroupId);
            if (mesajGroupId.IsNullOrWhiteSpace() == false)
            {
                ViewBag.MesajGroupID = secilenMesaj != null ? mesajGroupId : "";
            }
            else ViewBag.MesajGroupID = "";
            // OnaylamaAylikUpdate();
            return View();

        }

        //public void OnaylamaAylikUpdate()
        //{
        //    var birimmadde = db.VASurecleriBirimMaddes.Where(p => p.VeriGirisiOnaylandi).ToList();
        //    var GirilenDegerler = db.VASurecleriMaddeGirilenDegers.ToList();
        //    foreach (var item in GirilenDegerler)
        //    {
        //        var OnayBilgi = birimmadde.Where(p => p.VASurecleriBirimID == item.VASurecleriBirimID && p.VASurecleriMaddeID == item.VASurecleriMaddeID).FirstOrDefault();
        //        if (OnayBilgi != null)
        //        {

        //            if (OnayBilgi.VeriGirisiOnaylandi)
        //            {
        //                item.VeriGirisiOnaylandi = true;
        //                item.OnayIslemTarihi = OnayBilgi.IslemTarihi;
        //                item.OnayIslemYapanID = OnayBilgi.IslemYapanID;
        //                item.OnayIslemYapanIP = OnayBilgi.IslemYapanIP;
        //            }
        //        }
        //    }
        //    db.SaveChanges();
        //}
        public ActionResult AuthenticatedControl()
        {
            if (Request.Browser.IsMobileDevice) { }
            return Json(UserIdentity.Current.IsAuthenticated, "application/json", JsonRequestBehavior.AllowGet);
        }
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
