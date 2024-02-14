using BiskaUtil;
using Database;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApp.Models;

namespace WebApp.Business
{
    public static class BfrDonemlerBus
    {
        public static int? GetAktifBfrDonemId()
        {
            using (var db = new VysDBEntities())
            {
                var nowDate = DateTime.Now.Date;
                var donem = db.BFRDonemleris.FirstOrDefault(a => (a.BaslangicTarihi <= nowDate && a.BitisTarihi >= nowDate) && a.IsAktif);
                return donem?.BFRDonemID;
            }
        }

        public static FrBfrDonemleri GetBfrDonemKontrol(int bfrDonemId)
        {
            using (var db = new VysDBEntities())
            {
                var nowDate = DateTime.Now;
                var xD = (from s in db.BFRDonemleris.Where(p => p.BFRDonemID == bfrDonemId)
                          join k in db.Kullanicilars on s.IslemYapanID equals k.KullaniciID
                          select new FrBfrDonemleri
                          {
                              BFRDonemID = s.BFRDonemID,
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

        public static ChkListModel GetBDonemFormlar(int? bfrDonemId, List<int> secilenler = null)
        {
            var model = new ChkListModel();
            using (var db = new VysDBEntities())
            {

                if (secilenler == null && bfrDonemId.HasValue && bfrDonemId > 0)
                {
                    var data = db.BFRDonemleris.FirstOrDefault(p => p.BFRDonemID == bfrDonemId);
                    secilenler = data.BFRDonemlerForms.Where(p => p.IsAktif).Select(s => s.BFRFormID).ToList();

                }
                if (secilenler == null) secilenler = new List<int>();
                var formlars = db.BFRFormlars.Where(p => p.IsAktif).ToList();
                var dataR = formlars.Select(s => new CheckObject<ChkListDataModel>
                {
                    Value = new ChkListDataModel { ID = s.BFRFormID, Caption = s.FormAdi },
                    Checked = secilenler.Contains(s.BFRFormID)
                }).OrderByDescending(o => o.Checked);
                model.Data = dataR;
                return model;
            }
        }
    }
}