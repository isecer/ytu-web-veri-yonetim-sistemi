using BiskaUtil;
using System.Web;
using System.Web.Mvc;
using WebApp.Business;
using WebApp.Models;
using WebApp.Utilities.Dtos;
using WebApp.Utilities.Extensions;
using WebApp.Utilities.Helpers;
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
            var birimId = UserIdentity.GetPageSelectedTableId(RoleNames.VeriGirisi, RollTableIDName.BirimID);
            var vaSurecId = VeriGirisiBus.GetLastVaSurecId(UserIdentity.GetPageSelectedTableId(RoleNames.VeriGirisi, RollTableIDName.DonemID));


            var maddeTurId = UserIdentity.GetPageSelectedTableId(RoleNames.VeriGirisi, RollTableIDName.MaddeTurID);

            if (!maddeTurId.HasValue && !RoleNames.SurecIslemleri.InRole()) maddeTurId = -1;

            return Index(new FmVeriGiris { PageSize = 15, VaSurecId = vaSurecId, Expand = vaSurecId.HasValue, BirimId = birimId, MaddeTurId = maddeTurId });
        }

        [HttpPost]
        public ActionResult Index(FmVeriGiris model, bool export = false)
        {
            model.VaSurecId = model.VaSurecId ?? 0;
            var surecBilgi = SurecIslemleriBus.GetVaSurecKontrol(model.VaSurecId.Value);
            model.IsAktif = surecBilgi.AktifSurec;
            UserIdentity.SetPageSelectedTableId(RoleNames.VeriGirisi, RollTableIDName.BirimID, model.BirimId);
            UserIdentity.SetPageSelectedTableId(RoleNames.VeriGirisi, RollTableIDName.DonemID, model.VaSurecId);
            UserIdentity.SetPageSelectedTableId(RoleNames.VeriGirisi, RollTableIDName.MaddeTurID, model.MaddeTurId);
            model = VeriGirisiBus.GetVerigirisDataModel(model);
            #region export
            //if (export && false && model.RowCount > 0)
            //{
            //    var gv = new GridView();
            //    var vaSurecleriBirimIDs = qExpData.Select(s => s.VASurecleriBirimID).Distinct().ToList();
            //    var vaSurecleriMaddeIDs = qExpData.Select(s => s.VASurecleriMaddeID).Distinct().ToList();
            //    var birim = db.Birimlers.First(p => p.BirimID == model.BirimID);
            //    var girilenDegerler = qExpData.Where(p => vaSurecleriBirimIDs.Contains(p.VASurecleriBirimID) && vaSurecleriMaddeIDs.Contains(p.VASurecleriMaddeID)).SelectMany(s => s.VaSurecleriMaddeGirilenDegers).ToList();

            //    var qeData = (from s in qExpData
            //                  join dg in girilenDegerler on new { s.VASurecleriBirimID, s.VASurecleriMaddeID, VACokluVeriDonemID = (int?)null } equals
            //                                                new { dg.VASurecleriBirimID, dg.VASurecleriMaddeID, dg.VACokluVeriDonemID } into deg
            //                  from g in deg.DefaultIfEmpty()
            //                  join dg1 in girilenDegerler on new { s.VASurecleriBirimID, s.VASurecleriMaddeID, VACokluVeriDonemID = 1 } equals
            //                                                 new { dg1.VASurecleriBirimID, dg1.VASurecleriMaddeID, VACokluVeriDonemID = dg1.VACokluVeriDonemID ?? 0 } into deg1
            //                  from g1 in deg1.DefaultIfEmpty()
            //                  join dg2 in girilenDegerler on new { s.VASurecleriBirimID, s.VASurecleriMaddeID, VACokluVeriDonemID = 2 } equals
            //                                                new { dg2.VASurecleriBirimID, dg2.VASurecleriMaddeID, VACokluVeriDonemID = dg2.VACokluVeriDonemID ?? 0 } into deg2
            //                  from g2 in deg2.DefaultIfEmpty()
            //                  join dg3 in girilenDegerler on new { s.VASurecleriBirimID, s.VASurecleriMaddeID, VACokluVeriDonemID = 3 } equals
            //                                          new { dg3.VASurecleriBirimID, dg3.VASurecleriMaddeID, VACokluVeriDonemID = dg3.VACokluVeriDonemID ?? 0 } into deg3
            //                  from g3 in deg3.DefaultIfEmpty()
            //                  join dg4 in girilenDegerler on new { s.VASurecleriBirimID, s.VASurecleriMaddeID, VACokluVeriDonemID = 4 } equals
            //                                       new { dg4.VASurecleriBirimID, dg4.VASurecleriMaddeID, VACokluVeriDonemID = dg4.VACokluVeriDonemID ?? 0 } into deg4
            //                  from g4 in deg4.DefaultIfEmpty()
            //                  join dg5 in girilenDegerler on new { s.VASurecleriBirimID, s.VASurecleriMaddeID, VACokluVeriDonemID = 5 } equals
            //                                       new { dg5.VASurecleriBirimID, dg5.VASurecleriMaddeID, VACokluVeriDonemID = dg5.VACokluVeriDonemID ?? 0 } into deg5
            //                  from g5 in deg5.DefaultIfEmpty()
            //                  join dg6 in girilenDegerler on new { s.VASurecleriBirimID, s.VASurecleriMaddeID, VACokluVeriDonemID = 6 } equals
            //                                       new { dg6.VASurecleriBirimID, dg6.VASurecleriMaddeID, VACokluVeriDonemID = dg6.VACokluVeriDonemID ?? 0 } into deg6
            //                  from g6 in deg6.DefaultIfEmpty()
            //                  join dg7 in girilenDegerler on new { s.VASurecleriBirimID, s.VASurecleriMaddeID, VACokluVeriDonemID = 7 } equals
            //                                       new { dg7.VASurecleriBirimID, dg7.VASurecleriMaddeID, VACokluVeriDonemID = dg7.VACokluVeriDonemID ?? 0 } into deg7
            //                  from g7 in deg7.DefaultIfEmpty()
            //                  join dg8 in girilenDegerler on new { s.VASurecleriBirimID, s.VASurecleriMaddeID, VACokluVeriDonemID = 8 } equals
            //                                       new { dg8.VASurecleriBirimID, dg8.VASurecleriMaddeID, VACokluVeriDonemID = dg8.VACokluVeriDonemID ?? 0 } into deg8
            //                  from g8 in deg8.DefaultIfEmpty()
            //                  join dg9 in girilenDegerler on new { s.VASurecleriBirimID, s.VASurecleriMaddeID, VACokluVeriDonemID = 9 } equals
            //                                       new { dg9.VASurecleriBirimID, dg9.VASurecleriMaddeID, VACokluVeriDonemID = dg9.VACokluVeriDonemID ?? 0 } into deg9
            //                  from g9 in deg9.DefaultIfEmpty()
            //                  join dg10 in girilenDegerler on new { s.VASurecleriBirimID, s.VASurecleriMaddeID, VACokluVeriDonemID = 10 } equals
            //                                       new { dg10.VASurecleriBirimID, dg10.VASurecleriMaddeID, VACokluVeriDonemID = dg10.VACokluVeriDonemID ?? 0 } into deg10
            //                  from g10 in deg10.DefaultIfEmpty()
            //                  join dg11 in girilenDegerler on new { s.VASurecleriBirimID, s.VASurecleriMaddeID, VACokluVeriDonemID = 11 } equals
            //                                       new { dg11.VASurecleriBirimID, dg11.VASurecleriMaddeID, VACokluVeriDonemID = dg11.VACokluVeriDonemID ?? 0 } into deg11
            //                  from g11 in deg11.DefaultIfEmpty()
            //                  join dg12 in girilenDegerler on new { s.VASurecleriBirimID, s.VASurecleriMaddeID, VACokluVeriDonemID = 12 } equals
            //                                       new { dg12.VASurecleriBirimID, dg12.VASurecleriMaddeID, VACokluVeriDonemID = dg12.VACokluVeriDonemID ?? 0 } into deg12
            //                  from g12 in deg12.DefaultIfEmpty()
            //                  select new
            //                  {
            //                      MaddeKodu = s.MaddeKod,
            //                      s.MaddeAdi,
            //                      MaddeAciklamasi = s.Aciklama,
            //                      s.Yil,
            //                      Ocak = g1 != null ? g1.GirilenDeger + "" : "",
            //                      Subat = g2 != null ? g2.GirilenDeger + "" : "",
            //                      Mart = g3 != null ? g3.GirilenDeger + "" : "",
            //                      Nisan = g4 != null ? g4.GirilenDeger + "" : "",
            //                      Mayis = g5 != null ? g5.GirilenDeger + "" : "",
            //                      Haziran = g6 != null ? g6.GirilenDeger + "" : "",
            //                      Temmuz = g7 != null ? g7.GirilenDeger + "" : "",
            //                      Agustos = g8 != null ? g8.GirilenDeger + "" : "",
            //                      Eylul = g9 != null ? g9.GirilenDeger + "" : "",
            //                      Ekim = g10 != null ? g10.GirilenDeger + "" : "",
            //                      Kasim = g11 != null ? g11.GirilenDeger + "" : "",
            //                      Aralik = g12 != null ? g12.GirilenDeger + "" : "",
            //                      YillikDeger = g != null ? g.GirilenDeger + "" : "",
            //                      PlanlananHedef = s.PlanlananDeger,
            //                      s.YilSonuDegerHesaplamaAdi,
            //                      YilSonuHesaplananDeger = s.HesaplananSonucDegeri,
            //                      PlanlananHedefGelecekYil = s.PlanlananDegerGelecekYil,
            //                  }).ToList();


            //    gv.DataSource = qeData;
            //    gv.DataBind();
            //    var sw = new StringWriter();
            //    var htw = new HtmlTextWriter(sw);
            //    Response.ContentType = "application/ms-excel";
            //    Response.ContentEncoding = System.Text.Encoding.UTF8;
            //    Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());
            //    gv.RenderControl(htw);
            //    return File(System.Text.Encoding.UTF8.GetBytes(sw.ToString()), Response.ContentType, birim.BirimAdi + "_Birimi_" + surecBilgi.Yil + "_Yılı_MaddeListesi.xls");

            //}
            #endregion
            ViewBag.VASurecID = new SelectList(SurecIslemleriBus.CmbVaSurecler(false), "Value", "Caption", model.VaSurecId);
            ViewBag.BirimID = new SelectList(VeriGirisiBus.CmbYetkiliVaSurecBirimlerKullanici(model.VaSurecId.Value, false), "Value", "Caption", model.BirimId);
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
        public ActionResult VgAciklamaSil(int id)
        {
            var result = VeriGirisiBus.VeriAciklamasiSil(id);
            return new
            {
                success = result.Success,
                message = string.Join("<br/>", result.Message.Messages)
            }.ToJsonResult();
        }

    }
}