using BiskaUtil;
using Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Business;
using WebApp.Models;
using WebApp.Utilities.Extensions;
using WebApp.Utilities.Helpers;
using WebApp.Utilities.MenuAndRoles;
using WebApp.Utilities.MessageBox;
using WebApp.Utilities.SystemData;
using WebApp.Utilities.SystemSetting;


namespace WebApp.Controllers
{
    [Authorize]
    [OutputCache(NoStore = false, Duration = 4, VaryByParam = "*")]
    public class KullanicilarController : Controller
    {
        private readonly VysDBEntities db = new VysDBEntities();
        [Authorize(Roles = RoleNames.Kullanicilar)]
        public ActionResult Index()
        {
            return Index(new FmKullanicilar() { PageSize = 15, Expand = false });
        }
        [HttpPost]
        [Authorize(Roles = RoleNames.Kullanicilar)]
        public ActionResult Index(FmKullanicilar model, List<int> rollId = null)
        {

            rollId = rollId ?? new List<int>();
            var brms = db.sp_BirimAgaci().ToList();
            var q = from s in db.Kullanicilars
                    join ktl in db.YetkiGruplaris on s.YetkiGrupID equals ktl.YetkiGrupID
                    where !model.YetkiliBirimID.HasValue || s.KullaniciBirimleris.Any(a => a.BirimID == model.YetkiliBirimID)
                    select new FrKullanicilar
                    {
                        KullaniciID = s.KullaniciID,
                        YetkiGrupID = s.YetkiGrupID,
                        YetkiGrupAdi = ktl.YetkiGrupAdi,
                        UnvanID = s.UnvanID,
                        UnvanAdi = s.Unvanlar.UnvanAdi,
                        BirimID = s.BirimID,
                        BirimAdi = s.Birimler.BirimAdi,
                        Ad = s.Ad,
                        Soyad = s.Soyad,
                        Tel = s.Tel,
                        EMail = s.EMail,
                        ResimAdi = s.ResimAdi,
                        KullaniciAdi = s.KullaniciAdi,
                        Sifre = s.Sifre,
                        FixedHeader = s.FixedHeader,
                        FixedSidebar = s.FixedSidebar,
                        ScrollSidebar = s.ScrollSidebar,
                        RightSidebar = s.RightSidebar,
                        CustomNavigation = s.CustomNavigation,
                        ToggledNavigation = s.ToggledNavigation,
                        BoxedOrFullWidth = s.BoxedOrFullWidth,
                        ThemeName = s.ThemeName,
                        BackgroundImage = s.BackgroundImage,
                        SifresiniDegistirsin = s.SifresiniDegistirsin,
                        IsAktif = s.IsAktif,
                        IsActiveDirectoryUser = s.IsActiveDirectoryUser,
                        IsAdmin = s.IsAdmin,
                        Aciklama = s.Aciklama,
                        ParolaSifirlamaKodu = s.ParolaSifirlamaKodu,
                        ParolaSifirlamGecerlilikTarihi = s.ParolaSifirlamGecerlilikTarihi,
                        OlusturmaTarihi = s.OlusturmaTarihi,
                        LastLogonDate = s.LastLogonDate,
                        LastLogonIP = s.LastLogonIP,
                        IslemTarihi = s.IslemTarihi,
                        IslemYapanIP = s.IslemYapanIP
                    };

            if (!model.AdSoyad.IsNullOrWhiteSpace()) q = q.Where(p => (p.Ad + " " + p.Soyad).Contains(model.AdSoyad) || p.EMail.Contains(model.AdSoyad) || p.Tel.Contains(model.AdSoyad) || p.KullaniciAdi.Contains(model.AdSoyad));

            if (model.YetkiGrupID.HasValue) q = q.Where(p => p.YetkiGrupID == model.YetkiGrupID.Value);
            if (model.IsAktif.HasValue) q = q.Where(p => p.IsAktif == model.IsAktif.Value);
            if (model.IsActiveDirectoryUser.HasValue) q = q.Where(p => p.IsActiveDirectoryUser == model.IsActiveDirectoryUser.Value);

            if (model.BirimID.HasValue)
            {
                var sbKods = model.BirimID.Value.GetSubBirimIDs();
                q = q.Where(p => sbKods.Contains(p.BirimID));
            }

            if (model.IsAdmin.HasValue) q = q.Where(p => p.IsAdmin == model.IsAdmin);

            model.RowCount = q.Count();
            var indexModel = new MIndexBilgi
            {
                Toplam = model.RowCount,
                Pasif = q.Count(p => p.IsAktif == false)
            };

            if (!model.Sort.IsNullOrWhiteSpace())
                if (model.Sort == "AdSoyad") q = q.OrderBy(o => o.Ad).ThenBy(o => o.Soyad);
                else if (model.Sort.Contains("AdSoyad") && model.Sort.Contains("DESC")) q = q.OrderByDescending(o => o.Ad).ThenByDescending(o => o.Soyad);
                else q = DynamicQueryable.OrderBy(q, model.Sort);
            else q = q.OrderBy(o => o.Ad).ThenBy(t => t.Soyad);
            model.Data = q.Skip(model.PagingStartRowIndex).Take(model.PageSize).ToArray();
            foreach (var item in model.Data)
            {
                var secilenB = brms.FirstOrDefault(p => p.BirimID == item.BirimID);
                item.BirimAdi = secilenB.BirimTreeAdi2;
            }
            ViewBag.YetkiGrupID = new SelectList(YetkiGrupBus.CmbYetkiGruplari(), "Value", "Caption", model.YetkiGrupID);
            ViewBag.IsAktif = new SelectList(ComboData.GetCmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            ViewBag.IsAdmin = new SelectList(ComboData.GetCmbVarYokData(), "Value", "Caption", model.IsAdmin);
            ViewBag.RollID = new SelectList(RollerBus.GetAllRoles(), "RolID", "GorunurAdi");
            var birimlers = BirimlerBus.CmbBirimlerTree();
            ViewBag.BirimID = new SelectList(birimlers, "Value", "Caption", model.BirimID);
            ViewBag.YetkiliBirimID = new SelectList(birimlers, "Value", "Caption", model.YetkiliBirimID);
            ViewBag.UnvanID = new SelectList(UnvanlarBus.CmbUnvanlar(), "Value", "Caption", model.UnvanID);
            ViewBag.IsActiveDirectoryUser = new SelectList(ComboData.CmbIsActiveDirectoryUserData(), "Value", "Caption", model.IsActiveDirectoryUser);
            ViewBag.SelectedRolls = rollId;

            ViewBag.kIds = q.Select(s => s.KullaniciID).ToList();
            return View(model);
        }
        [Authorize(Roles = RoleNames.KullaniciKayit)]
        public ActionResult Kayit(int? id)
        {
            var mmMessage = new MmMessage();
            ViewBag.MmMessage = mmMessage;
            var model = new Kullanicilar
            {
                IsAktif = true
            };
            var resimVar = false;
            if (id > 0)
            {
                var data = db.Kullanicilars.FirstOrDefault(p => p.KullaniciID == id);
                if (data != null)
                {
                    resimVar = data.ResimAdi.IsNullOrWhiteSpace() == false;
                    data.ResimAdi = data.ResimAdi.ToKullaniciResim();
                    model = data;
                }
                model.Sifre = "";
            }

            ViewBag.ResimVar = resimVar;
            ViewBag.YetkiGrupID = new SelectList(YetkiGrupBus.CmbYetkiGruplari(), "Value", "Caption", model.YetkiGrupID);
            ViewBag.BirimID = new SelectList(BirimlerBus.GetBirimler().ToOrderedList("BirimID", "UstBirimID", "BirimAdi"), "BirimID", "BirimAdi", model.BirimID);
            ViewBag.UnvanID = new SelectList(UnvanlarBus.CmbUnvanlar(), "Value", "Caption", model.UnvanID);
            ViewBag.Kullanici = UserBus.GetUser(model.KullaniciID);
            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = RoleNames.KullaniciKayit)]
        public ActionResult Kayit(Kullanicilar kModel, HttpPostedFileBase profilResmi, int? yetkilendirmeyeGit = null)
        {
            var mmMessage = new MmMessage();
            bool resimVar = false;
            if (kModel.KullaniciID > 0)
            {
                var kul = db.Kullanicilars.First(p => p.KullaniciID == kModel.KullaniciID);
                resimVar = kul.ResimAdi.IsNullOrWhiteSpace() == false;
                kModel.ResimAdi = kul.ResimAdi.ToKullaniciResim();
            }
            #region Kontrol
            kModel.KullaniciAdi = kModel.KullaniciAdi != null ? kModel.KullaniciAdi.Trim() : "";

            if (kModel.Ad.IsNullOrWhiteSpace())
            {
                mmMessage.Messages.Add("Ad Giriniz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Ad" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Ad" });
            if (kModel.Soyad.IsNullOrWhiteSpace())
            {
                mmMessage.Messages.Add("Soyad Giriniz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Soyad" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Soyad" });


            if (kModel.BirimID <= 0)
            {
                mmMessage.Messages.Add("Birim Seçiniz");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BirimID" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "BirimID" });
            if (kModel.UnvanID <= 0)
            {
                mmMessage.Messages.Add("Ünvan Seçiniz");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "UnvanID" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "UnvanID" });
            if (kModel.Tel.IsNullOrWhiteSpace())
            {
                mmMessage.Messages.Add("Cep telefonu bilgisini giriniz");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Tel" });
            }
            else
            {
                if (kModel.Tel.IsNullOrWhiteSpace() == false) mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Tel" });
            }

            if (kModel.EMail.IsNullOrWhiteSpace())
            {
                mmMessage.Messages.Add("E Mail Giriniz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "EMail" });
            }
            else if (kModel.EMail.ToIsValidEmail())
            {
                mmMessage.Messages.Add("Lütfen EMail Formatını Doğru Giriniz");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "EMail" });
            }
            else
            {
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "EMail" });
            }
            if (kModel.KullaniciAdi.IsNullOrWhiteSpace())
            {
                mmMessage.Messages.Add("Kullanıcı Adı Giriniz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "KullaniciAdi" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "KullaniciAdi" });


            if (kModel.KullaniciID <= 0)
            {
                if (kModel.Sifre.IsNullOrWhiteSpace())
                {
                    mmMessage.Messages.Add("Şifre Giriniz.");
                    mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Sifre" });
                }
                else if (kModel.Sifre.Length < 4)
                {
                    mmMessage.Messages.Add("Şifre en az 4 haneli olmalıdır.");
                    mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Sifre" });
                }
                else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Sifre" });
            }
            else if (!kModel.Sifre.IsNullOrWhiteSpace())
            {
                if (kModel.Sifre.Length < 4 && kModel.KullaniciID > 0)
                {
                    mmMessage.Messages.Add("Şifre en az 4 haneli olmalıdır.");
                    mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Sifre" });
                }
                else if (kModel.Sifre.Length >= 4 && kModel.KullaniciID > 0) mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Sifre" });
            }
            if (kModel.YetkiGrupID <= 0)
            {
                mmMessage.Messages.Add("Kullanıcı hesabı için yetki grubu seçiniz");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "YetkiGrupID" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "YetkiGrupID" });
            #endregion



            if (mmMessage.Messages.Count == 0)
            {
                kModel.Ad = kModel.Ad.Trim();
                kModel.Soyad = kModel.Soyad.Trim();
                kModel.EMail = kModel.EMail.Trim();
                kModel.Tel = kModel.Tel.Trim();
                kModel.KullaniciAdi = kModel.KullaniciAdi.Trim();
                var qKullanici = db.Kullanicilars.AsQueryable();

                var cUserName = qKullanici.Count(p => p.IsAktif && p.KullaniciID != kModel.KullaniciID && p.KullaniciAdi == kModel.KullaniciAdi);
                if (cUserName > 0)
                {
                    mmMessage.Messages.Add("Tanımlamak istediğiniz kullanıcı adı sistemde zaten mevcut!");
                    mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "KullaniciAdi" });
                }
                else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "KullaniciAdi" });
                //var cEmail = qKullanici.Where(p => p.IsAktif && p.KullaniciID != kModel.KullaniciID && p.EMail == kModel.EMail).Count();
                //if (cEmail > 0)
                //{
                //    string msg = "Tanımlamak istediğiniz Email sistemde zaten mevcut!";
                //    MmMessage.Messages.Add(msg);
                //    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "EMail", Message = msg });
                //}
                //else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "EMail" });

            }


            if (mmMessage.Messages.Count == 0)
            {
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanIP = UserIdentity.Ip;
                var yeniKullanici = kModel.KullaniciID <= 0;
                if (yeniKullanici)
                {
                    var sfr = kModel.Sifre;
                    kModel.OlusturmaTarihi = DateTime.Now;
                    kModel.Sifre = kModel.Sifre.ComputeHash(StaticDefinitions.Tuz);
                    kModel.IsAktif = true;
                    kModel.FixedHeader = false;
                    kModel.FixedSidebar = false;
                    kModel.ScrollSidebar = false;
                    kModel.RightSidebar = false;
                    kModel.CustomNavigation = true;
                    kModel.ToggledNavigation = false;
                    kModel.BoxedOrFullWidth = true;
                    kModel.ThemeName = "/Content/css/theme-forest.css";
                    kModel.BackgroundImage = "wall_2";

                    if (profilResmi != null)
                    {
                        kModel.ResimAdi = UserBus.ResimKaydet(profilResmi);

                    }

                    kModel = db.Kullanicilars.Add(kModel);
                    db.SaveChanges();

                    var excpt = MailManager.YeniHesapMailGonder(kModel, sfr);
                    if (excpt != null)
                    {
                        mmMessage.Messages.Add(kModel.KullaniciAdi + " kullanıcı hesabı oluşturuldu fakat kullanıcıya bilgi maili atılırken bir hata oluştu! Hata:" + excpt.ToExceptionMessage());
                        MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, mmMessage.Messages.ToArray());
                    }

                }
                else
                {
                    var data = db.Kullanicilars.First(p => p.KullaniciID == kModel.KullaniciID);
                    data.YetkiGrupID = kModel.YetkiGrupID;
                    data.Ad = kModel.Ad;
                    data.Soyad = kModel.Soyad;
                    data.BirimID = kModel.BirimID;
                    data.UnvanID = kModel.UnvanID;
                    data.IsAktif = kModel.IsAktif;
                    data.Tel = kModel.Tel;
                    data.EMail = kModel.EMail;
                    data.KullaniciAdi = kModel.KullaniciAdi;
                    if (!kModel.Sifre.IsNullOrWhiteSpace())
                        data.Sifre = kModel.Sifre.ComputeHash(StaticDefinitions.Tuz);
                    data.SifresiniDegistirsin = kModel.SifresiniDegistirsin;
                    data.Aciklama = kModel.Aciklama;
                    data.IsActiveDirectoryUser = kModel.IsActiveDirectoryUser;
                    data.IsAdmin = kModel.IsAdmin;
                    data.IslemYapanID = kModel.IslemYapanID;
                    data.IslemTarihi = kModel.IslemTarihi;
                    data.IslemYapanIP = kModel.IslemYapanIP;
                    if (profilResmi != null)
                    {
                        if (data.ResimAdi.IsNullOrWhiteSpace() == false)
                        {
                            var rsmYol = SistemAyar.KullaniciResimYolu.GetAyar();
                            var rsm = Server.MapPath("~/" + rsmYol + "/" + data.ResimAdi);
                            if (System.IO.File.Exists(rsm)) System.IO.File.Delete(rsm);
                        }
                        data.ResimAdi = UserBus.ResimKaydet(profilResmi);
                    }
                    db.SaveChanges();
                    if (data.KullaniciID == UserIdentity.Current.Id) { UserIdentity.Current.ImagePath = data.ResimAdi.ToKullaniciResim(); }

                }

                return GetRoute(kModel.KullaniciID, yetkilendirmeyeGit);
            }

            MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, mmMessage.Messages.ToArray());
            ViewBag.ResimVar = resimVar;
            ViewBag.YetkiGrupID = new SelectList(YetkiGrupBus.CmbYetkiGruplari(), "Value", "Caption", kModel.YetkiGrupID);
            ViewBag.BirimID = new SelectList(BirimlerBus.GetBirimler().ToOrderedList("BirimID", "UstBirimID", "BirimAdi"), "BirimID", "BirimAdi", kModel.BirimID);
            ViewBag.UnvanID = new SelectList(UnvanlarBus.CmbUnvanlar(), "Value", "Caption", kModel.UnvanID);


            ViewBag.MmMessage = mmMessage;
            ViewBag.Kullanici = UserBus.GetUser(kModel.KullaniciID);
            return View(kModel);
        }

        private RedirectToRouteResult GetRoute(int kullaniciId, int? yetkilendirmeyeGit = null)
        {
            switch (yetkilendirmeyeGit)
            {
                case 1:
                    return RedirectToAction("KullaniciBirimYetkileri", new { id = kullaniciId });
                case 2:
                    return RedirectToAction("Yetkilendirme", new { id = kullaniciId });
                case 3:
                    return RedirectToAction("KullaniciHesapKodYetkileri", new { id = kullaniciId });
                case 4:
                    return RedirectToAction("KullaniciHesapNoYetkileri", new { id = kullaniciId });
                default:
                    return RedirectToAction("Index");
            }
        }
        [Authorize(Roles = RoleNames.KullaniciKayit)]
        public ActionResult Yetkilendirme(int? id)
        {
            if (id.HasValue == false) return RedirectToAction("Index");
            var kid = id;
            var roles = RollerBus.GetAllRoles().ToList();
            var userRoles = UserBus.GetUserRoles(kid.Value);
            var kullanici = UserBus.GetUser(kid.Value);
            ViewBag.Kullanici = kullanici;
            var data = roles.Select(s => new CheckObject<Roller>
            {
                Value = s,
                Disabled = userRoles.YetkiGrupRolleri.Any(a => a.RolID == s.RolID),
                Checked = userRoles.TumRoller.Any(p => p.RolID == s.RolID)
            });
            ViewBag.Roller = data;
            var kategr = roles.Select(s => s.Kategori).Distinct().ToArray();
            var menuK = db.Menulers.Where(a => a.BagliMenuID == 0 && kategr.Contains(a.MenuAdi)).ToList();
            var dct = new List<ComboModelInt>();
            foreach (var item in menuK)
            {
                dct.Add(new ComboModelInt { Value = item.SiraNo.Value, Caption = item.MenuAdi });
            }
            ViewBag.cats = dct;
            ViewBag.YetkiGrupID = new SelectList(YetkiGrupBus.CmbYetkiGruplari(), "Value", "Caption", kullanici.YetkiGrupID);
            return View();
        }
        [HttpPost, ActionName("Yetkilendirme")]
        [Authorize(Roles = RoleNames.KullaniciKayit)]
        public ActionResult Yetkilendirme(List<int> rolId, int kullaniciId, int yetkiGrupId, int? yetkilendirmeyeGit = null)
        {
            rolId = rolId ?? new List<int>();
            UserBus.SetUserRoles(kullaniciId, rolId, yetkiGrupId);
            OnlineUsers.YetkiYenile(kullaniciId);
            MessageBox.Show("Yetkiler Kaydedildi", MessageBox.MessageType.Success);
            return GetRoute(kullaniciId, yetkilendirmeyeGit);
        }
        public ActionResult GetYetkiGrubuRolIDs(int id)
        {
            var rolIDs = db.YetkiGrupRolleris.Where(p => p.YetkiGrupID == id).Select(s => new { s.RolID, s.Roller.GorunurAdi }).ToList();
            return Json(rolIDs, "application/json", JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = RoleNames.KullaniciKayit)]
        public ActionResult KullaniciBirimYetkileri(int? id)
        {
            if (id.HasValue == false) return RedirectToAction("Index");
            var birimlers = db.Birimlers.ToList();
            var tBirimlers = birimlers.ToOrderedList("BirimID", "UstBirimID", "BirimAdi");
            //var mAgacs = db.sp_MaddeAgaci().ToList();
            //foreach (var item in tMaddelers)
            //{
            //    item.MaddeAdi = mAgacs.Where(p => p.MaddeID == item.MaddeID).FirstOrDefault().MaddeTreeAdi;
            //}
            var kullanici = UserBus.GetUser(id.Value);
            ViewBag.YetkiliBirimleri = db.KullaniciBirimleris.Where(p => p.KullaniciID == id.Value).ToList();
            ViewBag.Kullanici = kullanici;
            return View(tBirimlers);
        }
        [HttpPost]
        [Authorize(Roles = RoleNames.KullaniciKayit)]
        public ActionResult KullaniciBirimYetkileri(List<int> birimId, int kullaniciId, int? yetkilendirmeyeGit = null)
        {
            if (kullaniciId <= 0)
            {
                return RedirectToAction("Index");
            }
            birimId = birimId ?? new List<int>();
            var kMadde = db.KullaniciBirimleris.Where(p => p.KullaniciID == kullaniciId).ToList();
            db.KullaniciBirimleris.RemoveRange(kMadde);
            var kul = db.Kullanicilars.First(p => p.KullaniciID == kullaniciId);
            kul.KullaniciBirimleris = birimId.Select(s => new KullaniciBirimleri { BirimID = s }).ToList();
            db.SaveChanges();
            OnlineUsers.YetkiYenile(kul.KullaniciID);
            MessageBox.Show("Birim Yetkileri Kaydedildi", MessageBox.MessageType.Success);

            return GetRoute(kullaniciId, yetkilendirmeyeGit);

        }


        [Authorize(Roles = RoleNames.KullaniciKayit)]
        public ActionResult KullaniciHesapKodYetkileri(int? id)
        {
            if (id.HasValue == false) return RedirectToAction("Index");
            var birimlers = db.GTBirimlers.OrderBy(o => o.BirimAdi).ToList();
            //var mAgacs = db.sp_MaddeAgaci().ToList();
            //foreach (var item in tMaddelers)
            //{
            //    item.MaddeAdi = mAgacs.Where(p => p.MaddeID == item.MaddeID).FirstOrDefault().MaddeTreeAdi;
            //}
            var kullanici = UserBus.GetUser(id.Value);
            ViewBag.YetkiliHesapKodlari = db.KullaniciGTHesapKodlaris.Where(p => p.KullaniciID == id.Value).ToList();
            ViewBag.Kullanici = kullanici;
            return View(birimlers);
        }
        [HttpPost]
        [Authorize(Roles = RoleNames.KullaniciKayit)]
        public ActionResult KullaniciHesapKodYetkileri(List<string> gtHesapKodIDs, int kullaniciId, int? yetkilendirmeyeGit = null)
        {
            if (kullaniciId <= 0)
            {
                return RedirectToAction("Index");
            }
            var hesapKodYetkileri = gtHesapKodIDs.Select(s => new { GTBirimID = s.Split('/')[0].ToInt().Value, GTHesapKodID = s.Split('/')[1].ToInt().Value }).ToList();

            var kul = db.Kullanicilars.First(p => p.KullaniciID == kullaniciId);
            db.KullaniciGTHesapKodlaris.RemoveRange(kul.KullaniciGTHesapKodlaris);
            kul.KullaniciGTHesapKodlaris = hesapKodYetkileri.Select(s => new KullaniciGTHesapKodlari { GTBirimID = s.GTBirimID, GTHesapKodID = s.GTHesapKodID }).ToList();
            db.SaveChanges();
            OnlineUsers.YetkiYenile(kul.KullaniciID);

            MessageBox.Show("Hesap Kodu Yetkileri Kaydedildi", MessageBox.MessageType.Success);

            return GetRoute(kullaniciId, yetkilendirmeyeGit);


        }
        [Authorize(Roles = RoleNames.KullaniciKayit)]
        public ActionResult KullaniciHesapNoYetkileri(int? id)
        {
            if (id.HasValue == false) return RedirectToAction("Index");
            var birimlers = db.GTBirimlers.OrderBy(o => o.BirimAdi).ToList();

            var kullanici = UserBus.GetUser(id.Value);
            ViewBag.YetkiliHesapNumalari = db.KullaniciGTHesapNumaralaris.Where(p => p.KullaniciID == id.Value).ToList();
            ViewBag.Kullanici = kullanici;
            return View(birimlers);
        }
        [HttpPost]
        [Authorize(Roles = RoleNames.KullaniciKayit)]
        public ActionResult KullaniciHesapNoYetkileri(List<string> gtHesapNoIDs, int kullaniciId)
        {
            if (kullaniciId <= 0)
            {
                return RedirectToAction("Index");
            }
            var hesapNoYetkileri = gtHesapNoIDs.Select(s => new { GTBirimID = s.Split('/')[0].ToInt().Value, GTHesapNoID = s.Split('/')[1].ToInt().Value }).ToList();

            var kul = db.Kullanicilars.First(p => p.KullaniciID == kullaniciId);
            db.KullaniciGTHesapNumaralaris.RemoveRange(kul.KullaniciGTHesapNumaralaris);
            kul.KullaniciGTHesapNumaralaris = hesapNoYetkileri.Select(s => new KullaniciGTHesapNumaralari { GTBirimID = s.GTBirimID, GTHesapNoID = s.GTHesapNoID }).ToList();
            db.SaveChanges();
            OnlineUsers.YetkiYenile(kul.KullaniciID);

            MessageBox.Show("Hesap Numarası Yetkileri Kaydedildi", MessageBox.MessageType.Success);
            return RedirectToAction("Index");


        }





        [Authorize(Roles = RoleNames.KullaniciKayit)]
        public ActionResult Sil(int id)
        {
            var kayit = db.Kullanicilars.FirstOrDefault(p => p.KullaniciID == id);

            string message;
            var success = true;
            if (kayit != null)
            {
                try
                {
                    message = "'" + kayit.Ad + " " + kayit.Soyad + "' Kullanıcısı Silindi!";
                    db.Kullanicilars.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.Ad + " " + kayit.Soyad + "' Kullanıcısı  Silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    SistemBilgilendirmeBus.SistemBilgisiKaydet(message, "Kullanicilar/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Kullanıcı sistemde bulunamadı!";
            }
            return Json(new { success, message }, "application/json", JsonRequestBehavior.AllowGet);
        }
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
