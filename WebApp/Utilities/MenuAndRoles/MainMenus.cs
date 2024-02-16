using BiskaUtil;

namespace WebApp.Utilities.MenuAndRoles
{
    public class MainMenus : IMenu
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
}