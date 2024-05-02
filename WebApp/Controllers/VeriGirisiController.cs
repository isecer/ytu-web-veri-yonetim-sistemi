using System.IO;
using BiskaUtil;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApp.Business;
using WebApp.Models;
using WebApp.Utilities.Dtos;
using WebApp.Utilities.Extensions;
using WebApp.Utilities.Helpers;
using WebApp.Utilities.Helpers.Hesaplama;
using WebApp.Utilities.MenuAndRoles;
using WebApp.Utilities.SystemData;

namespace WebApp.Controllers
{
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [Authorize(Roles = RoleNames.VeriGirisi)]
    public class VeriGirisiController : Controller
    {
        // GET: VeriGirisi
        public ActionResult Index()
        {
            var birimId = UserIdentity.GetPageSelectedTableId(RoleNames.VeriGirisi, RollTableIdName.BirimId);
            var vaSurecId = VeriGirisiBus.GetLastVaSurecId(UserIdentity.GetPageSelectedTableId(RoleNames.VeriGirisi, RollTableIdName.DonemId));


            var maddeTurId = UserIdentity.GetPageSelectedTableId(RoleNames.VeriGirisi, RollTableIdName.MaddeTurId);

            if (!maddeTurId.HasValue && !RoleNames.SurecIslemleri.InRole()) maddeTurId = -1;

            return Index(new FmVeriGiris { PageSize = 20, VaSurecId = vaSurecId, Expand = vaSurecId.HasValue, BirimId = birimId, MaddeTurId = maddeTurId });
        }

        [HttpPost]
        public ActionResult Index(FmVeriGiris model)
        {
            model.VaSurecId = model.VaSurecId ?? 0;
            var surecBilgi = SurecIslemleriBus.GetVaSurecKontrol(model.VaSurecId.Value);
            model.IsAktif = surecBilgi.AktifSurec;
            UserIdentity.SetPageSelectedTableId(RoleNames.VeriGirisi, RollTableIdName.BirimId, model.BirimId);
            UserIdentity.SetPageSelectedTableId(RoleNames.VeriGirisi, RollTableIdName.DonemId, model.VaSurecId);
            UserIdentity.SetPageSelectedTableId(RoleNames.VeriGirisi, RollTableIdName.MaddeTurId, model.MaddeTurId);
            model = VeriGirisiBus.GetVerigirisDataModel(model);
            if (model.Export)
            {

                var data = new BirimRaporHesaplama(model.VaSurecId.Value, model.BirimId, model.FilteredMaddeIds).Hesapla();

                var gv = new GridView();
                gv.DataSource = data;
                gv.DataBind();
                var stringWriter = new StringWriter();
                var htmlTextWriter = new HtmlTextWriter(stringWriter);
                Response.ContentType = "application/ms-excel";
                Response.ContentEncoding = System.Text.Encoding.UTF8;
                Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());
                gv.RenderControl(htmlTextWriter);
                return File(System.Text.Encoding.UTF8.GetBytes(stringWriter.ToString()), Response.ContentType, "Madde Veri Listesi.xls");


            }
            ViewBag.VASurecID = new SelectList(SurecIslemleriBus.CmbVaSurecler(false), "Value", "Caption", model.VaSurecId);

            ViewBag.BirimData = VeriGirisiBus.VaSurecBirimlerTreeList(model.VaSurecId.Value);
            ViewBag.MaddeVeriGirisDurumID = new SelectList(ComboData.CmbMaddeDurum(), "Value", "Caption", model.MaddeVeriGirisDurumId);
            ViewBag.MaddeTurID = new SelectList(VeriGirisiBus.CmbGetVgMaddeTurleri(model.VaSurecId, model.BirimId, true, true), "Value", "Caption", model.MaddeTurId);
            ViewBag.VeriGirisiOnaylandi = new SelectList(ComboData.CmbVeriGirisOnayDurum(), "Value", "Caption", model.VeriGirisiOnaylandi);

            return View(model);
        }


        public ActionResult GetDetail(int vaSurecId, int birimId, int maddeId)
        {
            var mdl = VeriGirisiBus.GetDetailModel(vaSurecId, birimId, maddeId);
            var page = ViewRenderHelper.RenderPartialView("VeriGirisi", "DetaySablon", mdl);
            var veriGirisOnayDurumHtml = mdl.VeriGirisi.VeriGirisOnayDurumHtml();
            return new
            {
                success = true,
                page,
                VeriGirisOnayDurumHtml = veriGirisOnayDurumHtml.ToString(),
                UserIdentity.Current.IsAuthenticated
            }.ToJsonResult();
        }


        public ActionResult PlanlananDegerPost(int vaSurecId, int maddeId, int birimId, bool isBuYilOrGelecekYil, decimal? planlananDeger)
        {

            var result = VeriGirisiBus.PlanlananDegerKayit(vaSurecId, maddeId, birimId, isBuYilOrGelecekYil, planlananDeger);
            return new
            {
                success = true,
                messageModel = result.Message,
                UserIdentity.Current.IsAuthenticated
            }.ToJsonResult();
        }

        public ActionResult IsVeriVarPost(VgModel kModel)
        {
            var result = VeriGirisiBus.VeriDurumKayit(kModel);
            return result.Message.ToJsonResult();
        }
        [ValidateInput(false)]
        public ActionResult VgPost(VgModel kModel)
        {
            var result = VeriGirisiBus.VeriGirisiKayit(kModel);
            return new { result.Message, ShowKanitDosyaEkle = result.Data }.ToJsonResult();
        }


        public ActionResult VgOnay(int vaSurecId, int birimId, int maddeId, int? vaCokluVeriDonemId, bool veriGirisiOnaylandi)
        {
            var result = VeriGirisiBus.VeriOnayiKayit(vaSurecId, birimId, maddeId, vaCokluVeriDonemId, veriGirisiOnaylandi);
            return result.Message.ToJsonResult();
        }
        public ActionResult GetViewKanitDosyasiEkle(int vaSurecId, int birimId, int maddeId, int? vaCokluVeriDonemId)
        {
            var model = VeriGirisiBus.GetVeriKanitModel(vaSurecId, birimId, maddeId, vaCokluVeriDonemId);
            var page = ViewRenderHelper.RenderPartialView("VeriGirisi", "ViewKanitDosyasiEkle", model);

            return new { page }.ToJsonResult();
        }
        public ActionResult GetViewKanitDosyasiEklenenler(int vaSurecId, int birimId, int maddeId, int? vaCokluVeriDonemId)
        {
            var model = VeriGirisiBus.GetMaddeEklenenDosyalar(vaSurecId, birimId, maddeId, vaCokluVeriDonemId);
            var page = ViewRenderHelper.RenderPartialView("VeriGirisi", "ViewKanitDosyasiEklenenler", model);
            return new { page }.ToJsonResult();
        }
        public ActionResult VgKanitDosyasiPost(int vaSurecId, int birimId, int maddeId, int? vaCokluVeriDonemId, HttpPostedFileBase kanitDosyasi)
        {
            var result = VeriGirisiBus.VeriKanitDosyasiKayit(vaSurecId, birimId, maddeId, vaCokluVeriDonemId, kanitDosyasi);
            return result.Message.ToJsonResult();
        }
        public ActionResult VgKanitDosyasiSil(int id)
        {
            var result = VeriGirisiBus.VeriKanitDosyasiSil(id);
            return new
            {
                success = result.Success,
                message = string.Join("<br/>", result.Message.Messages)
            }.ToJsonResult();
        }

        public ActionResult GetViewAciklamaEkle(int? vaSurecleriMaddeEklenenAciklamaId, int vaSurecId, int birimId, int maddeId)
        {
            var model = VeriGirisiBus.GetVeriAciklamaDialogData(vaSurecleriMaddeEklenenAciklamaId, vaSurecId, birimId, maddeId);
            var page = ViewRenderHelper.RenderPartialView("VeriGirisi", "ViewAciklamaEkle", model);
            return new { page }.ToJsonResult();
        }
        public ActionResult GetViewAciklamaEklenenler(int vaSurecId, int birimId, int maddeId)
        {
            var model = VeriGirisiBus.GetVeriAciklamaModelData(vaSurecId, birimId, maddeId);
            var page = ViewRenderHelper.RenderPartialView("VeriGirisi", "ViewAciklamaEklenenler", model);
            return new { page }.ToJsonResult();
        }
        public ActionResult VgAciklamaPost(int? vaSurecleriMaddeEklenenAciklamaId, int vaSurecId, int birimId, int maddeId, int? vaCokluVeriDonemId, string aciklama)
        {
            var result = VeriGirisiBus.VeriAciklamasiKayit(vaSurecleriMaddeEklenenAciklamaId, vaSurecId, birimId, maddeId, vaCokluVeriDonemId, aciklama);
            return result.Message.ToJsonResult();
        }
        public ActionResult VgAciklamaSil(int vaSurecleriMaddeEklenenAciklamaId)
        {
            var result = VeriGirisiBus.VeriAciklamasiSil(vaSurecleriMaddeEklenenAciklamaId);
            return new
            {
                success = result.Success,
                message = string.Join("<br/>", result.Message.Messages)
            }.ToJsonResult();
        }

    }
}