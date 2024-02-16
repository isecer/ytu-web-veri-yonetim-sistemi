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

// ReSharper disable PossibleNullReferenceException

namespace WebApp.Controllers
{
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [Authorize(Roles = RoleNames.BFRFormOluştur)]
    public class BfrFormOlusturController : Controller
    {
        // GET: FRFormOlustur
        private readonly VysDBEntities entities = new VysDBEntities();

        public ActionResult Index()
        {

            return Index(new FmBfrFormOlsur() { PageSize = 30 });
        }
        [HttpPost]
        public ActionResult Index(FmBfrFormOlsur model)
        {
            var q = (from s in entities.BFRFormlars
                     join kul in entities.Kullanicilars on s.IslemYapanID equals kul.KullaniciID
                     select new FrBfrFormOlsur
                     {
                         BFRFormID = s.BFRFormID,
                         FormAdi = s.FormAdi,
                         DosyaAdi = s.DosyaAdi,
                         DosyaYolu = s.DosyaYolu,
                         IsAktif = s.IsAktif,
                         IslemTarihi = s.IslemTarihi,
                         IslemYapan = kul.KullaniciAdi,
                         IslemYapanID = s.IslemYapanID,
                         IslemYapanIP = s.IslemYapanIP,
                         FormBirimleri = entities.Vw_BirimlerTree.Where(p => s.BFRFormlarBirims.Any(a => a.BirimID == p.BirimID)).OrderBy(o => o.BirimTreeAdi).ToList()
                     }).AsQueryable();

            if (!model.FormAdi.IsNullOrWhiteSpace()) q = q.Where(p => p.FormAdi.Contains(model.FormAdi) || p.FormBirimleri.Any(a => a.BirimAdi.Contains(model.FormAdi)));
            if (model.IsAktif.HasValue) q = q.Where(p => p.IsAktif == model.IsAktif.Value);
            model.RowCount = q.Count();
            model.AktifCount = q.Count(p => p.IsAktif);

            q = !model.Sort.IsNullOrWhiteSpace() ? q.OrderBy(model.Sort) : q.OrderBy(o => o.FormAdi);
           


            model.Data = q.Skip(model.PagingStartRowIndex).Take(model.PageSize).ToList(); ;

            ViewBag.IsAktif = new SelectList(ComboData.GetCmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            return View(model);
        }

        public ActionResult Kayit(int? id)
        {
            var mmMessage = new MmMessage();
            ViewBag.MmMessage = mmMessage;
            var model = new BFRFormlar
            {
                IsAktif = true
            };
            if (!id.HasValue) return View(model);
            var data = entities.BFRFormlars.FirstOrDefault(p => p.BFRFormID == id);
            if (data != null)
            {
                model = data;
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.BFRFormOluşturKayitYetkisi)]
        public ActionResult Kayit(BFRFormlar kModel, HttpPostedFileBase form, List<int> birimId)
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

            if (form == null && kModel.BFRFormID <= 0)
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
                    var path = "/BFRFormSablonlari/" + fileName;
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



                BFRFormlar table;

                if (kModel.BFRFormID <= 0)
                {
                    table = entities.BFRFormlars.Add(kModel);
                }
                else
                {
                    table = entities.BFRFormlars.First(p => p.BFRFormID == kModel.BFRFormID);
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
                var bfRbVarolanlar = table.BFRFormlarBirims.Where(p => birimId.Contains(p.BirimID)).ToList();
                var bfRbSilinenler = table.BFRFormlarBirims.Where(p => !birimId.Contains(p.BirimID)).ToList();
                if (bfRbSilinenler.Any()) entities.BFRFormlarBirims.RemoveRange(bfRbSilinenler);

                var bfRmEklenecekler = birimId.Where(p => bfRbVarolanlar.Select(s => s.BirimID).All(a => a != p)).ToList();
                foreach (var item in bfRmEklenecekler)
                    table.BFRFormlarBirims.Add(new BFRFormlarBirim { BirimID = item });
                #endregion 
                entities.SaveChanges();



                return RedirectToAction("Index");
            }
            MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, mmMessage.Messages.ToArray());
            ViewBag.SecilenlerMd = birimId;
            ViewBag.MmMessage = mmMessage;
            return View(kModel);
        }





        [Authorize(Roles = RoleNames.BFRFormOluşturKayitYetkisi)]
        public ActionResult Sil(int id)
        {
            var kayit = entities.BFRFormlars.FirstOrDefault(p => p.BFRFormID == id);
            string message;
            var success = true;
            if (kayit != null)
            {

                try
                {
                    message = "'" + kayit.FormAdi + "' isimli form silindi!";
                    entities.BFRFormlars.Remove(kayit);
                    entities.SaveChanges();
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
                            SistemBilgilendirmeBus.SistemBilgisiKaydet(msg, "BFRFormOlustur/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                        }
                    }
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.FormAdi + "' isimli form silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    SistemBilgilendirmeBus.SistemBilgisiKaydet(message, "BFRFormOlustur/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
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
            entities.Dispose();
            base.Dispose(disposing);
        }
    }
}