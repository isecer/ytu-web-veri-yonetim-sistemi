using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BiskaUtil;
using Database;
using WebApp.Models;

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
                var maddelers = db.sp_MaddeAgaci().Where(p => p.VeriGirisSekliID > 0 && p.IsAktif == true).Select(s => new Maddeler { MaddeID = s.MaddeID.Value, MaddeKod = s.MaddeKod, MaddeAdi = s.MaddeTreeAdi }).OrderBy(o => o.MaddeAdi).ToList();
                var dataR = maddelers.Select(s => new CheckObject<ChkListDataModel>
                {
                    Value = new ChkListDataModel { ID = s.MaddeID, Code = s.MaddeKod, Caption = s.MaddeAdi },
                    Checked = secilenler.Contains(s.MaddeID)
                }).OrderByDescending(o => o.Checked);
                model.Data = dataR;
                return model;
            }
        }
    }
}