using BiskaUtil;
using Database;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebApp.Utilities.Extensions;
using WebApp.Utilities.MenuAndRoles;

namespace WebApp.Controllers
{
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [Authorize(Roles = RoleNames.SistemAyarlari)]
    public class SistemAyarlariController : Controller
    {
        private readonly VysDBEntities db = new VysDBEntities();
        public ActionResult Index()
        {
            var data = db.Ayarlars.OrderBy(o => o.Kategori).ThenBy(t => t.SiraNo).ToList();
            var cats = data.Select(s => new { s.Kategori, Toggle = true }).Distinct().ToList();
            var panelToggledDct = new Dictionary<string, bool>();
            foreach (var item in cats)
            {
                panelToggledDct.Add(item.Kategori, item.Toggle);
            }
            ViewBag.PanelToggled = panelToggledDct;
            return View(data);
        }
        [HttpPost]
        public ActionResult Index(List<string> ayarAdi, List<string> ayarDegeri, List<string> panelToggled)
        {
            var qSistemAyarAdi = ayarAdi.Select((s, index) => new { inx = index, s }).ToList();
            var qSistemAyarDegeri = ayarDegeri.Select((s, index) => new { inx = index, s }).ToList();

            var qModel = (from sa in qSistemAyarAdi
                          join sad in qSistemAyarDegeri on sa.inx equals sad.inx
                          select new
                          {
                              RowID = sa.inx,
                              AyarAdi = sa.s,
                              AyarDegeri = sad.s,
                          }).ToList();
            foreach (var item in qModel)
            {
                var ayar = db.Ayarlars.FirstOrDefault(p => p.AyarAdi == item.AyarAdi);
                if (ayar != null)
                {
                    ayar.AyarDegeri = item.AyarDegeri;
                }
            }
            db.SaveChanges();
            MessageBox.Show("Sistem Ayarları Güncellendi", MessageBox.MessageType.Success);
            var data = db.Ayarlars.OrderBy(o => o.Kategori).ThenBy(t => t.SiraNo).ToList();
            var panelToggledDct = new Dictionary<string, bool>();
            foreach (var item in panelToggled)
            {
                var ptg = item.Replace("__", "◘").Split('◘');
                panelToggledDct.Add(ptg[0], ptg[1].ToBoolean() ?? false);
            }
            ViewBag.PanelToggled = panelToggledDct;
            return View(data);
        }

    }

}
