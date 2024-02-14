using BiskaUtil;
using Database;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WebApp.Business;
using WebApp.Models;
using WebApp.Utilities.Extensions;
using WebApp.Utilities.Helpers;
using WebApp.Utilities.MenuAndRoles;
using WebApp.Utilities.MessageBox;
using WebApp.Utilities.SystemSetting;

namespace WebApp.Controllers
{
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class AjaxController : Controller
    {
        private readonly VysDBEntities db = new VysDBEntities();
        public ActionResult GetThemeSetting()
        {
            var k = db.Kullanicilars.Where(p => p.KullaniciID == UserIdentity.Current.Id).Select(s => new
            {
                s.FixedHeader,
                s.FixedSidebar,
                s.ScrollSidebar,
                s.RightSidebar,
                s.CustomNavigation,
                s.ToggledNavigation,
                s.BoxedOrFullWidth,
                s.ThemeName,
                s.BackgroundImage
            }).FirstOrDefault();
            return Json(k, "application/json", JsonRequestBehavior.AllowGet);
        }
        public ActionResult SetThemeSetting(string columnName, string value)
        {

            var kullanici = db.Kullanicilars.FirstOrDefault(p => p.KullaniciID == UserIdentity.Current.Id);
            if (columnName == "st_head_fixed") kullanici.FixedHeader = value.ToBoolean().Value;
            if (columnName == "st_sb_fixed") kullanici.FixedSidebar = value.ToBoolean().Value;
            if (columnName == "st_sb_scroll") kullanici.ScrollSidebar = value.ToBoolean().Value;
            if (columnName == "st_sb_right") kullanici.RightSidebar = value.ToBoolean().Value;
            if (columnName == "st_sb_custom") kullanici.CustomNavigation = value.ToBoolean().Value;
            if (columnName == "st_sb_toggled") kullanici.ToggledNavigation = value.ToBoolean().Value;
            if (columnName == "st_layout_boxed") kullanici.BoxedOrFullWidth = value.ToBoolean().Value;
            if (columnName == "ThemeName") kullanici.ThemeName = value;
            if (columnName == "BackgroundImage") kullanici.BackgroundImage = value;
            db.SaveChanges();
            if (columnName == "st_head_fixed") UserIdentity.Current.Informations["FixedHeader"] = value.ToBoolean().Value;
            if (columnName == "st_sb_fixed") UserIdentity.Current.Informations["FixedSidebar"] = value.ToBoolean().Value;
            if (columnName == "st_sb_scroll") UserIdentity.Current.Informations["ScrollSidebar"] = value.ToBoolean().Value;
            if (columnName == "st_sb_right") UserIdentity.Current.Informations["RightSidebar"] = value.ToBoolean().Value;
            if (columnName == "st_sb_custom") UserIdentity.Current.Informations["CustomNavigation"] = value.ToBoolean().Value;
            if (columnName == "st_sb_toggled") UserIdentity.Current.Informations["ToggledNavigation"] = value.ToBoolean().Value;
            if (columnName == "st_layout_boxed") UserIdentity.Current.Informations["BoxedOrFullWidth"] = value.ToBoolean().Value;
            if (columnName == "ThemeName") UserIdentity.Current.Informations["ThemeName"] = value;
            if (columnName == "BackgroundImage") UserIdentity.Current.Informations["BackgroundImage"] = value;
            return Json("true", "application/json", JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoginControl(string userName, string password, string captchaRequestCode, string captchaText, bool? rememberMe, string returnUrl)
        {

            var mmMessage = new AjaxLoginModel
            {
                ReturnUrl = returnUrl,
                UserName = userName,
                Password = password
            };
            rememberMe = rememberMe ?? false;

            string hata = null;
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
                            var ld = new LdapService.SecureSoapClient();
                            var wsPwd = ConfigurationManager.AppSettings["ldapServicePassword"];
                            var isSuccess = ld.Login(userName, password, wsPwd);
                            if (isSuccess)
                            {
                                loginUser = user;
                            }
                            else
                            {
                                mmMessage.IsSuccess = false;
                                msg = "Active Directory Kontrolünden Geçilemedi!";
                                SistemBilgilendirmeBus.SistemBilgisiKaydet("Active Directory Kontrolünden Geçilemedi! Kullanıcı Adı: " + userName, "Acconunt/Login", BilgiTipi.LoginHatalari, null, UserIdentity.Ip);
                            }
                        }

                        if (loginUser != null && !loginUser.IsAktif)
                        {
                            hata = "Kullanıcı Hesabı Pasif Durumda!";
                            mmMessage.IsSuccess = false;
                        }
                        else if (loginUser == null)
                        {
                            hata = "Kullanıcı Adı veya Şifre Hatalı. " + msg;
                            mmMessage.IsSuccess = false;
                        }
                        else
                        {
                            mmMessage.IsSuccess = true;
                        }
                    }
                    else
                    {
                        mmMessage.IsSuccess = false;
                        SistemBilgilendirmeBus.SistemBilgisiKaydet("Kullanıcı Sistemde Bulunamadı! Kullanıcı Adı: " + userName, "Acconunt/Login", BilgiTipi.LoginHatalari, null, UserIdentity.Ip);
                        hata = "Kullanıcı sistemde bulunamadı.";
                    }
                }

            }
            catch (Exception ex)
            {
                mmMessage.IsSuccess = false;
                hata = "Sisteme Giriş Yapılırken Bir Hata Oluştu! Hata: " + ex.ToExceptionMessage();
            }
            mmMessage.Message = hata;
            if (mmMessage.IsSuccess == false)
            {
                mmMessage.NewGuid = Guid.NewGuid().ToString().Replace("-", "");
                mmMessage.NewSrc = Url.Action("Generatecaptcha", "Account", new { captchaRequestCode = mmMessage.NewGuid });

            }
            else
            {
                FormsAuthenticationUtil.SetAuthCookie(userName, "", rememberMe != null && rememberMe.Value);
            }
            return mmMessage.ToJsonResult();
        }

        public ActionResult SignOut(string returnUrl)
        {
            var mmMessage = new AjaxLoginModel();

            if (UserIdentity.Current.IsAuthenticated)
            {
                var kulId = UserIdentity.Current.Id;
                var kul = db.Kullanicilars.First(p => p.KullaniciID == kulId);
                kul.LastLogonDate = DateTime.Now;
                db.SaveChanges();
                FormsAuthenticationUtil.SignOut();
            }

            mmMessage.ReturnUrl = returnUrl.IsNullOrWhiteSpace() ? Url.Action("Index", "Home") : returnUrl;
            mmMessage.IsSuccess = true;
            return mmMessage.ToJsonResult();
        }
        [Authorize]
        public ActionResult YetkiYenile(string returnUrl)
        {
            var mmMessage = new MmMessage();

            if (UserIdentity.Current.IsAuthenticated)
            {
                var userIdentity = UserBus.GetUserIdentity(UserIdentity.Current.Name);
                userIdentity.Impersonate();
                Session["UserIdentity"] = userIdentity;
                mmMessage.Messages.Add("Yetkileriniz yeniden yiklenmiştir.");
            }

            if (returnUrl.IsNullOrWhiteSpace()) mmMessage.ReturnUrl = Url.Action("Index", "Home");
            else mmMessage.ReturnUrl = returnUrl;
            mmMessage.IsSuccess = true;
            return mmMessage.ToJsonResult();
        }
        [HttpGet]
        public ActionResult GetKullaniciDetay(int kullaniciId)
        {

            if (RoleNames.Kullanicilar.InRole() != true) kullaniciId = UserIdentity.Current.Id;
            var data = UserBus.GetUser(kullaniciId);
            ViewBag.ResimVar = data.ResimAdi.IsNullOrWhiteSpace() == false;
            ViewBag.KullaniciTipAdi = db.YetkiGruplaris.First(p => p.YetkiGrupID == data.YetkiGrupID).YetkiGrupAdi;
            data.ResimAdi = data.ResimAdi.ToKullaniciResim();

            var userRoles = UserBus.GetUserRoles(kullaniciId);

            ViewBag.KRoller = userRoles;
            return View(data);
        }

        public ActionResult SifreResetle(string mailAddress)
        {

            var mmMessage = new MmMessage();

            if (mailAddress.IsNullOrWhiteSpace() || mailAddress.ToIsValidEmail())
            {
                mmMessage.IsSuccess = false;
                mmMessage.Title = "Girdiğiniz mail mail formatına uygun değildir. Lütfen kontrol ediniz.";
            }
            else
            {
                var kul = db.Kullanicilars.FirstOrDefault(p => p.EMail.Equals(mailAddress));
                var gecerlilikTarihi = DateTime.Now.AddHours(2);
                if (kul == null)
                {
                    mmMessage.IsSuccess = false;
                    mmMessage.Title = "Girdiğiniz mail sistem üzerinde kayıtlı herhangi bir kullanıcı ile eşleşmemektedir.";
                }
                else
                {
                    if (kul.IsActiveDirectoryUser)
                    {
                        mmMessage.IsSuccess = false;
                        mmMessage.Title = "Girdiğiniz mail " + kul.KullaniciAdi + " kullanıcısı ile eşleşmiştir. Fakat eşleşen kullanıcı için şifre kabul türü Active Directory (EBSY, USIS Şifre kabul) sistemine entegre edilmiştir. Bu tarz eşleştirme yapılan kullanıcıların şifreleri 'Performans.yildiz.edu.tr' sistemi tarafından değiştirilemez.";
                    }
                    else
                    {
                        var erisimAdresi = SistemAyar.AyarSistemErisimAdresi.GetAyar();
                        var mRowModel = new List<MailTableRow>();
                        var guid = Guid.NewGuid().ToString().Substring(0, 20);
                        mRowModel.Add(new MailTableRow { Baslik = "Şifre Sıfırlama Linki", Aciklama = "<a target='_blank' href='" + erisimAdresi + "/Account/ParolaSifirla?psKod=" + guid + "'> Şifrenizi sıfırlamak için tıklayınız </a>" });
                        mRowModel.Add(new MailTableRow { Baslik = "Link Geçerlilik Tarihi", Aciklama = "Yukarıdaki link '" + gecerlilikTarihi.ToFormatDateAndTime() + "' tarihine kadar geçerlidir." });

                        var mmmC = new MdlMailMainContent
                        {
                            UniversiteAdi = "Yıldız Teknik Üniversitesi"
                        };
                        var ea = erisimAdresi;
                        var wurlAddr = ea.Split('/').ToList();
                        if (ea.Contains("//"))
                            ea = wurlAddr[0] + "//" + wurlAddr.Skip(2).Take(1).First();
                        else
                            ea = "http://" + wurlAddr.First();
                        mmmC.LogoPath = ea + "/Content/assets/images/ytu_logo_tr.png";
                        var mtc = new MailTableContent
                        {
                            AciklamaBasligi = "Şifre Sıfırlama İşlemi",
                            AciklamaDetayi = "Şifrenizi sıfırlamak için aşağıda bulunan linke tıklayınız ve açılan sayfa da yeni şifrenizi tanımlayınız.",
                            Detaylar = mRowModel
                        };
                        var tavleContent = ViewRenderHelper.RenderPartialView("Ajax", "getMailTableContent", mtc);
                        mmmC.Content = tavleContent;

                        var htmlMail = ViewRenderHelper.RenderPartialView("Ajax", "getMailContent", mmmC);
                        var lstMail = new List<string> { kul.EMail };
                        var rtVal = MailManager.sendMailRetVal("Şifre Sıfırlama İşlemi", htmlMail, lstMail, new List<Attachment>());
                        if (rtVal == null)
                        {
                            mmMessage.IsSuccess = true;
                            mmMessage.Title = "Şifre sıfırlama linki '" + kul.EMail + "' adresine gönderilmiştir!";
                            kul.ParolaSifirlamaKodu = guid;
                            kul.ParolaSifirlamGecerlilikTarihi = gecerlilikTarihi;
                            db.SaveChanges();
                        }
                        else
                        {
                            mmMessage.IsSuccess = false;
                            SistemBilgilendirmeBus.SistemBilgisiKaydet("Şifre sıfırlama! Hata: " + rtVal.ToExceptionMessage(), rtVal.ToExceptionStackTrace(), BilgiTipi.Hata, kul.KullaniciID, UserIdentity.Ip);
                            mmMessage.Title = "Şifre sıfırlama linki '" + kul.EMail + "' adresine gönderilemedi!";
                        }
                    }
                }

            }
            return mmMessage.ToJsonResult();
        }

        [Authorize]
        public ActionResult RotateImage(bool leftOrRight, int kullaniciId)
        {
            if (RoleNames.KullaniciKayit.InRole() == false) kullaniciId = UserIdentity.Current.Id;
            var user = db.Kullanicilars.First(p => p.KullaniciID == kullaniciId);
            var folname = SistemAyar.KullaniciResimYolu.GetAyar();
            if (user.ResimAdi.IsNullOrWhiteSpace())
                return new { ResimAdi = folname + "/" + user.ResimAdi }.ToJsonResult();
            var imgPath = folname + "/" + user.ResimAdi;
            var pth = Server.MapPath(GlobalSistemSetting.GetRoot() + imgPath);

            using (var img = Image.FromFile(pth))
            {
                img.RotateFlip(leftOrRight ? RotateFlipType.Rotate270FlipNone : RotateFlipType.Rotate90FlipNone);
                //  var format = (System.Drawing.Imaging.ImageFormat)img.RawFormat;
                img.Save(pth, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            return new { ResimAdi = folname + "/" + user.ResimAdi }.ToJsonResult();
        }

        [Authorize]
        public ActionResult GetImageUpload(int kullaniciId)
        {
            if (RoleNames.KullaniciKayit.InRole() == false) kullaniciId = UserIdentity.Current.Id;
            var kullanici = UserBus.GetUser(kullaniciId);
            return View(kullanici);
        }
        [Authorize]
        public ActionResult GetImageUploadPost(int kullaniciId, HttpPostedFileBase kProfilResmi)
        {
            var mMessage = new MmMessage();
            var yeniResim = "";
            mMessage.Title = "Profil resmi yükleme işlemi başarısız";
            mMessage.IsSuccess = false;
            mMessage.MessageType = Msgtype.Warning;
            var anaResmiDegistir = false;
            if (kProfilResmi == null || kProfilResmi.ContentLength <= 0)
            {
                mMessage.Messages.Add("Lütfen Resim Seçiniz.");
                // MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "ProfilResmi", Message = msg });
            }
            else if (RoleNames.KullaniciKayit.InRole() == false && kullaniciId != UserIdentity.Current.Id)
            {
                mMessage.Messages.Add("Başka bir kullanıcı adına resim yüklemesi yapmaya yetkili değilsiniz.");
            }
            else
            {
                var contentlength = kProfilResmi.ContentLength;
                var uzanti = Path.GetExtension(kProfilResmi.FileName);
                if ((uzanti == ".jpg" || uzanti == ".JPG" || uzanti == ".jpeg" || uzanti == ".JPEG" || uzanti == ".png" || uzanti == ".PNG" || uzanti == ".bmp" || uzanti == ".BMP") == false)

                {
                    mMessage.Messages.Add("Ekleyeceğiniz resim '.jpg, .JPG, .jpeg, .JPEG, .png, .PNG, .bmp, .BMP' formatlarından biri olmalıdır! ");
                }
                else if (contentlength > 2048000)
                {
                    mMessage.Messages.Add("Ekleyeceğiniz resim maksimum 2MB boyutunda olmalıdır! ");
                }
                else
                {
                    var kul = db.Kullanicilars.First(p => p.KullaniciID == kullaniciId);
                    var eskiResim = kul.ResimAdi;
                    kul.ResimAdi = yeniResim = UserBus.ResimKaydet(kProfilResmi);
                    kul.IslemYapanID = UserIdentity.Current.Id;
                    kul.IslemYapanIP = UserIdentity.Ip;
                    kul.IslemTarihi = DateTime.Now;
                    db.SaveChanges();
                    mMessage.Title = "Progil Resmi başarılı bir şekilde yüklenmiştir.";
                    mMessage.IsSuccess = true;
                    mMessage.MessageType = Msgtype.Success;
                    if (kullaniciId == UserIdentity.Current.Id)
                    {
                        anaResmiDegistir = true;
                        var userIdentity = UserBus.GetUserIdentity(UserIdentity.Current.Name);
                        userIdentity.Impersonate();
                        Session["UserIdentity"] = userIdentity;
                    }

                    if (eskiResim.IsNullOrWhiteSpace())
                        return new
                        {
                            mMessage,
                            ResimAdi = yeniResim.ToKullaniciResim(),
                            AnaResmiDegistir = anaResmiDegistir
                        }.ToJsonResult();
                    var rsmYol = SistemAyar.KullaniciResimYolu.GetAyar();
                    var rsm = Server.MapPath("~/" + rsmYol + "/" + eskiResim);
                    if (System.IO.File.Exists(rsm)) System.IO.File.Delete(rsm);
                }
            }
            return new { mMessage, ResimAdi = yeniResim.ToKullaniciResim(), AnaResmiDegistir = anaResmiDegistir }.ToJsonResult();
        }
        public ActionResult GetMessage(MmMessage model)
        {
            return View(model);
        }

        public ActionResult GetMailContent(MdlMailMainContent model)
        {
            return View(model);
        }

        public ActionResult GetMailTableContent(MailTableContent model)
        {
            return View(model);
        }




        [Authorize(Roles = RoleNames.MailIslemleri)]
        [ValidateInput(false)]
        public ActionResult MailGonder(List<string> kullaniciId, string setKonu, string setAciklama, int? id, bool topluMail = false, bool toOrBcc = false)
        {

            var model = new GonderilenMailler
            {
                MesajID = id,

            };

            var mmMessage = new MmMessage();
            ViewBag.MmMessage = mmMessage;
            if (topluMail)
            {
                ViewBag.strAlicis = kullaniciId;
                kullaniciId = new List<string>();

            }


            var eList = new List<ComboModelString>();
            kullaniciId = kullaniciId ?? new List<string>();
            var ids = kullaniciId.Where(p => p.IsNumber()).Select(s => s.ToInt().Value).ToList();
            var mails = kullaniciId.Where(p => p.IsNumber() == false).Select(s => s).ToList();
            db.Kullanicilars.Where(p => ids.Contains(p.KullaniciID)).ToList().ForEach((k) => { eList.Add(new ComboModelString { Value = k.KullaniciID.ToString(), Caption = k.EMail }); });
            eList.AddRange(mails.Where(p => p != "").Select(item => new ComboModelString { Value = item, Caption = item }));
            ViewBag.EmailList = eList;
            if (setKonu.IsNullOrWhiteSpace() == false) model.Konu = setKonu;

            ViewBag.MailSablonlariID = new SelectList(MailIslemleriBus.CmbMailSablonlari(true, false), "Value", "Caption");
            ViewBag.SetAciklama = setAciklama ?? "";
            ViewBag.TopluMail = topluMail;
            ViewBag.toOrBcc = toOrBcc;

            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        [Authorize(Roles = RoleNames.MailIslemleri)]
        public ActionResult MailGonderPost(int? mesajId, string alici, string konu, string aciklama, string aciklamaHtml, List<HttpPostedFileBase> dosyaEki, List<string> dosyaEkiAdi, List<string> ekYolu, bool topluMail = false, string strAlicis = "")
        {
            var mmMessage = new MmMessage
            {
                Title = "Mail gönderme işlemi"
            };

            dosyaEki = dosyaEki ?? new List<HttpPostedFileBase>();
            dosyaEkiAdi = dosyaEkiAdi ?? new List<string>();
            ekYolu = ekYolu ?? new List<string>();
            var secilenAlicilar = new List<string>();
            if (alici.IsNullOrWhiteSpace() == false) alici.Split(',').ToList().ForEach((itm) => { secilenAlicilar.Add(itm); });

            if (aciklama.IsNullOrWhiteSpace() == false)
            {
                var cevapA = "";
                var geriDonusLink = "";
                if (mesajId.HasValue)
                {
                    var mesaj = db.Mesajlars.First(p => p.MesajID == mesajId.Value);
                    if (mesaj.Mesajlar2 != null) mesaj = mesaj.Mesajlar2;
                    mesajId = mesaj.MesajID;
                    var cevapAdresi = SistemAyar.AyarSistemErisimAdresi.GetAyar() + "/Home/Index?MesajGroupID=" + mesaj.GroupID;
                    cevapA = "<div style='color:#A9A9A9;'>" + mesaj.AciklamaHtml + "</div>";
                    geriDonusLink = "<a target='_blank' href='" + cevapAdresi + "' style='color:green;font-size:12pt;'> >> Bu maile sistem üzerinden cevap yazmak için lütfen tıklayınız << </a>";
                }
                var nAck = "</br><p><span style='color:red'>Not: Cevaplama İşlemini Lütfen Sistem Üzerinden Yapınız. Bu mail sistem maili olduğundan yazılan cevaplar okunmamaktadır.</span></br><span style='color:red'>------------------------------<wbr>------------------------------<wbr>------------------------------<wbr>------------------------------<wbr>------------------</span></p> " + cevapA;

                aciklamaHtml += geriDonusLink + nAck;
            }
            if (topluMail)
            {
                secilenAlicilar.AddRange(strAlicis.Split(',').ToList());
            }
            var qDosyaEkAdi = dosyaEkiAdi.Select((s, inx) => new { s, inx }).ToList();
            var qDosyaEki = dosyaEki.Select((s, inx) => new { s, inx }).ToList();
            var qDosyaEkYolu = ekYolu.Select((s, inx) => new { s, inx }).ToList();

            var qDosyalar = (from dek in qDosyaEkAdi
                             join de in qDosyaEki on dek.inx equals de.inx
                             join deY in qDosyaEkYolu on dek.inx equals deY.inx
                             select new
                             {
                                 dek.inx,
                                 DosyaEkAdi = dek.s,
                                 Dosya = de.s,
                                 mDosyaAdi = de.s != null ? (dek.s.Replace(".", "") + "." + de.s.FileName.Split('.').Last()) : (dek.s.Replace(".", "") + "." + deY.s.Split('.').Last()),
                                 DosyaYolu = de.s != null ? ("/MailDosyalari/" + dek.s + "_" + Guid.NewGuid().ToString().Substring(0, 4) + "." + de.s.FileName.Split('.').Last()) : (deY.s)
                             }).ToList();

            #region Kontrol 
            if (secilenAlicilar.Count == 0)
            {
                const string msg = "Mail Gönderilecek Hiçbir Alıcı Belirlenemedi!";
                mmMessage.Messages.Add(msg);
            }

            if (konu.IsNullOrWhiteSpace())
            {
                const string msg = "Konu Giriniz.";
                mmMessage.Messages.Add(msg);
            }

            if (aciklama.IsNullOrWhiteSpace() && aciklamaHtml.IsNullOrWhiteSpace())
            {
                const string msg = "İçerik Giriniz.";
                mmMessage.Messages.Add(msg);
            }
            #endregion
            var kModel = new GonderilenMailler
            {
                Tarih = DateTime.Now
            };
            if (mmMessage.Messages.Count == 0)
            {
                kModel.MesajID = mesajId;
                kModel.IslemTarihi = DateTime.Now;
                kModel.Konu = konu;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                kModel.Aciklama = aciklama ?? "";
                kModel.AciklamaHtml = aciklamaHtml ?? "";

                var gonderilenMailEkleri = new List<GonderilenMailEkleri>();
                foreach (var item in qDosyalar)
                {
                    if (item.Dosya != null)
                        item.Dosya.SaveAs(Server.MapPath("~" + item.DosyaYolu));
                    gonderilenMailEkleri.Add(new GonderilenMailEkleri
                    {
                        EkAdi = item.mDosyaAdi,
                        EkDosyaYolu = item.DosyaYolu
                    });
                }
                var gonderilenMailKullanicilar = new List<GonderilenMailKullanicilar>();
                secilenAlicilar = secilenAlicilar.Distinct().ToList();
                if (secilenAlicilar.Count > 0)
                {
                    var qscIDs = secilenAlicilar.Where(p => p.IsNumber()).Select(s => s.ToInt().Value).ToList();
                    var qscMails = secilenAlicilar.Where(p => p.IsNumber() == false).ToList();
                    var dataqx = (from s in db.Kullanicilars
                                  where qscIDs.Contains(s.KullaniciID)
                                  select new
                                  {
                                      Email = s.EMail,
                                      s.KullaniciID
                                  }).ToList();
                    gonderilenMailKullanicilar.AddRange(dataqx.Select(item => new GonderilenMailKullanicilar { Email = item.Email, KullaniciID = item.KullaniciID }));

                    gonderilenMailKullanicilar.AddRange(qscMails.Select(item => new GonderilenMailKullanicilar { Email = item, KullaniciID = null }));
                }
                kModel.GonderilenMailEkleris = gonderilenMailEkleri;
                kModel.GonderilenMailKullanicilars = gonderilenMailKullanicilar;
                var eklenen = db.GonderilenMaillers.Add(kModel);


                if (mesajId.HasValue)
                {
                    var mesaj = db.Mesajlars.FirstOrDefault(p => p.MesajID == mesajId.Value);
                    if (mesaj != null)
                    {
                        mesaj.IsAktif = true;
                    }
                }




                gonderilenMailKullanicilar = db.GonderilenMailKullanicilars.AddRange(gonderilenMailKullanicilar.Distinct()).ToList();

                var attach = new List<Attachment>();
                foreach (var item in qDosyalar)
                {
                    var ekTamYol = Server.MapPath("~" + item.DosyaYolu);
                    if (System.IO.File.Exists(ekTamYol))
                        attach.Add(new Attachment(new MemoryStream(System.IO.File.ReadAllBytes(ekTamYol)), item.mDosyaAdi, MediaTypeNames.Application.Octet));
                    else SistemBilgilendirmeBus.SistemBilgisiKaydet("Mail gönderilirken eklenen dosya eki sistemde bulunamadı!<br/>Dosya Adı:" + item.mDosyaAdi + " <br/>Dosya Yolu:" + ekTamYol, "Ajax/MailGonderPost", BilgiTipi.Hata);
                }


                var gidecekler = gonderilenMailKullanicilar.Select(s => s.Email).ToList();
                var dct = new Dictionary<int, List<string>>();

                var inx = 0;
                while (gidecekler.Count > 800)
                {
                    dct.Add(inx, gidecekler.Take(800).ToList());
                    gidecekler = gidecekler.Skip(800).ToList();
                    inx++;
                }
                inx++;
                dct.Add(inx, gidecekler);
                var toOrBcc = !topluMail;
                foreach (var item in dct)
                {
                    var excpt = MailManager.sendMailRetVal(kModel.Konu, kModel.AciklamaHtml, item.Value, attach, toOrBcc);
                    if (excpt == null)
                    {
                        eklenen.Gonderildi = true;
                        db.SaveChanges();
                        mmMessage.Messages.Add("Mail gönderildi!");
                        mmMessage.IsSuccess = true;
                        mmMessage.MessageType = Msgtype.Success;
                    }
                    else
                    {
                        var msgerr = excpt.ToExceptionMessage().Replace("\r\n", "<br/>");
                        mmMessage.Messages.Add("Mail gönderilirken bir hata oluştu! </br>Hata:" + msgerr);
                        mmMessage.IsSuccess = false;
                        mmMessage.MessageType = Msgtype.Error;
                        try
                        {
                            db.GonderilenMaillers.Remove(eklenen);
                            foreach (var item2 in qDosyalar)
                            {
                                if (System.IO.File.Exists(Server.MapPath("~" + item2.DosyaYolu)))
                                    System.IO.File.Delete(Server.MapPath("~" + item2.DosyaYolu));
                            }
                            db.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            SistemBilgilendirmeBus.SistemBilgisiKaydet(ex.ToExceptionMessage(), "Ajax/MailGonderPost<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.Hata);
                        }
                    }
                }
            }
            else
            {
                mmMessage.IsSuccess = false;
                mmMessage.MessageType = Msgtype.Warning;
            }

            var strView = ViewRenderHelper.RenderPartialView("Ajax", "getMessage", mmMessage);
            //return Content(strView, MediaTypeNames.Text.Html);
            return Json(new { success = mmMessage.IsSuccess, responseText = strView }, JsonRequestBehavior.AllowGet);
            //return new JsonResult { Data = new { IsSuccess = mmMessage.IsSuccess, Message = strView } };

        }


        [Authorize(Roles = RoleNames.MailIslemleri)]
        public ActionResult GetTumMailListesi(string term, string ids)
        {
            var kullaniciIDs = new JavaScriptSerializer().Deserialize<List<string>>(ids).Where(p => p.ToIntObj().HasValue).Select(s => s.ToInt().Value);
            var qKullanicilar = (from k in db.Kullanicilars
                                 orderby k.Ad, k.Soyad
                                 where k.EMail.Contains("@") && (k.EMail.StartsWith(term) || (k.Ad + " " + k.Soyad).Contains(term)) && !kullaniciIDs.Contains(k.KullaniciID)
                                 select new
                                 {
                                     id = k.KullaniciID,
                                     AdSoyad = k.Ad + " " + k.Soyad,
                                     text = k.EMail,
                                     Images = k.ResimAdi

                                 }).Take(25).ToList();
            var kul = qKullanicilar.Select(k => new
            {
                id = k.id.ToString(),
                k.AdSoyad,
                k.text,
                Images = k.Images.ToKullaniciResim()

            }).ToList();
            return kul.ToJsonResult();
        }

        public ActionResult GetSablonlar(int mailSablonlariId)
        {
            var sbl = db.MailSablonlaris.Where(p => p.MailSablonlariID == mailSablonlariId).Select(s => new { s.SablonAdi, s.Sablon, s.SablonHtml, MailSablonlariEkleri = s.MailSablonlariEkleris.Select(s2 => new { s2.MailSablonlariEkiID, s2.EkAdi, s2.EkDosyaYolu }) }).First();
            return Json(new { sbl.SablonAdi, sbl.Sablon, sbl.SablonHtml, sbl.MailSablonlariEkleri }, "application/json", JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMsjKategoris()
        {
            var ots = MesajKategorileriBus.CmbMesajKategorileri(true, true);
            return ots.Select(s => new { s.Value, s.Caption }).ToJsonResult();
        }
        public ActionResult GetKtNot(int mesajKategoriId)
        {
            string not = "";
            var mkNot = db.MesajKategorileris.FirstOrDefault(p => p.MesajKategoriID == mesajKategoriId);
            if (mkNot != null) not = mkNot.KategoriAciklamasi;
            return Json(new { NotBilgisi = not });
        }
        public ActionResult MesajKaydet(string groupId)
        {
            var model = new Mesajlar();
            var mmMessage = new MmMessage();
            ViewBag.MmMessage = mmMessage;

            if (groupId.IsNullOrWhiteSpace() == false)
            {
                model = db.Mesajlars.First(p => p.GroupID == groupId);
                if (UserIdentity.Current.IsAuthenticated)
                {
                    if (model.KullaniciID != UserIdentity.Current.Id)
                    {
                        model.AdSoyad = UserIdentity.Current.AdSoyad;
                        model.Email = UserIdentity.Current.EMail;
                    }
                }

            }
            else if (UserIdentity.Current.IsAuthenticated)
            {
                model.AdSoyad = UserIdentity.Current.AdSoyad;
                model.Email = UserIdentity.Current.EMail;
            }

            ViewBag.MesajKategoriID = new SelectList(MesajKategorileriBus.CmbMesajKategorileri(true, true), "Value", "Caption", model.MesajKategoriID);
            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult MesajKaydetPost(int mesajId, string groupId, int mesajKategoriId, string konu, string adSoyad, string email, string aciklama, string aciklamaHtml, List<HttpPostedFileBase> dosyaEki, List<string> dosyaEkiAdi)
        {
            var mmMessage = new MmMessage
            {
                Title = "Dilek/Öneri/Şikayet gönderme işlemi"
            };

            dosyaEki = dosyaEki ?? new List<HttpPostedFileBase>();
            dosyaEkiAdi = dosyaEkiAdi ?? new List<string>();

            var qDosyaEkAdi = dosyaEkiAdi.Select((s, inx) => new { s, inx }).ToList();
            var qDosyaEki = dosyaEki.Select((s, inx) => new { s, inx }).ToList();

            var qDosyalar = (from dek in qDosyaEkAdi
                             join de in qDosyaEki on dek.inx equals de.inx
                             select new
                             {
                                 dek.inx,
                                 DosyaEkAdi = dek.s,
                                 Dosya = de.s,
                                 mDosyaAdi = dek.s.Replace(".", "") + "." + de.s.FileName.Split('.').Last(),
                                 DosyaYolu = "/MailDosyalari/" + dek.s + "_" + Guid.NewGuid().ToString().Substring(0, 4) + "." + de.s.FileName.Split('.').Last()
                             }).ToList();

            #region Kontrol 

            if (mesajId <= 0)
            {
                if (konu.IsNullOrWhiteSpace())
                {
                    const string msg = "Konu Giriniz.";
                    mmMessage.Messages.Add(msg);
                }
            }
            else
            {
                var mesaj = db.Mesajlars.First(p => p.MesajID == mesajId && p.GroupID == groupId);
                mesaj.IsAktif = false;
                konu = mesaj.Konu;
                mesajKategoriId = mesaj.MesajKategoriID;
                if (UserIdentity.Current.IsAuthenticated && mesaj.KullaniciID != UserIdentity.Current.Id)
                {
                    email = UserIdentity.Current.EMail;
                    adSoyad = UserIdentity.Current.AdSoyad;
                }
                else
                {
                    email = mesaj.Email;
                    adSoyad = mesaj.AdSoyad;

                }
            }
            if (email.IsNullOrWhiteSpace())
            {
                mmMessage.Messages.Add("E Mail Giriniz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "EMail" });
            }
            else if (email.ToIsValidEmail())
            {
                mmMessage.Messages.Add("Lütfen EMail Formatını Doğru Giriniz");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "EMail" });
            }
            if (aciklama.IsNullOrWhiteSpace() && aciklamaHtml.IsNullOrWhiteSpace())
            {
                mmMessage.Messages.Add("İçerik Giriniz.");
            }

            var kModel = new Mesajlar();
            #endregion
            if (mmMessage.Messages.Count == 0)
            {
                kModel.MesajKategoriID = mesajKategoriId;
                if (UserIdentity.Current.IsAuthenticated == false)
                {
                    kModel.AdSoyad = adSoyad;
                    kModel.Email = email;

                }
                else
                {
                    var kul = db.Kullanicilars.First(p => p.KullaniciID == UserIdentity.Current.Id);
                    kModel.AdSoyad = kul.Ad + " " + kul.Soyad;
                    kModel.Email = kul.EMail;
                    kModel.KullaniciID = UserIdentity.Current.Id;
                    kModel.IslemYapanID = UserIdentity.Current.Id;
                }
                kModel.UstMesajID = mesajId <= 0 ? (int?)null : mesajId;
                kModel.GroupID = Guid.NewGuid().ToString();
                kModel.Tarih = DateTime.Now;
                kModel.IslemTarihi = DateTime.Now;
                kModel.Konu = konu;
                kModel.IslemYapanIP = UserIdentity.Ip;
                kModel.Aciklama = aciklama ?? "";
                kModel.AciklamaHtml = aciklamaHtml ?? "";
                kModel.IsAktif = false;
                var mesajEkler = new List<MesajEkleri>();
                foreach (var item in qDosyalar)
                {
                    item.Dosya.SaveAs(Server.MapPath("~" + item.DosyaYolu));
                    mesajEkler.Add(new MesajEkleri
                    {
                        EkAdi = item.mDosyaAdi,
                        EkDosyaYolu = item.DosyaYolu
                    });
                }

                var eklenen = db.Mesajlars.Add(kModel);
                eklenen.MesajEkleris = mesajEkler;
                db.SaveChanges();
                mmMessage.IsSuccess = true;
            }
            else
            {
                mmMessage.IsSuccess = false;
            }

            return Json(new { success = mmMessage.IsSuccess, responseText = mmMessage.IsSuccess ? "Mesaj gönderme işlemi başarılı!" : "Mesaj gönderilirken bir hata oluştu! Hata: " + string.Join("</br>", mmMessage.Messages.ToList()) }, JsonRequestBehavior.AllowGet);

        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult PdfViewer()
        {
            return View();
        }



        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }


    }
}
