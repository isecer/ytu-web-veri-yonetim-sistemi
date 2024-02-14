using Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WebApp.Business;
using WebApp.Models;
using WebApp.Utilities.Extensions;

namespace WebApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static void Log(string log)
        {
            System.IO.File.AppendAllText(@"C:\inetpub\wwwroot\VysWeb\Log\log.txt", log + "\r\n");
        }


        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BiskaUtil.Membership.OnRequireUserIdentity += Membership_OnRequireUserIdentity;
            BiskaUtil.SystemInformation.OnEvent += SystemInformation_OnEvent;
            RollerBus.UpdateRoles();
            MenulerBus.UpdateMenus();
            OnlineUsers.users = new List<OnlineUser>();
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            if (exception == null && Context.AllErrors != null && Context.AllErrors.Length > 0)
            {
                exception = Context.AllErrors[0];
            }

            Response.Clear();
            var routeData = new RouteData();
            if (exception == null)
            {

                routeData.Values.Add("controller", "Home");
                routeData.Values.Add("action", "Index");
            }
            else //It's an Http Exception, Let's handle it.
            {
                var errCode = HttpContext.Current.Response.StatusCode;
                IController errorController;
                if (errCode == HttpDurumKod.NotFound || errCode == HttpDurumKod.Unauthorized)
                {
                    var url = HttpContext.Current.Request.Url;
                    routeData.Values.Add("error", url);
                    routeData.Values.Add("ErrC", errCode);
                    errorController = new Controllers.AppEventController();
                    routeData.Values.Add("controller", "AppEvent");
                    routeData.Values.Add("action", "PageNotFound");
                    Response.TrySkipIisCustomErrors = true;
                    Server.ClearError();
                    errorController.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
                }
                else
                {
                    //routeData.Values.Add("controller", "Home");
                    //routeData.Values.Add("action", "Index");
                    errorController = new Controllers.AppEventController();
                    var url = HttpContext.Current.Request.Url;
                    routeData.Values.Add("error", url);
                    routeData.Values.Add("ErrC", errCode);
                    routeData.Values.Add("controller", "AppEvent");
                    routeData.Values.Add("action", "Error");
                    routeData.Values.Add("exception", exception);

                    Response.TrySkipIisCustomErrors = true;
                    Server.ClearError();
                    errorController.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
                }

            }
        }
        protected void Application_EndRequest(Object sender, EventArgs e)
        {
        }
        void SystemInformation_OnEvent(BiskaUtil.SystemInformation info)
        {
            SistemBilgilendirmeBus.AddMessage(info);
        }

        void Membership_OnRequireUserIdentity(string userName, ref BiskaUtil.UserIdentity userIdentity)
        {
            userIdentity = UserBus.GetUserIdentity(userName);

        }
        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            BiskaUtil.UserIdentity.SetCurrent();
            var session = HttpContext.Current.Session;
            if (session == null) return;
            var uniqueId = Session["UserId"].ToStrObj();
            if (uniqueId == null) return;
            var usr = OnlineUsers.users.FirstOrDefault(p => p.UniqueId == uniqueId);
            if (usr == null) return;
            var platform = HttpContext.Current.Request.Browser.IsMobileDevice ? HttpContext.Current.Request.Browser.MobileDeviceManufacturer + " " + HttpContext.Current.Request.Browser.MobileDeviceModel : HttpContext.Current.Request.Browser.Platform;
            var browser = HttpContext.Current.Request.Browser.Browser;
            var version = HttpContext.Current.Request.Browser.Version;
            if (User.Identity.IsAuthenticated)
            {
                if (usr.IsYetkiyeniye)
                {
                    BiskaUtil.UserIdentity.SetCurrent(usr.UserName);
                    usr.IsYetkiyeniye = false;
                }
                var user = UserBus.GetUser();
                usr.KullaniciID = user.KullaniciID;
                usr.Name = user.Ad + " " + user.Soyad;
                usr.UserName = user.KullaniciAdi;
                usr.Platform = platform;
                usr.Browser = browser;
                usr.Version = version;
                usr.YetkiGrupAdi = user.YetkiGrupAdi;
                usr.ResimAdi = user.ResimAdi.ToKullaniciResim();
                usr.IsAuthenticated = true;
            }
            else
            {
                usr.Name = "Misafir";
                usr.ResimAdi = "".ToKullaniciResim();
                usr.YetkiGrupAdi = "";
                usr.Platform = platform;
                usr.Browser = browser;
                usr.Version = version;
                usr.IsAuthenticated = false;
            }

        }

        private void Session_Start(object sender, EventArgs e)
        {
            var uniqueId = Guid.NewGuid().ToString();
            Session["UserId"] = uniqueId;
            OnlineUsers.AddUser(uniqueId, null);

        }

        void Session_End(object sender, EventArgs e)
        {
            if (Session["UserId"] == null) return;
            var uniqueId = Session["UserId"].ToString();
            var oUser = OnlineUsers.users.FirstOrDefault(p => p.UniqueId == uniqueId);
            if (oUser?.KullaniciID != null)
            {
                using (var db = new VysDBEntities())
                {
                    var kul = db.Kullanicilars.FirstOrDefault(p => p.KullaniciID == oUser.KullaniciID);
                    if (kul != null)
                    {
                        kul.LastLogonDate = DateTime.Now;
                        db.SaveChanges();
                    }
                }

            }
            OnlineUsers.RemoveUser(uniqueId);
        }
    }
}
