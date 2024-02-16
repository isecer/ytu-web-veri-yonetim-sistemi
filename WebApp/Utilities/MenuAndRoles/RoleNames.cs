using BiskaUtil;

namespace WebApp.Utilities.MenuAndRoles
{
    public abstract class RoleNames : IRoleName, IMenu
    {

        [Menu(BagliMenuID = 80000, MenuAdi = "Süreç İşlemleri", MenuCssClass = "fa fa-clock-o", MenuUrl = "SurecIslemleri/Index", DilCeviriYap = false, SiraNo = 1)]
        [Role(GorunurAdi = "Süreç İşlemleri", Kategori = "Veri Giriş İşlemleri", Aciklama = "")]
        public const string SurecIslemleri = "Süreç İşlemleri";
        [Role(GorunurAdi = "Süreç İşlemleri Kayıt Yetkisi", Kategori = "Veri Giriş İşlemleri", Aciklama = "")]
        public const string SurecIslemleriKayitYetkisi = "Süreç İşlemleri Kayıt Yetkisi";

        [Menu(BagliMenuID = 80000, MenuAdi = "Veri Girişi", MenuCssClass = "fa fa-file-text-o", MenuUrl = "VeriGirisi/Index", DilCeviriYap = false, SiraNo = 3)]
        [Role(GorunurAdi = "Veri Girişi", Kategori = "Veri Giriş İşlemleri", Aciklama = "")]
        public const string VeriGirisi = "Veri Girişi";

        [Role(GorunurAdi = "Veri Girişi Planlanan Hedef Kayıt Yetkisi", Kategori = "Veri Giriş İşlemleri", Aciklama = "")]
        public const string VeriGirisiPlanlananHedefKayitYetkisi = "Veri Girişi Planlanan Hedef Kayıt Yetkisi";

        [Role(GorunurAdi = "Veri Girişi Kayıt Yetkisi", Kategori = "Veri Giriş İşlemleri", Aciklama = "")]
        public const string VeriGirisiKayitYetkisi = "Veri Girişi Kayıt Yetkisi";

        [Role(GorunurAdi = "Veri Girişi Onay Yetkisi", Kategori = "Veri Giriş İşlemleri", Aciklama = "")]
        public const string VeriGirisiOnayYetkisi = "Veri Girişi Onay Yetkisi";



        [Menu(BagliMenuID = 80000, MenuAdi = "Madde Türleri", MenuCssClass = "fa fa-list-alt", MenuUrl = "MaddeTurleri/Index", DilCeviriYap = false, SiraNo = 5)]
        [Role(GorunurAdi = "Madde Türleri", Kategori = "Veri Giriş İşlemleri", Aciklama = "")]
        public const string MaddeTurler = "Madde Türleri";
        [Role(GorunurAdi = "Madde Türleri Kayıt", Kategori = "Veri Giriş İşlemleri", Aciklama = "")]
        public const string MaddeTurleriKayit = "Madde Türleri Kayıt";


        [Menu(BagliMenuID = 80000, MenuAdi = "Maddeler", MenuCssClass = "fa fa-list-alt", MenuUrl = "Maddeler/Index", DilCeviriYap = false, SiraNo = 8)]
        [Role(GorunurAdi = "Maddeler", Kategori = "Veri Giriş İşlemleri", Aciklama = "")]
        public const string Maddeler = "Maddeler";
        [Role(GorunurAdi = "Maddeler Kayıt", Kategori = "Veri Giriş İşlemleri", Aciklama = "")]
        public const string MaddelerKayit = "Maddeler Kayıt";

        [Menu(BagliMenuID = 80000, MenuAdi = "Faaliyetler", MenuCssClass = "fa fa-list-alt", MenuUrl = "Faaliyetler/Index", DilCeviriYap = false, SiraNo = 11)]
        [Role(GorunurAdi = "Faaliyetler", Kategori = "Veri Giriş İşlemleri", Aciklama = "")]
        public const string Faaliyetler = "Faaliyetler";
        [Role(GorunurAdi = "Faaliyetler Kayıt", Kategori = "Veri Giriş İşlemleri", Aciklama = "")]
        public const string FaaliyetlerKayit = "Faaliyetler Kayıt";

        [Menu(BagliMenuID = 84000, MenuAdi = "Rapor Tipleri", MenuCssClass = "fa fa-list-alt", MenuUrl = "RaporTipleri/Index", DilCeviriYap = false, SiraNo = 5)]
        [Role(GorunurAdi = "Rapor Tipleri", Kategori = "Veri Giriş İşlemleri", Aciklama = "")]
        public const string RaporTipleri = "Rapor Tipleri";
        [Role(GorunurAdi = "Rapor Tipleri Kayıt", Kategori = "Veri Giriş İşlemleri", Aciklama = "")]
        public const string RaporTipleriKayit = "Rapor Tipleri Kayıt";




        [Menu(BagliMenuID = 84000, MenuAdi = "Birim Raporları", MenuCssClass = "fa fa-bar-chart-o", MenuUrl = "RprBirimRaporlari/Index", DilCeviriYap = false, SiraNo = 1)]
        [Role(GorunurAdi = "Birim Raporları", Kategori = "Veri Giriş Raporları", Aciklama = "")]
        public const string BirimRaporlari = "Birim Raporları";
        [Menu(BagliMenuID = 84000, MenuAdi = "Kurum Raporları", MenuCssClass = "fa fa-bar-chart-o", MenuUrl = "RprKurumRaporlari/Index", DilCeviriYap = false, SiraNo = 2)]
        [Role(GorunurAdi = "Kurum Raporları", Kategori = "Veri Giriş Raporları", Aciklama = "")]
        public const string KurumRaporlari = "Kurum Raporları";

        [Menu(BagliMenuID = 85000, MenuAdi = "Kullanıcılar", MenuCssClass = "fa fa-user-o", MenuUrl = "Kullanicilar/Index", DilCeviriYap = false, SiraNo = 1)]
        [Role(GorunurAdi = "Kullanıcılar", Kategori = "Kullanıcı İşlemleri", Aciklama = "")]
        public const string Kullanicilar = "Kullanıcılar Listesi";
        [Role(GorunurAdi = "Kullanıcılar Kayıt", Kategori = "Kullanıcı İşlemleri", Aciklama = "")]
        public const string KullaniciKayit = "Kullanıcı Kayıt";
        [Role(GorunurAdi = "Online Kullanıcıları Gör", Kategori = "Kullanıcı İşlemleri", Aciklama = "")]
        public const string KullanicilarOnlineList = "Online Kullanıcıları Gör";
        [Role(GorunurAdi = "Kullanıcı Hesabına Geçme Yetkisi", Kategori = "Kullanıcı İşlemleri", Aciklama = "Bu yetki bir kullanıcı detayından kullanıcının hesabına geçebilmeyi sağlayan yetkidir.")]
        public const string KullaniciHesabinaGecmeYetkisi = "Kullanıcı Hesabına Geçme Yetkisi";


        [Menu(BagliMenuID = 85000, MenuAdi = "Yetki Grupları", MenuCssClass = "fa fa-lock", MenuUrl = "YetkiGruplari/Index", DilCeviriYap = false, SiraNo = 2)]
        [Role(GorunurAdi = "Yetki Grupları", Kategori = "Kullanıcı İşlemleri", Aciklama = "")]
        public const string YetkiGruplari = "Yetki Grupları";


        [Menu(BagliMenuID = 85000, MenuAdi = "Birimler", MenuCssClass = "fa fa-home", MenuUrl = "Birimler/Index", DilCeviriYap = false, SiraNo = 3)]
        [Role(GorunurAdi = "Birimler", Kategori = "Kullanıcı İşlemleri", Aciklama = "")]
        public const string Birimler = "Birimler";
        //[MenuAttribute(BagliMenuID = 85000, MenuAdi = "Ünvanlar", MenuCssClass = "fa fa-list-alt", MenuUrl = "Unvanlar/Index", DilCeviriYap = false, SiraNo = 4)]
        //[RoleAttribute(GorunurAdi = "Ünvanlar", Kategori = "Tanımlamalar", Aciklama = "")]
        public const string Unvanlar = "Ünvanlar";





        [Menu(BagliMenuID = 85500, MenuAdi = "Dönem İşlemleri", MenuCssClass = "fa fa-clock-o", MenuUrl = "BFRDonemIslemleri/Index", DilCeviriYap = false, SiraNo = 1)]
        [Role(GorunurAdi = "Dönem İşlemleri", Kategori = "Bütçe Hazırlık Formları", Aciklama = "")]
        public const string BFRDonemIslemleri = "BFR Dönem İşlemleri";
        [Role(GorunurAdi = "Dönem İşlemleri Kayıt Yetkisi", Kategori = "Bütçe Hazırlık Formları", Aciklama = "")]
        public const string BFRDonemIslemleriKayitYetkisi = "BFR Dönem İşlemleri Kayıt Yetkisi";


        [Menu(BagliMenuID = 85500, MenuAdi = "Form Yükle", MenuCssClass = "fa fa-cloud-upload", MenuUrl = "BFRFormYukle/Index", DilCeviriYap = false, SiraNo = 4)]
        [Role(GorunurAdi = "Form Yükle", Kategori = "Bütçe Hazırlık Formları", Aciklama = "")]
        public const string BFRFormYukle = "BFR Yükle";
        [Role(GorunurAdi = "Form Yükle Kayıt Yetkisi", Kategori = "Bütçe Hazırlık Formları", Aciklama = "")]
        public const string BFRFormYukleKayitYetkisi = "BFR Yukle Kayıt Yetkisi";

        [Menu(BagliMenuID = 85500, MenuAdi = "Form Oluştur", MenuCssClass = "fa fa-files-o", MenuUrl = "BFRFormOlustur/Index", DilCeviriYap = false, SiraNo = 7)]
        [Role(GorunurAdi = "Form Oluştur", Kategori = "Bütçe Hazırlık Formları", Aciklama = "")]
        public const string BFRFormOluştur = "BFR Form Oluştur";
        [Role(GorunurAdi = "Form Oluştur Kayıt Yetkisi", Kategori = "Bütçe Hazırlık Formları", Aciklama = "")]
        public const string BFRFormOluşturKayitYetkisi = "BFR Form Oluştur Kayıt Yetkisi";


        [Menu(BagliMenuID = 85700, MenuAdi = "Dönem İşlemleri", MenuCssClass = "fa fa-clock-o", MenuUrl = "FRDonemIslemleri/Index", DilCeviriYap = false, SiraNo = 1)]
        [Role(GorunurAdi = "Dönem İşlemleri", Kategori = "Faaliyet Raporları", Aciklama = "")]
        public const string FRDonemIslemleri = "FR Dönem İşlemleri";
        [Role(GorunurAdi = "Dönem İşlemleri Kayıt Yetkisi", Kategori = "Faaliyet Raporları", Aciklama = "")]
        public const string FRDonemIslemleriKayitYetkisi = "FR Dönem İşlemleri Kayıt Yetkisi";


        [Menu(BagliMenuID = 85700, MenuAdi = "Form Yükle", MenuCssClass = "fa fa-cloud-upload", MenuUrl = "FRFormYukle/Index", DilCeviriYap = false, SiraNo = 4)]
        [Role(GorunurAdi = "Form Yükle", Kategori = "Faaliyet Raporları", Aciklama = "")]
        public const string FRFormYukle = "Fr Yükle";
        [Role(GorunurAdi = "Form Yükle Kayıt Yetkisi", Kategori = "Faaliyet Raporları", Aciklama = "")]
        public const string FRFormYukleKayitYetkisi = "Form Yukle Kayıt Yetkisi";

        [Menu(BagliMenuID = 85700, MenuAdi = "Form Oluştur", MenuCssClass = "fa fa-files-o", MenuUrl = "FRFormOlustur/Index", DilCeviriYap = false, SiraNo = 7)]
        [Role(GorunurAdi = "Form Oluştur", Kategori = "Faaliyet Raporları", Aciklama = "")]
        public const string FRFormOluştur = "Fr Form Oluştur";
        [Role(GorunurAdi = "Form Oluştur Kayıt Yetkisi", Kategori = "Faaliyet Raporları", Aciklama = "")]
        public const string FRFormOluşturKayitYetkisi = "Form Oluştur Kayıt Yetkisi";


        [Menu(BagliMenuID = 86500, MenuAdi = "Dönem İşlemleri", MenuCssClass = "fa fa-clock-o", MenuUrl = "GTDonemIslemleri/Index", DilCeviriYap = false, SiraNo = 1)]
        [Role(GorunurAdi = "Dönem İşlemleri", Kategori = "Gelir Takibi İşlemleri", Aciklama = "")]
        public const string GTDonemIslemleri = "GT Dönem İşlemleri";
        [Role(GorunurAdi = "Dönem İşlemleri Kayıt Yetkisi", Kategori = "Gelir Takibi İşlemleri", Aciklama = "")]
        public const string GTDonemIslemleriKayitYetkisi = "GT Dönem İşlemleri Kayıt Yetkisi";

        [Menu(BagliMenuID = 86500, MenuAdi = "Veri Girişi", MenuCssClass = "fa fa-try", MenuUrl = "GTVeriGirisi/Index", DilCeviriYap = false, SiraNo = 3)]
        [Role(GorunurAdi = "Veri Girişi", Kategori = "Gelir Takibi İşlemleri", Aciklama = "")]
        public const string GTVeriGirisi = "GT Veri Girişi";
        [Role(GorunurAdi = "Veri Girişi Kayıt Yetkisi", Kategori = "Gelir Takibi İşlemleri", Aciklama = "")]
        public const string GTVeriGirisiKayitYetkisi = "GT Veri Girişi Kayıt Yetkisi";
        [Role(GorunurAdi = "Veri Girişi Onay Yetkisi", Kategori = "Gelir Takibi İşlemleri", Aciklama = "")]
        public const string GTVeriGirisiOnayYetkisi = "GT Veri Girişi Onay Yetkisi";
        [Role(GorunurAdi = "Veri Girişi Yevmiye Eklendi Yetkisi", Kategori = "Gelir Takibi İşlemleri", Aciklama = "")]
        public const string GTVeriGirisiYevmiyeEklendiYetkisi = "GT Veri Girişi Yevmiye Eklendi Yetkisi";

        [Menu(BagliMenuID = 86500, MenuAdi = "Birimler", MenuCssClass = "fa fa-list-alt", MenuUrl = "GTBirimler/Index", DilCeviriYap = false, SiraNo = 6)]
        [Role(GorunurAdi = "Birimler", Kategori = "Gelir Takibi İşlemleri", Aciklama = "")]
        public const string GTBirimler = "GT Birimler";
        [Role(GorunurAdi = "Birimler Kayıt", Kategori = "Gelir Takibi İşlemleri", Aciklama = "")]
        public const string GTBirimlerKayit = "GT Birimler Kayıt";

        [Menu(BagliMenuID = 86500, MenuAdi = "Hesap Kodları", MenuCssClass = "fa fa-list-alt", MenuUrl = "GTHesapKodlari/Index", DilCeviriYap = false, SiraNo = 9)]
        [Role(GorunurAdi = "Hesap Kodları", Kategori = "Gelir Takibi İşlemleri", Aciklama = "")]
        public const string GTHesapKodlari = "GT Hesap Kodları";
        [Role(GorunurAdi = "Hesap Kodları Kayıt", Kategori = "Gelir Takibi İşlemleri", Aciklama = "")]
        public const string GTHesapKodlariKayit = "GT Hesap Kodları Kayıt";

        [Menu(BagliMenuID = 86500, MenuAdi = "Hesap Numaraları", MenuCssClass = "fa fa-list-alt", MenuUrl = "GTHesapNumaralari/Index", DilCeviriYap = false, SiraNo = 12)]
        [Role(GorunurAdi = "Hesap Numaraları", Kategori = "Gelir Takibi İşlemleri", Aciklama = "")]
        public const string GTHesapNumaralari = "GT Hesap Numaraları";
        [Role(GorunurAdi = "Hesap Numaraları Kayıt", Kategori = "Gelir Takibi İşlemleri", Aciklama = "")]
        public const string GTHesapNumaralariKayit = "GT Hesap Numaraları Kayıt";

        [Menu(BagliMenuID = 86500, MenuAdi = "Vergi Kimlik Numaraları", MenuCssClass = "fa fa-list-alt", MenuUrl = "GTVergiKimlikNumaralari/Index", DilCeviriYap = false, SiraNo = 15)]
        [Role(GorunurAdi = "Vergi Kimlik Numaraları", Kategori = "Gelir Takibi İşlemleri", Aciklama = "")]
        public const string GTVergiKimlikNumaralari = "GT Hesap Numaraları";
        [Role(GorunurAdi = "Vergi Kimlik Numaraları Kayıt", Kategori = "Gelir Takibi İşlemleri", Aciklama = "")]
        public const string GTVergiKimlikNumaralariKayit = "GT Vergi Kimlik Numaraları Kayıt";



        [Menu(BagliMenuID = 100000, MenuAdi = "Mail Şablonları", MenuCssClass = "fa fa-pencil", MenuUrl = "MailSablonlari/Index", DilCeviriYap = false, SiraNo = 3)]
        [Role(GorunurAdi = "Mail Şablonları", Kategori = "Sistem", Aciklama = "")]
        public const string MailSablonlari = "Mail Şablonları";

        [Menu(BagliMenuID = 100000, MenuAdi = "Mail Şablonları (Sistem)", MenuCssClass = "fa fa-gear", MenuUrl = "MailSablonlariSistem/Index", DilCeviriYap = false, SiraNo = 6)]
        [Role(GorunurAdi = "Mail Şablonları (Sistem)", Kategori = "Sistem", Aciklama = "")]
        public const string MailSablonlariSistem = "Mail Şablonları (Sistem)";

        [Menu(BagliMenuID = 100000, MenuAdi = "Mail İşlemleri", MenuCssClass = "fa fa-envelope", MenuUrl = "MailIslemleri/Index", DilCeviriYap = false, SiraNo = 9)]
        [Role(GorunurAdi = "Mail İşlemleri", Kategori = "Sistem", Aciklama = "")]
        public const string MailIslemleri = "Mail İşlemleri";

        [Menu(BagliMenuID = 100000, MenuAdi = "Mesaj Kategorileri", MenuCssClass = "fa fa-pencil", MenuUrl = "MesajKategorileri/Index", DilCeviriYap = false, SiraNo = 12)]
        [Role(GorunurAdi = "Mesaj Kategorileri", Kategori = "Sistem", Aciklama = "")]
        public const string MesajlarKategorileri = "Mesaj Kategorileri";

        [Menu(BagliMenuID = 100000, MenuAdi = "Gelen Mesajlar", MenuCssClass = "fa fa-envelope", MenuUrl = "Mesajlar/Index", DilCeviriYap = false, SiraNo = 15)]
        [Role(GorunurAdi = "Gelen Mesajlar", Kategori = "Sistem", Aciklama = "")]
        public const string Mesajlar = "Gelen Mesajlar";

        [Menu(BagliMenuID = 100000, MenuAdi = "Duyurular", MenuCssClass = "fa fa-bullhorn", MenuUrl = "Duyurular/Index", DilCeviriYap = false, SiraNo = 18)]
        [Role(GorunurAdi = "Duyurular", Kategori = "Sistem", Aciklama = "")]
        public const string Duyurular = "Duyurular";

        [Menu(BagliMenuID = 100000, MenuAdi = "Sistem Ayarları", MenuCssClass = "fa fa-puzzle-piece", MenuUrl = "SistemAyarlari/Index", DilCeviriYap = false, SiraNo = 21)]
        [Role(GorunurAdi = "Sistem Ayarları", Kategori = "Sistem", Aciklama = "")]
        public const string SistemAyarlari = "Sistem Ayarları";

        [Menu(BagliMenuID = 100000, MenuAdi = "Sistem Bilgilendirme", MenuCssClass = "fa fa-info", MenuUrl = "SistemBilgilendirme/Index", DilCeviriYap = false, SiraNo = 24)]
        [Role(GorunurAdi = "Sistem Bilgilendirme", Kategori = "Sistem", Aciklama = "")]
        public const string SistemBilgilendirme = "Sistem Bilgilendirme";


    }
}