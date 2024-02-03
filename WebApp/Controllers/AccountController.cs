using BiskaUtil;
using WebApp.Models;
using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Database;
using WebApp.Business;
using WebApp.Utilities.Extensions;
using WebApp.Utilities.Helpers;
using WebApp.Utilities.MenuAndRoles;
using WebApp.Utilities.MessageBox;

namespace WebApp.Controllers
{
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class AccountController : Controller
    {
        private readonly VysDBEntities db = new VysDBEntities();
        public ActionResult Login(bool? logout)
        {

            if (logout == true)
            {
                FormsAuthenticationUtil.SignOut();
                return RedirectToAction("Index", "Home");
            }
            if (UserIdentity.Current.IsAuthenticated) return RedirectToAction("Index", "Home");
            ViewBag.UserName = "";
            var mmMessage = new MmMessage();
            ViewBag.MmMessage = mmMessage;
            return PartialView();
        }
        [HttpPost]
        public ActionResult Login(string userName, string password, string captchaRequestCode, string captchaText, bool? rememberMe, string returnUrl)
        {
            var mmMessage = new MmMessage();
            ViewBag.UserName = userName;
            ViewBag.Password = password;
            string hata;
            var xCaptchaxText = CaptchaImageRequests.GetCode(captchaRequestCode);
            var yCaptchaText = CaptchaImageRequests.GetCode(Session.SessionID);

            try
            {
                if (userName.IsNullOrWhiteSpace())
                {
                    hata = "Kullanıcı Adı Giriniz";
                }
                else if (password.IsNullOrWhiteSpace())
                {
                    hata = "Şifre Giriniz";
                }
                else if (captchaText.IsNullOrWhiteSpace())
                {
                    hata = "Resimdeki Karakterleri Giriniz";
                }
                else if (!(xCaptchaxText == captchaText || yCaptchaText == captchaText))
                {
                    hata = "Resimdeki Karakterleri Hatalı Girdiniz";
                }
                else
                {
                    var msg = "";
                    var user = UserBus.GetUser(userName);
                    Kullanicilar loginUser = null;
                    if (user != null)
                    {
                        if (user.IsActiveDirectoryUser == false)
                        {
                            loginUser = UserBus.Login(userName, password);
                        }
                        else
                        {
                            //var ld = new LdapService.SecureSoapClient();
                            //var wsPwd = ConfigurationManager.AppSettings["ldapServicePassword"];
                            //var isSuccess = ld.Login(userName, password, wsPwd);
                            //if (isSuccess)
                            //{
                            //    loginUser = user;
                            //}
                            //else
                            //{
                            //    msg = "Active Directory Kontrolünden Geçilemedi!";
                            //    SistemBilgilendirmeBus.SistemBilgisiKaydet("Active Directory Kontrolünden Geçilemedi! Kullanıcı Adı: " + userName, "Acconunt/Login", BilgiTipi.LoginHatalari, null, UserIdentity.Ip);
                            //    //log.Info("Login Yapılamadı", "");
                            //}
                        }

                        if (loginUser != null && loginUser.IsAktif)
                        {
                            rememberMe = rememberMe ?? false;
                            FormsAuthenticationUtil.SetAuthCookie(userName, "", rememberMe.Value);
                            UserBus.SetLastLogon();

                            if (returnUrl.IsNullOrWhiteSpace()) return RedirectToAction("Index", "Home");
                            return Redirect(returnUrl);


                        }

                        if (loginUser != null && !loginUser.IsAktif) hata = "Kullanıcı Hesabı Pasif Durumda!";
                        else hata = "Kullanıcı Adı veya Şifre Hatalı. " + msg;
                    }
                    else
                    {
                        SistemBilgilendirmeBus.SistemBilgisiKaydet("Kullanıcı Sistemde Bulunamadı! Kullanıcı Adı: " + userName, "Acconunt/Login", BilgiTipi.LoginHatalari, null, UserIdentity.Ip);
                        hata = "Kullanıcı sistemde bulunamadı.";
                    }
                }

            }
            catch (Exception ex)
            {
                mmMessage.IsSuccess = false;
                mmMessage.Messages.Add("Sisteme Giriş Yapılırken Bir Hata Oluştu! Hata: " + ex.ToExceptionMessage());
                hata = "Sisteme Giriş Yapılırken Bir Hata Oluştu! Hata: " + ex.ToExceptionMessage();
            }
            ViewBag.Hata = hata;
            ViewBag.MmMessage = mmMessage;
            return PartialView();

        }
        [Authorize(Roles = RoleNames.KullanicilarOnlineList)]
        public ActionResult OnlineUserCnt()
        { 
            var users = OnlineUsers.users; 
            return users.Count().ToJsonResult(); 
        }

        public ActionResult getOnlineUserList()
        {
            var users = OnlineUsers.users;
            return View(users);
        }
        public ActionResult Generatecaptcha(string captchaRequestCode)
        {
            var ci = new CaptchaImage(string.Empty, 180, 50, 12);
            var text = ci.Text;
            CaptchaImageRequests.AddCode(captchaRequestCode, text);
            CaptchaImageRequests.AddCode(Session.SessionID, text);
            var ms = new System.IO.MemoryStream();
            ci.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            var fcr = new FileContentResult(ms.ToArray(), "image/jpeg")
            {
                FileDownloadName = "captcha.jpeg"
            };
            return fcr;
        }



        public ActionResult ParolaSifirla(string psKod, int? kullaniciId = null)
        {

            var msg = new MmMessage
            {
                ReturnUrlTimeOut = 4000
            };
            if (psKod.IsNullOrWhiteSpace() && kullaniciId.HasValue == false) return RedirectToAction("Index", "Home");

            var kul = new Kullanicilar();
            if (kullaniciId.HasValue == false)
            {
                kul = db.Kullanicilars.FirstOrDefault(p => p.ParolaSifirlamaKodu == psKod);

                if (kul != null)
                {
                    kul.ResimAdi = kul.ResimAdi.ToKullaniciResim();
                    if (kul.ParolaSifirlamGecerlilikTarihi.HasValue && kul.ParolaSifirlamGecerlilikTarihi.Value < DateTime.Now)
                    {
                        msg.IsSuccess = false;
                        msg.Messages.Add("Parola Sıfırlama linkinin geçerlilik süresi dolmuştur");
                        msg.ReturnUrl = Url.Action("Index", "Home");
                    }
                }
                else
                {
                    msg.IsSuccess = false;
                    msg.Messages.Add("Şifre sıfırlama linki herhangi bir kullanıcıya eşleştirilemedi");
                    msg.ReturnUrl = Url.Action("Index", "Home");


                }
            }
            else
            {
                if (UserIdentity.Current.IsAuthenticated)
                {
                    kullaniciId = UserIdentity.Current.Id;
                    kul = db.Kullanicilars.FirstOrDefault(p => p.KullaniciID == kullaniciId);
                    if (kul != null)
                    {
                        kul.ResimAdi = kul.ResimAdi.ToKullaniciResim();
                    }
                }
                else
                {
                    msg.IsSuccess = false;
                    msg.Messages.Add("Lütfen Giriş Yapın");
                    msg.ReturnUrl = Url.Action("Index", "Home");

                }
            }
            Session["ShwMesaj"] = msg;
            ViewBag.MmMessage = msg;
            ViewBag.KullaniciID = kullaniciId;
            ViewBag.EskiSifre = "";
            ViewBag.YeniSifre = "";
            ViewBag.YeniSifreTekrar = "";
            return View(kul);
        }
        [HttpPost]
        public ActionResult ParolaSifirla(string psKod, string eskiSifre, string yeniSifre, string yeniSifreTekrar, int? kullaniciId = null)
        {
            var mmMessage = new MmMessage
            {
                ReturnUrlTimeOut = 4000
            };
            if (psKod.IsNullOrWhiteSpace())
            {
                mmMessage.MessageType = Msgtype.Error;
                mmMessage.Title = "Şifre değiştirme işlemi başarısız";
                mmMessage.ReturnUrl = Url.Action("Index", "Home");
            }

            var kul = kullaniciId.HasValue
                ? db.Kullanicilars.FirstOrDefault(p => p.KullaniciID == kullaniciId)
                : db.Kullanicilars.FirstOrDefault(p => p.ParolaSifirlamaKodu == psKod);
            if (kul != null)
            {
                if (kullaniciId.HasValue == false)
                    if (kul.ParolaSifirlamGecerlilikTarihi.HasValue && kul.ParolaSifirlamGecerlilikTarihi.Value < DateTime.Now)
                    {
                        mmMessage.MessageType = Msgtype.Error;
                        mmMessage.Messages.Add("Parola Sıfırlama linkinin geçerlilik süresi dolmuştur");
                        mmMessage.ReturnUrl = Url.Action("Index", "Home");
                    }
                if (kullaniciId.HasValue)
                {
                    if (eskiSifre.IsNullOrWhiteSpace())
                    {
                        mmMessage.Messages.Add("Varolan şifrenizi giriniz");
                        mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "EskiSifre" });
                    }
                    else if (kul.Sifre != eskiSifre.ComputeHash(StaticDefinitions.Tuz))
                    {
                        mmMessage.Messages.Add("Varolan şifrenizi yanlış girdiniz");
                        mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "EskiSifre" });
                    }
                    else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "EskiSifre" });
                }
                if (mmMessage.Messages.Count == 0)
                {

                    if (yeniSifre.Length < 4)
                    {
                        mmMessage.Messages.Add("Yeni şifreniz en az 4 haneli olmalıdır");
                        mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YeniSifre" });
                    }
                    else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "YeniSifre" });
                    if (yeniSifreTekrar.Length < 4)
                    {
                        mmMessage.Messages.Add("Yeni şifre tekrar en az 4 haneli olmalıdır");
                        mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YeniSifreTekrar" });
                    }
                    else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YeniSifreTekrar" });
                    if (mmMessage.Messages.Count == 0)
                    {
                        if (yeniSifreTekrar != yeniSifre)
                        {
                            mmMessage.Messages.Add("Yeni şifre ile yeni şifre tekrar birbiriyle uyuşmuyor");
                            mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YeniSifreTekrar" });
                            mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YeniSifre" });
                        }
                        else
                        {
                            mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "YeniSifre" });
                            mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "YeniSifreTekrar" });
                        }
                    }
                }

                if (mmMessage.Messages.Count == 0)
                {
                    kul.Sifre = yeniSifreTekrar.ComputeHash(StaticDefinitions.Tuz);
                    kul.ParolaSifirlamGecerlilikTarihi = DateTime.Now;
                    db.SaveChanges();
                    mmMessage.MessageType = Msgtype.Success;
                    mmMessage.Title = "Şifre değiştirme işlemi";
                    if (kullaniciId.HasValue == false)
                    {
                        mmMessage.Messages.Add("Şifreniz değiştirildi! Giriş sayfasına yönlendiriliyorsunuz...");
                        mmMessage.ReturnUrl = Url.Action("Login", "Account");
                    }
                    else
                    { 
                        mmMessage.Messages.Add("Şifreniz değiştirildi!");
                    }
                }
                else
                {
                    mmMessage.MessageType = Msgtype.Error;
                    mmMessage.Title = "Şifre değiştirme işlemi başarısız!";
                    SistemBilgilendirmeBus.SistemBilgisiKaydet("Şifre değiştirme işlemi başarısız! Hata:" + string.Join("\r\n", mmMessage.Messages) + "\r\n KullanıcıAdı:" + kul.KullaniciAdi, "Account/ParolaSifirla", BilgiTipi.Bilgi);
                }
                kul.ResimAdi = kul.ResimAdi.ToKullaniciResim();
            }
            else
            {
                mmMessage.MessageType = Msgtype.Error;
                mmMessage.Title = "Şifre değiştirme işlemi başarısız!";
            }
            if (mmMessage.Messages.Count > 0)
            {
                if (UserIdentity.Current.IsAuthenticated)
                {
                    MessageBox.Show(mmMessage.Title, mmMessage.MessageType == Msgtype.Success ? MessageBox.MessageType.Success : MessageBox.MessageType.Error, mmMessage.Messages.ToArray());
                }
                else
                {
                    Session["ShwMesaj"] = mmMessage;
                }
            }


            ViewBag.MmMessage = mmMessage;
            ViewBag.KullaniciID = kullaniciId;
            ViewBag.EskiSifre = eskiSifre;
            ViewBag.YeniSifre = yeniSifre;
            ViewBag.YeniSifreTekrar = yeniSifreTekrar;
            return View(kul);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
