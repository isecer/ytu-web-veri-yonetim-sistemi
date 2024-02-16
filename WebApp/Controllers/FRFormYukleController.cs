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
    [Authorize(Roles = RoleNames.FRFormYukle)]
    public class FrFormYukleController : Controller
    {
        private readonly VysDBEntities db = new VysDBEntities();


        // GET: FRFormYukle
        public ActionResult Index()
        {
            var birimId = UserIdentity.GetPageSelectedTableId(RoleNames.FRFormYukle, RollTableIdName.BirimId);
            var frDonemId = UserIdentity.GetPageSelectedTableId(RoleNames.FRFormYukle, RollTableIdName.DonemId);

            if (!db.FRDonemleris.Any(a => a.FRDonemID == frDonemId)) frDonemId = db.FRDonemleris.OrderByDescending(o => o.Yil).Select(s => s.FRDonemID).FirstOrDefault();
            return Index(new FmFrFormYukle { PageSize = 30, FrDonemId = frDonemId, Expand = frDonemId.HasValue, BirimId = birimId });
        }
        [HttpPost]
        public ActionResult Index(FmFrFormYukle model)
        {
            model.FrDonemId = model.FrDonemId ?? 0;
            var birimYetkileri = UserIdentity.Current.TableRollId[RollTableIdName.BirimId];

            UserIdentity.SetPageSelectedTableId(RoleNames.FRFormYukle, RollTableIdName.BirimId, model.BirimId);
            UserIdentity.SetPageSelectedTableId(RoleNames.FRFormYukle, RollTableIdName.DonemId, model.FrDonemId);

            var surecBilgi = FrDonemlerBus.GetFrDonemKontrol(model.FrDonemId.Value);
            model.SurecBilgi = surecBilgi;
            model.IsAktif = surecBilgi != null && (surecBilgi.IsAktif && model.SurecBilgi.AktifSurec);
            var q = (from s in db.FRDonemleris.Where(p => p.FRDonemID == model.FrDonemId)
                     join df in db.FRDonemlerForms.Where(p => p.FRDonemlerFormBirims.Any(a => birimYetkileri.Contains(a.BirimID) && a.BirimID == model.BirimId)) on s.FRDonemID equals df.FRDonemID
                     join fr in db.FRFormlars on df.FRFormID equals fr.FRFormID
                     let yF = db.FRDonemlerFormGirisleris.FirstOrDefault(p => p.FRDonemlerForm.FRDonemlerFormID == df.FRDonemlerFormID && p.BirimID == model.BirimId)
                     join kul in db.Kullanicilars on yF.IslemYapanID equals kul.KullaniciID into defK
                     from k in defK.DefaultIfEmpty()
                     where df.IsAktif
                     select new FrFrFormYukle
                     {
                         BirimID = model.BirimId.Value,
                         FRFormID = fr.FRFormID,
                         FormAdi = fr.FormAdi,
                         Aciklama = fr.Aciklama,
                         FormDosyaAdi = fr.DosyaAdi,
                         FormDosyaYolu = fr.DosyaYolu,
                         DosyaAdi = yF != null ? yF.DosyaAdi : null,
                         DosyaYolu = yF != null ? yF.DosyaYolu : null,
                         IslemYapan = k != null ? k.Ad + " " + k.Soyad : "",
                         FormYuklendi = yF != null,
                         FrDonemlerFormGirisleri = yF,
                     }).AsQueryable();

            if (!model.Aranan.IsNullOrWhiteSpace()) q = q.Where(p => p.FormAdi.Contains(model.Aranan));
            if (model.FormYuklemeDurumId.HasValue) q = q.Where(p => p.FormYuklendi == model.FormYuklemeDurumId.Value);

            model.RowCount = q.Count();
            model.AktifCount = q.Count(p => p.FormYuklendi);


            q = !model.Sort.IsNullOrWhiteSpace() ? q.OrderBy(model.Sort) : q.OrderBy(o => o.DosyaAdi);


            model.Data = q.Skip(model.PagingStartRowIndex).Take(model.PageSize).ToList(); ;
            ViewBag.FRDonemID = new SelectList(FrFormYukleBus.CmbFrDonemler(false), "Value", "Caption", model.FrDonemId);
            ViewBag.BirimID = new SelectList(UserBus.CmbYetkiliBirimlerKullanici(false), "Value", "Caption", model.BirimId);
            ViewBag.FormYuklemeDurumID = new SelectList(ComboData.CmbFormYuklemeDurum(), "Value", "Caption", model.FormYuklemeDurumId);

            return View(model);


        }


        public ActionResult GetFormYukle(int frDonemId, int frFormId, int birimId)
        {
            var frDonem = FrDonemlerBus.GetFrDonemKontrol(frDonemId);
            var form = db.FRFormlars.First(p => p.FRFormID == frFormId);
            var birim = db.Vw_BirimlerTree.First(p => p.BirimID == birimId);
            var yuklenenForm = db.FRDonemlerFormGirisleris.FirstOrDefault(p => p.FRDonemlerForm.FRDonemID == frDonemId && p.FRFormID == frFormId && p.BirimID == birimId);
            ViewBag.Form = form;
            ViewBag.Birim = birim;
            ViewBag.YuklenenForm = yuklenenForm;

            ViewBag.FRDonem = frDonem;
            return View();

        }

        [HttpPost]
        [Authorize(Roles = RoleNames.FRFormYukleKayitYetkisi)]
        public ActionResult FormYuklePost(int frDonemId, int birimId, int frFormId, HttpPostedFileBase dosyaEki)
        {

            var mMessage = new MmMessage
            {
                IsSuccess = false,
                Title = "Faaliyet raporu form yükleme işlemi",
                MessageType = Msgtype.Warning
            };
            var frDonem = FrDonemlerBus.GetFrDonemKontrol(frDonemId);

            var pModel = new DosyaYuklePartialModel
            {
                FRFormID = frFormId
            };

            if (!frDonem.IsAktif || !frDonem.AktifSurec)
            {
                mMessage.Messages.Add(frDonem.DonemYilAdi + " Aktif değildir veri yükleme işlemi yapılamaz!");
            }
            else if (!UserIdentity.Current.TableRollId[RollTableIdName.BirimId].Contains(birimId))
            {
                mMessage.Messages.Add("İşlem yapmaya çalıştığınız birim için yetkili değilsiniz.");
            }
            if (dosyaEki == null)
            {
                mMessage.Messages.Add("Faaliyet raporu form dosyası ekleyiniz.");
            }
            else
            {
                var extension = Path.GetExtension(dosyaEki.FileName);
                if (extension != ".xls" && extension != ".xlsx")
                {
                    mMessage.Messages.Add(dosyaEki.FileName + " doyasının excel formatında olması gerekmektedir. Eki kaldırın ve Excel formatında tekrar ekleyiniz.");
                }
            }

            if (mMessage.Messages.Count == 0)
            {
                try
                {
                    var donemForm = db.FRDonemlerForms.First(p => p.FRDonemID == frDonemId && p.FRFormID == frFormId);
                    var birim = db.Birimlers.First(p => p.BirimID == birimId);

                    var unqCode = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
                    var extension = Path.GetExtension(dosyaEki.FileName);
                    var fileName = frDonem.Yil + "_" + birim.BirimKod + "_" + dosyaEki.FileName.Replace(extension, "").ReplaceSpecialCharacter() + "_" + unqCode.ReplaceSpecialCharacter() + extension;
                    var path = "/FRFormYuklemeleri/" + fileName;
                    var form = donemForm.FRDonemlerFormGirisleris.FirstOrDefault(p => p.BirimID == birimId);
                    var oncekiDosyaYolu = "";
                    if (form != null)
                    {
                        oncekiDosyaYolu = form.DosyaYolu;
                        form.DosyaAdi = dosyaEki.FileName;
                        form.DosyaYolu = path;
                    }
                    else
                    {
                        form = db.FRDonemlerFormGirisleris.Add(new FRDonemlerFormGirisleri
                        {
                            FRDonemlerFormID = donemForm.FRDonemlerFormID,
                            BirimID = birimId,
                            FRFormID = frFormId,
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
                    var msg = "Faaliyet raporu form dosyası yükleme işlemi yapılırken bir hata oluştu! Hata:" + ex.ToExceptionMessage();
                    mMessage.Messages.Add(msg);
                    SistemBilgilendirmeBus.SistemBilgisiKaydet(msg, ex.ToExceptionStackTrace(), BilgiTipi.Hata);
                }
            }
            var pView = "";
            if (mMessage.IsSuccess)
            {
                pView = ViewRenderHelper.RenderPartialView("FRFormYukle", "DosyaYuklePartial", pModel);
            }
            return new { mMessage, pView }.ToJsonResult();
        }

        public ActionResult DosyaYuklePartial()
        {
            return View();
        }
        public ActionResult GetDetail(int frDonemId, int birimId, int frFormId)
        {
            var birimIDs = UserIdentity.Current.TableRollId[RollTableIdName.BirimId];
            var mdl = new FrFaaliyetDetayModel
            {
                FrDonemId = frDonemId,
                BirimId = birimId,
                FrFormId = frFormId
            };
            mdl.BirimBilgi = db.Vw_BirimlerTree.First(p => p.BirimID == mdl.BirimId);
            mdl.FrFaaliyetler = (from s in db.FRDonemleris.Where(p => p.FRDonemID == frDonemId)
                                 join df in db.FRDonemlerForms.Where(p => p.FRDonemlerFormBirims.Any(a => birimIDs.Contains(a.BirimID) && a.BirimID == birimId)) on s.FRDonemID equals df.FRDonemID
                                 join fr in db.FRFormlars on df.FRFormID equals fr.FRFormID
                                 let yF = db.FRDonemlerFormGirisleris.FirstOrDefault(p => p.FRDonemlerForm.FRDonemlerFormID == df.FRDonemlerFormID && p.BirimID == birimId)
                                 join kul in db.Kullanicilars on yF.IslemYapanID equals kul.KullaniciID into defK
                                 from k in defK.DefaultIfEmpty()
                                 where fr.FRFormID == frFormId
                                 select new FrFrFormYukle
                                 {
                                     BirimID = birimId,
                                     FRFormID = fr.FRFormID,
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
                                     FrDonemlerFormGirisleri = yF,
                                 }).FirstOrDefault();


            var page = ViewRenderHelper.RenderPartialView("FRFormYukle", "DetaySablon", mdl);
            return Json(new
            {
                page,
                UserIdentity.Current.IsAuthenticated
            }, "application/json", JsonRequestBehavior.AllowGet);
        }
        public ActionResult DetaySablon(FrFaaliyetDetayModel model)
        {
            return View(model);
        }
        public ActionResult FormSil(int id)
        {

            var kayit = db.FRDonemlerFormGirisleris.FirstOrDefault(p => p.FRDonemlerFormGirisID == id);
            var message = "";
            var pModel = new DosyaYuklePartialModel
            {
                KayitYetkisi = true
            };
            var success = true;
            if (kayit != null)
            {
                var frFormId = kayit.FRDonemlerForm.FRFormID;
                var frDonemId = kayit.FRDonemlerForm.FRDonemID;

                pModel.FRFormID = frFormId;
                var birimIDs = UserIdentity.Current.TableRollId[RollTableIdName.BirimId];
                var mMessage = new MmMessage
                {
                    Title = "Faaliyet Raporu Form Dosyası Silme İşlemi",
                    MessageType = Msgtype.Warning
                };

                var kayitYetki = RoleNames.FRFormYukleKayitYetkisi.InRole();
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
                        message = "'" + kayit.FRFormlar.FormAdi + "' formuna ait '" + kayit.DosyaAdi + "' dosyası sistemden silindi!";
                        db.FRDonemlerFormGirisleris.Remove(kayit);
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        success = false;
                        message = "'" + kayit.FRFormlar.FormAdi + "' formuna ait '" + kayit.DosyaAdi + "' dosyası sistemden silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                        SistemBilgilendirmeBus.SistemBilgisiKaydet(message, "FRFormYukle/FormSil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
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
                message = "Silmek istediğiniz form dosyası sistemde bulunamadı!";
            }
            if (!success)
                pModel.EklenenDosya = kayit;
            var pView = ViewRenderHelper.RenderPartialView("FRFormYukle", "DosyaYuklePartial", pModel);
            return Json(new { success, message, pView }, "application/json", JsonRequestBehavior.AllowGet);
        }
    }
}