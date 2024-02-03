using BiskaUtil;
using Database;
using System;
using System.Collections.Generic;
using System.Linq;
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
    [Authorize(Roles = RoleNames.FRDonemIslemleri)]
    public class FrDonemIslemleriController : Controller
    {
        // GET: FRDonemIslemleri
        private readonly VysDBEntities db = new VysDBEntities();
        public ActionResult Index()
        {
            return Index(new FmFrDonemleri() { PageSize = 15 });
        }
        [HttpPost]
        public ActionResult Index(FmFrDonemleri model)
        {
            var nowDate = DateTime.Now.Date;
            var q = from s in db.FRDonemleris
                    join k in db.Kullanicilars on s.IslemYapanID equals k.KullaniciID
                    select new FrFrDonemleri
                    {
                        FRDonemID = s.FRDonemID,
                        DonemYilAdi = s.Yil + " Yılı Dönemi",
                        Yil = s.Yil,
                        BaslangicTarihi = s.BaslangicTarihi,
                        BitisTarihi = s.BitisTarihi,
                        IsAktif = s.IsAktif,
                        IslemYapanID = s.IslemYapanID,
                        IslemYapan = k.KullaniciAdi,
                        IslemTarihi = s.IslemTarihi,
                        IslemYapanIP = s.IslemYapanIP,
                        AktifSurec = (s.BaslangicTarihi <= nowDate && s.BitisTarihi >= nowDate)
                    };
            model.RowCount = q.Count();
            q = !model.Sort.IsNullOrWhiteSpace() ? q.OrderBy(model.Sort) : q.OrderByDescending(t => t.Yil).ThenByDescending(t => t.BaslangicTarihi);
            model.Data = q.Skip(model.StartRowIndex).Take(model.PageSize).ToList();
            model.CountIngfos =  new MIndexBilgi() { Toplam = model.RowCount, Pasif = q.Count(p => !p.IsAktif),Aktif = q.Count(p=>p.IsAktif)};
            
            
            ViewBag.IsAktif = new SelectList(ComboData.GetCmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            return View(model);
        }
        public ActionResult Kayit(int? id)
        {
            var mmMessage = new MmMessage();
            ViewBag.MmMessage = mmMessage;
            var model = new KmFrDonemleri
            {
                IsAktif = true
            };
            if (id > 0)
            {
                var data = db.FRDonemleris.FirstOrDefault(p => p.FRDonemID == id);

                if (data != null)
                {
                    model.FRDonemID = data.FRDonemID;
                    model.Yil = data.Yil;
                    model.BaslangicTarihi = data.BaslangicTarihi;
                    model.BitisTarihi = data.BitisTarihi;
                    model.IsAktif = data.IsAktif;
                    model.IslemTarihi = DateTime.Now;
                    model.IslemYapanID = data.IslemYapanID;
                    model.IslemYapanIP = data.IslemYapanIP;
                }

            }
            else
            {
                model.Yil = DateTime.Now.Year;
            }

            ViewBag.IsAktif = new SelectList(ComboData.GetCmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            ViewBag.Yil = new SelectList(ComboData.CmbSurecKayitYillari(), "Value", "Caption", model.Yil);

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = RoleNames.FRDonemIslemleriKayitYetkisi)]
        public ActionResult Kayit(KmFrDonemleri kModel, List<int> frFormId)
        {
            var mmMessage = new MmMessage();
            frFormId = frFormId ?? new List<int>();
            #region Kontrol  
            if (kModel.Yil <= 0)
            {
                mmMessage.Messages.Add("Dönem Yılını Giriniz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Yil" });

            }
            else if (db.FRDonemleris.Any(p => p.FRDonemID != kModel.FRDonemID && p.Yil == kModel.Yil))
            {
                mmMessage.Messages.Add("Seçtiğiniz Faaliyet Rapor Dönem Yılı Daha Önceden Kayıt Edilmiştir. Tekrar Kayıt Edilemez.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Yil" });
            }
            else
            {
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Yil" });
            }
            if (!kModel.BaslangicTarihi.HasValue || !kModel.BitisTarihi.HasValue)
            {
                if (!kModel.BaslangicTarihi.HasValue)
                {
                    mmMessage.Messages.Add("Başlangıç Tarihi Seçiniz.");
                    mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BaslangicTarihi" });

                }
                if (!kModel.BitisTarihi.HasValue)
                {
                    mmMessage.Messages.Add("Bitiş Tarihi Seçiniz.");
                    mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BitisTarihi" });

                }

            }
            else if (kModel.BaslangicTarihi >= kModel.BitisTarihi)
            {
                mmMessage.Messages.Add("Başlangıç Tarihi Bitiş Tarihinden Büyük Yada Eşit Olamaz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BitisTarihi" });
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BaslangicTarihi" });
            }
            else
            {
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "BaslangicTarihi" });
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "BitisTarihi" });
            }
            if (frFormId.Count == 0)
            {
                mmMessage.Messages.Add("En az bir Form seçilmelidir.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "FRFormID" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "FRFormID" });
            #endregion

            if (mmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                kModel.Yil = kModel.Yil;
                FRDonemleri table;
                if (kModel.FRDonemID <= 0)
                {
                    table = db.FRDonemleris.Add(new FRDonemleri
                    {

                        Yil = kModel.Yil,
                        BaslangicTarihi = kModel.BaslangicTarihi.Value,
                        BitisTarihi = kModel.BitisTarihi.Value,
                        IsAktif = kModel.IsAktif,
                        IslemTarihi = kModel.IslemTarihi,
                        IslemYapanID = kModel.IslemYapanID,
                        IslemYapanIP = kModel.IslemYapanIP

                    });
                }
                else
                {
                    table = db.FRDonemleris.First(p => p.FRDonemID == kModel.FRDonemID);
                    table.Yil = kModel.Yil;
                    table.BaslangicTarihi = kModel.BaslangicTarihi.Value;
                    table.BitisTarihi = kModel.BitisTarihi.Value;
                    table.IsAktif = kModel.IsAktif;
                    table.IslemTarihi = DateTime.Now;
                    table.IslemYapanID = kModel.IslemYapanID;
                    table.IslemYapanIP = kModel.IslemYapanIP;


                }
                var donemFormlari = db.FRDonemlerForms.Where(p => p.FRDonemID == table.FRDonemID).ToList();
                var formlar = db.FRFormlars.Where(p => frFormId.Contains(p.FRFormID)).ToList();
                var eklenecek = formlar.Where(p => donemFormlari.All(a => a.FRFormID != p.FRFormID)).ToList();
                var silinecekler = donemFormlari.Where(p => !frFormId.Contains(p.FRFormID)).ToList();
                var guncellenecekler = donemFormlari.Where(p => frFormId.Contains(p.FRFormID)).ToList();
                foreach (var item in guncellenecekler)
                {
                    var form = formlar.First(p => p.FRFormID == item.FRFormID);
                    item.IsAktif = form.IsAktif;
                    item.IslemTarihi = DateTime.Now;
                    item.IslemYapanIP = UserIdentity.Ip;
                    item.IslemYapanID = UserIdentity.Current.Id;
                    db.FRDonemlerFormBirims.RemoveRange(item.FRDonemlerFormBirims);
                    foreach (var itemB in form.FRFormlarBirims)
                        item.FRDonemlerFormBirims.Add(new FRDonemlerFormBirim { BirimID = itemB.BirimID, FRFormID = itemB.FRFormID });
                }

                foreach (var item in eklenecek)
                {
                    table.FRDonemlerForms.Add(new FRDonemlerForm
                    {
                        FRFormID = item.FRFormID,
                        IsAktif = item.IsAktif,
                        IslemTarihi = DateTime.Now,
                        IslemYapanIP = UserIdentity.Ip,
                        IslemYapanID = UserIdentity.Current.Id,
                        FRDonemlerFormBirims = item.FRFormlarBirims.Select(s => new FRDonemlerFormBirim { BirimID = s.BirimID, FRFormID = s.FRFormID }).ToList()

                    });
                }
                foreach (var item in silinecekler)
                    item.IsAktif = false;

                db.SaveChanges();

                return RedirectToAction("Index");
            }
            MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, mmMessage.Messages.ToArray());
            ViewBag.SecilenlerMd = frFormId;
            ViewBag.MmMessage = mmMessage;
            ViewBag.IsAktif = new SelectList(ComboData.GetCmbAktifPasifData(), "Value", "Caption", kModel.IsAktif);
            ViewBag.Yil = new SelectList(ComboData.CmbSurecKayitYillari(), "Value", "Caption", kModel.Yil);
            return View(kModel);
        }

        [Authorize(Roles = RoleNames.FRDonemIslemleriKayitYetkisi)]
        public ActionResult Sil(int id)
        {
            var mmMessage = new MmMessage(); 
            var kayit = db.FRDonemleris.FirstOrDefault(p => p.FRDonemID == id); 
            string message;
            if (kayit != null)
            {
                try
                {
                    message = "'" + kayit.Yil + "' Yılı Faaliyet Rapor Dönemi Silindi!";
                    db.FRDonemleris.Remove(kayit);
                    db.SaveChanges();
                    mmMessage.Title = "Uyarı";
                    mmMessage.Messages.Add(message);
                    mmMessage.MessageType = Msgtype.Success;
                    mmMessage.IsSuccess = true;
                }
                catch (Exception ex)
                {
                    message = "'" + kayit.Yil + "' Yılı Faaliyet Rapor Dönemi Silinirken Bir Hata Oluştu! </br> Hata:" + ex.ToExceptionMessage();
                    SistemBilgilendirmeBus.SistemBilgisiKaydet(message, "FRDonemIslemleri/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                    mmMessage.Title = "Hata";
                    mmMessage.Messages.Add(message);
                    mmMessage.MessageType = Msgtype.Error;
                    mmMessage.IsSuccess = false;
                }
            }
            else
            {
                message = "Silmek İstediğiniz Faaliyet Rapor Dönemi Sistemde Bulunamadı!";
                mmMessage.Title = "Hata";
                mmMessage.Messages.Add(message);
                mmMessage.MessageType = Msgtype.Error;
                mmMessage.IsSuccess = true;
            }
            var strView = ViewRenderHelper.RenderPartialView("Ajax", "getMessage", mmMessage);
            return Json(new { mmMessage.IsSuccess, Messages = strView }, "application/json", JsonRequestBehavior.AllowGet);
        }
    }
}