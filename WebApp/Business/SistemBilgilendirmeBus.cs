using System;
using BiskaUtil;
using Database;
using WebApp.Utilities.Extensions;

namespace WebApp.Business
{
    public static class SistemBilgilendirmeBus
    {
        public static void AddMessage(SystemInformation sis)
        {
            int? currid = UserIdentity.Current == null ? null : (int?)UserIdentity.Current.Id;
            using (VysDBEntities db = new VysDBEntities())
            {
                db.SistemBilgilendirmes.Add(new SistemBilgilendirme
                {
                    BilgiTipi = (byte)sis.InfoType,
                    Kategori = sis.Category,
                    Message = sis.Message,
                    StackTrace = sis.StackTrace,
                    IslemYapanIP = UserIdentity.Ip,
                    IslemTarihi = DateTime.Now,
                    IslemYapanID = currid
                });
                db.SaveChanges();
            }
        }

        public static void SistemBilgisiKaydet(Exception ex, byte bilgiTipi)
        {
            using (var db = new VysDBEntities())
            {


                db.SistemBilgilendirmes.Add(new SistemBilgilendirme
                {
                    BilgiTipi = bilgiTipi,
                    Message = ex.ToExceptionMessage(),
                    IslemYapanID = UserIdentity.Current.Id,
                    IslemYapanIP = UserIdentity.Ip,
                    IslemTarihi = DateTime.Now,
                    StackTrace = ex.ToExceptionStackTrace()
                });
                db.SaveChanges();
            }
        }

        public static void SistemBilgisiKaydet(Exception ex, string message, byte bilgiTipi)
        {
            using (var db = new VysDBEntities())
            {


                db.SistemBilgilendirmes.Add(new SistemBilgilendirme
                {
                    BilgiTipi = bilgiTipi,
                    Message = message,
                    IslemYapanID = UserIdentity.Current.Id,
                    IslemYapanIP = UserIdentity.Ip,
                    IslemTarihi = DateTime.Now,
                    StackTrace = ex.ToExceptionStackTrace()
                });
                db.SaveChanges();
            }
        }
        public static void SistemBilgisiKaydet(string mesaj, string stakTrace, byte bilgiTipi)
        {
            using (var db = new VysDBEntities())
            {
                var kulId = (UserIdentity.Ip == "" || UserIdentity.Current.Id <= 0 ? (int?)null : UserIdentity.Current.Id);

                db.SistemBilgilendirmes.Add(new SistemBilgilendirme
                {
                    Message = mesaj,
                    BilgiTipi = bilgiTipi,
                    IslemYapanID = kulId,
                    IslemYapanIP = UserIdentity.Ip,
                    IslemTarihi = DateTime.Now,
                    StackTrace = stakTrace
                });
                db.SaveChanges();
            }
        }

        public static void SistemBilgisiKaydet(string mesaj, string stakTrace, byte bilgiTipi, int? kullaniciId, string kullaniciIp)
        {
            using (var db = new VysDBEntities())
            {
                db.SistemBilgilendirmes.Add(new SistemBilgilendirme
                {
                    Message = mesaj,
                    BilgiTipi = bilgiTipi,
                    IslemYapanID = kullaniciId,
                    IslemYapanIP = !kullaniciIp.IsNullOrWhiteSpace() ? kullaniciIp : UserIdentity.Ip,
                    IslemTarihi = DateTime.Now,
                    StackTrace = stakTrace
                });
                db.SaveChanges();
            }
        }
    }
}