using System.Collections.Generic;
using System.Linq;
using Database;
using WebApp.Models;

namespace WebApp.Business
{
    public static class MaddeTurleriBus
    {
        public static IEnumerable<ComboModelInt> CmbMaddeTurleri(bool bosSecimVar = true, bool? isAktif = null)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { });
            using (var db = new VysDBEntities())
            {
                var data = db.MaddeTurleris.Where(p => p.IsAktif == (isAktif ?? p.IsAktif)).OrderBy(o => o.MaddeTurAdi).ToList();
                dct.AddRange(data.Select(item => new ComboModelInt { Value = item.MaddeTurID, Caption = item.MaddeTurAdi }));
            }
            return dct;

        }
    }
}