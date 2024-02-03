using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Database;
using WebApp.Business;
using WebApp.Models; 
using WebApp.Utilities.Dtos;

namespace WebApp.Utilities.SystemSetting
{
    public static class SistemAyar
    {
        public const string AyarSmtpHost = "Smtp Host Adresi";
        public const string AyarSmtpPort = "Smtp Port Adresi";
        public const string AyarSmtpSsl = "Smtp SSL";
        public const string AyarSmtpMail = "Smtp Mail Adresi";
        public const string AyarSmtpUser = "Smtp Kullanıcı Adı";
        public const string AyarSmtpPassword = "Smtp Şifre";
        public const string AyarSistemErisimAdresi = "Sistem Erişim Adresi";
        public const string KullaniciResimYolu = "Kullanıcı Resim Yolu";
        public const string KullaniciDefaultResim = "Kullanıcı Default Resim";

        public const string KullaniciResimKaydiBoyutlandirma = "Resim Kaydında Boyutlandırma Yap";
        public const string KullaniciResimKaydiWidthPx = "Resim Kaydı Width (Px)";
        public const string KullaniciResimKaydiHeightPx = "Resim Kaydı Height (Px)";
        public const string KullaniciResimKaydiKaliteOpt = "Resim Kaydında Kalite Optimizasyonu Yap";
        public const string RotasyonuDegisenResimleriLogla = "Rotasyonu Değişen Resimleri Logla";

        public static void SetAyar(string ayarAdi, string ayarDegeri)
        {
            using (var db = new VysDBEntities())
            {
                var qq = db.Ayarlars.FirstOrDefault(p => p.AyarAdi == ayarAdi);
                if (qq != null)
                {
                    qq.AyarDegeri = ayarDegeri;
                }
                else
                {
                    db.Ayarlars.Add(new Ayarlar { AyarAdi = ayarAdi, AyarDegeri = ayarDegeri });

                }
                db.SaveChanges();
            }

        }
        public static string GetAyar(this string ayarAdi, string varsayilanDeger = "")
        {
            using (var db = new VysDBEntities())
            {
                var qq = db.Ayarlars.FirstOrDefault(p => p.AyarAdi == ayarAdi);
                return qq != null ? qq.AyarDegeri : varsayilanDeger;
            }
        }
    }
}