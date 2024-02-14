using BiskaUtil;
using Database;
using System.Collections.Generic;
using System.Linq;
using WebApp.Models;

namespace WebApp.Business
{
    public static class GtVeriGirisiBus
    {
        public static IEnumerable<ComboModelInt> CmbGtVeriGirisDurumlari(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt());
            using (var db = new VysDBEntities())
            {
                var data = db.GTVeriGirisDurumlaris.ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.GTVeriGirisDurumID, Caption = item.DurumAdi });
                }
            }
            return dct;

        }

        public static IEnumerable<ComboModelInt> CmbHesapNumaralari(int? gtBirimId, bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { });
            using (var db = new VysDBEntities())
            {
                var hesapNos = UserIdentity.GetSelectedTableIDs(RollTableIDName.GTHesapNoID, gtBirimId).SelectMany(s => s.RefTableIDs).ToList();
                var data = db.GTHesapNumaralaris.Where(p => p.GTBirimHesapNumaralaris.Any(a => a.GTBirimID == (gtBirimId ?? a.GTBirimID)) && hesapNos.Contains(p.GTHesapNoID)).OrderBy(o => o.HesapNoAdi).ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.GTHesapNoID, Caption = item.HesapNo + " / " + item.HesapNoAdi });
                }
            }
            return dct;

        }
    }
}