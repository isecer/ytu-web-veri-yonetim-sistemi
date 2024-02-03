using BiskaUtil;
using Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
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
    [Authorize(Roles = RoleNames.Maddeler)]
    public class MaddelerController : Controller
    {

        private readonly VysDBEntities db = new VysDBEntities();
        // GET: Maddeler
        public ActionResult Index()
        {

            return Index(new FmMaddeler { PageSize = 50, IsAktif = true, Expand = true });
        }
        [HttpPost]
        public ActionResult Index(FmMaddeler model, bool export = false)
        {
            var q = (from s in db.Maddelers
                     join mtr in db.MaddeTurleris on new { s.MaddeTurID } equals new { MaddeTurID = (int?)mtr.MaddeTurID } into df1
                     from mtr in df1.DefaultIfEmpty()
                     join mt in db.Vw_MaddelerTree on s.MaddeID equals mt.MaddeID
                     join kul in db.Kullanicilars on s.IslemYapanID equals kul.KullaniciID
                     select new FrMaddeler
                     {
                         MaddeID = s.MaddeID,
                         MaddeKod = s.MaddeKod,
                         UstMaddeID = s.UstMaddeID,
                         MaddeAdi = s.MaddeAdi,
                         MaddeTreeAdi = mt.MaddeTreeAdi,
                         IsAktif = s.IsAktif,
                         IslemTarihi = s.IslemTarihi,
                         IslemYapan = kul.KullaniciAdi,
                         IslemYapanID = s.IslemYapanID,
                         IslemYapanIP = s.IslemYapanIP,
                         VeriGirisSekliID = s.VeriGirisSekliID,
                         VeriGirisSekilleri = s.VeriGirisSekilleri,
                         MaddeYilSonuDegerHesaplamaTipID = s.MaddeYilSonuDegerHesaplamaTipID,
                         MaddeYilSonuDegerHesaplamaTipleri = s.MaddeYilSonuDegerHesaplamaTipleri,
                         YilSonuDegerHesaplamaAdi = s.MaddeYilSonuDegerHesaplamaTipID.HasValue ? s.MaddeYilSonuDegerHesaplamaTipleri.YilSonuDegerHesaplamaAdi : "Hesaplama Yok",
                         MaddelerVeriGirisDonemleris = s.MaddelerVeriGirisDonemleris,
                         VeriTipID = s.VeriTipID,
                         HesaplamaFormulu = s.HesaplamaFormulu,
                         BirimMaddeleris = s.BirimMaddeleris,
                         MaddeTurID = s.MaddeTurID,
                         MaddeTurleri = s.MaddeTurleri,
                         MaddeTurAdi = mtr != null ? mtr.MaddeTurAdi : "",
                         Aciklama = s.Aciklama,
                     }).AsQueryable();

            if (model.MaddeTurID.HasValue) q = q.Where(p => p.MaddeTurID == model.MaddeTurID.Value);
            if (model.VeriGirisSekliID.HasValue) q = q.Where(p => p.VeriGirisSekliID == model.VeriGirisSekliID.Value);
            if (model.EslestirmeDurum.HasValue)
            {
                q = model.EslestirmeDurum.Value ? q.Where(p => p.BirimMaddeleris.Count > 0) : q.Where(p => p.BirimMaddeleris.Count == 0 && p.VeriGirisSekliID != VeriGirisSekli.VeriGirisiYok);
            }
            if (model.MaddeYilSonuDegerHesaplamaTipID.HasValue) q = q.Where(p => p.MaddeYilSonuDegerHesaplamaTipID == model.MaddeYilSonuDegerHesaplamaTipID.Value);
            if (model.IsAktif.HasValue) q = q.Where(p => p.IsAktif == model.IsAktif.Value); 
            if (model.IsDosyaYuklensin.HasValue) q = q.Where(p => p.MaddelerVeriGirisDonemleris.Any(a => a.IsDosyaYuklensin == model.IsDosyaYuklensin));
            if (!model.Aranan.IsNullOrWhiteSpace()) q = q.Where(p => p.MaddeKod == model.Aranan || p.MaddeTreeAdi.ToLower().Contains(model.MaddeAdi.ToLower()));

            model.RowCount = q.Count();
            q = !model.Sort.IsNullOrWhiteSpace() ? q.OrderBy(model.Sort) : q.OrderBy(o => o.MaddeTreeAdi);
            model.CountIngfos = new MIndexBilgi() { Toplam = model.RowCount, Pasif = q.Count(p => !p.IsAktif), Aktif = q.Count(p => p.IsAktif) };



            model.Data = q.Skip(model.PagingStartRowIndex).Take(model.PageSize).ToList(); ;
            #region export
            if (export && model.RowCount > 0)
            {
                var gv = new GridView();


                var qeData = q.Select(s => new
                {
                    s.MaddeKod,
                    s.MaddeAdi,
                    s.MaddeTreeAdi,
                    s.MaddeTurAdi,
                    VeriGirisSekli = s.VeriGirisSekilleri.VeriGirisSekliAdi,
                    EslestirilenBirimSayisi = s.VeriGirisSekliID != VeriGirisSekli.VeriGirisiYok ? s.BirimMaddeleris.Count + "" : "",
                    Durum = s.IsAktif ? "Aktif" : "Pasif",
                }).ToList();



                gv.DataSource = qeData;
                gv.DataBind();
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                Response.ContentType = "application/ms-excel";
                Response.ContentEncoding = System.Text.Encoding.UTF8;
                Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());
                gv.RenderControl(htw);
                return File(System.Text.Encoding.UTF8.GetBytes(sw.ToString()), Response.ContentType, "MaddeListesi.xls");

            }
            #endregion



            ViewBag.IsAktif = new SelectList(ComboData.GetCmbAktifPasifData(true), "Value", "Caption", model.IsAktif);
            ViewBag.IsCokluVeriGiris = new SelectList(ComboData.GetCmbVarYokData(true), "Value", "Caption", model.IsCokluVeriGiris);
            ViewBag.IsDosyaYuklensin = new SelectList(ComboData.GetCmbEvetHayirData(true), "Value", "Caption", model.IsDosyaYuklensin);
            ViewBag.VeriGirisSekliID = new SelectList(MaddelerBus.CmbVeriGirisSekilleri(), "Value", "Caption", model.VeriGirisSekliID);
            ViewBag.MaddeYilSonuDegerHesaplamaTipID = new SelectList(MaddelerBus.CmbMaddeYilsonuDegerHesaplamaTipleri(), "Value", "Caption", model.MaddeYilSonuDegerHesaplamaTipID);
            ViewBag.EslestirmeDurum = new SelectList(ComboData.CmbEslesenEslesmeyenData(), "Value", "Caption", model.EslestirmeDurum);
            ViewBag.MaddeTurID = new SelectList(MaddelerBus.CmbMaddeTurleriFilterData(), "Value", "Caption", model.MaddeTurID);
            ViewBag.MaddeTurID2 = new SelectList(MaddeTurleriBus.CmbMaddeTurleri(true, true), "Value", "Caption", model.MaddeTurID);
            ViewBag.MaddeYilSonuDegerHesaplamaTipID2 = new SelectList(MaddelerBus.CmbMaddeYilsonuDegerHesaplamaTipleri(), "Value", "Caption", model.MaddeYilSonuDegerHesaplamaTipID);
            return View(model);
        }
        public ActionResult Kayit(int? id)
        {
            var mmMessage = new MmMessage();
            ViewBag.MmMessage = mmMessage;
            var model = new Maddeler
            {
                IsAktif = true
            };
            if (id.HasValue)
            {
                var data = db.Maddelers.FirstOrDefault(p => p.MaddeID == id);
                if (data != null)
                {
                    model = data;
                }
            }

            ViewBag.UstMaddeID = new SelectList(MaddelerBus.CmbUstMaddelerTree(true, model.MaddeID), "Value", "Caption", model.UstMaddeID);
            ViewBag.VeriGirisSekliID = new SelectList(MaddelerBus.CmbVeriGirisSekilleri(false, true), "Value", "Caption", model.VeriGirisSekliID);
            ViewBag.MaddeYilSonuDegerHesaplamaTipID = new SelectList(MaddelerBus.CmbMaddeYilsonuDegerHesaplamaTipleri(), "Value", "Caption", model.MaddeYilSonuDegerHesaplamaTipID);
            ViewBag.VeriTipID = new SelectList(MaddelerBus.CmbVeriTipleri(true, true), "Value", "Caption", model.VeriTipID);
            ViewBag.MaddeTurID = new SelectList(MaddeTurleriBus.CmbMaddeTurleri(model.MaddeID <= 0, true), "Value", "Caption", model.MaddeTurID);
            ViewBag.IsUstMadde = db.Maddelers.Any(a => a.UstMaddeID == model.MaddeID);
            return View(model);
        }

        [HttpPost]
        public ActionResult Kayit(Maddeler kModel, List<int> birimId, List<string> vaCokluVeriDonemIDs)
        {
            var tVaCokluVeriDonemIDs = vaCokluVeriDonemIDs ?? new List<string>();
            var vaCokluVeriDonems = tVaCokluVeriDonemIDs.Select(s => new MaddelerVeriGirisDonemleri
            {
                VACokluVeriDonemID = s.Split('_')[0].ToInt() ?? 0,
                IsDosyaYuklensin = s.Split('_')[1].ToBoolean() ?? false,

            });
            birimId = birimId ?? new List<int>();
            var mmMessage = new MmMessage();
            #region Kontrol

            if (kModel.MaddeKod.IsNullOrWhiteSpace())
            {
                mmMessage.Messages.Add("Madde Kodunu giriniz");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "MaddeKod" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "MaddeKod" });
            if (kModel.MaddeAdi.IsNullOrWhiteSpace())
            {
                mmMessage.Messages.Add("Madde Adı boş bırakılamaz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "MaddeAdi" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "MaddeAdi" });

            var maddelers = new List<Maddeler>();

            if (kModel.VeriGirisSekliID != VeriGirisSekli.VeriGirisiYok && kModel.MaddeYilSonuDegerHesaplamaTipID.HasValue == false)
            {
                mmMessage.Messages.Add("Maddeye Ait Yılsonu Değeri Hesaplama Şekli seçiniz");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "MaddeYilSonuDegerHesaplamaTipID" });

            }
            if (kModel.VeriGirisSekliID != VeriGirisSekli.VeriGirisiYok && kModel.VeriTipID.HasValue == false)
            {
                mmMessage.Messages.Add("Veri girişi için gerekli olacak Veri Tipini seçiniz");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "VeriTipID" });

            }
            if (kModel.VeriGirisSekliID == VeriGirisSekli.FormulleHesaplanacak)
            {
                if (kModel.HesaplamaFormulu.IsNullOrWhiteSpace())
                {
                    mmMessage.Messages.Add("Hesaplama Formülü boş bırakılamaz.");
                    mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "HesaplamaFormulu" });
                }
                else
                {
                    var kodlar = kModel.HesaplamaFormulu.ToIkiKarakterArasiKodBul("[", "]");
                    if (kodlar.Any())
                    {
                        var msg = new List<string>();
                        foreach (var item in kodlar)
                        {
                            var madde = db.Maddelers.FirstOrDefault(a => a.MaddeKod == item && a.VeriGirisSekliID != VeriGirisSekli.VeriGirisiYok);
                            if (madde == null)
                                msg.Add(item + " Kodlu madde sistemde bulunamadı! Lütfen uygun bir maddde kodu giriniz");
                            else maddelers.Add(madde);
                        }
                        if (msg.Any() == false) mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "HesaplamaFormulu" });
                        else
                        {
                            mmMessage.Messages.AddRange(msg);
                            mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "HesaplamaFormulu" });
                        }
                    }
                    else
                    {
                        mmMessage.Messages.Add("Girilen Formülde hiçbir madde eşleşmesine rastlanmadı! Lütfen uygun bir maddde kodu giriniz.");
                        mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "HesaplamaFormulu" });
                    }
                }
            }


            #endregion
            if (mmMessage.Messages.Count == 0)
            {

                kModel.MaddeKod = kModel.MaddeKod.ReplaceSpecialCharacter().Trim();
                if (db.Maddelers.Any(a => a.MaddeID != kModel.MaddeID && a.MaddeKod == kModel.MaddeKod.Trim()))
                {
                    mmMessage.Messages.Add("Girdiğiniz Madde Kodu başka bir maddeye aittir tekrar kullanamazsınız");
                    mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "MaddeKod" });
                }
                if (kModel.MaddeID > 0 && kModel.VeriTipID.HasValue)
                {
                    if (db.Maddelers.Any(a => a.UstMaddeID == kModel.MaddeID))
                    {
                        mmMessage.Messages.Add("Veri girişine açılmak istenen bu maddenin altında başka maddeler bulunduğu için Veri girişine açılamaz.");
                        mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "MaddeKod" });
                        mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "MaddeAdi" });
                    }
                }
                if (mmMessage.Messages.Count == 0 && kModel.VeriGirisSekliID != VeriGirisSekli.VeriGirisiYok)
                {
                    if (birimId.Count == 0)
                    {
                        mmMessage.Messages.Add("Veri girişi yapılacak maddeler için en az 1 Birim seçilmesi gerekmektedir.");
                    }
                }
            }
            if (mmMessage.Messages.Count == 0)
            {

                if (vaCokluVeriDonemIDs.Count == 0)
                {
                    mmMessage.Messages.Add("Çoklu veri girişi seçeneği evet seçilir ise en az 1 Veri Girilecek Veri Dönemi bilgisi seçilmesi gerekmektedir.");
                }
            }
            if (mmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                Maddeler table;
                if (kModel.VeriGirisSekliID != VeriGirisSekli.FormulleHesaplanacak)
                {
                    kModel.HesaplamaFormulu = null;
                }
                if (kModel.VeriGirisSekliID == VeriGirisSekli.VeriGirisiYok)
                {
                    kModel.MaddeTurID = null;
                    kModel.MaddeYilSonuDegerHesaplamaTipID = null;
                }
                if (kModel.MaddeID <= 0)
                {
                    table = db.Maddelers.Add(kModel);
                }
                else
                {
                    table = db.Maddelers.First(p => p.MaddeID == kModel.MaddeID);
                    table.UstMaddeID = kModel.UstMaddeID;
                    table.MaddeKod = kModel.MaddeKod;
                    table.VeriGirisSekliID = kModel.VeriGirisSekliID;
                    table.MaddeYilSonuDegerHesaplamaTipID = kModel.MaddeYilSonuDegerHesaplamaTipID;
                    table.VeriTipID = kModel.VeriTipID;
                    table.MaddeTurID = kModel.MaddeTurID;
                    table.MaddeAdi = kModel.MaddeAdi;
                    table.Aciklama = kModel.Aciklama;
                    table.HesaplamaFormulu = kModel.HesaplamaFormulu;
                    table.IsAktif = kModel.IsAktif;
                    table.IslemTarihi = kModel.IslemTarihi;
                    table.IslemYapanID = kModel.IslemYapanID;
                    table.IslemYapanIP = kModel.IslemYapanIP;
                }

                #region BirimlerAylarSet
                if (table.VeriGirisSekliID != VeriGirisSekli.VeriGirisiYok)
                {



                    var birimMaddeleri = db.BirimMaddeleris.Where(p => p.MaddeID == kModel.MaddeID).ToList();
                    var varolanlar = birimMaddeleri.Where(p => p.MaddeID == kModel.MaddeID && birimId.Contains(p.BirimID)).ToList();
                    var silinenler = birimMaddeleri.Where(p => p.MaddeID == kModel.MaddeID && !birimId.Contains(p.BirimID)).ToList();
                    db.BirimMaddeleris.RemoveRange(silinenler);
                    var eklenecekler = birimId.Where(p => varolanlar.Select(s => s.BirimID).All(a => a != p)).ToList();
                    foreach (var item in eklenecekler)
                        table.BirimMaddeleris.Add(new BirimMaddeleri { BirimID = item });


                    db.MaddelerFormulEslesenMaddelers.RemoveRange(table.MaddelerFormulEslesenMaddelers);
                    if (table.VeriGirisSekliID == VeriGirisSekli.FormulleHesaplanacak)
                        foreach (var item in maddelers)
                        {
                            table.MaddelerFormulEslesenMaddelers.Add(new MaddelerFormulEslesenMaddeler { EslesenMaddeKod = item.MaddeKod, EslesenMaddeID = item.MaddeID });
                        }

                }

                #endregion
                #region AylarSet


                var maddeAylari = db.MaddelerVeriGirisDonemleris.Where(p => p.MaddeID == kModel.MaddeID).ToList();
                var maVarolanlar = maddeAylari.Where(p => p.MaddeID == kModel.MaddeID && vaCokluVeriDonems.Any(a => a.VACokluVeriDonemID == p.VACokluVeriDonemID)).ToList();

                foreach (var maVarolan in maVarolanlar)
                {
                    var secilen = maddeAylari.First(f => f.VACokluVeriDonemID == maVarolan.VACokluVeriDonemID);
                    maVarolan.IsDosyaYuklensin = secilen.IsDosyaYuklensin;
                }

                var maSilinenler = maddeAylari.Where(p => p.MaddeID == kModel.MaddeID && vaCokluVeriDonems.All(a => a.VACokluVeriDonemID != p.VACokluVeriDonemID)).ToList();
                db.MaddelerVeriGirisDonemleris.RemoveRange(maSilinenler);
                var maEklenecekler = vaCokluVeriDonems.Where(p => maVarolanlar.Select(s => s.VACokluVeriDonemID).All(a => a != p.VACokluVeriDonemID)).ToList();

                foreach (var item in maEklenecekler)
                    table.MaddelerVeriGirisDonemleris.Add(item);


                #endregion
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, mmMessage.Messages.ToArray());
            ViewBag.SecilenlerBrm = birimId;
            ViewBag.UstMaddeID = new SelectList(MaddelerBus.CmbUstMaddelerTree(true, kModel.MaddeID), "Value", "Caption", kModel.UstMaddeID);
            ViewBag.VeriGirisSekliID = new SelectList(MaddelerBus.CmbVeriGirisSekilleri(false), "Value", "Caption", kModel.VeriGirisSekliID);
            ViewBag.MaddeYilSonuDegerHesaplamaTipID = new SelectList(MaddelerBus.CmbMaddeYilsonuDegerHesaplamaTipleri(), "Value", "Caption", kModel.MaddeYilSonuDegerHesaplamaTipID);
            ViewBag.VeriTipID = new SelectList(MaddelerBus.CmbVeriTipleri(), "Value", "Caption", kModel.VeriTipID);
            ViewBag.MaddeTurID = new SelectList(MaddeTurleriBus.CmbMaddeTurleri(kModel.MaddeID <= 0, true), "Value", "Caption", kModel.MaddeTurID);
            ViewBag.MmMessage = mmMessage;
            ViewBag.IsUstMadde = db.Maddelers.Any(a => a.UstMaddeID == kModel.MaddeID);
            ViewBag.SecilenlerAy = vaCokluVeriDonems;
            return View(kModel);
        }

        public ActionResult MaddeTurGuncelle(int id, int? maddeTurId)
        {
            var kayit = db.Maddelers.FirstOrDefault(p => p.MaddeID == id);
            string message;
            bool success = true;
            if (kayit != null)
            {

                try
                {
                    kayit.MaddeTurID = maddeTurId;
                    db.SaveChanges();
                    string maddeTurAdi = "Kaldırıldı.";
                    if (kayit.MaddeTurID.HasValue) maddeTurAdi = "'" + kayit.MaddeTurleri.MaddeTurAdi + "' Şeklinde Güncellendi.";
                    message = "'" + kayit.MaddeAdi + "' isimli Maddenin Tür bilgisi " + maddeTurAdi;
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.MaddeAdi + "' isimli Maddenin Tür bilgisi güncellenemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    SistemBilgilendirmeBus.SistemBilgisiKaydet(message, "Maddeler/MaddeTurGuncelle<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.Hata);
                }
            }
            else
            {
                success = false;
                message = "Güncellemek istediğiniz Madde sistemde bulunamadı!";
            }

            return new { success, message }.ToJsonResult();
        }
        public ActionResult MaddeYsHesapTipGuncelle(int id, int? maddeYilSonuDegerHesaplamaTipId)
        {
            var kayit = db.Maddelers.FirstOrDefault(p => p.MaddeID == id);
            string message;
            var success = true;
            if (kayit != null)
            {

                try
                {
                    kayit.MaddeYilSonuDegerHesaplamaTipID = maddeYilSonuDegerHesaplamaTipId;
                    db.SaveChanges();
                    string hesapTipAdiTurAdi = "Kaldırıldı.";
                    if (kayit.MaddeYilSonuDegerHesaplamaTipID.HasValue) hesapTipAdiTurAdi = "'" + kayit.MaddeYilSonuDegerHesaplamaTipleri.YilSonuDegerHesaplamaAdi + "' Şeklinde Güncellendi.";
                    message = "'" + kayit.MaddeAdi + "' isimli Maddenin yıl sonu değer hesaplama tip bilgisi " + hesapTipAdiTurAdi;
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.MaddeAdi + "' isimli Maddenin yıl sonu değer hesaplama tip bilgisi güncellenemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    SistemBilgilendirmeBus.SistemBilgisiKaydet(message, "Maddeler/MaddeYSHesapTipGuncelle<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.Hata);
                }
            }
            else
            {
                success = false;
                message = "Güncellemek istediğiniz Madde sistemde bulunamadı!";
            }
            return new { success, message }.ToJsonResult();
        }

        public ActionResult Sil(int id)
        {
            var kayit = db.Maddelers.FirstOrDefault(p => p.MaddeID == id);
            string message;
            var success = true;
            if (kayit != null)
            {

                try
                {
                    message = "'" + kayit.MaddeAdi + "' isimli Madde silindi!";
                    db.Maddelers.Remove(kayit);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    success = false;
                    message = "'" + kayit.MaddeAdi + "' isimli Madde silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                    SistemBilgilendirmeBus.SistemBilgisiKaydet(message, "Maddeler/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
                }
            }
            else
            {
                success = false;
                message = "Silmek istediğiniz Madde sistemde bulunamadı!";
            }
            return new { success, message }.ToJsonResult();
        }
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}