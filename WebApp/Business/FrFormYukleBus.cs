using System.Collections.Generic;
using System.Linq;
using BiskaUtil;
using Database;
using WebApp.Models;

namespace WebApp.Business
{
    public static class FrFormYukleBus
    {
        public static IEnumerable<ComboModelInt> CmbFrDonemler(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Value = null, Caption = "" });

            using (var db = new VysDBEntities())
            {
                var data = db.FRDonemleris.OrderByDescending(o => o.Yil).ToList();

                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.FRDonemID, Caption = item.Yil + " Yılı Dönemi" });
                }
            }
            return dct;

        }

        public static ChkListModel GetFormBirimleri(int? frFormId, List<int> secilenler = null)
        {
            var model = new ChkListModel();
            using (var db = new VysDBEntities())
            {

                if (secilenler == null && frFormId.HasValue && frFormId > 0)
                {
                    var data = db.FRFormlars.FirstOrDefault(p => p.FRFormID == frFormId);
                    secilenler = data.FRFormlarBirims.Select(s => s.BirimID).ToList();

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