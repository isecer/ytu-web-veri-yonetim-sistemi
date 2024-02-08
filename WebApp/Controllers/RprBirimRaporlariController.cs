using BiskaUtil;
using Database;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApp.Business;
using WebApp.Models;
using WebApp.Utilities.Dtos;
using WebApp.Utilities.MenuAndRoles;
using WebApp.Utilities.MessageBox;

namespace WebApp.Controllers
{
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [Authorize(Roles = RoleNames.BirimRaporlari)]
    public class RprBirimRaporlariController : Controller
    {
        private readonly VysDBEntities _entities = new VysDBEntities();
        // GET: RprBirimRaporlari
        public ActionResult Index()
        {
            ViewBag.RaporTipID = new SelectList(RaporlarBus.CmbRaporTipleri(), "Value", "Caption");
            ViewBag.VASurecID = new SelectList(SurecIslemleriBus.CmbVaSurecler(), "Value", "Caption");
            ViewBag.BirimID = new SelectList(UserBus.CmbYetkiliBirimlerKullanici(false), "Value", "Caption");

            ViewBag.MmMessage = new MmMessage();
            return View();
        }
        [HttpPost]
        public ActionResult Index(int? raporTipId, int? vaSurecId, int? birimId, bool export = false)
        {
            var mmMessage = new MmMessage();
            if (!raporTipId.HasValue)
            {
                mmMessage.Messages.Add("Rapor Tipi seçiniz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "RaporTipID" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "RaporTipID" });
            if (!vaSurecId.HasValue)
            {
                mmMessage.Messages.Add("Süreç seçiniz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "VASurecID" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "VASurecID" });

            if (mmMessage.Messages.Count == 0)
            {
                #region export
                if (export)
                {

                    var raporTip = _entities.RaporTipleris.FirstOrDefault(p => p.RaporTipID == raporTipId);
                    var rtMaddeIDs = raporTip.RaporTipleriSecilenMaddelers.Select(s => s.MaddeID).ToList();
                    var gv = new GridView();



                    var q =
                      (from s in _entities.Vw_MaddeVeriGirisDurum.Where(p => p.VASurecID == vaSurecId && p.IsAktif)
                         join vm in _entities.VASurecleriMaddes on s.VASurecleriMaddeID equals vm.VASurecleriMaddeID

                       where s.BirimID == (birimId ?? s.BirimID) && rtMaddeIDs.Contains(s.MaddeID)
                       select new FrVgMaddeler
                       {
                           VASurecID=s.VASurecID, 
                           Yil = s.Yil,
                           BirimID=s.BirimID, 
                           BirimAdi=s.BirimTreeAdi,
                           MaddeTurID = s.MaddeTurID,
                           MaddeID = s.MaddeID,
                           MaddeKod = s.MaddeKod,
                           MaddeAdi = s.MaddeAdi,
                           MaddeTreeAdi = s.MaddeTreeAdi,
                           VeriGirisiOnaylandi = s.GirilecekVeriSayisi == s.OnaylananVeriSayisi,
                           OnaylananVeriSayisi = s.OnaylananVeriSayisi ?? 0,
                           IsPlanlananDegerOlacak = s.IsPlanlananDegerOlacak,
                           IsPlanlananDegerOlacakGelecekYil = s.IsPlanlananDegerOlacakGelecekYil,
                           PlanlananDeger = s.PlanlananDeger,
                           PlanlananDegerGelecekYil = s.PlanlananDegerGelecekYil,
                           GirilecekVeriSayisi = s.GirilecekVeriSayisi ?? 0,
                           GirilenVeriSayisi = s.GirilenVeriSayisi ?? 0,
                           YilSonuDegerHesaplamaAdi = s.YilSonuDegerHesaplamaAdi,
                           HesaplamaFormulu = s.HesaplamaFormulu,
                           VeriGirisSekliID = s.VeriGirisSekliID,
                           MaddeYilSonuDegerHesaplamaTipID = s.MaddeYilSonuDegerHesaplamaTipID,
                           HesaplananSonucDegeri = s.HesaplananSonucDegeri,
                           MaddeVeriGirisDurumId = s.OnaylananVeriSayisi > 0 && s.GirilecekVeriSayisi == s.OnaylananVeriSayisi ? 1 : 0, 
                           VASurecleriMaddeID = s.VASurecleriMaddeID,
                           Aciklama = s.Aciklama,
                           VeriGirisSekliAdi = s.VeriGirisSekliAdi,
                         //  VaSurecleriMaddeGirilenDegers = vbr.VASurecleriMaddeGirilenDegers.Where(p => p.VASurecleriMaddeID == s.VASurecleriMaddeID).ToList(),
                           VaSurecleriMaddeVeriGirisDonemleris = vm.VASurecleriMaddeVeriGirisDonemleris.ToList()
                       }).AsQueryable();
                    var qExp = q.ToList();
                    foreach (var itemM in qExp.Where(p => p.VeriGirisSekliID == VeriGirisSekli.FormulleHesaplanacak))
                    {
                        var row = VeriGirisiBus.VeriGirisRowSet(itemM, itemM.VASurecID, itemM.BirimID, itemM.MaddeID);
                        itemM.VaSurecleriMaddeGirilenDegers = row.VaSurecleriMaddeGirilenDegers;
                        itemM.VeriGirisiOnaylandi = row.VeriGirisiOnaylandi;
                        itemM.PlanlananDeger = row.PlanlananDeger;
                        itemM.PlanlananDegerGelecekYil = row.PlanlananDegerGelecekYil;
                        itemM.GirilecekVeriSayisi = row.GirilecekVeriSayisi;
                        itemM.GirilenVeriSayisi = row.GirilenVeriSayisi;
                        itemM.HesaplananSonucDegeri = row.HesaplananSonucDegeri;
                    }  
                    //var vaSurecleriBirimIDs = qExp.Select(s => s.VASurecleriBirimID).Distinct().ToList();
                    //var vaSurecleriMaddeIDs = qExp.Select(s => s.VASurecleriMaddeID).Distinct().ToList();
                    //var girilenDegerler = qExp.Where(p => vaSurecleriBirimIDs.Contains(p.VASurecleriBirimID) && vaSurecleriMaddeIDs.Contains(p.VASurecleriMaddeID)).SelectMany(s=>s.VaSurecleriMaddeGirilenDegers).ToList();

                    //var qeData = (from s in qExp
                    //              join dg in girilenDegerler on new { s.VASurecleriBirimID, s.VASurecleriMaddeID, VACokluVeriDonemID = (int?)null } equals
                    //                                            new { dg.VASurecleriBirimID, dg.VASurecleriMaddeID, dg.VACokluVeriDonemID } into deg
                    //              from g in deg.DefaultIfEmpty()
                    //              join dg1 in girilenDegerler on new { s.VASurecleriBirimID, s.VASurecleriMaddeID, VACokluVeriDonemID = 1 } equals
                    //                                             new { dg1.VASurecleriBirimID, dg1.VASurecleriMaddeID, VACokluVeriDonemID = dg1.VACokluVeriDonemID ?? 0 } into deg1
                    //              from g1 in deg1.DefaultIfEmpty()
                    //              join dg2 in girilenDegerler on new { s.VASurecleriBirimID, s.VASurecleriMaddeID, VACokluVeriDonemID = 2 } equals
                    //                                            new { dg2.VASurecleriBirimID, dg2.VASurecleriMaddeID, VACokluVeriDonemID = dg2.VACokluVeriDonemID ?? 0 } into deg2
                    //              from g2 in deg2.DefaultIfEmpty()
                    //              join dg3 in girilenDegerler on new { s.VASurecleriBirimID, s.VASurecleriMaddeID, VACokluVeriDonemID = 3 } equals
                    //                                      new { dg3.VASurecleriBirimID, dg3.VASurecleriMaddeID, VACokluVeriDonemID = dg3.VACokluVeriDonemID ?? 0 } into deg3
                    //              from g3 in deg3.DefaultIfEmpty()
                    //              join dg4 in girilenDegerler on new { s.VASurecleriBirimID, s.VASurecleriMaddeID, VACokluVeriDonemID = 4 } equals
                    //                                   new { dg4.VASurecleriBirimID, dg4.VASurecleriMaddeID, VACokluVeriDonemID = dg4.VACokluVeriDonemID ?? 0 } into deg4
                    //              from g4 in deg4.DefaultIfEmpty()
                    //              join dg5 in girilenDegerler on new { s.VASurecleriBirimID, s.VASurecleriMaddeID, VACokluVeriDonemID = 5 } equals
                    //                                   new { dg5.VASurecleriBirimID, dg5.VASurecleriMaddeID, VACokluVeriDonemID = dg5.VACokluVeriDonemID ?? 0 } into deg5
                    //              from g5 in deg5.DefaultIfEmpty()
                    //              join dg6 in girilenDegerler on new { s.VASurecleriBirimID, s.VASurecleriMaddeID, VACokluVeriDonemID = 6 } equals
                    //                                   new { dg6.VASurecleriBirimID, dg6.VASurecleriMaddeID, VACokluVeriDonemID = dg6.VACokluVeriDonemID ?? 0 } into deg6
                    //              from g6 in deg6.DefaultIfEmpty()
                    //              join dg7 in girilenDegerler on new { s.VASurecleriBirimID, s.VASurecleriMaddeID, VACokluVeriDonemID = 7 } equals
                    //                                   new { dg7.VASurecleriBirimID, dg7.VASurecleriMaddeID, VACokluVeriDonemID = dg7.VACokluVeriDonemID ?? 0 } into deg7
                    //              from g7 in deg7.DefaultIfEmpty()
                    //              join dg8 in girilenDegerler on new { s.VASurecleriBirimID, s.VASurecleriMaddeID, VACokluVeriDonemID = 8 } equals
                    //                                   new { dg8.VASurecleriBirimID, dg8.VASurecleriMaddeID, VACokluVeriDonemID = dg8.VACokluVeriDonemID ?? 0 } into deg8
                    //              from g8 in deg8.DefaultIfEmpty()
                    //              join dg9 in girilenDegerler on new { s.VASurecleriBirimID, s.VASurecleriMaddeID, VACokluVeriDonemID = 9 } equals
                    //                                   new { dg9.VASurecleriBirimID, dg9.VASurecleriMaddeID, VACokluVeriDonemID = dg9.VACokluVeriDonemID ?? 0 } into deg9
                    //              from g9 in deg9.DefaultIfEmpty()
                    //              join dg10 in girilenDegerler on new { s.VASurecleriBirimID, s.VASurecleriMaddeID, VACokluVeriDonemID = 10 } equals
                    //                                   new { dg10.VASurecleriBirimID, dg10.VASurecleriMaddeID, VACokluVeriDonemID = dg10.VACokluVeriDonemID ?? 0 } into deg10
                    //              from g10 in deg10.DefaultIfEmpty()
                    //              join dg11 in girilenDegerler on new { s.VASurecleriBirimID, s.VASurecleriMaddeID, VACokluVeriDonemID = 11 } equals
                    //                                   new { dg11.VASurecleriBirimID, dg11.VASurecleriMaddeID, VACokluVeriDonemID = dg11.VACokluVeriDonemID ?? 0 } into deg11
                    //              from g11 in deg11.DefaultIfEmpty()
                    //              join dg12 in girilenDegerler on new { s.VASurecleriBirimID, s.VASurecleriMaddeID, VACokluVeriDonemID = 12 } equals
                    //                                   new { dg12.VASurecleriBirimID, dg12.VASurecleriMaddeID, VACokluVeriDonemID = dg12.VACokluVeriDonemID ?? 0 } into deg12
                    //              from g12 in deg12.DefaultIfEmpty()
                    //              select new
                    //              {
                    //                  s.BirimAdi,
                    //                  MaddeKodu = s.MaddeKod,
                    //                  s.MaddeAdi,
                    //                  s.Yil, 
                    //                  Ocak = g1 != null ? g1.GirilenDeger.ToString() : "",
                    //                  Subat = g2 != null ? g2.GirilenDeger.ToString() : "",
                    //                  Mart = g3 != null ? g3.GirilenDeger.ToString() : "",
                    //                  Nisan = g4 != null ? g4.GirilenDeger.ToString() : "",
                    //                  Mayis = g5 != null ? g5.GirilenDeger.ToString() : "",
                    //                  Haziran = g6 != null ? g6.GirilenDeger.ToString() : "",
                    //                  Temmuz = g7 != null ? g7.GirilenDeger.ToString() : "",
                    //                  Agustos = g8 != null ? g8.GirilenDeger.ToString() : "",
                    //                  Eylul = g9 != null ? g9.GirilenDeger.ToString() : "",
                    //                  Ekim = g10 != null ? g10.GirilenDeger.ToString() : "",
                    //                  Kasim = g11 != null ? g11.GirilenDeger.ToString() : "",
                    //                  Aralik = g12 != null ? g12.GirilenDeger.ToString() : "",
                    //                  YillikDeger = g != null ? g.GirilenDeger.ToString() : "",
                    //                  PlanlananHedef = s.PlanlananDeger,
                    //                  s.YilSonuDegerHesaplamaAdi,
                    //                  YilSonuHesaplananDeger = s.HesaplananSonucDegeri,
                    //                  PlanlananHedefGelecekYil = s.PlanlananDegerGelecekYil,
                    //              }).ToList();


                    //gv.DataSource = qeData;
                    gv.DataBind();
                    StringWriter sw = new StringWriter();
                    HtmlTextWriter htw = new HtmlTextWriter(sw);
                    Response.ContentType = "application/ms-excel";
                    Response.ContentEncoding = System.Text.Encoding.UTF8;
                    Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());
                    gv.RenderControl(htw);
                    return File(System.Text.Encoding.UTF8.GetBytes(sw.ToString()), Response.ContentType, "Birim Madde Veri Listesi.xls"); 

                }
                #endregion
            }
            else
            {
                MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, mmMessage.Messages.ToArray());
            }
            ViewBag.RaporTipID = new SelectList(RaporlarBus.CmbRaporTipleri(), "Value", "Caption", raporTipId);
            ViewBag.VASurecID = new SelectList(SurecIslemleriBus.CmbVaSurecler(), "Value", "Caption", vaSurecId);
            ViewBag.BirimID = new SelectList(UserBus.CmbYetkiliBirimlerKullanici(false), "Value", "Caption", birimId);

            ViewBag.MmMessage = mmMessage;
            return View();
        }
    }
}