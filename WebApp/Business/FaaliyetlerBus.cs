using System.Collections.Generic;
using System.Linq;
using BiskaUtil;
using Database;
using WebApp.Models;

namespace WebApp.Business
{
    public static class FaaliyetlerBus
    {
        public static ChkListModel GetFaaliyetMaddeleri(int? faaliyetId, List<int> secilenler = null)
        {
            var model = new ChkListModel();
            using (var db = new VysDBEntities())
            {

                if (secilenler == null && faaliyetId.HasValue && faaliyetId > 0)
                {
                    var data = db.Faaliyetlers.FirstOrDefault(p => p.FaaliyetID == faaliyetId);
                    secilenler = data.FaaliyetlerMaddes.Select(s => s.MaddeID).ToList();

                }
                if (secilenler == null) secilenler = new List<int>();//madde türüne göre uzun süre aynı maddeler seçili kalmaktadır.
                var maddelers = db.sp_MaddeAgaci().Where(p => p.VeriGirisSekliID > 0 && p.MaddeTurID == 11).Select(s => new Maddeler { MaddeID = s.MaddeID.Value, MaddeKod = s.MaddeKod, MaddeAdi = s.MaddeTreeAdi }).OrderBy(o => o.MaddeAdi).ToList();
                var dataR = maddelers.Select(s => new CheckObject<ChkListDataModel>
                {
                    Value = new ChkListDataModel { ID = s.MaddeID, Code = s.MaddeKod, Caption = s.MaddeAdi },
                    Checked = secilenler.Contains(s.MaddeID)
                }).OrderByDescending(o => o.Checked);
                model.Data = dataR;
                return model;
            }
        }

        public static ChkListModel GetFaaliyetKaynaklari(int? faaliyetId, List<int> secilenler = null)
        {
            var model = new ChkListModel();

            using (var db = new VysDBEntities())
            {

                if (secilenler == null && faaliyetId.HasValue && faaliyetId > 0)
                {
                    var data = db.Faaliyetlers.FirstOrDefault(p => p.FaaliyetID == faaliyetId);
                    secilenler = data.FaaliyetlerKaynaks.Select(s => s.KaynakID).ToList();

                }
                if (secilenler == null) secilenler = new List<int>();
                var kaynaklars = db.Kaynaklars.ToList();
                var dataR = kaynaklars.Select(s => new CheckObject<ChkListDataModel>
                {
                    Value = new ChkListDataModel { ID = s.KaynakID, Caption = s.KaynakAdi },
                    Checked = secilenler.Contains(s.KaynakID)
                }).OrderByDescending(o => o.Checked);
                model.Data = dataR;
                return model;
            }
        }

        public static ChkListModel GetFaaliyetAylari(int? faaliyetId, List<int> secilenler = null)
        {
            var model = new ChkListModel();

            using (var db = new VysDBEntities())
            {

                if (secilenler == null && faaliyetId.HasValue && faaliyetId > 0)
                {
                    var data = db.Faaliyetlers.FirstOrDefault(p => p.FaaliyetID == faaliyetId);
                    secilenler = data.FaaliyetlerAys.Select(s => s.AyID).ToList();

                }
                if (secilenler == null) secilenler = new List<int>();
                var aylars = db.Aylars.OrderBy(o => o.AyID).ToList();
                var dataR = aylars.Select(s => new CheckObject<ChkListDataModel>
                {
                    Value = new ChkListDataModel { ID = s.AyID, Caption = s.AyAdi },
                    Checked = secilenler.Contains(s.AyID)
                }).OrderByDescending(o => o.Checked);
                model.Data = dataR;
                return model;
            }
        }
    }
}