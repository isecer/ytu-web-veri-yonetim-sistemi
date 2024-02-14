using Database;
using System.Collections.Generic;
using System.Linq;
using WebApp.Models;

namespace WebApp.Business
{
    public static class MailIslemleriBus
    {
        public static IEnumerable<ComboModelInt> CmbMailSablonTipleri(bool? sistemMaili = null, bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Value = null, Caption = "" });
            using (var db = new VysDBEntities())
            {
                var data = db.MailSablonTipleris.Where(p => p.SistemMaili == (sistemMaili ?? p.SistemMaili)).OrderBy(o => o.MailSablonTipID).ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.MailSablonTipID, Caption = item.SablonTipAdi });
                }
            }

            return dct;

        }

        public static IEnumerable<ComboModelInt> CmbMailSablonlari(bool bosSecimVar = true, bool? sistemMailFiltre = null)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Value = null, Caption = "" });
            using (var db = new VysDBEntities())
            {
                var data = db.MailSablonlaris.Where(p => p.IsAktif && p.MailSablonTipleri.SistemMaili == (sistemMailFiltre ?? p.MailSablonTipleri.SistemMaili)).OrderBy(o => o.SablonAdi).ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.MailSablonlariID, Caption = item.SablonAdi });
                }
            }

            return dct;

        }
    }
}