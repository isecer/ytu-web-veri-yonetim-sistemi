using BiskaUtil;
using Database;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApp.Models;
using WebApp.Utilities.Extensions;

namespace WebApp.Business
{
    public static class MaddelerBus
    {
        public static List<ComboModelInt> CmbVeriGirisSekilleri(bool bosSecimVar = true, bool? isAktif = null)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Caption = "" });
            using (var db = new VysDBEntities())
            {
                var data = db.VeriGirisSekilleris.Where(p => p.IsAktif == (isAktif ?? p.IsAktif)).ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.VeriGirisSekliID, Caption = item.VeriGirisSekliAdi });
                }
            }
            return dct;

        }

        public static List<ComboModelInt> CmbVeriTipleri(bool bosSecimVar = true, bool? isAktif = null)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt());
            using (var db = new VysDBEntities())
            {
                var data = db.VeriTipleris.Where(p => p.IsAktif == (isAktif ?? p.IsAktif)).ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.VeriTipID, Caption = item.VeriTipAdi });
                }
            }
            return dct;

        }

        public static List<ComboModelInt> CmbUstMaddelerTree(bool bosSecimVar = true, int? haricMaddeId = null)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Value = null, Caption = "" });
            using (var db = new VysDBEntities())
            {
                var b = db.Maddelers.Where(p => p.VeriGirisSekliID == VeriGirisSekli.VeriGirisiYok).AsQueryable();
                if (haricMaddeId.HasValue)
                {
                    var subBid = haricMaddeId.Value.GetSubMaddeIDs();
                    b = b.Where(p => subBid.Contains(p.MaddeID) == false);
                }
                var data = b.OrderBy(o => o.MaddeAdi).ToList();
                var dataTree = data.ToOrderedList("MaddeID", "UstMaddeID", "MaddeAdi");
                foreach (var item in dataTree)
                {
                    dct.Add(new ComboModelInt { Value = item.MaddeID, Caption = item.MaddeAdi });
                }
            }
            return dct;
        }

        public static List<ComboModelInt> CmbMaddeTurleriFilterData(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt());
            using (var db = new VysDBEntities())
            {
                var maddeTurIds = db.Maddelers.Where(p => p.MaddeTurID.HasValue).Select(s => s.MaddeTurID).Distinct().ToList();
                var data = db.MaddeTurleris.Where(p => maddeTurIds.Contains(p.MaddeTurID)).Select(s => new
                {
                    s.MaddeTurID,
                    s.MaddeTurAdi,
                    s.IsAktif,
                }).Distinct().OrderBy(o => !o.IsAktif).ThenBy(o => o.MaddeTurAdi).ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.MaddeTurID, Caption = item.MaddeTurAdi + (!item.IsAktif ? " (Pasif)" : "") });
                }
            }
            return dct;

        }

        public static List<ComboModelInt> CmbMaddeYilsonuDegerHesaplamaTipleri(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt());
            using (var db = new VysDBEntities())
            {
                var data = db.MaddeYilSonuDegerHesaplamaTipleris.ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.MaddeYilSonuDegerHesaplamaTipID, Caption = item.YilSonuDegerHesaplamaAdi });
                }
            }
            return dct;

        }

        public static ChkListModel GetMaddeVeriGirisDonemleri(int? maddeId, List<MaddelerVeriGirisDonemleri> secilenler = null)
        {
            var model = new ChkListModel();
            using (var db = new VysDBEntities())
            {
                var vaCokluVeriDonemleris = db.VACokluVeriDonemleris.OrderBy(o => o.VACokluVeriDonemID).ToList();

                if (secilenler == null && maddeId > 0)
                {
                    var data = db.Maddelers.FirstOrDefault(p => p.MaddeID == maddeId);
                    secilenler = data.MaddelerVeriGirisDonemleris.ToList();
                    model.Data = (from dnm in vaCokluVeriDonemleris
                                  join sc in secilenler on dnm.VACokluVeriDonemID equals sc.VACokluVeriDonemID into defSec
                                  from scDef in defSec.DefaultIfEmpty()

                                  select new CheckObject<ChkListDataModel>
                                  {
                                      Value = new ChkListDataModel { ID = dnm.VACokluVeriDonemID, Caption = dnm.CokluVeriDonemAdi, IsDosyaYuklensin = scDef?.IsDosyaYuklensin ?? dnm.IsDosyaYuklensin },
                                      Checked = secilenler.Any(a => a.VACokluVeriDonemID == dnm.VACokluVeriDonemID)
                                  }).OrderBy(o => o.Value.ID);
                    return model;
                }
                if (secilenler == null) secilenler = new List<MaddelerVeriGirisDonemleri>();
                var dataR = vaCokluVeriDonemleris.Select(s => new CheckObject<ChkListDataModel>
                {
                    Value = new ChkListDataModel { ID = s.VACokluVeriDonemID, Caption = s.CokluVeriDonemAdi, IsDosyaYuklensin = s.IsDosyaYuklensin },
                    Checked = secilenler.Any(a => a.VACokluVeriDonemID == s.VACokluVeriDonemID)
                }).OrderBy(o => o.Value.ID);
                model.Data = dataR;
                return model;
            }
        }

        private static List<int> GetSubMaddeIDs(this int maddeId, List<int> liste = null)
        {
            if (liste == null) liste = new List<int>();
            var bids = GetfncSubMaddeIDs(maddeId, liste);
            return bids;
        }

        private static readonly Func<int, List<int>, List<int>> GetfncSubMaddeIDs = (y, lst) =>
        {
            using (var db = new VysDBEntities())
            {
                lst.Add(y);
                var loks = db.Maddelers.Where(p => p.UstMaddeID == y).ToList();
                foreach (var item2 in loks)
                {
                    GetfncSubMaddeIDs(item2.MaddeID, lst);
                }
                return lst;

            }

        };
    }
}