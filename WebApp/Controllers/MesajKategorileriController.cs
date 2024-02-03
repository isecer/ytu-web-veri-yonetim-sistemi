using System; 
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;
using BiskaUtil;
using Database;
using WebApp.Business;
using WebApp.Utilities.Extensions;
using WebApp.Utilities.Helpers;
using WebApp.Utilities.MenuAndRoles;
using WebApp.Utilities.MessageBox;
using WebApp.Utilities.SystemData;

namespace WebApp.Controllers
{
    [Authorize(Roles = RoleNames.MesajlarKategorileri)]
    [System.Web.Mvc.OutputCache(NoStore = false, Duration = 0, VaryByParam = "*")]
    public class MesajKategorileriController : Controller
    {
        private VysDBEntities db = new VysDBEntities();
        public ActionResult Index()
        {
            return Index(new FmMesajKategorileri { });
        }
        [HttpPost]
        public ActionResult Index(FmMesajKategorileri model)
        {
            var q = from s in db.MesajKategorileris
                    select new FrMesajKategorileri
                    {
                        MesajKategoriID = s.MesajKategoriID,
                        KategoriAdi = s.KategoriAdi,
                        KategoriAciklamasi = s.KategoriAciklamasi,
                        IsAktif = s.IsAktif,
                        IslemTarihi = s.IslemTarihi,
                        IslemYapan = s.Kullanicilar.KullaniciAdi,
                        IslemYapanID = s.IslemYapanID,
                        IslemYapanIP = s.IslemYapanIP,
                    };

            if (!model.KategoriAdi.IsNullOrWhiteSpace()) q = q.Where(p => p.KategoriAdi.Contains(model.KategoriAdi));
            if (!model.KategoriAciklamasi.IsNullOrWhiteSpace()) q = q.Where(p => p.KategoriAciklamasi.Contains(model.KategoriAciklamasi));
            if (model.IsAktif.HasValue) q = q.Where(p => p.IsAktif == model.IsAktif);
            model.RowCount = q.Count();
            if (!model.Sort.IsNullOrWhiteSpace())
            {
                q = q.OrderBy(model.Sort);
            }
            else q = q.OrderBy(o => o.KategoriAdi);
            
           
            model.Data = q.Skip(model.PagingStartRowIndex).Take(model.PageSize).ToList();
            var IndexModel = new MIndexBilgi();
            IndexModel.Toplam = model.RowCount;
            IndexModel.Aktif = q.Where(p => p.IsAktif).Count();
            IndexModel.Pasif = q.Where(p => !p.IsAktif).Count();
            
            ViewBag.IsAktif = new SelectList(ComboData.GetCmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            return View(model);
        }
        public ActionResult Kayit(int? id)
        {
            var MmMessage = new MmMessage();
            ViewBag.MmMessage = MmMessage;
            var model = new MesajKategorileri();
            if (id.HasValue)
            {
                model = db.MesajKategorileris.Where(p => p.MesajKategoriID == id).FirstOrDefault();

            }

            ViewBag.IsAktif = new SelectList(ComboData.GetCmbAktifPasifData(), "Value", "Caption", model.IsAktif);
            return View(model);
        }
        [HttpPost]
        public ActionResult Kayit(MesajKategorileri kModel)
        {
            var MmMessage = new MmMessage();
            if (kModel.KategoriAdi.IsNullOrWhiteSpace())
            { 
                MmMessage.Messages.Add("Kayıt işlemini yapabilmeniz için kategori adını girmeniz gerekmektedir!");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "KategoriAdi" });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "KategoriAdi" });
            if (kModel.KategoriAciklamasi.IsNullOrWhiteSpace())
            { 
                MmMessage.Messages.Add("Kayıt işlemini yapabilmeniz için kategori açıklamasını girmeniz gerekmektedir!");
                MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "KategoriAciklamasi"  });
            }
            else MmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "KategoriAciklamasi" });


            if (MmMessage.Messages.Count == 0)
            {

                if (kModel.MesajKategoriID <= 0)
                {
                    kModel.IsAktif = true;
                    kModel.IslemYapanID = UserIdentity.Current.Id;
                    kModel.IslemYapanIP = UserIdentity.Ip;
                    kModel.IslemTarihi = DateTime.Now;
                    var enst = db.MesajKategorileris.Add(kModel);
                }
                else
                {
                    var data = db.MesajKategorileris.Where(p => p.MesajKategoriID == kModel.MesajKategoriID).First();
                    data.IsAktif = kModel.IsAktif;
                    data.KategoriAdi = kModel.KategoriAdi;
                    data.KategoriAciklamasi = kModel.KategoriAciklamasi; 
                    data.IslemYapanID = UserIdentity.Current.Id;
                    data.IslemYapanIP = UserIdentity.Ip;
                    data.IslemTarihi = DateTime.Now;

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
            return View(kModel);
        }
        public ActionResult Sil(int id)
        {
            var kayit = db.MesajKategorileris.Where(p => p.MesajKategoriID == id).FirstOrDefault();
            string message = "";
            bool success = true;
            if (kayit != null)
            {

                try
                {
                    message = "'" + kayit.KategoriAdi + "' İsimli Kategori Silindi!";
                    db.MesajKategorileris.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.KategoriAdi + "' İsimli Kategori Silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    SistemBilgilendirmeBus.SistemBilgisiKaydet(message, "MesajKategorileri/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Kategori sistemde bulunamadı!";
            }
            return Json(new { success = success, message = message }, "application/json", JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}