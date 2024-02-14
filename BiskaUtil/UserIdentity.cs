using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace BiskaUtil
{

    [Serializable()]
    public class UserIdentity : IIdentity
    {
        public enum GenderType { None = 0, Male = 1, Female = 2 }
        public string AuthenticationType => "Forms";
        private bool isAuthenticated = true;
        public bool IsAuthenticated => isAuthenticated;
        private string userName;
        public string Name => userName;
        public bool IsAdmin { get; set; }
        public int Id { get; set; }
        public Guid UserKey { get; set; }
        public int BirimId { get; set; }
        public int YetkiGrupId { get; set; }
        public string AdSoyad { get; set; }
        public string EMail { get; set; }
        public string Description { get; set; }
        public bool IsActiveDirectoryUser { get; set; }
        public bool? IsActiveDirectoryImpersonateWorking { get; set; }
        public string ImagePath { get; set; }
        public Dictionary<string, List<int>> TableRollId { get; set; }
        public List<SelectedTableRoll> SelectedTableRoll { get; set; }

        private Dictionary<string, object> informations = new Dictionary<string, object>();
        public Dictionary<string, object> Informations
        {
            get => informations;
            set => informations = value;
        }

        private List<string> roles = new List<string>();
        public List<string> Roles
        {
            get => roles;
            set => roles = value;
        }
        public UserIdentity(string name)
        {
            userName = name;
        }
        public UserIdentity(string name, bool isAuthenticated)
        {
            userName = name;
            this.isAuthenticated = isAuthenticated;
        }
        public UserIdentity(string name, string[] roles)
        {
            userName = name;
            if (roles != null)
                this.Roles.AddRange(roles);
        }
        public UserPrincipal ToPrincipal()
        {
            return new UserPrincipal(this);
        }


        public bool HasToChahgePassword { get; set; }
        public static string Ip
        {
            get
            {
                try
                {
                    string ip;
                    var forwarderFor = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (string.IsNullOrWhiteSpace(forwarderFor) || forwarderFor.ToLower().Contains("unknown"))
                        ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                    else if (forwarderFor.Contains(","))
                    {
                        ip = forwarderFor.Substring(0, forwarderFor.IndexOf(",", StringComparison.Ordinal));
                    }
                    else if (forwarderFor.Contains(";"))
                    {
                        ip = forwarderFor.Substring(0, forwarderFor.IndexOf(";", StringComparison.Ordinal));
                    }
                    else ip = forwarderFor;
                    var len = ip.Length > 30 ? 30 : ip.Length;
                    return ip.Substring(0, len).Trim();
                }
                catch
                {
                    return "";
                }
            }
        }

        public void Impersonate()
        {
            #region Impersonate

            HttpContext.Current.User = this.ToPrincipal();
            #endregion
        }
        public static void SetUserIdentityOnSession(HttpSessionStateBase session)
        {
            if (session == null)
            {
                SetCurrent();
            }
            else
            {
                if (HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    UserIdentity kimlik;
                    if ((session["UserIdentity"] != null))
                    {
                        kimlik = (UserIdentity)session["UserIdentity"];
                        kimlik.Impersonate();
                    }
                    else if (HttpContext.Current.User != null)
                    {
                        if (!(HttpContext.Current.User.Identity is NotAuthenticatedUser))
                        {
                            //kimlik = AccountModel.GetKimlik(HttpContext.Current.User.Identity.Name);
                            kimlik = Membership.GetUserIdentity(HttpContext.Current.User.Identity.Name);
                            if (kimlik.Id > 0)
                            {
                                kimlik.Impersonate();
                                session["Kimlik"] = kimlik;
                            }
                            else
                            {
                                session["Kimlik"] = null;
                                FormsAuthenticationUtil.SignOut();
                                if (HttpContext.Current != null)
                                {
                                    try
                                    {
                                        if (HttpContext.Current.Session != null) HttpContext.Current.Session.Abandon();
                                        // clear authentication cookie
                                        HttpCookie cookie1 = new HttpCookie(FormsAuthentication.FormsCookieName, "");
                                        cookie1.Expires = DateTime.Now.AddYears(-1);
                                        HttpContext.Current.Response.Cookies.Add(cookie1);

                                        // clear session cookie (not necessary for your current problem but i would recommend you do it anyway)
                                        HttpCookie cookie2 = new HttpCookie("ASP.NET_SessionId", "");
                                        cookie2.Expires = DateTime.Now.AddYears(-1);
                                        HttpContext.Current.Response.Cookies.Add(cookie2);
                                    }
                                    catch
                                    {
                                        // ignored
                                    }
                                }
                                HttpContext.Current.User = new GenericPrincipal(new NotAuthenticatedUser(), new string[0]);
                                //IPrincipal user = HttpContext.Current.User;
                            }
                        }
                    }
                }
            }
        }
        public static void SetPageSelectedTableId(string roleName, string tableIdName, int? tableId)
        {
            if (Current.SelectedTableRoll != null)
            {
                var sRol = Current.SelectedTableRoll.FirstOrDefault(p => p.TableIdName == tableIdName && p.RoleName == roleName);
                if (sRol != null)
                {
                    sRol.SelectedId = tableId;
                }
                else
                {
                    Current.SelectedTableRoll.Add(new SelectedTableRoll { TableIdName = tableIdName, RoleName = roleName, SelectedId = tableId });
                }
            }
            //  UserIdentity.Current.SelectedTableRollID
        }
        public static int? GetPageSelectedTableId(string roleName, string tableIdName)
        {
            var sRol = Current.SelectedTableRoll.FirstOrDefault(p => p.TableIdName == tableIdName && p.RoleName == roleName);
            return sRol?.SelectedId;
        }
        public static List<SelectedTableRoll> GetSelectedTableIDs(string roleName, int? tableId)
        {
            return Current.SelectedTableRoll != null ? Current.SelectedTableRoll.Where(p => p.TableId == (tableId ?? p.TableId) && p.RoleName == roleName).ToList() : new List<SelectedTableRoll>();
        }
        public static void SetCurrent(string userName = null)
        {
            if (HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
            {
                UserIdentity kimlik;
                HttpSessionState session = HttpContext.Current.Session;
                if (userName != null)
                {
                    kimlik = Membership.GetUserIdentity(userName);
                    if (kimlik.Id > 0)
                    {
                        kimlik.Impersonate();
                        session["UserIdentity"] = kimlik;
                    }
                    else
                    {
                        session["UserIdentity"] = null;
                        //IPrincipal user = HttpContext.Current.User;
                    }
                }
                else if ((HttpContext.Current.Session != null) && (HttpContext.Current.Session["UserIdentity"] != null))
                {
                    kimlik = (UserIdentity)session["UserIdentity"];
                    kimlik.Impersonate();

                }
                else if (HttpContext.Current.Session != null && HttpContext.Current.User != null)
                {
                    if (!(HttpContext.Current.User.Identity is NotAuthenticatedUser))
                    {
                        //kimlik = AccountModel.GetKimlik(HttpContext.Current.User.Identity.Name);
                        kimlik = Membership.GetUserIdentity(HttpContext.Current.User.Identity.Name);
                        if (kimlik.Id > 0)
                        {
                            kimlik.Impersonate();
                            session["UserIdentity"] = kimlik;
                        }
                        else
                        {
                            session["UserIdentity"] = null;
                            FormsAuthenticationUtil.SignOut();
                            HttpContext.Current.User = new GenericPrincipal(new NotAuthenticatedUser(), new string[0]);
                            //IPrincipal user = HttpContext.Current.User;
                        }
                    }
                }
            }
        }

        public static UserIdentity Current
        {
            get
            {
                if (HttpContext.Current.User.Identity is UserIdentity)
                    return (UserIdentity)HttpContext.Current.User.Identity;
                if (HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
                    return Membership.GetUserIdentity(HttpContext.Current.User.Identity.Name);
                return new UserIdentity("None", false);
            }
        }

    }
    [Serializable()]
    public class UserPrincipal : IPrincipal
    {
        private UserIdentity kimlik;
        public IIdentity Identity => kimlik;

        public bool IsInRole(string role)
        {
            return kimlik.Roles.IndexOf(role) >= 0;
        }
        internal UserPrincipal(UserIdentity kimlik)
        {
            this.kimlik = kimlik;
        }
    }
}