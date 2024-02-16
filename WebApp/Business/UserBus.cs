using BiskaUtil;
using Database;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security;
using System.Web;
using WebApp.Models;
using WebApp.Utilities.Extensions;
using WebApp.Utilities.Helpers;
using WebApp.Utilities.MenuAndRoles;
using WebApp.Utilities.SystemSetting;

namespace WebApp.Business
{
    public static class UserBus
    {
        public static List<ComboModelInt> CmbYetkiliBirimlerKullanici(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt());
            using (var db = new VysDBEntities())
            {
                var birimIDs = UserIdentity.Current.TableRollId[RollTableIdName.BirimId];
                var data = db.Vw_BirimlerTree.Where(p => p.IsMaddeEklenebilir && birimIDs.Contains(p.BirimID)).OrderBy(o => o.BirimTreeAdi).ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.BirimID, Caption = item.BirimTreeAdi });
                }
            }
            return dct;

        }
        public static List<ComboModelInt> CmbYetkiliGtBirimlerKullanici(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt());
            using (var db = new VysDBEntities())
            {

                var birimIDs = UserIdentity.Current.TableRollId[RollTableIdName.GtBirimId];
                var data = db.GTBirimlers.ToList().Where(p => birimIDs.Contains(p.GTBirimID)).OrderBy(o => o.BirimAdi).ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.GTBirimID, Caption = item.BirimAdi });
                }
            }
            return dct;

        }

        public static List<ComboModelInt> CmbYetkiliGtHesapKodKullanici(int? gtBirimId, bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt());
            using (var db = new VysDBEntities())
            {
                var gtHesapKodIDs = UserIdentity.GetSelectedTableIDs(RollTableIdName.GtHesapKodId, gtBirimId).SelectMany(s => s.RefTableIDs).ToList();
                var data = db.GTHesapKodlaris.Where(p2 => gtHesapKodIDs.Contains(p2.GTHesapKodID)).OrderBy(o => o.HesapKodAdi).ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.GTHesapKodID, Caption = item.HesapKod + " - " + item.HesapKodAdi });
                }
            }
            return dct;

        }

        public static UserIdentity GetUserIdentity(string userName)
        {
            var kull = userName != null ? GetUser(userName) : GetUser();
            if (kull == null)
            {
                FormsAuthenticationUtil.SignOut();
                return null;
            }


            var roller = GetUserRoles(kull.KullaniciID);

            var ui = new UserIdentity(userName)
            {
                YetkiGrupId = kull.YetkiGrupID,
                Id = kull.KullaniciID,
                UserKey = kull.UserKey,
            };  
            ui.Roles.AddRange(roller.TumRoller.Select(s => s.RolAdi).ToArray());
            ui.AdSoyad = kull.Ad + " " + kull.Soyad;
            ui.Description = kull.Aciklama;
            ui.EMail = kull.EMail;
            ui.IsAdmin = kull.IsAdmin;
            ui.BirimId = kull.BirimID;
            ui.TableRollId = kull.TableRollId;
            ui.SelectedTableRoll = kull.SelectedTableRoll;
            ui.HasToChahgePassword = kull.SifresiniDegistirsin;
            ui.IsActiveDirectoryImpersonateWorking = false;
            ui.IsActiveDirectoryUser = kull.IsActiveDirectoryUser;
            ui.ImagePath = kull.ResimAdi.ToKullaniciResim();
            ui.Informations.Add("FixedHeader", kull.FixedHeader);
            ui.Informations.Add("FixedSidebar", kull.FixedSidebar);
            ui.Informations.Add("ScrollSidebar", kull.ScrollSidebar);
            ui.Informations.Add("RightSidebar", kull.RightSidebar);
            ui.Informations.Add("CustomNavigation", kull.CustomNavigation);
            ui.Informations.Add("ToggledNavigation", kull.ToggledNavigation);
            ui.Informations.Add("BoxedOrFullWidth", kull.BoxedOrFullWidth);
            ui.Informations.Add("ThemeName", kull.ThemeName);
            ui.Informations.Add("BackgroundImage", kull.BackgroundImage);

            return ui;
            //return RedirectToAction("HomePage", "Home");             
        }
        public static FrKullanicilar GetUser()
        {
            var userName = HttpContext.Current.User.Identity.Name;
            return GetUser(null, userName);

        }
        public static FrKullanicilar GetUser(int kullaniciId)
        {
            return GetUser(kullaniciId, null);
        }
        public static FrKullanicilar GetUser(string kullaniciAdi)
        {
            return GetUser(null, kullaniciAdi);
        }

        private static FrKullanicilar GetUser(int? kullaniciId, string kullaniciAdi = null)
        {
            using (var db = new VysDBEntities())
            {

                var frKullanici = (from s in db.Kullanicilars
                                   join ktl in db.YetkiGruplaris on s.YetkiGrupID equals ktl.YetkiGrupID
                                   where kullaniciId.HasValue ? s.KullaniciID == kullaniciId.Value : kullaniciAdi == null || s.KullaniciAdi == kullaniciAdi
                                   select new FrKullanicilar
                                   {
                                       KullaniciID = s.KullaniciID,
                                       UserKey = s.UserKey,
                                       YetkiGrupID = s.YetkiGrupID,
                                       YetkiGrupAdi = ktl.YetkiGrupAdi,
                                       Ad = s.Ad,
                                       Soyad = s.Soyad,
                                       Tel = s.Tel,
                                       EMail = s.EMail,
                                       BirimID = s.BirimID,
                                       BirimAdi = s.Birimler.BirimAdi,
                                       UnvanID = s.UnvanID,
                                       UnvanAdi = s.Unvanlar.UnvanAdi,
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
                                       KullaniciBirimleris = s.KullaniciBirimleris,
                                       KullaniciGTHesapKodlaris = s.KullaniciGTHesapKodlaris,
                                       KullaniciGTHesapNumaralaris = s.KullaniciGTHesapNumaralaris,
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
                                       IslemYapanIP = s.IslemYapanIP,

                                   }).FirstOrDefault();
                if (frKullanici != null)
                {
                    frKullanici.BirimTreeAdi = db.sp_BirimAgaciGetBr(frKullanici.BirimID).FirstOrDefault()?.BirimTreeAdi;

                    if (frKullanici.YetkiGrupID == 2) //admin ise
                    {
                        frKullanici.TableRollId.Add(RollTableIdName.BirimId, db.Birimlers.Where(p => p.IsMaddeEklenebilir).Select(s => s.BirimID).ToList());

                    }
                    else
                    {
                        frKullanici.TableRollId.Add(RollTableIdName.BirimId, frKullanici.KullaniciBirimleris.Select(s => s.BirimID).ToList());
                    }


                    //VeriGirisi 

                    var vaSurecId = SurecIslemleriBus.GetAktifSurecId();
                    if (vaSurecId.HasValue == false)
                    {
                        var surec = db.VASurecleris.OrderByDescending(o => o.BaslangicTarihi).FirstOrDefault();
                        if (surec != null) vaSurecId = surec.VASurecID;
                    }
                    frKullanici.SelectedTableRoll.Add(new SelectedTableRoll { TableIdName = RollTableIdName.DonemId, RoleName = RoleNames.VeriGirisi, SelectedId = vaSurecId });

                    var kullaniciBirimIDs = frKullanici.TableRollId[RollTableIdName.BirimId];
                    var vgBirimId = (from s in db.VASurecleriMaddeBirims.Where(p => kullaniciBirimIDs.Contains(p.BirimID) && p.VASurecleriMadde.IsAktif)
                                     join b in db.Vw_BirimlerTree on s.BirimID equals b.BirimID
                                     select new
                                     {
                                         b.BirimID,
                                         b.BirimTreeAdi
                                     }).OrderBy(o => o.BirimTreeAdi).Select(s => s.BirimID).FirstOrDefault();
                    frKullanici.SelectedTableRoll.Add(new SelectedTableRoll { TableIdName = RollTableIdName.BirimId, RoleName = RoleNames.VeriGirisi, SelectedId = vgBirimId });


                    // Gelir Takip 

                    var kullaniciGtHesapNoIDs = frKullanici.KullaniciGTHesapNumaralaris.GroupBy(g => new { g.GTBirimID }).Select(s => new SelectedTableRoll
                    {
                        RoleName = RollTableIdName.GtHesapNoId,
                        TableId = s.Key.GTBirimID,
                        RefTableIDs = s.Select(s2 => s2.GTHesapNoID).ToList()
                    }).ToList();
                    frKullanici.SelectedTableRoll.AddRange(kullaniciGtHesapNoIDs);
                    var kullaniciGtHesapKodIDs = frKullanici.KullaniciGTHesapKodlaris.GroupBy(g => new { g.GTBirimID }).Select(s => new SelectedTableRoll
                    {
                        RoleName = RollTableIdName.GtHesapKodId,
                        TableId = s.Key.GTBirimID,
                        RefTableIDs = s.Select(s2 => s2.GTHesapKodID).ToList()
                    }).ToList();
                    frKullanici.SelectedTableRoll.AddRange(kullaniciGtHesapKodIDs);

                    var gtBirimIDs = kullaniciGtHesapNoIDs.Select(s => s.TableId).ToList();
                    gtBirimIDs.AddRange(kullaniciGtHesapKodIDs.Select(s => s.TableId));
                    frKullanici.TableRollId.Add(RollTableIdName.GtBirimId, gtBirimIDs);

                    frKullanici.SelectedTableRoll.Add(new SelectedTableRoll { TableIdName = RollTableIdName.GtBirimId, RoleName = RoleNames.GTVeriGirisi, SelectedId = gtBirimIDs.FirstOrDefault() });
                    var gtDonemId = GtDonemlerBus.GetAktifGtDonemId();
                    if (gtDonemId.HasValue == false)
                    {
                        var donem = db.GTDonemleris.OrderByDescending(o => o.BaslangicTarihi).FirstOrDefault();
                        if (donem != null) gtDonemId = donem.GTDonemID;
                    }
                    frKullanici.SelectedTableRoll.Add(new SelectedTableRoll { TableIdName = RollTableIdName.DonemId, RoleName = RoleNames.GTVeriGirisi, SelectedId = gtDonemId });

                    // Faaliyet Rapor
                    frKullanici.SelectedTableRoll.Add(new SelectedTableRoll { TableIdName = RollTableIdName.BirimId, RoleName = RoleNames.FRFormYukle, SelectedId = kullaniciBirimIDs.FirstOrDefault() });
                    var frDonemId = FrDonemlerBus.GetAktifFrDonemId();
                    if (frDonemId.HasValue == false)
                    {
                        var donem = db.FRDonemleris.OrderByDescending(o => o.BaslangicTarihi).FirstOrDefault();
                        if (donem != null) frDonemId = donem.FRDonemID;
                    }
                    frKullanici.SelectedTableRoll.Add(new SelectedTableRoll { TableIdName = RollTableIdName.DonemId, RoleName = RoleNames.FRFormYukle, SelectedId = frDonemId });

                    // Bütçe Hazırlık Rapor
                    frKullanici.SelectedTableRoll.Add(new SelectedTableRoll { TableIdName = RollTableIdName.BirimId, RoleName = RoleNames.BFRFormYukle, SelectedId = kullaniciBirimIDs.FirstOrDefault() });
                    var bfrDonemId = BfrDonemlerBus.GetAktifBfrDonemId();
                    if (bfrDonemId.HasValue == false)
                    {
                        var donem = db.BFRDonemleris.OrderByDescending(o => o.BaslangicTarihi).FirstOrDefault();
                        if (donem != null) bfrDonemId = donem.BFRDonemID;
                    }
                    frKullanici.SelectedTableRoll.Add(new SelectedTableRoll { TableIdName = RollTableIdName.DonemId, RoleName = RoleNames.BFRFormYukle, SelectedId = bfrDonemId });


                }
                return frKullanici;

            }
        }
        public static Kullanicilar Login(string uid, string pwd)
        {
            using (var db = new VysDBEntities())
            {
                var sifre = pwd.ComputeHash(StaticDefinitions.Tuz);
                var u = db.Kullanicilars.FirstOrDefault(p => p.KullaniciAdi == uid && p.Sifre == sifre);
                return u;
            }
        }
        public static URoles GetUserRoles(int kullaniciId)
        {
            var rolls = new URoles();
            using (var db = new VysDBEntities())
            {
                var kull = db.Kullanicilars.FirstOrDefault(p => p.KullaniciID == kullaniciId);
                if (kull == null) throw new SecurityException("Kullanıcı Tanımlı Değil");

                var dRoll = kull.Rollers.ToList();

                var ygRols = kull.YetkiGruplari.YetkiGrupRolleris.Select(s => s.Roller).ToList();
                rolls.YetkiGrupID = kull.YetkiGrupID;
                rolls.YetkiGrupAdi = kull.YetkiGruplari.YetkiGrupAdi;
                rolls.YetkiGrupRolleri = ygRols;
                rolls.TumRoller.AddRange(ygRols);
                rolls.TumRoller.AddRange(dRoll.Where(p => ygRols.All(a => a.RolID != p.RolID)).Distinct());
                rolls.EklenenRoller.AddRange(rolls.TumRoller.Where(p => rolls.YetkiGrupRolleri.Any(a => a.RolID == p.RolID) == false));
                return rolls;


            }
        }
        public static Menuler[] GetUserMenus()
        {
            string userName = HttpContext.Current.User.Identity.Name;
            if (userName.IsNullOrWhiteSpace()) return new Menuler[] { };
            using (VysDBEntities db = new VysDBEntities())
            {
                var menus = new List<Menuler>();
                var kull = db.Kullanicilars.FirstOrDefault(p => p.KullaniciAdi == userName);
                if (kull == null) FormsAuthenticationUtil.SignOut();
                var kullRoll = kull.Rollers.SelectMany(s => s.Menulers).Distinct().OrderBy(o => o.SiraNo).ToList();
                var ygRoll = kull.YetkiGruplari.YetkiGrupRolleris.SelectMany(s => s.Roller.Menulers).Distinct().OrderBy(o => o.SiraNo).ToList();
                menus.AddRange(kullRoll);
                menus.AddRange(ygRoll.Where(p => kullRoll.All(a => a.MenuID != p.MenuID)));
                return menus.ToArray();
            }
        }


        public static void SetUserRoles(int kullaniciId, List<int> rolIDs, int yetkiGrupId)
        {
            using (var db = new VysDBEntities())
            {
                var k = db.Kullanicilars.FirstOrDefault(p => p.KullaniciID == kullaniciId);
                if (k != null)
                {
                    var droles = k.Rollers.ToArray();
                    foreach (var drole in droles)
                        k.Rollers.Remove(drole);
                    k.YetkiGrupID = yetkiGrupId;
                    db.SaveChanges();
                    var uRoles = GetUserRoles(k.KullaniciID);
                    rolIDs = rolIDs.Where(p => uRoles.YetkiGrupRolleri.All(a => a.RolID != p)).ToList();

                    if (rolIDs.Count <= 0) return;

                    var newRoles = db.Rollers.Where(p => rolIDs.Contains(p.RolID));
                    foreach (var nr in newRoles)
                        k.Rollers.Add(nr);
                    db.SaveChanges();

                }
                else
                    throw new SecurityException("Kullanıcı Tanımlı Değil");
            }
        }
        public static string ResimKaydet(HttpPostedFileBase resim)
        {
            try
            {
                var fileStream = resim.InputStream;
                var bmp = new Bitmap(fileStream);

                var folderName = SistemAyar.KullaniciResimYolu.GetAyar();
                var rotasYonDegisimLog = SistemAyar.RotasyonuDegisenResimleriLogla.GetAyar().ToBoolean().Value;
                var kaliteOpt = SistemAyar.KullaniciResimKaydiKaliteOpt.GetAyar().ToBoolean().Value;
                var boyutlandirma = SistemAyar.KullaniciResimKaydiBoyutlandirma.GetAyar().ToBoolean().Value;




                var unqCode = Guid.NewGuid().ToString().Substring(0, 6);
                var i = 0;
                var fileName = "";
                //var ext = "." + Resim.FileName.Split('.').Last();
                foreach (var item in Path.GetFileName(resim.FileName).Split('.'))
                {
                    i++;
                    if (i != Path.GetFileName(resim.FileName).Split('.').Count()) fileName += item;
                    //else fileName += ".jpg";
                }

                fileName = unqCode.ReplaceSpecialCharacter() + "_" + fileName.ReplaceSpecialCharacter() + ".jpg";
                var path = Path.Combine(HttpContext.Current.Server.MapPath("~/" + folderName), fileName);


                if (boyutlandirma)
                {
                    try
                    {
                        var uzn = SistemAyar.KullaniciResimKaydiHeightPx.GetAyar();
                        var gens = SistemAyar.KullaniciResimKaydiWidthPx.GetAyar();

                        var uzunluk = uzn.IsNullOrWhiteSpace() ? 560 : uzn.ToInt().Value;
                        var genislik = gens.IsNullOrWhiteSpace() ? 560 : gens.ToInt().Value;
                        var img = bmp.ResizeImage(new Size(genislik, uzunluk));
                        img.Save(path, ImageFormat.Jpeg);
                    }
                    catch (Exception ex)
                    {
                        SistemBilgilendirmeBus.SistemBilgisiKaydet(ex, "Resmin boyutlandırma işlemi yapılıp kayıt edilirken bir hata oluştu.\r\n Hata:" + ex.ToExceptionMessage(), BilgiTipi.OnemsizHata);
                    }
                }
                else
                {
                    bmp.Save(path, ImageFormat.Jpeg);
                }

                if (kaliteOpt)
                {
                    #region Quality check
                    try
                    {

                        var bmpQ = new Bitmap(path);

                        var jpgEncoder = ImageHelper.GetEncoder(ImageFormat.Jpeg);


                        var quality = 100L;
                        if (resim.ContentLength >= 80000 && resim.ContentLength < 200000) quality = 80;
                        else if (resim.ContentLength >= 200000 && resim.ContentLength < 400000) quality = 70;
                        else if (resim.ContentLength >= 400000 && resim.ContentLength < 600000) quality = 60;
                        else if (resim.ContentLength >= 600000 && resim.ContentLength < 800000) quality = 50;
                        else if (resim.ContentLength >= 800000 && resim.ContentLength < 1000000) quality = 40;
                        else if (resim.ContentLength >= 1000000) quality = 30;
                        var myEncoder = Encoder.Quality;
                        var path2 = path + Guid.NewGuid().ToString().Substring(0, 4) + ".jpg";
                        var myEncoderParameters = new EncoderParameters(1);
                        var myEncoderParameter = new EncoderParameter(myEncoder, quality);
                        myEncoderParameters.Param[0] = myEncoderParameter;
                        bmpQ.Save(path2, jpgEncoder, myEncoderParameters);
                        bmpQ.Dispose();
                        if (File.Exists(path))
                            File.Delete(path);
                        var imgTmp = Image.FromFile(path2);
                        imgTmp.Save(path, ImageFormat.Jpeg);
                        imgTmp.Dispose();
                        File.Delete(path2);
                    }
                    catch (Exception errQuality)
                    {
                        SistemBilgilendirmeBus.SistemBilgisiKaydet(errQuality, "Resmin kalitesi değiştirilirken hata oluştu.\r\n Hata:" + errQuality.ToExceptionMessage(), BilgiTipi.OnemsizHata);
                    }
                    #endregion
                }

                #region Rotation
                try
                {

                    var img1 = Image.FromFile(path);
                    var prop = img1.PropertyItems.FirstOrDefault(p => p.Id == 0x0112);
                    if (prop != null)
                    {
                        int orientationValue = img1.GetPropertyItem(prop.Id).Value[0];
                        var rotateFlipType = ImageHelper.GetOrientationToFlipType(orientationValue);
                        img1.RotateFlip(rotateFlipType);
                        var path2 = path + Guid.NewGuid().ToString().Substring(0, 4) + ".jpg";
                        img1.Save(path2);
                        img1.Dispose();
                        if (File.Exists(path))
                            File.Delete(path);
                        var imgTmp = Image.FromFile(path2);
                        imgTmp.Save(path, ImageFormat.Jpeg);
                        imgTmp.Dispose();
                        File.Delete(path2);
                        if (rotasYonDegisimLog)
                        {
                            SistemBilgilendirmeBus.SistemBilgisiKaydet("Rotasyon farklılığı görünen resim düzeltildi! Resim:" + path, "Management/resimKaydet", BilgiTipi.Bilgi);
                        }
                    }
                }
                catch (Exception ex)
                {
                    SistemBilgilendirmeBus.SistemBilgisiKaydet(ex, "Hesap kayıt sırasında resim rotasyonu yapılırken bir hata oluştu.\r\n Hata:" + ex.ToExceptionMessage(), BilgiTipi.OnemsizHata);
                }
                #endregion


                return fileName;
            }
            catch (Exception ex)
            {
                SistemBilgilendirmeBus.SistemBilgisiKaydet("Resim kaydedilirken bir hata oluştu! Hata: " + ex.ToExceptionMessage(), ex.ToExceptionStackTrace(), BilgiTipi.Hata, null, UserIdentity.Ip);
                return "";
            }
        }
        public static void SetLastLogon()
        {
            var userName = HttpContext.Current.User.Identity.Name;
            using (var db = new VysDBEntities())
            {
                var kull = db.Kullanicilars.FirstOrDefault(p => p.KullaniciAdi == userName);
                if (kull == null) return;
                kull.IslemYapanID = UserIdentity.Current.Id;
                kull.LastLogonDate = DateTime.Now;
                kull.LastLogonIP = UserIdentity.Ip;
                db.SaveChanges();
            }
        }
    }
}