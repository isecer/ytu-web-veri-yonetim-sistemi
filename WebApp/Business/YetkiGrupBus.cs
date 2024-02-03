using System.Collections.Generic;
using System.Linq;
using Database;
using WebApp.Models;

namespace WebApp.Business
{
    public static class YetkiGrupBus
    {
        public static List<Roller> GetYetkiGrupRoles(int yetkiGrupId)
        {
            using (var db = new VysDBEntities())
            {
                var kull = db.YetkiGrupRolleris.Where(p => p.YetkiGrupID == yetkiGrupId).ToList();

                var rolIDs = kull.Select(s => s.RolID).ToList();
                return db.Rollers.Where(p => rolIDs.Contains(p.RolID)).ToList();


            }
        }

        public static IEnumerable<ComboModelInt> CmbYetkiGruplari(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Value = null, Caption = "" });

            using (var db = new VysDBEntities())
            {
                var data = db.YetkiGruplaris.OrderBy(o => o.YetkiGrupAdi).ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.YetkiGrupID, Caption = item.YetkiGrupAdi });
                }
            }
            return dct;

        }
    }
}