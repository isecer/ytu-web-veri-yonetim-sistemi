using BiskaUtil;
using Database;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApp.Models;
using WebApp.Utilities.Extensions;

namespace WebApp.Business
{
    public static class BirimlerBus
    {
        public static List<ComboModelInt> CmbBirimlerTree(bool bosSecimVar = true, int? haricBirimId = null)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Value = null, Caption = "" });
            using (var db = new VysDBEntities())
            {
                var b = db.Birimlers.AsQueryable();
                if (haricBirimId.HasValue)
                {
                    var subBid = haricBirimId.Value.GetSubBirimIDs();
                    b = b.Where(p => subBid.Contains(p.BirimID) == false);
                }
                var data = b.OrderBy(o => o.BirimAdi).ToList().ToOrderedList("BirimID", "UstBirimID", "BirimAdi");
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.BirimID, Caption = item.BirimAdi });
                }
            }
            return dct;

        }

        public static List<Birimler> GetBirimler()
        {

            using (var db = new VysDBEntities())
            {
                return db.Birimlers.OrderBy(o => o.BirimAdi).ToList();

            }
        }
        public static ChkListModel GetBirimMaddeleri(int? birimId, List<int> secilenler = null)
        {
            var model = new ChkListModel();

            using (var db = new VysDBEntities())
            {

                if (secilenler == null && birimId.HasValue && birimId > 0)
                {
                    var data = db.Birimlers.FirstOrDefault(p => p.BirimID == birimId);
                    secilenler = data.BirimMaddeleris.Select(s => s.MaddeID).ToList();

                }
                if (secilenler == null) secilenler = new List<int>();
                var maddeTurleris = db.MaddeTurleris.ToList();
                var maddeAgaci = db.sp_MaddeAgaci().Where(p => p.VeriGirisSekliID.HasValue && p.IsAktif == true).ToList();
                var maddelers = (from madde in maddeAgaci
                                 join maddeturu in maddeTurleris on madde.MaddeTurID equals maddeturu.MaddeTurID
                                 select new Maddeler
                                 {
                                     MaddeID = madde.MaddeID.Value,
                                     MaddeKod = madde.MaddeKod,
                                     MaddeAdi = maddeturu.MaddeTurAdi + " => " + madde.MaddeTreeAdi
                                 }
                                 ).OrderBy(o => o.MaddeAdi).ToList();
                var dataR = maddelers.Select(s => new CheckObject<ChkListDataModel>
                {
                    Value = new ChkListDataModel { ID = s.MaddeID, Code = s.MaddeKod, Caption = s.MaddeAdi },
                    Checked = secilenler.Contains(s.MaddeID)
                }).OrderByDescending(o => o.Checked);
                model.Data = dataR;
                return model;
            }
        }
        public static ChkListModel GetMaddeBirimleri(int? maddeId, List<int> secilenler = null)
        {
            var model = new ChkListModel();
            using (var db = new VysDBEntities())
            {

                if (secilenler == null && maddeId.HasValue && maddeId > 0)
                {
                    var data = db.Maddelers.FirstOrDefault(p => p.MaddeID == maddeId);
                    secilenler = data.BirimMaddeleris.Select(s => s.BirimID).ToList();

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
        public static ChkListModel GetGtBirimHesapNumaralari(int? gtBirimId, List<int> secilenler = null)
        {
            var model = new ChkListModel();

            using (var db = new VysDBEntities())
            {

                if (secilenler == null && gtBirimId.HasValue && gtBirimId > 0)
                {
                    var data = db.GTBirimlers.FirstOrDefault(p => p.GTBirimID == gtBirimId);
                    secilenler = data.GTBirimHesapNumaralaris.Select(s => s.GTHesapNoID).ToList();

                }
                if (secilenler == null) secilenler = new List<int>();
                var hesapNumaralari = db.GTHesapNumaralaris.Where(p => p.IsAktif).ToList();
                var dataR = hesapNumaralari.Select(s => new CheckObject<ChkListDataModel>
                {
                    Value = new ChkListDataModel { ID = s.GTHesapNoID, Code = s.HesapNo, Caption = s.HesapNo + " " + s.HesapNoAdi },
                    Checked = secilenler.Contains(s.GTHesapNoID)
                }).OrderByDescending(o => o.Checked);
                model.Data = dataR;
                return model;
            }
        }
        public static ChkListModel GetGtBirimHesapKodlari(int? gtBirimId, List<int> secilenler = null)
        {
            var model = new ChkListModel();

            using (var db = new VysDBEntities())
            {

                if (secilenler == null && gtBirimId.HasValue && gtBirimId > 0)
                {
                    var data = db.GTBirimlers.FirstOrDefault(p => p.GTBirimID == gtBirimId);
                    secilenler = data.GTBirimHesapKodlaris.Select(s => s.GTHesapKodID).ToList();

                }
                if (secilenler == null) secilenler = new List<int>();
                var hesapKodlari = db.GTHesapKodlaris.Where(p => p.IsAktif).ToList();
                var dataR = hesapKodlari.Select(s => new CheckObject<ChkListDataModel>
                {
                    Value = new ChkListDataModel { ID = s.GTHesapKodID, Code = s.HesapKod, Caption = s.HesapKod + " " + s.HesapKodAdi },
                    Checked = secilenler.Contains(s.GTHesapKodID)
                }).OrderByDescending(o => o.Checked);
                model.Data = dataR;
                return model;
            }
        }

        public static List<int> GetSubBirimIDs(this int birimId, List<int> liste = null)
        {
            if (liste == null) liste = new List<int>();
            var bids = GetfncSubBirimIDs(birimId, liste);
            return bids;
        }

        private static readonly Func<int, List<int>, List<int>> GetfncSubBirimIDs = (y, lst) =>
        {
            using (var db = new VysDBEntities())
            {
                lst.Add(y);
                var loks = db.Birimlers.Where(p => p.UstBirimID == y).ToList();
                foreach (var item2 in loks)
                {
                    GetfncSubBirimIDs(item2.BirimID, lst);
                }
                return lst;

            }

        };
    }
}