using BiskaUtil;
using Database;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApp.Models;

namespace WebApp.Business
{
    public static class FrDonemlerBus
    {
        public static int? GetAktifFrDonemId()
        {
            using (var db = new VysDBEntities())
            {
                var nowDate = DateTime.Now.Date;
                var donem = db.FRDonemleris.FirstOrDefault(a => (a.BaslangicTarihi <= nowDate && a.BitisTarihi >= nowDate) && a.IsAktif);
                return donem?.FRDonemID;
            }
        }

        public static FrFrDonemleri GetFrDonemKontrol(int frDonemId)
        {
            using (var db = new VysDBEntities())
            {
                var nowDate = DateTime.Now.Date;
                var xD = (from s in db.FRDonemleris.Where(p => p.FRDonemID == frDonemId)
                          join k in db.Kullanicilars on s.IslemYapanID equals k.KullaniciID
                          select new FrFrDonemleri
                          {
                              FRDonemID = s.FRDonemID,
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

        public static ChkListModel GetFrDonemFormlar(int? frDonemId, List<int> secilenler = null)
        {
            var model = new ChkListModel();
            using (var db = new VysDBEntities())
            {

                if (secilenler == null && frDonemId > 0)
                {
                    var data = db.FRDonemleris.FirstOrDefault(p => p.FRDonemID == frDonemId);
                    secilenler = data.FRDonemlerForms.Where(p => p.IsAktif).Select(s => s.FRFormID).ToList();

                }
                if (secilenler == null) secilenler = new List<int>();
                var formlars = db.FRFormlars.Where(p => p.IsAktif).ToList();
                var dataR = formlars.Select(s => new CheckObject<ChkListDataModel>
                {
                    Value = new ChkListDataModel { ID = s.FRFormID, Caption = s.FormAdi },
                    Checked = secilenler.Contains(s.FRFormID)
                }).OrderByDescending(o => o.Checked);
                model.Data = dataR;
                return model;
            }
        }
    }
}