using BiskaUtil;
using Database;
using System;
using System.IO;
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


namespace WebApp.Controllers
{
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [Authorize(Roles = RoleNames.BFRFormYukle)]
    public class BfrFormYukleController : Controller
    {
        private readonly VysDBEntities db = new VysDBEntities();


        // GET: FRFormYukle
        public ActionResult Index()
        {
            var birimId = UserIdentity.GetPageSelectedTableId(RoleNames.BFRFormYukle, RollTableIdName.BirimId);
            var bfrDonemId = UserIdentity.GetPageSelectedTableId(RoleNames.BFRFormYukle, RollTableIdName.DonemId);

            if (!db.BFRDonemleris.Any(a => a.BFRDonemID == bfrDonemId)) bfrDonemId = db.BFRDonemleris.OrderByDescending(o => o.Yil).Select(s => s.BFRDonemID).FirstOrDefault();
            return Index(new FmBfrFormYukle { PageSize = 30, BfrDonemId = bfrDonemId, Expand = bfrDonemId.HasValue, BirimId = birimId });
        }
        [HttpPost]
        public ActionResult Index(FmBfrFormYukle model, bool export = false)
        {
            model.BfrDonemId = model.BfrDonemId ?? 0;
            var birimYetkileri = UserIdentity.Current.TableRollId[RollTableIdName.BirimId];

            UserIdentity.SetPageSelectedTableId(RoleNames.BFRFormYukle, RollTableIdName.BirimId, model.BirimId);
            UserIdentity.SetPageSelectedTableId(RoleNames.BFRFormYukle, RollTableIdName.DonemId, model.BfrDonemId);

            var surecBilgi = BfrDonemlerBus.GetBfrDonemKontrol(model.BfrDonemId.Value);
            model.SurecBilgi = surecBilgi;
            model.IsAktif = surecBilgi != null && (surecBilgi.IsAktif && model.SurecBilgi.AktifSurec);
            var q = (from s in db.BFRDonemleris.Where(p => p.BFRDonemID == model.BfrDonemId)
                     join df in db.BFRDonemlerForms.Where(p => p.BFRDonemlerFormBirims.Any(a => birimYetkileri.Contains(a.BirimID) && a.BirimID == model.BirimId)) on s.BFRDonemID equals df.BFRDonemID
                     join fr in db.BFRFormlars on df.BFRFormID equals fr.BFRFormID
                     let yF = db.BFRDonemlerFormGirisleris.FirstOrDefault(p => p.BFRDonemlerForm.BFRDonemlerFormID == df.BFRDonemlerFormID && p.BirimID == model.BirimId)
                     join kul in db.Kullanicilars on yF.IslemYapanID equals kul.KullaniciID into defK
                     from k in defK.DefaultIfEmpty()
                     where df.IsAktif
                     select new FrBfrFormYukle
                     {
                         BirimID = model.BirimId.Value,
                         BFRFormID = fr.BFRFormID,
                         FormAdi = fr.FormAdi,
                         Aciklama = fr.Aciklama,
                         FormDosyaAdi = fr.DosyaAdi,
                         FormDosyaYolu = fr.DosyaYolu,
                         DosyaAdi = yF != null ? yF.DosyaAdi : null,
                         DosyaYolu = yF != null ? yF.DosyaYolu : null,
                         IslemYapan = k != null ? k.Ad + " " + k.Soyad : "",
                         FormYuklendi = yF != null,
                         BfrDonemlerFormGirisleri = yF,
                     }).AsQueryable();

            if (!model.Aranan.IsNullOrWhiteSpace()) q = q.Where(p => p.FormAdi.Contains(model.Aranan));
            if (model.FormYuklemeDurumId.HasValue) q = q.Where(p => p.FormYuklendi == model.FormYuklemeDurumId.Value);
            model.RowCount = q.Count(); 
            model.AktifCount = q.Count(p => p.FormYuklendi);  
            q = !model.Sort.IsNullOrWhiteSpace() ? q.OrderBy(model.Sort) : q.OrderBy(o => o.DosyaAdi);


            model.Data = q.Skip(model.PagingStartRowIndex).Take(model.PageSize).ToList(); ;
            ViewBag.BFRDonemID = new SelectList(BfrFormYukleBus.CmbBfrDonemler(false), "Value", "Caption", model.BfrDonemId);
            ViewBag.BirimID = new SelectList(UserBus.CmbYetkiliBirimlerKullanici(false), "Value", "Caption", model.BirimId);
            ViewBag.FormYuklemeDurumID = new SelectList(ComboData.CmbFormYuklemeDurum(), "Value", "Caption", model.FormYuklemeDurumId);

            return View(model);


        }


        public ActionResult GetFormYukle(int bfrDonemId, int bfrFormId, int birimId)
        {
            var bfrDonem = BfrDonemlerBus.GetBfrDonemKontrol(bfrDonemId);
            var form = db.BFRFormlars.First(p => p.BFRFormID == bfrFormId);
            var birim = db.Vw_BirimlerTree.First(p => p.BirimID == birimId);
            var yuklenenForm = db.BFRDonemlerFormGirisleris.FirstOrDefault(p => p.BFRDonemlerForm.BFRDonemID == bfrDonemId && p.BFRFormID == bfrFormId && p.BirimID == birimId);
            ViewBag.Form = form;
            ViewBag.Birim = birim;
            ViewBag.YuklenenForm = yuklenenForm;

            ViewBag.BFRDonem = bfrDonem;
            return View();

        }

        [HttpPost]
        [Authorize(Roles = RoleNames.BFRFormYukleKayitYetkisi)]
        public ActionResult FormYuklePost(int bfrDonemId, int birimId, int bfrFormId, HttpPostedFileBase dosyaEki)
        {

            var mMessage = new MmMessage
            {
                IsSuccess = false,
                Title = "Faaliyet raporu form yükleme işlemi",
                MessageType = Msgtype.Warning
            };
            var bfrDonem = BfrDonemlerBus.GetBfrDonemKontrol(bfrDonemId);

            var pModel = new DosyaYuklePartialModelBFR
            {
                BFRFormID = bfrFormId
            };

            if (!bfrDonem.IsAktif || !bfrDonem.AktifSurec)
            {
                mMessage.Messages.Add(bfrDonem.DonemYilAdi + " Aktif değildir veri yükleme işlemi yapılamaz!");
            }
            else if (!UserIdentity.Current.TableRollId[RollTableIdName.BirimId].Contains(birimId))
            {
                mMessage.Messages.Add("İşlem yapmaya çalıştığınız birim için yetkili değilsiniz.");
            }
            if (dosyaEki == null)
            {
                mMessage.Messages.Add("Bütçe Hazırlık raporu form dosyası ekleyiniz.");
            }
            else
            {
                var extension = Path.GetExtension(dosyaEki.FileName);
                if (extension != ".xls" && extension != ".xlsx")
                {
                    mMessage.Messages.Add(dosyaEki.FileName + " doyasının excel formatında olması gerekmektedir. Eki kaldırın ve Excel formatında tekrar ekleyiniz.");
                }


                if (mMessage.Messages.Count == 0)
                {
                    var oncekiDosyaYolu = "";
                    try
                    {
                        var donemForm = db.BFRDonemlerForms.First(p => p.BFRDonemID == bfrDonemId && p.BFRFormID == bfrFormId);
                        var birim = db.Birimlers.First(p => p.BirimID == birimId);

                        var unqCode = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
                        extension = Path.GetExtension(dosyaEki.FileName);
                        var fileName = bfrDonem.Yil + "_" + birim.BirimKod + "_" + dosyaEki.FileName.Replace(extension, "").ReplaceSpecialCharacter() + "_" + unqCode.ReplaceSpecialCharacter() + extension;
                        var path = "/BFRFormYuklemeleri/" + fileName;
                        var form = donemForm.BFRDonemlerFormGirisleris.FirstOrDefault(p => p.BirimID == birimId);
                        if (form != null)
                        {
                            oncekiDosyaYolu = form.DosyaYolu;
                            form.DosyaAdi = dosyaEki.FileName;
                            form.DosyaYolu = path;
                        }
                        else
                        {
                            form = db.BFRDonemlerFormGirisleris.Add(new BFRDonemlerFormGirisleri
                            {
                                BFRDonemlerFormID = donemForm.BFRDonemlerFormID,
                                BirimID = birimId,
                                BFRFormID = bfrFormId,
                                DosyaAdi = dosyaEki.FileName,
                                DosyaYolu = path,
                                IslemTarihi = DateTime.Now,
                                IslemYapanID = UserIdentity.Current.Id,
                                IslemYapanIP = UserIdentity.Ip

                            });
                        }
                        db.SaveChanges();

                        dosyaEki.SaveAs(Server.MapPath("~" + path));
                        if (!oncekiDosyaYolu.IsNullOrWhiteSpace())
                        {
                            path = Server.MapPath("~" + oncekiDosyaYolu);
                            if (System.IO.File.Exists(path))
                            {
                                try
                                {
                                    System.IO.File.Delete(path);
                                }
                                catch (Exception ex)
                                {
                                    var msg = "Yüklenen eski form dosyası silinirken bir hata oluştu! <br /> DosyaYolu:" + path + " <br /> Hata: " + ex.ToExceptionMessage();
                                    SistemBilgilendirmeBus.SistemBilgisiKaydet(msg, ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                                }
                            }
                        }

                        pModel.EklenenDosya = form;
                        pModel.KayitYetkisi = true;


                        mMessage.IsSuccess = true;
                        mMessage.Messages.Add("Seçilen faaliyet raporu form dosyası sisteme yüklendi.");
                        mMessage.MessageType = Msgtype.Success;

                    }
                    catch (Exception ex)
                    {
                        var msg = "Bütçe Hazırlık raporu form dosyası yükleme işlemi yapılırken bir hata oluştu! Hata:" + ex.ToExceptionMessage();
                        mMessage.Messages.Add(msg);
                        SistemBilgilendirmeBus.SistemBilgisiKaydet(msg, ex.ToExceptionStackTrace(), BilgiTipi.Hata);
                    }
                }

            }
            var pView = "";
            if (mMessage.IsSuccess)
            {
                pView = ViewRenderHelper.RenderPartialView("BFRFormYukle", "DosyaYuklePartial", pModel);
            }
            return new { mMessage, pView }.ToJsonResult();
        }

        public ActionResult DosyaYuklePartial()
        {
            return View();
        }
        public ActionResult GetDetail(int bfrDonemId, int birimId, int bfrFormId)
        {
            var birimIDs = UserIdentity.Current.TableRollId[RollTableIdName.BirimId];
            var mdl = new FrFaaliyetDetayModelBf
            {
                BfrDonemId = bfrDonemId,
                BirimId = birimId,
                BfrFormId = bfrFormId
            };
            mdl.BirimBilgi = db.Vw_BirimlerTree.First(p => p.BirimID == mdl.BirimId);
            mdl.BFrFaaliyetler = (from s in db.BFRDonemleris.Where(p => p.BFRDonemID == bfrDonemId)
                                  join df in db.BFRDonemlerForms.Where(p => p.BFRDonemlerFormBirims.Any(a => birimIDs.Contains(a.BirimID) && a.BirimID == birimId)) on s.BFRDonemID equals df.BFRDonemID
                                  join fr in db.BFRFormlars on df.BFRFormID equals fr.BFRFormID
                                  let yF = db.BFRDonemlerFormGirisleris.FirstOrDefault(p => p.BFRDonemlerForm.BFRDonemlerFormID == df.BFRDonemlerFormID && p.BirimID == birimId)
                                  join kul in db.Kullanicilars on yF.IslemYapanID equals kul.KullaniciID into defK
                                  from k in defK.DefaultIfEmpty()
                                  where fr.BFRFormID == bfrFormId
                                  select new FrBfrFormYukle
                                  {
                                      BirimID = birimId,
                                      BFRFormID = fr.BFRFormID,
                                      FormAdi = fr.FormAdi,
                                      Aciklama = fr.Aciklama,
                                      FormDosyaAdi = fr.DosyaAdi,
                                      FormDosyaYolu = fr.DosyaYolu,
                                      DosyaAdi = yF != null ? yF.DosyaAdi : null,
                                      DosyaYolu = yF != null ? yF.DosyaYolu : null,
                                      IslemYapan = k != null ? k.Ad + " " + k.Soyad : "",
                                      IslemTarihi = yF != null ? yF.IslemTarihi : DateTime.Now,
                                      IslemYapanIP = yF != null ? yF.IslemYapanIP : null,
                                      FormYuklendi = yF != null,
                                      BfrDonemlerFormGirisleri = yF,
                                  }).FirstOrDefault();


            var page = ViewRenderHelper.RenderPartialView("BFRFormYukle", "DetaySablon", mdl);
            return Json(new
            {
                page,
                UserIdentity.Current.IsAuthenticated
            }, "application/json", JsonRequestBehavior.AllowGet);
        }

        public ActionResult FormSil(int id)
        {

            var kayit = db.BFRDonemlerFormGirisleris.FirstOrDefault(p => p.BFRDonemlerFormGirisID == id);
            var message = "";
            var pModel = new DosyaYuklePartialModelBFR
            {
                KayitYetkisi = true
            };
            var success = true;
            if (kayit != null)
            {
                var frFormId = kayit.BFRDonemlerForm.BFRFormID;
                var frDonemId = kayit.BFRDonemlerForm.BFRDonemID;

                pModel.BFRFormID = frFormId;
                var birimIDs = UserIdentity.Current.TableRollId[RollTableIdName.BirimId];
                var mMessage = new MmMessage
                {
                    Title = "Bütçe Hazırlık Raporu Form Dosyası Silme İşlemi",
                    MessageType = Msgtype.Warning
                };

                var kayitYetki = RoleNames.BFRFormYukleKayitYetkisi.InRole();
                var birimYetki = birimIDs.Contains(kayit.BirimID);
                var surec = FrDonemlerBus.GetFrDonemKontrol(frDonemId);
                if (!surec.IsAktif || !surec.AktifSurec)
                {
                    mMessage.Messages.Add("İşlem yapmaya çalıştığınız dönem aktif olmadığından silme işlemi yapamazsınız");
                }
                else if (!kayitYetki)
                {
                    mMessage.Messages.Add("Veri girişi için yetkiniz bulunmamaktadır.");
                }
                else if (!birimYetki)
                {
                    mMessage.Messages.Add("İşlem yapmaya çalıştığınız birim yetkiniz dahilinde değildir.");
                }
                else
                {
                    try
                    {
                        message = "'" + kayit.BFRFormlar.FormAdi + "' formuna ait '" + kayit.DosyaAdi + "' dosyası sistemden silindi!";
                        db.BFRDonemlerFormGirisleris.Remove(kayit);
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        success = false;
                        message = "'" + kayit.BFRFormlar.FormAdi + "' formuna ait '" + kayit.DosyaAdi + "' dosyası sistemden silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                        SistemBilgilendirmeBus.SistemBilgisiKaydet(message, "BFRFormYukle/FormSil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                    }

                }
                if (mMessage.Messages.Count > 0)
                {
                    message = mMessage.Messages.First();
                    success = false;
                }

            }
            else
            {
                success = false;
                message = "Silmek istenen form dosyası sistemde bulunamadı!";
            }
            if (!success)
                pModel.EklenenDosya = kayit;
            var pView = ViewRenderHelper.RenderPartialView("FRFormYukle", "DosyaYuklePartial", pModel);
            return Json(new { success, message, pView }, "application/json", JsonRequestBehavior.AllowGet);
        }
    }
}