using BiskaUtil;
using Database;
using System;
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
    [Authorize(Roles = RoleNames.GTDonemIslemleri)]
    public class GtDonemIslemleriController : Controller
    {
        // GET: BTDonemIslemleri
        private VysDBEntities db = new VysDBEntities();
        public ActionResult Index()
        {
            return Index(new FmGTDonemleri() { PageSize = 15 });
        }
        [HttpPost]
        public ActionResult Index(FmGTDonemleri model)
        {
            var nowDate = DateTime.Now.Date;
            var q = from s in db.GTDonemleris
                    join k in db.Kullanicilars on s.IslemYapanID equals k.KullaniciID
                    select new FrGTDonemleri
                    {
                        GTDonemID = s.GTDonemID,
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
            model.AktifCount = q.Count(p => p.IsAktif);
         
            q = !model.Sort.IsNullOrWhiteSpace() ? q.OrderBy(model.Sort) : q.OrderByDescending(t => t.Yil).ThenByDescending(t => t.BaslangicTarihi);
            model.Data = q.Skip(model.StartRowIndex).Take(model.PageSize).ToList();  
            ViewBag.IsAktif = new SelectList(ComboData.GetCmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            return View(model);
        }
        public ActionResult Kayit(int? id)
        {
            var MmMessage = new MmMessage();
            ViewBag.MmMessage = MmMessage;
            var model = new KmGTDonemleri();
            model.IsAktif = true;
            if (id.HasValue && id > 0)
            {
                var data = db.GTDonemleris.Where(p => p.GTDonemID == id).FirstOrDefault();

                if (data != null)
                {
                    model.GTDonemID = data.GTDonemID;
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
        [Authorize(Roles = RoleNames.GTDonemIslemleriKayitYetkisi)]
        public ActionResult Kayit(KmGTDonemleri kModel, bool IsSinavVar = false)
        {
            var MmMessage = new MmMessage();

            #region Kontrol  
            if (kModel.Yil <= 0)
            {
                MmMessage.Messages.Add("Dönem Yılını Giriniz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Yil" });

            }
            else if (db.GTDonemleris.Any(p => p.GTDonemID != kModel.GTDonemID && p.Yil == kModel.Yil))
            {
                MmMessage.Messages.Add("Seçtiğiniz Gelir Takip Dönem Yılı Daha Önceden Kayıt Edilmiştir. Tekrar Kayıt Edilemez.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Yil" });
            }
            else
            {
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "Yil" });
            }
            if (!kModel.BaslangicTarihi.HasValue || !kModel.BitisTarihi.HasValue)
            {
                if (!kModel.BaslangicTarihi.HasValue)
                {
                    MmMessage.Messages.Add("Başlangıç Tarihi Seçiniz.");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BaslangicTarihi" });

                }
                if (!kModel.BitisTarihi.HasValue)
                {
                    MmMessage.Messages.Add("Bitiş Tarihi Seçiniz.");
                    MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BitisTarihi" });

                }

            }
            else if (kModel.BaslangicTarihi >= kModel.BitisTarihi)
            {
                MmMessage.Messages.Add("Başlangıç Tarihi Bitiş Tarihinden Büyük Yada Eşit Olamaz.");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BitisTarihi" });
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "BaslangicTarihi" });
            }
            else
            {
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "BaslangicTarihi" });
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "BitisTarihi" });
            }

            #endregion

            if (MmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                kModel.Yil = kModel.Yil;
                var Table = new GTDonemleri();
                if (kModel.GTDonemID <= 0)
                {
                    Table = db.GTDonemleris.Add(new GTDonemleri
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
                    Table = db.GTDonemleris.Where(p => p.GTDonemID == kModel.GTDonemID).First();
                    Table.Yil = kModel.Yil;
                    Table.BaslangicTarihi = kModel.BaslangicTarihi.Value;
                    Table.BitisTarihi = kModel.BitisTarihi.Value;
                    Table.IsAktif = kModel.IsAktif;
                    Table.IslemTarihi = DateTime.Now;
                    Table.IslemYapanID = kModel.IslemYapanID;
                    Table.IslemYapanIP = kModel.IslemYapanIP;
                }

                db.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, MmMessage.Messages.ToArray());
            }
            ViewBag.MmMessage = MmMessage;
            ViewBag.IsAktif = new SelectList(ComboData.GetCmbAktifPasifData(), "Value", "Caption", kModel.IsAktif);
            ViewBag.Yil = new SelectList(ComboData.CmbSurecKayitYillari(), "Value", "Caption", kModel.Yil);
            return View(kModel);
        }

        [Authorize(Roles = RoleNames.GTDonemIslemleriKayitYetkisi)]
        public ActionResult Sil(int id)
        {
            var mmMessage = new MmMessage();

            var kayit = db.GTDonemleris.Where(p => p.GTDonemID == id).FirstOrDefault();

            string message = "";
            if (kayit != null)
            {
                try
                {
                    message = "'" + kayit.Yil + "' Yılı Gelir Takip Dönemi Silindi!";
                    db.GTDonemleris.Remove(kayit);
                    db.SaveChanges();
                    mmMessage.Title = "Uyarı";
                    mmMessage.Messages.Add(message);
                    mmMessage.MessageType = Msgtype.Success;
                    mmMessage.IsSuccess = true;
                }
                catch (Exception ex)
                {
                    message = "'" + kayit.Yil + "' Yılı Gelir Takip Dönemi Silinirken Bir Hata Oluştu! </br> Hata:" + ex.ToExceptionMessage();
                    SistemBilgilendirmeBus.SistemBilgisiKaydet(message, "GTDonemIslemleri/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                    mmMessage.Title = "Hata";
                    mmMessage.Messages.Add(message);
                    mmMessage.MessageType = Msgtype.Error;
                    mmMessage.IsSuccess = false;
                }
            }
            else
            {
                message = "Silmek İstediğiniz Gelir Takip Dönemi Sistemde Bulunamadı!";
                mmMessage.Title = "Hata";
                mmMessage.Messages.Add(message);
                mmMessage.MessageType = Msgtype.Error;
                mmMessage.IsSuccess = true;
            }
            var strView = ViewRenderHelper.RenderPartialView("Ajax", "getMessage", mmMessage);
            return Json(new { IsSuccess = mmMessage.IsSuccess, Messages = strView }, "application/json", JsonRequestBehavior.AllowGet);
        }
    }
}