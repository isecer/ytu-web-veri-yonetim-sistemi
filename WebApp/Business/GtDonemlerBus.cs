using Database;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApp.Models;
using WebApp.Utilities.Extensions;

namespace WebApp.Business
{
    public static class GtDonemlerBus
    {
        public static int? GetAktifGtDonemId()
        {
            using (var db = new VysDBEntities())
            {
                var nowDate = DateTime.Now.Date;
                var donem = db.GTDonemleris.FirstOrDefault(a => (a.BaslangicTarihi <= nowDate && a.BitisTarihi >= nowDate) && a.IsAktif);
                return donem?.GTDonemID;
            }
        }

        public static List<ComboModelInt> CmbGtDonemler(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Value = null, Caption = "" });

            using (var db = new VysDBEntities())
            {
                var data = db.GTDonemleris.OrderByDescending(o => o.Yil).ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.GTDonemID, Caption = item.Yil + " Yılı Dönemi (" + item.BaslangicTarihi.ToFormatDate() + " / " + item.BitisTarihi.ToFormatDate() + ")" });
                }
            }
            return dct;

        }

        public static FrGTDonemleri GetGtDonemKontrol(int gtDonemId)
        {
            using (var db = new VysDBEntities())
            {
                var nowDate = DateTime.Now.Date;
                var xD = (from s in db.GTDonemleris.Where(p => p.GTDonemID == gtDonemId)
                          join k in db.Kullanicilars on s.IslemYapanID equals k.KullaniciID
                          select new FrGTDonemleri
                          {
                              GTDonemID = s.GTDonemID,
                              DonemYilAdi = s.Yil + " Yılı Dönemi",
                              Yil = s.Yil,
                              BaslangicTarihi = s.BaslangicTarihi,
                              BitisTarihi = s.BitisTarihi,
                              IsAktif = s.IsAktif,
                              IslemYapanID = s.IslemYapanID,
                              IslemYapan = k.KullaniciAdi,
                              IslemTarihi = s.IslemTarihi,
                              IslemYapanIP = s.IslemYapanIP,
                              AktifSurec = (s.BaslangicTarihi <= nowDate && s.BitisTarihi >= nowDate)
                          }).FirstOrDefault();
                return xD;
            }
        }
    }
}