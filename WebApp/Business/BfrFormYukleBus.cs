using System;
using System.Collections.Generic;
using System.Linq;
using BiskaUtil;
using Database;
using WebApp.Models;

namespace WebApp.Business
{
    public static class BfrFormYukleBus
    {
        public static List<ComboModelInt> CmbBfrDonemler(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Value = null, Caption = "" });

            using (var db = new VysDBEntities())
            {
                var nowDate = DateTime.Now;
                var data = db.BFRDonemleris.OrderByDescending(o => o.Yil).ToList();

                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.BFRDonemID, Caption = item.Yil + " Yılı Dönemi" });
                }
            }
            return dct;

        }

        public static ChkListModel GetBFormBirimleri(int? bfrFormId, List<int> secilenler = null)
        {
            var model = new ChkListModel();
            using (var db = new VysDBEntities())
            {

                if (secilenler == null && bfrFormId.HasValue && bfrFormId > 0)
                {
                    var data = db.BFRFormlars.FirstOrDefault(p => p.BFRFormID == bfrFormId);
                    secilenler = data.BFRFormlarBirims.Select(s => s.BirimID).ToList();

                }
                if (secilenler == null) secilenler = new List<int>();
                var birimlers = db.sp_BirimAgaci().Where(p => p.IsMaddeEklenebilir == true).Select(s => new Birimler { BirimID = s.BirimID.Value, BirimKod = s.BirimKod, BirimAdi = s.BirimTreeAdi }).OrderBy(o => o.BirimAdi).ToList();
                var dataR = birimlers.Select(s => new CheckObject<ChkListDataModel>
                {
                    Value = new ChkListDataModel { ID = s.BirimID, Code = s.BirimKod, Caption = s.BirimAdi },
                    Checked = secilenler.Contains(s.BirimID)
                }).OrderByDescending(o => o.Checked);
                model.Data = dataR;
                return model;
            }
        }
    }
}