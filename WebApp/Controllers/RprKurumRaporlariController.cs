using BiskaUtil;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApp.Business;
using WebApp.Utilities.Helpers.Hesaplama;
using WebApp.Utilities.MenuAndRoles;
using WebApp.Utilities.MessageBox;

namespace WebApp.Controllers
{
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [Authorize(Roles = RoleNames.KurumRaporlari)]
    public class RprKurumRaporlariController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.VASurecID = new SelectList(SurecIslemleriBus.CmbVaSurecler(), "Value", "Caption");
            ViewBag.MmMessage = new MmMessage();
            return View();
        }
        [HttpPost]
        public ActionResult Index(List<int> raporTipIDs, int? vaSurecId, bool export = false)
        {
            raporTipIDs = raporTipIDs ?? new List<int>();
            var mmMessage = new MmMessage();
            if (!raporTipIDs.Any())
            {
                mmMessage.Messages.Add("En az 1 Rapor Tipi seçiniz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "RaporTipIDs" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "RaporTipIDs" });
            if (!vaSurecId.HasValue)
            {
                mmMessage.Messages.Add("Süreç seçiniz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "VASurecID" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "VASurecID" });

            if (mmMessage.Messages.Count == 0)
            {
                if (export)
                {
                    var kurumsalRaporHesap = new KurumsalRaporHesaplama(vaSurecId.Value, raporTipIDs);
                    var veriler = kurumsalRaporHesap.HesaplaTumu();
                    var gv = new GridView();
                    gv.DataSource = veriler;
                    gv.DataBind();
                    var stringWriter = new StringWriter();
                    var htmlTextWriter = new HtmlTextWriter(stringWriter);
                    Response.ContentType = "application/ms-excel";
                    Response.ContentEncoding = System.Text.Encoding.UTF8;
                    Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());
                    gv.RenderControl(htmlTextWriter);
                    return File(System.Text.Encoding.UTF8.GetBytes(stringWriter.ToString()), Response.ContentType, "Kurumsal Rapor - Madde Veri Listesi.xls");
                }
            }
            else
            {
                MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, mmMessage.Messages.ToArray());
            }
            ViewBag.VASurecID = new SelectList(SurecIslemleriBus.CmbVaSurecler(), "Value", "Caption", vaSurecId);
            ViewBag.RaporTipIds = raporTipIDs;
            ViewBag.MmMessage = mmMessage;
            return View();
        }
    }
}