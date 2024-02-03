using BiskaUtil;
using Database;
using System;
using System.Collections.Generic;
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
    [Authorize(Roles = RoleNames.FRFormOluştur)]
    public class FrFormOlusturController : Controller
    {
        // GET: FRFormOlustur
        private readonly VysDBEntities db = new VysDBEntities();

        public ActionResult Index()
        {

            return Index(new FmFrFormOlsur { PageSize = 30 });
        }
        [HttpPost]
        public ActionResult Index(FmFrFormOlsur model)
        {
            var q = (from s in db.FRFormlars
                     join kul in db.Kullanicilars on s.IslemYapanID equals kul.KullaniciID
                     select new FrFrFormOlsur
                     {
                         FRFormID = s.FRFormID,
                         FormAdi = s.FormAdi,
                         DosyaAdi = s.DosyaAdi,
                         DosyaYolu = s.DosyaYolu,
                         IsAktif = s.IsAktif,
                         IslemTarihi = s.IslemTarihi,
                         IslemYapan = kul.KullaniciAdi,
                         IslemYapanID = s.IslemYapanID,
                         IslemYapanIP = s.IslemYapanIP,
                         FormBirimleri = db.Vw_BirimlerTree.Where(p => s.FRFormlarBirims.Any(a => a.BirimID == p.BirimID)).OrderBy(o => o.BirimTreeAdi).ToList()
                     }).AsQueryable();

            if (!model.FormAdi.IsNullOrWhiteSpace()) q = q.Where(p => p.FormAdi.Contains(model.FormAdi) || p.FormBirimleri.Any(a => a.BirimAdi.Contains(model.FormAdi)));
            if (model.IsAktif.HasValue) q = q.Where(p => p.IsAktif == model.IsAktif.Value);
            model.RowCount = q.Count();
            q = !model.Sort.IsNullOrWhiteSpace() ? q.OrderBy(model.Sort) : q.OrderBy(o => o.FormAdi);
            model.CountIngfos =  new MIndexBilgi() { Toplam = model.RowCount, Pasif = q.Count(p => !p.IsAktif),Aktif = q.Count(p=>p.IsAktif)};
            
            
           
            model.Data = q.Skip(model.PagingStartRowIndex).Take(model.PageSize).ToList();;
            
            ViewBag.IsAktif = new SelectList(ComboData.GetCmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            return View(model);
        }

        public ActionResult Kayit(int? id)
        {
            var mmMessage = new MmMessage();
            ViewBag.MmMessage = mmMessage;
            var model = new FRFormlar
            {
                IsAktif = true
            };
            if (id.HasValue)
            {
                var data = db.FRFormlars.FirstOrDefault(p => p.FRFormID == id);
                if (data != null)
                {
                    model = data;
                }
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.FRFormOluşturKayitYetkisi)]
        public ActionResult Kayit(FRFormlar kModel, HttpPostedFileBase form, List<int> birimId)
        {
            birimId = birimId ?? new List<int>();
            var mmMessage = new MmMessage();
            #region Kontrol

            if (kModel.FormAdi.IsNullOrWhiteSpace())
            {
                mmMessage.Messages.Add("Form Adı boş bırakılamaz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "FormAdi" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "FormAdi" });
            if (!kModel.Aciklama.IsNullOrWhiteSpace())
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Aciklama" });

            if (form == null && kModel.FRFormID <= 0)
            {
                mmMessage.Messages.Add("Form şablonu seçiniz");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Form" });
            }
            else if (form != null)
            {
                string extension = Path.GetExtension(form.FileName);
                if (extension != ".xls" && extension != ".xlsx")
                {
                    mmMessage.Messages.Add(form.FileName + " doyasının excel formatında olması gerekmektedir. Eki kaldırın ve Excel formatında tekrar ekleyiniz.");
                }
                else { mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Form" }); }
            }

            if (birimId.Count == 0)
            {
                mmMessage.Messages.Add("En az bir Birim seçilmelidir.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BirimID" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "BirimID" });

            var yeniDosyaAdi = "";
            var yeniDosyaYolu = "";
            if (mmMessage.Messages.Count == 0 && form != null)
            {
                try
                {
                    var unqCode = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
                    var extension = Path.GetExtension(form.FileName);
                    var fileName = form.FileName.Replace(extension, "").ReplaceSpecialCharacter() + "_" + unqCode.ReplaceSpecialCharacter() + extension;
                    var path = "/FRFormSablonlari/" + fileName;
                    form.SaveAs(Server.MapPath("~" + path));
                    kModel.DosyaAdi = yeniDosyaAdi = form.FileName;
                    kModel.DosyaYolu = yeniDosyaYolu = path;
                }
                catch (Exception ex)
                {
                    var msg = "Form şablonu eklenirken bir hata oluştu! Hata: " + ex.ToExceptionMessage();
                    mmMessage.Messages.Add(msg);
                    SistemBilgilendirmeBus.SistemBilgisiKaydet(msg, ex.ToExceptionStackTrace(), BilgiTipi.Hata);
                }
            }
            #endregion

            if (mmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;



                FRFormlar table;

                if (kModel.FRFormID <= 0)
                {
                    table = db.FRFormlars.Add(kModel);
                }
                else
                {
                    table = db.FRFormlars.First(p => p.FRFormID == kModel.FRFormID);
                    table.FormAdi = kModel.FormAdi;
                    table.Aciklama = kModel.Aciklama;
                    if (!yeniDosyaAdi.IsNullOrWhiteSpace())
                    {

                        var path = Server.MapPath("~" + table.DosyaYolu);
                        if (System.IO.File.Exists(path))
                        {
                            try
                            {
                                System.IO.File.Delete(path);
                            }
                            catch (Exception ex)
                            {
                                var msg = "Form şablonu dosyası silinirken bir hata oluştu! <br /> DosyaYolu:" + path + " <br /> Hata: " + ex.ToExceptionMessage();
                                SistemBilgilendirmeBus.SistemBilgisiKaydet(msg, ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                            }
                        }
                        table.DosyaAdi = yeniDosyaAdi;
                        table.DosyaYolu = yeniDosyaYolu;

                    }
                    table.IsAktif = kModel.IsAktif;
                    table.IslemTarihi = kModel.IslemTarihi;
                    table.IslemYapanID = kModel.IslemYapanID;
                    table.IslemYapanIP = kModel.IslemYapanIP;
                }
                #region AddMaddeler 
                var fRbVarolanlar = table.FRFormlarBirims.Where(p => birimId.Contains(p.BirimID)).ToList();
                var fRbSilinenler = table.FRFormlarBirims.Where(p => !birimId.Contains(p.BirimID)).ToList();
                if (fRbSilinenler.Any()) db.FRFormlarBirims.RemoveRange(fRbSilinenler);

                var fRmEklenecekler = birimId.Where(p => fRbVarolanlar.Select(s => s.BirimID).All(a => a != p)).ToList();
                foreach (var item in fRmEklenecekler)
                    table.FRFormlarBirims.Add(new FRFormlarBirim { BirimID = item });
                #endregion 
                db.SaveChanges();



                return RedirectToAction("Index");
            }

            MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, mmMessage.Messages.ToArray());
            ViewBag.SecilenlerMd = birimId;
            ViewBag.MmMessage = mmMessage;
            return View(kModel);
        }





        [Authorize(Roles = RoleNames.FRFormOluşturKayitYetkisi)]
        public ActionResult Sil(int id)
        {
            var kayit = db.FRFormlars.FirstOrDefault(p => p.FRFormID == id);
            var message = "";
            var success = true;
            if (kayit != null)
            {

                try
                {
                    message = "'" + kayit.FormAdi + "' isimli form silindi!";
                    db.FRFormlars.Remove(kayit);
                    db.SaveChanges();
                    var path = Server.MapPath("~" + kayit.DosyaYolu);
                    if (System.IO.File.Exists(path))
                    {
                        try
                        {
                            System.IO.File.Delete(path);
                        }
                        catch (Exception ex)
                        {
                            var msg = "Form şablonu silinirken bir hata oluştu! Hata: " + ex.ToExceptionMessage();
                            SistemBilgilendirmeBus.SistemBilgisiKaydet(msg, "FRFormOlustur/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                        }
                    }
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.FormAdi + "' isimli form silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    SistemBilgilendirmeBus.SistemBilgisiKaydet(message, "FRFormOlustur/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz form sistemde bulunamadı!";
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