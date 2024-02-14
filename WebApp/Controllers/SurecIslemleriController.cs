using BiskaUtil;
using Database;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebApp.Business;
using WebApp.Models;
using WebApp.Utilities.Extensions;
using WebApp.Utilities.Helpers;
using WebApp.Utilities.Helpers.FileController;
using WebApp.Utilities.MenuAndRoles;
using WebApp.Utilities.MessageBox;
using WebApp.Utilities.SystemData;

namespace WebApp.Controllers
{
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [Authorize(Roles = RoleNames.SurecIslemleri)]
    public class SurecIslemleriController : Controller
    {
        private readonly VysDBEntities _entities = new VysDBEntities();
        public ActionResult Index(int? showDetailVaSurecId = null)
        {
            return Index(new FmSurecIslemleri { PageSize = 15, ShowDetailVaSurecId = showDetailVaSurecId });
        }
        [HttpPost]
        public ActionResult Index(FmSurecIslemleri model)
        {
            var result = SurecIslemleriBus.GetSurecler(model);
            model = result.Data;
            ViewBag.IsAktif = new SelectList(ComboData.GetCmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            return View(model);
        }
        public ActionResult Kayit(int? id)
        {

            var kmSurec = SurecIslemleriBus.GetSurec(id);
            ViewBag.IsAktif = new SelectList(ComboData.GetCmbAktifPasifData(), "Value", "Caption", kmSurec.IsAktif);
            ViewBag.Yil = new SelectList(ComboData.CmbSurecKayitYillari(), "Value", "Caption", kmSurec.Yil);
            ViewBag.MmMessage = new MmMessage();
            return View(kmSurec);
        }
        [HttpPost]
        [Authorize(Roles = RoleNames.SurecIslemleriKayitYetkisi)]
        public ActionResult Kayit(KmSurecIslemleri kModel)
        {

            var isNewOrEdit = kModel.VASurecID <= 0;
            var result = SurecIslemleriBus.Kayit(kModel);
            if (result.Success)
            {
                return RedirectToAction("Index", new { showDetailVaSurecId = isNewOrEdit ? kModel.VASurecID : (int?)null });
            }
            MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, result.Message.Messages.ToArray());
            ViewBag.MmMessage = result.Message;
            ViewBag.IsAktif = new SelectList(ComboData.GetCmbAktifPasifData(), "Value", "Caption", kModel.IsAktif);
            ViewBag.Yil = new SelectList(ComboData.CmbSurecKayitYillari(), "Value", "Caption", kModel.Yil);
            return View(kModel);
        }


        public ActionResult VeriKopyalamaIslemi(int id)
        {
            var maddetTurleris = _entities.MaddeTurleris.Where(p => p.IsAktif).ToList().OrderBy(o => o.MaddeTurAdi).ToList();
            ViewBag.SurecMaddeTurIds = _entities.VASurecleriMaddeTurs.Where(p => p.VASurecID == id).Select(s => s.MaddeTurID).ToList();
            ViewBag.SurecBilgi = SurecIslemleriBus.GetVaSurecKontrol(id);
            return View(maddetTurleris);
        }
        [Authorize(Roles = RoleNames.SurecIslemleriKayitYetkisi)]
        public ActionResult VeriKopyalamaIslemiPost(int vaSurecId, List<int> maddeTurId)
        {
            maddeTurId = maddeTurId ?? new List<int>();
            var result = SurecIslemleriBus.SurecMaddeSenkronizasyonu(vaSurecId, maddeTurId);
            return result.Message.ToJsonResult();
        }

        public ActionResult KanitDosyalariniIndir(int vaSurecId)
        {
            return new FileController().SurecKanitDosyalariIndir(vaSurecId);
        }

        public ActionResult GetDetail(int id, int tbInx)
        {
            var mdl = SurecIslemleriBus.GetVaSurecKontrol(id);
            mdl.SelectedTabIndex = tbInx;
            var page = ViewRenderHelper.RenderPartialView("SurecIslemleri", "DetaySablon", mdl);
            return Json(new { page }, "application/json", JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetSubData(int id, int? selectDurumId, int tbInx)
        {
            var page = SurecIslemleriBus.GetSubPage(id, selectDurumId, tbInx);
            return new
            {
                page
            }.ToJsonResult();

        }
        public ActionResult ViewBirimMaddeleri(int vaSurecId, int birimId)
        {
            var birimMaddeleri = SurecIslemleriBus.GetSurecBirimMaddeleri(vaSurecId, birimId);
            ViewBag.VASurecID = vaSurecId;
            ViewBag.BirimID = birimId;
            return View(birimMaddeleri);
        }

        public ActionResult ViewMaddeBirimleri(int vaSurecId, int maddeId)
        {
            var maddeBirimleri = SurecIslemleriBus.GetSurecMaddeBirimleri(vaSurecId, maddeId);
            ViewBag.VASurecID = vaSurecId;
            ViewBag.MaddeID = maddeId;
            return View(maddeBirimleri);
        }
        public ActionResult MaddeTurIsVeriGirisDurumKayit(int vaSurecId, int maddeTurId, bool isVeriGirisiAcik)
        {
            var result = SurecIslemleriBus.MaddeTurIsVeriGirisDurumKayit(vaSurecId, maddeTurId, isVeriGirisiAcik);
            var messageView = ViewRenderHelper.RenderPartialView("Ajax", "GetMessage", result.Message);
            return Json(new { result.Message, messageView }, "application/json", JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = RoleNames.SurecIslemleriKayitYetkisi)]
        public ActionResult Sil(int id)
        {
            var result = SurecIslemleriBus.SurecSil(id);
            var messageView = ViewRenderHelper.RenderPartialView("Ajax", "GetMessage", result.Message);
            return Json(new { result.Message, messageView }, "application/json", JsonRequestBehavior.AllowGet);
        }
    }
}