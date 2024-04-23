using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System;
using System.Web;
using Database;

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

        public static string ConnectionInfo()
        {
            try
            {
                // Bağlantı dizesi
                var connectionString = ConfigurationManager.ConnectionStrings[nameof(VysDBEntities)].ConnectionString;
                // Entity Framework bağlantı nesnesi oluştur
                EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder(connectionString);

                // Entity Framework model bağlantı nesnesi oluştur
                var providerConnectionString = entityBuilder.ProviderConnectionString;
                var providerBuilder = new SqlConnectionStringBuilder(providerConnectionString);

                // Veri kaynağı adını al
                var dataSource = providerBuilder.DataSource == "." ? "localhost" : providerBuilder.DataSource;

                return $"{dataSource}";

            }
            catch (Exception e)
            {
                return "Connection Info Error: " + e.Message;
            }

        }
    }
}