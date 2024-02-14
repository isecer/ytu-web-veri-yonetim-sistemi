using System.Web;

namespace WebApp.Utilities.SystemSetting
{
    public static class GlobalSistemSetting
    {
        public static string GetRoot()
        {
            var root = HttpRuntime.AppDomainAppVirtualPath;
            root = root.EndsWith("/") ? root : root + "/";
            return root;
        }

        public static string Tuz => "@BİSKAmcumu";
        public static int UniversiteYtuKod => 67;
        public static int SystemDefaultAdminKullaniciId => 1;
        public static int PageTableRowSize = 15;
    }
}