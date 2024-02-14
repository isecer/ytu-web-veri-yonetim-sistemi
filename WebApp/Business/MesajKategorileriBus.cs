using Database;
using System.Collections.Generic;
using System.Linq;
using WebApp.Models;

namespace WebApp.Business
{
    public static class MesajKategorileriBus
    {
        public static IEnumerable<ComboModelInt> CmbMesajKategorileri(bool bosSecimVar = true, bool? isAktif = null)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Value = null, Caption = "" });
            using (var db = new VysDBEntities())
            {
                var qdata = db.MesajKategorileris.AsQueryable();
                if (isAktif.HasValue) qdata = qdata.Where(p => p.IsAktif == isAktif.Value);
                var data = qdata.OrderBy(o => o.KategoriAdi).ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.MesajKategoriID, Caption = item.KategoriAdi });
                }
            }
            return dct;

        }
    }
}