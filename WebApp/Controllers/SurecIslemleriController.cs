using System.Linq;
using System.Web.Mvc;
using WebApp.Models;
using Database;
using BiskaUtil;
using WebApp.Business;
using WebApp.Utilities.Extensions;
using WebApp.Utilities.Helpers;
using WebApp.Utilities.MenuAndRoles;
using WebApp.Utilities.MessageBox;
using WebApp.Utilities.SystemData;

namespace WebApp.Controllers
{
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [Authorize(Roles = RoleNames.SurecIslemleri)]
    public class SurecIslemleriController : Controller
    { 
        private readonly VysDBEntities db = new VysDBEntities();
        public ActionResult Index(int? veriAktarilacakSurecId = null)
        {
            return Index(new FmSurecIslemleri() { PageSize = 15, VeriAktarilacakSurecID = veriAktarilacakSurecId });
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
                return RedirectToAction("Index", new { veriAktarilacakSurecId = isNewOrEdit ? kModel.VASurecID : (int?)null });
            }
            MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, result.Message.Messages.ToArray());
            ViewBag.MmMessage = result.Message;
            ViewBag.IsAktif = new SelectList(ComboData.GetCmbAktifPasifData(), "Value", "Caption", kModel.IsAktif);
            ViewBag.Yil = new SelectList(ComboData.CmbSurecKayitYillari(), "Value", "Caption", kModel.Yil);
            return View(kModel);
        }


        public ActionResult VeriKopyalamaIslemi(int id)
        {
            var model = new SurecVeriKopyalaModel
            {
                Data = db.MaddeTurleris.Select(s => new SurecVeriKopyalaRow
                {
                    IsPlanlananDegerOlacak = s.IsPlanlananDegerOlacak,
                    MaddeTurID = s.MaddeTurID,
                    MaddeTurAdi = s.MaddeTurAdi,

                }).OrderBy(o => o.MaddeTurAdi).ToList(),
                SurecBilgi = SurecIslemleriBus.GetVaSurecKontrol(id)
            };
            return View(model);
        }
        [Authorize(Roles = RoleNames.SurecIslemleriKayitYetkisi)]
        public ActionResult VeriKopyalamaIslemiPost(SurecVeriKopyalaModel kModel)
        {
            var result = SurecIslemleriBus.VeriKopyala(kModel);
            return result.Message.ToJsonResult();
        }



        public ActionResult GetDetail(int id, int tbInx)
        {
            var mdl = SurecIslemleriBus.GetVaSurecKontrol(id);
            mdl.SelectedTabIndex = tbInx;
            var page = ViewRenderHelper.RenderPartialView("SurecIslemleri", "DetaySablon", mdl);
            return Json(new { page, UserIdentity.Current.IsAuthenticated }, "application/json", JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetSubData(int id, int? selectDurumId, int tbInx)
        {
            var page = SurecIslemleriBus.GetSubPage(id, selectDurumId, tbInx);
            return new
            {
                page,
                UserIdentity.Current.IsAuthenticated
            }.ToJsonResult();

        } 
        public ActionResult GetBirimMaddeleri(int vaSurecId, int birimId)
        {
            var birimMaddeleri = SurecIslemleriBus.GetSurecBirimMaddeleri(vaSurecId, birimId);
            ViewBag.VASurecID = vaSurecId;
            ViewBag.BirimID = birimId;
            return View(birimMaddeleri);
        }

        public ActionResult GetMaddeBirimleri(int vaSurecId, int maddeId)
        {
            var maddeBirimleri = SurecIslemleriBus.GetSurecMaddeBirimleri(vaSurecId, maddeId);
            ViewBag.VASurecID = vaSurecId;
            ViewBag.MaddeID = maddeId;
            return View(maddeBirimleri);
        }
        [Authorize(Roles = RoleNames.SurecIslemleriKayitYetkisi)]
        public ActionResult Sil(int id)
        {
            var result = SurecIslemleriBus.SurecSil(id);
            var strView = ViewRenderHelper.RenderPartialView("Ajax", "GetMessage", result.Message);
            return Json(new { result.Message.IsSuccess, Messages = strView }, "application/json", JsonRequestBehavior.AllowGet);
        }
    }
}