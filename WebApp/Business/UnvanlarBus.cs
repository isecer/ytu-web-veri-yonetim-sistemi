using Database;
using System.Collections.Generic;
using System.Linq;
using WebApp.Models;

namespace WebApp.Business
{
    public static class UnvanlarBus
    {
        public static IEnumerable<ComboModelInt> CmbUnvanlar(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Value = null, Caption = "" });
            using (var db = new VysDBEntities())
            {
                var data = db.Unvanlars.OrderBy(o => o.UnvanID == 1 ? 0 : 1).ThenBy(o => o.UnvanAdi).ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.UnvanID, Caption = item.UnvanAdi });
                }
            }
            return dct;

        }
    }
}