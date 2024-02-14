using BiskaUtil;
using Database;
using System.Collections.Generic;
using System.Linq;
using WebApp.Models;
using WebApp.Utilities.Helpers;

namespace WebApp.Business
{
    public static class RaporlarBus
    {
        public static IEnumerable<ComboModelInt> CmbRaporTipleri(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Value = null, Caption = "" });

            using (var db = new VysDBEntities())
            {
                var data = db.RaporTipleris.OrderByDescending(o => o.RaporTipAdi).ToList();

                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.RaporTipID, Caption = item.RaporTipAdi });
                }
            }
            return dct;

        }

        public static ChkListModel GetRaporTipMaddeleri(int? raporTipId, List<int> secilenler = null)
        {
            var model = new ChkListModel();
            using (var db = new VysDBEntities())
            {

                if (secilenler == null && raporTipId > 0)
                {
                    var data = db.RaporTipleris.FirstOrDefault(p => p.RaporTipID == raporTipId);
                    secilenler = data.RaporTipleriSecilenMaddelers.Select(s => s.MaddeID).ToList();

                }
                if (secilenler == null) secilenler = new List<int>();
                var maddeTurleri = db.MaddeTurleris.Select(s => new { s.MaddeTurID, s.MaddeTurAdi }).ToList();
                var maddelers = db.sp_MaddeAgaci().Where(p => p.VeriGirisSekliID > 0 && p.IsAktif == true).Select(s => new Maddeler { MaddeID = s.MaddeID.Value, MaddeTurID = s.MaddeTurID, MaddeKod = s.MaddeKod, MaddeAdi = s.MaddeTreeAdi }).OrderBy(o => o.MaddeAdi).ToList();
                var dataR = (from madde in maddelers
                             join maddeturu in maddeTurleri on madde.MaddeTurID equals maddeturu.MaddeTurID
                             select new CheckObject<ChkListDataModel>
                             {
                                 Value = new ChkListDataModel { ID = madde.MaddeID, Code = madde.MaddeKod, Caption = maddeturu.MaddeTurAdi + " > " + madde.MaddeAdi },
                                 Checked = secilenler.Contains(madde.MaddeID)
                             }).OrderByDescending(o => o.Checked);
                model.Data = dataR.ToList();
                return model;
            }
        }

        public static ChkListModel GetRaporTipleri(List<int> secilenler = null)
        {
            var model = new ChkListModel();
            using (var db = new VysDBEntities())
            {
                if (secilenler == null) secilenler = new List<int>();
                var raporTipleris = db.RaporTipleris.Where(p => p.IsAktif).Select(s => new { s.RaporTipID, s.RaporTipAdi }).OrderBy(o => o.RaporTipAdi).ToList();
                var dataR = (from raporTipi in raporTipleris
                             select new CheckObject<ChkListDataModel>
                             {
                                 Value = new ChkListDataModel { ID = raporTipi.RaporTipID, Caption = raporTipi.RaporTipAdi },
                                 Checked = secilenler.Contains(raporTipi.RaporTipID)
                             }).OrderByDescending(o => o.Checked);
                model.Data = dataR;
                return model;
            }
        }

    }
}