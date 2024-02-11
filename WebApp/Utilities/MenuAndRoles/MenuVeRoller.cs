using BiskaUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Utilities.MenuAndRoles
{
    public class SystemMenus : IMenu
    {

        [MenuAttribute(MenuID = 80000, MenuAdi = "Veri Giriş İşlemleri", MenuCssClass = "fa fa-signal", MenuUrl = "", DilCeviriYap = false, SiraNo = 6)]
        public const string VeriGirisIslemleri = "Veri Giriş İşlemleri";

        [MenuAttribute(MenuID = 84000, MenuAdi = "Veri Giriş Raporları", MenuCssClass = "fa fa-area-chart", MenuUrl = "", DilCeviriYap = false, SiraNo = 7)]
        public const string RaporIslemleri = "RaporIslemleri";

        [MenuAttribute(MenuID = 85500, MenuAdi = "Bütçe Hazırlık Formları", MenuCssClass = "fa fa-list-alt", MenuUrl = "", DilCeviriYap = false, SiraNo = 14)]
        public const string HazirlikFormlari = "Hazırlık Formları";
        [MenuAttribute(MenuID = 85700, MenuAdi = "Faaliyet Raporları", MenuCssClass = "fa fa-list-alt", MenuUrl = "", DilCeviriYap = false, SiraNo = 18)]
        public const string FaaliyetRaporlari = "Faaliyet Raporları";
        [MenuAttribute(MenuID = 86500, MenuAdi = "Gelir Takibi İşlemleri", MenuCssClass = "fa fa-try", MenuUrl = "", DilCeviriYap = false, SiraNo = 20)]
        public const string GelirTakibiIslemleri = "Gelir Takibi İşlemleri";

        [MenuAttribute(MenuID = 85000, MenuAdi = "Kullanıcı İşlemleri", MenuCssClass = "fa fa-group", MenuUrl = "", DilCeviriYap = false, SiraNo = 22)]
        public const string Tanimlamalar = "Tanımlamalar";

   

        [MenuAttribute(MenuID = 100000, MenuAdi = "Sistem", MenuCssClass = "fa fa-desktop", MenuUrl = "", DilCeviriYap = false, SiraNo = 26)]
        public const string Sistem = "Sistem";
    }
    public class RoleNames : IRoleName, IMenu
    {

        [MenuAttribute(BagliMenuID = 80000, MenuAdi = "Süreç İşlemleri", MenuCssClass = "fa fa-clock-o", MenuUrl = "SurecIslemleri/Index", DilCeviriYap = false, SiraNo = 1)]
        [RoleAttribute(GorunurAdi = "Süreç İşlemleri", Kategori = "Veri Giriş İşlemleri", Aciklama = "")]
        public const string SurecIslemleri = "Süreç İşlemleri";
        [RoleAttribute(GorunurAdi = "Süreç İşlemleri Kayıt Yetkisi", Kategori = "Veri Giriş İşlemleri", Aciklama = "")]
        public const string SurecIslemleriKayitYetkisi = "Süreç İşlemleri Kayıt Yetkisi";

        [MenuAttribute(BagliMenuID = 80000, MenuAdi = "Veri Girişi", MenuCssClass = "fa fa-file-text-o", MenuUrl = "VeriGirisi/Index", DilCeviriYap = false, SiraNo = 3)]
        [RoleAttribute(GorunurAdi = "Veri Girişi", Kategori = "Veri Giriş İşlemleri", Aciklama = "")]
        public const string VeriGirisi = "Veri Girişi";

        [RoleAttribute(GorunurAdi = "Veri Girişi Planlanan Hedef Kayıt Yetkisi", Kategori = "Veri Giriş İşlemleri", Aciklama = "")]
        public const string VeriGirisiPlanlananHedefKayitYetkisi = "Veri Girişi Planlanan Hedef Kayıt Yetkisi";

        [RoleAttribute(GorunurAdi = "Veri Girişi Kayıt Yetkisi", Kategori = "Veri Giriş İşlemleri", Aciklama = "")]
        public const string VeriGirisiKayitYetkisi = "Veri Girişi Kayıt Yetkisi";

        [RoleAttribute(GorunurAdi = "Veri Girişi Onay Yetkisi", Kategori = "Veri Giriş İşlemleri", Aciklama = "")]
        public const string VeriGirisiOnayYetkisi = "Veri Girişi Onay Yetkisi";



        [MenuAttribute(BagliMenuID = 80000, MenuAdi = "Madde Türleri", MenuCssClass = "fa fa-list-alt", MenuUrl = "MaddeTurleri/Index", DilCeviriYap = false, SiraNo = 5)]
        [RoleAttribute(GorunurAdi = "Madde Türleri", Kategori = "Veri Giriş İşlemleri", Aciklama = "")]
        public const string MaddeTurler = "Madde Türleri";
        [RoleAttribute(GorunurAdi = "Madde Türleri Kayıt", Kategori = "Veri Giriş İşlemleri", Aciklama = "")]
        public const string MaddeTurleriKayit = "Madde Türleri Kayıt";


        [MenuAttribute(BagliMenuID = 80000, MenuAdi = "Maddeler", MenuCssClass = "fa fa-list-alt", MenuUrl = "Maddeler/Index", DilCeviriYap = false, SiraNo = 8)]
        [RoleAttribute(GorunurAdi = "Maddeler", Kategori = "Veri Giriş İşlemleri", Aciklama = "")]
        public const string Maddeler = "Maddeler";
        [RoleAttribute(GorunurAdi = "Maddeler Kayıt", Kategori = "Veri Giriş İşlemleri", Aciklama = "")]
        public const string MaddelerKayit = "Maddeler Kayıt";

        [MenuAttribute(BagliMenuID = 80000, MenuAdi = "Faaliyetler", MenuCssClass = "fa fa-list-alt", MenuUrl = "Faaliyetler/Index", DilCeviriYap = false, SiraNo = 11)]
        [RoleAttribute(GorunurAdi = "Faaliyetler", Kategori = "Veri Giriş İşlemleri", Aciklama = "")]
        public const string Faaliyetler = "Faaliyetler";
        [RoleAttribute(GorunurAdi = "Faaliyetler Kayıt", Kategori = "Veri Giriş İşlemleri", Aciklama = "")]
        public const string FaaliyetlerKayit = "Faaliyetler Kayıt";

        [MenuAttribute(BagliMenuID = 80000, MenuAdi = "Rapor Tipleri", MenuCssClass = "fa fa-list-alt", MenuUrl = "RaporTipleri/Index", DilCeviriYap = false, SiraNo = 14)]
        [RoleAttribute(GorunurAdi = "Rapor Tipleri", Kategori = "Veri Giriş İşlemleri", Aciklama = "")]
        public const string RaporTipleri = "Rapor Tipleri";
        [RoleAttribute(GorunurAdi = "Rapor Tipleri Kayıt", Kategori = "Veri Giriş İşlemleri", Aciklama = "")]
        public const string RaporTipleriKayit = "Rapor Tipleri Kayıt";




        [MenuAttribute(BagliMenuID = 84000, MenuAdi = "Birim Raporları", MenuCssClass = "fa fa-bar-chart-o", MenuUrl = "RprBirimRaporlari/Index", DilCeviriYap = false, SiraNo = 1)]
        [RoleAttribute(GorunurAdi = "Birim Raporları", Kategori = "Veri Giriş Raporları", Aciklama = "")]
        public const string BirimRaporlari = "Birim Raporları";
        [MenuAttribute(BagliMenuID = 84000, MenuAdi = "Kurum Raporları", MenuCssClass = "fa fa-bar-chart-o", MenuUrl = "RprKurumRaporlari/Index", DilCeviriYap = false, SiraNo = 2)]
        [RoleAttribute(GorunurAdi = "Kurum Raporları", Kategori = "Veri Giriş Raporları", Aciklama = "")]
        public const string KurumRaporlari = "Kurum Raporları";

        [MenuAttribute(BagliMenuID = 85000, MenuAdi = "Kullanıcılar", MenuCssClass = "fa fa-user-o", MenuUrl = "Kullanicilar/Index", DilCeviriYap = false, SiraNo = 1)]
        [RoleAttribute(GorunurAdi = "Kullanıcılar", Kategori = "Kullanıcı İşlemleri", Aciklama = "")]
        public const string Kullanicilar = "Kullanıcılar Listesi";
        [RoleAttribute(GorunurAdi = "Kullanıcılar Kayıt", Kategori = "Kullanıcı İşlemleri", Aciklama = "")]
        public const string KullaniciKayit = "Kullanıcı Kayıt";
        [RoleAttribute(GorunurAdi = "Online Kullanıcıları Gör", Kategori = "Kullanıcı İşlemleri", Aciklama = "")]
        public const string KullanicilarOnlineList = "Online Kullanıcıları Gör";


        [MenuAttribute(BagliMenuID = 85000, MenuAdi = "Yetki Grupları", MenuCssClass = "fa fa-lock", MenuUrl = "YetkiGruplari/Index", DilCeviriYap = false, SiraNo = 2)]
        [RoleAttribute(GorunurAdi = "Yetki Grupları", Kategori = "Kullanıcı İşlemleri", Aciklama = "")]
        public const string YetkiGruplari = "Yetki Grupları";


        [MenuAttribute(BagliMenuID = 85000, MenuAdi = "Birimler", MenuCssClass = "fa fa-home", MenuUrl = "Birimler/Index", DilCeviriYap = false, SiraNo = 3)]
        [RoleAttribute(GorunurAdi = "Birimler", Kategori = "Kullanıcı İşlemleri", Aciklama = "")]
        public const string Birimler = "Birimler";
        //[MenuAttribute(BagliMenuID = 85000, MenuAdi = "Ünvanlar", MenuCssClass = "fa fa-list-alt", MenuUrl = "Unvanlar/Index", DilCeviriYap = false, SiraNo = 4)]
        //[RoleAttribute(GorunurAdi = "Ünvanlar", Kategori = "Tanımlamalar", Aciklama = "")]
        public const string Unvanlar = "Ünvanlar";





        [MenuAttribute(BagliMenuID = 85500, MenuAdi = "Dönem İşlemleri", MenuCssClass = "fa fa-clock-o", MenuUrl = "BFRDonemIslemleri/Index", DilCeviriYap = false, SiraNo = 1)]
        [RoleAttribute(GorunurAdi = "Dönem İşlemleri", Kategori = "Bütçe Hazırlık Formları", Aciklama = "")]
        public const string BFRDonemIslemleri = "BFR Dönem İşlemleri";
        [RoleAttribute(GorunurAdi = "Dönem İşlemleri Kayıt Yetkisi", Kategori = "Bütçe Hazırlık Formları", Aciklama = "")]
        public const string BFRDonemIslemleriKayitYetkisi = "BFR Dönem İşlemleri Kayıt Yetkisi";


        [MenuAttribute(BagliMenuID = 85500, MenuAdi = "Form Yükle", MenuCssClass = "fa fa-cloud-upload", MenuUrl = "BFRFormYukle/Index", DilCeviriYap = false, SiraNo = 4)]
        [RoleAttribute(GorunurAdi = "Form Yükle", Kategori = "Bütçe Hazırlık Formları", Aciklama = "")]
        public const string BFRFormYukle = "BFR Yükle";
        [RoleAttribute(GorunurAdi = "Form Yükle Kayıt Yetkisi", Kategori = "Bütçe Hazırlık Formları", Aciklama = "")]
        public const string BFRFormYukleKayitYetkisi = "BFR Yukle Kayıt Yetkisi";

        [MenuAttribute(BagliMenuID = 85500, MenuAdi = "Form Oluştur", MenuCssClass = "fa fa-files-o", MenuUrl = "BFRFormOlustur/Index", DilCeviriYap = false, SiraNo = 7)]
        [RoleAttribute(GorunurAdi = "Form Oluştur", Kategori = "Bütçe Hazırlık Formları", Aciklama = "")]
        public const string BFRFormOluştur = "BFR Form Oluştur";
        [RoleAttribute(GorunurAdi = "Form Oluştur Kayıt Yetkisi", Kategori = "Bütçe Hazırlık Formları", Aciklama = "")]
        public const string BFRFormOluşturKayitYetkisi = "BFR Form Oluştur Kayıt Yetkisi";


        [MenuAttribute(BagliMenuID = 85700, MenuAdi = "Dönem İşlemleri", MenuCssClass = "fa fa-clock-o", MenuUrl = "FRDonemIslemleri/Index", DilCeviriYap = false, SiraNo = 1)]
        [RoleAttribute(GorunurAdi = "Dönem İşlemleri", Kategori = "Faaliyet Raporları", Aciklama = "")]
        public const string FRDonemIslemleri = "FR Dönem İşlemleri";
        [RoleAttribute(GorunurAdi = "Dönem İşlemleri Kayıt Yetkisi", Kategori = "Faaliyet Raporları", Aciklama = "")]
        public const string FRDonemIslemleriKayitYetkisi = "FR Dönem İşlemleri Kayıt Yetkisi";


        [MenuAttribute(BagliMenuID = 85700, MenuAdi = "Form Yükle", MenuCssClass = "fa fa-cloud-upload", MenuUrl = "FRFormYukle/Index", DilCeviriYap = false, SiraNo = 4)]
        [RoleAttribute(GorunurAdi = "Form Yükle", Kategori = "Faaliyet Raporları", Aciklama = "")]
        public const string FRFormYukle = "Fr Yükle";
        [RoleAttribute(GorunurAdi = "Form Yükle Kayıt Yetkisi", Kategori = "Faaliyet Raporları", Aciklama = "")]
        public const string FRFormYukleKayitYetkisi = "Form Yukle Kayıt Yetkisi";

        [MenuAttribute(BagliMenuID = 85700, MenuAdi = "Form Oluştur", MenuCssClass = "fa fa-files-o", MenuUrl = "FRFormOlustur/Index", DilCeviriYap = false, SiraNo = 7)]
        [RoleAttribute(GorunurAdi = "Form Oluştur", Kategori = "Faaliyet Raporları", Aciklama = "")]
        public const string FRFormOluştur = "Fr Form Oluştur";
        [RoleAttribute(GorunurAdi = "Form Oluştur Kayıt Yetkisi", Kategori = "Faaliyet Raporları", Aciklama = "")]
        public const string FRFormOluşturKayitYetkisi = "Form Oluştur Kayıt Yetkisi";


        [MenuAttribute(BagliMenuID = 86500, MenuAdi = "Dönem İşlemleri", MenuCssClass = "fa fa-clock-o", MenuUrl = "GTDonemIslemleri/Index", DilCeviriYap = false, SiraNo = 1)]
        [RoleAttribute(GorunurAdi = "Dönem İşlemleri", Kategori = "Gelir Takibi İşlemleri", Aciklama = "")]
        public const string GTDonemIslemleri = "GT Dönem İşlemleri";
        [RoleAttribute(GorunurAdi = "Dönem İşlemleri Kayıt Yetkisi", Kategori = "Gelir Takibi İşlemleri", Aciklama = "")]
        public const string GTDonemIslemleriKayitYetkisi = "GT Dönem İşlemleri Kayıt Yetkisi";

        [MenuAttribute(BagliMenuID = 86500, MenuAdi = "Veri Girişi", MenuCssClass = "fa fa-try", MenuUrl = "GTVeriGirisi/Index", DilCeviriYap = false, SiraNo = 3)]
        [RoleAttribute(GorunurAdi = "Veri Girişi", Kategori = "Gelir Takibi İşlemleri", Aciklama = "")]
        public const string GTVeriGirisi = "GT Veri Girişi";
        [RoleAttribute(GorunurAdi = "Veri Girişi Kayıt Yetkisi", Kategori = "Gelir Takibi İşlemleri", Aciklama = "")]
        public const string GTVeriGirisiKayitYetkisi = "GT Veri Girişi Kayıt Yetkisi";
        [RoleAttribute(GorunurAdi = "Veri Girişi Onay Yetkisi", Kategori = "Gelir Takibi İşlemleri", Aciklama = "")]
        public const string GTVeriGirisiOnayYetkisi = "GT Veri Girişi Onay Yetkisi";
        [RoleAttribute(GorunurAdi = "Veri Girişi Yevmiye Eklendi Yetkisi", Kategori = "Gelir Takibi İşlemleri", Aciklama = "")]
        public const string GTVeriGirisiYevmiyeEklendiYetkisi = "GT Veri Girişi Yevmiye Eklendi Yetkisi";

        [MenuAttribute(BagliMenuID = 86500, MenuAdi = "Birimler", MenuCssClass = "fa fa-list-alt", MenuUrl = "GTBirimler/Index", DilCeviriYap = false, SiraNo = 6)]
        [RoleAttribute(GorunurAdi = "Birimler", Kategori = "Gelir Takibi İşlemleri", Aciklama = "")]
        public const string GTBirimler = "GT Birimler";
        [RoleAttribute(GorunurAdi = "Birimler Kayıt", Kategori = "Gelir Takibi İşlemleri", Aciklama = "")]
        public const string GTBirimlerKayit = "GT Birimler Kayıt";

        [MenuAttribute(BagliMenuID = 86500, MenuAdi = "Hesap Kodları", MenuCssClass = "fa fa-list-alt", MenuUrl = "GTHesapKodlari/Index", DilCeviriYap = false, SiraNo = 9)]
        [RoleAttribute(GorunurAdi = "Hesap Kodları", Kategori = "Gelir Takibi İşlemleri", Aciklama = "")]
        public const string GTHesapKodlari = "GT Hesap Kodları";
        [RoleAttribute(GorunurAdi = "Hesap Kodları Kayıt", Kategori = "Gelir Takibi İşlemleri", Aciklama = "")]
        public const string GTHesapKodlariKayit = "GT Hesap Kodları Kayıt";

        [MenuAttribute(BagliMenuID = 86500, MenuAdi = "Hesap Numaraları", MenuCssClass = "fa fa-list-alt", MenuUrl = "GTHesapNumaralari/Index", DilCeviriYap = false, SiraNo = 12)]
        [RoleAttribute(GorunurAdi = "Hesap Numaraları", Kategori = "Gelir Takibi İşlemleri", Aciklama = "")]
        public const string GTHesapNumaralari = "GT Hesap Numaraları";
        [RoleAttribute(GorunurAdi = "Hesap Numaraları Kayıt", Kategori = "Gelir Takibi İşlemleri", Aciklama = "")]
        public const string GTHesapNumaralariKayit = "GT Hesap Numaraları Kayıt";

        [MenuAttribute(BagliMenuID = 86500, MenuAdi = "Vergi Kimlik Numaraları", MenuCssClass = "fa fa-list-alt", MenuUrl = "GTVergiKimlikNumaralari/Index", DilCeviriYap = false, SiraNo = 15)]
        [RoleAttribute(GorunurAdi = "Vergi Kimlik Numaraları", Kategori = "Gelir Takibi İşlemleri", Aciklama = "")]
        public const string GTVergiKimlikNumaralari = "GT Hesap Numaraları";
        [RoleAttribute(GorunurAdi = "Vergi Kimlik Numaraları Kayıt", Kategori = "Gelir Takibi İşlemleri", Aciklama = "")]
        public const string GTVergiKimlikNumaralariKayit = "GT Vergi Kimlik Numaraları Kayıt";



        [MenuAttribute(BagliMenuID = 100000, MenuAdi = "Mail Şablonları", MenuCssClass = "fa fa-pencil", MenuUrl = "MailSablonlari/Index", DilCeviriYap = false, SiraNo = 3)]
        [RoleAttribute(GorunurAdi = "Mail Şablonları", Kategori = "Sistem", Aciklama = "")]
        public const string MailSablonlari = "Mail Şablonları";

        [MenuAttribute(BagliMenuID = 100000, MenuAdi = "Mail Şablonları (Sistem)", MenuCssClass = "fa fa-gear", MenuUrl = "MailSablonlariSistem/Index", DilCeviriYap = false, SiraNo = 6)]
        [RoleAttribute(GorunurAdi = "Mail Şablonları (Sistem)", Kategori = "Sistem", Aciklama = "")]
        public const string MailSablonlariSistem = "Mail Şablonları (Sistem)";

        [MenuAttribute(BagliMenuID = 100000, MenuAdi = "Mail İşlemleri", MenuCssClass = "fa fa-envelope", MenuUrl = "MailIslemleri/Index", DilCeviriYap = false, SiraNo = 9)]
        [RoleAttribute(GorunurAdi = "Mail İşlemleri", Kategori = "Sistem", Aciklama = "")]
        public const string MailIslemleri = "Mail İşlemleri";

        [MenuAttribute(BagliMenuID = 100000, MenuAdi = "Mesaj Kategorileri", MenuCssClass = "fa fa-pencil", MenuUrl = "MesajKategorileri/Index", DilCeviriYap = false, SiraNo = 12)]
        [RoleAttribute(GorunurAdi = "Mesaj Kategorileri", Kategori = "Sistem", Aciklama = "")]
        public const string MesajlarKategorileri = "Mesaj Kategorileri";

        [MenuAttribute(BagliMenuID = 100000, MenuAdi = "Gelen Mesajlar", MenuCssClass = "fa fa-envelope", MenuUrl = "Mesajlar/Index", DilCeviriYap = false, SiraNo = 15)]
        [RoleAttribute(GorunurAdi = "Gelen Mesajlar", Kategori = "Sistem", Aciklama = "")]
        public const string Mesajlar = "Gelen Mesajlar";

        [MenuAttribute(BagliMenuID = 100000, MenuAdi = "Duyurular", MenuCssClass = "fa fa-bullhorn", MenuUrl = "Duyurular/Index", DilCeviriYap = false, SiraNo = 18)]
        [RoleAttribute(GorunurAdi = "Duyurular", Kategori = "Sistem", Aciklama = "")]
        public const string Duyurular = "Duyurular";

        [MenuAttribute(BagliMenuID = 100000, MenuAdi = "Sistem Ayarları", MenuCssClass = "fa fa-puzzle-piece", MenuUrl = "SistemAyarlari/Index", DilCeviriYap = false, SiraNo = 21)]
        [RoleAttribute(GorunurAdi = "Sistem Ayarları", Kategori = "Sistem", Aciklama = "")]
        public const string SistemAyarlari = "Sistem Ayarları";

        [MenuAttribute(BagliMenuID = 100000, MenuAdi = "Sistem Bilgilendirme", MenuCssClass = "fa fa-info", MenuUrl = "SistemBilgilendirme/Index", DilCeviriYap = false, SiraNo = 24)]
        [RoleAttribute(GorunurAdi = "Sistem Bilgilendirme", Kategori = "Sistem", Aciklama = "")]
        public const string SistemBilgilendirme = "Sistem Bilgilendirme";


    }
}