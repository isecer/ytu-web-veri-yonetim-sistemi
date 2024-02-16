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

namespace WebApp.Controllers
{
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [Authorize(Roles = RoleNames.GTVeriGirisi)]
    public class GtVeriGirisiController : Controller
    {
        // GET: GTVeriGirisi
        private VysDBEntities db = new VysDBEntities();
        public ActionResult Index()
        {

            var gtBirimId = UserIdentity.GetPageSelectedTableId(RoleNames.GTVeriGirisi, RollTableIdName.GtBirimId);
            var gtDonemId = UserIdentity.GetPageSelectedTableId(RoleNames.GTVeriGirisi, RollTableIdName.DonemId);

            if (!db.GTDonemleris.Any(a => a.GTDonemID == gtDonemId)) gtDonemId = db.GTDonemleris.OrderByDescending(o => o.Yil).Select(s => s.GTDonemID).FirstOrDefault();
            return Index(new FmGTVeriGirisi { PageSize = 30, GTDonemID = gtDonemId, Expand = gtDonemId.HasValue, GTBirimID = gtBirimId });
        }
        [HttpPost]
        public ActionResult Index(FmGTVeriGirisi model, bool export = false)
        {

            var gtHesapKods = UserIdentity.GetSelectedTableIDs(RollTableIdName.GtHesapKodId, null).SelectMany(s => s.RefTableIDs.Select(s2 => s.TableId + "" + s2)).ToList();
            var gtHesapNoIDs = UserIdentity.GetSelectedTableIDs(RollTableIdName.GtHesapNoId, null).SelectMany(s => s.RefTableIDs.Select(s2 => s.TableId + "" + s2)).ToList();
            var nowDate = DateTime.Now.Date;
            var q = (from vg in db.GTVeriGirisis
                     join br in db.GTBirimlers on vg.GTBirimID equals br.GTBirimID
                     join hn in db.GTHesapNumaralaris on vg.GTHesapNoID equals hn.GTHesapNoID
                     join kul in db.Kullanicilars on vg.IslemYapanID equals kul.KullaniciID
                     where gtHesapNoIDs.Contains(vg.GTBirimID + "" + vg.GTHesapNoID) || vg.GTVerigirisiDetays.Any(a => gtHesapKods.Contains(vg.GTBirimID + "" + a.GTHesapKodID))
                     select new FrGTVeriGirisi
                     {
                         Yil = vg.GTDonemleri.Yil,
                         GTVeriGirisiID = vg.GTVeriGirisiID,
                         GTDonemID = vg.GTDonemID,
                         GTHesapNoID = vg.GTHesapNoID,
                         GTBirimID = vg.GTBirimID,
                         BirimAdi = br.BirimAdi,
                         HesapNo = hn.HesapNo,
                         HesapNoAdi = hn.HesapNoAdi,
                         AktarimTarihi = vg.AktarimTarihi,
                         GelenTutar = vg.GelenTutar,
                         GTVeriGirisDurumID = vg.GTVeriGirisDurumID,
                         DurumAdi = vg.GTVeriGirisDurumlari.DurumAdi,
                         DurumColor = vg.GTVeriGirisDurumlari.DurumColor,
                         IslemTarihi = vg.IslemTarihi,
                         IslemYapan = kul.KullaniciAdi,
                         IslemYapanID = vg.IslemYapanID,
                         IslemYapanIP = vg.IslemYapanIP,
                         DetayCount = vg.GTVerigirisiDetays.Count,
                         IsAktifSurec = (vg.GTDonemleri.IsAktif && vg.GTDonemleri.BaslangicTarihi <= nowDate && vg.GTDonemleri.BitisTarihi >= nowDate),
                         GTVerigirisiDetayList = vg.GTVerigirisiDetays.Select(s2 => new FrGTVerigirisiDetay
                         {
                             GTVeriGirisDetayID = s2.GTVeriGirisDetayID,
                             GTVeriGirisID = s2.GTVeriGirisID,
                             GTHesapKodID = s2.GTHesapKodID,
                             HesapKod = s2.GTHesapKodlari.HesapKod,
                             HesapKodAdi = s2.GTHesapKodlari.HesapKodAdi,
                             GTHesapKodlariGelirNiteligiID = s2.GTHesapKodlariGelirNiteligiID,
                             GelirNiteligiAdi = s2.GTHesapKodlariGelirNitelikleri.GelirNiteligiAdi,
                             Tutar = s2.Tutar,
                             Aciklama = s2.Aciklama,
                             GTVergiKimlikNoID = s2.GTVergiKimlikNoID,
                             VergiKimlikNo = s2.GTVergiKimlikNumaralari.VergiKimlikNo,
                             AdSoyad = s2.GTVergiKimlikNumaralari.AdSoyad
                         }).ToList()
                     }).AsQueryable();
            if (model.BasTar.HasValue || model.BitTar.HasValue)
            {
                if (!model.BasTar.HasValue) model.BasTar = model.BitTar;
                else if (!model.BitTar.HasValue) model.BitTar = model.BasTar;
                q = q.Where(p => p.AktarimTarihi >= model.BasTar && p.AktarimTarihi <= model.BitTar);
            }
            if (model.GTVeriGirisDurumID.HasValue) q = q.Where(p => p.GTVeriGirisDurumID == model.GTVeriGirisDurumID);
            if (model.GTBirimID.HasValue) q = q.Where(p => p.GTBirimID == model.GTBirimID);
            if (model.GTDonemID.HasValue) q = q.Where(p => p.GTDonemID == model.GTDonemID);
            if (model.GTHesapNoID.HasValue) q = q.Where(p => p.GTHesapNoID == model.GTHesapNoID);
            if (model.GTHesapKodID.HasValue) q = q.Where(p => p.GTVerigirisiDetayList.Any(a => a.GTHesapKodID == model.GTHesapKodID));
            if (model.Aranan.IsNullOrWhiteSpace() == false) q = q.Where(p => p.HesapNo == model.Aranan || p.GTVerigirisiDetayList.Any(a => a.HesapKod == model.Aranan || a.VergiKimlikNo.Contains(model.Aranan) || a.AdSoyad.Contains(model.Aranan) || a.Aciklama.Contains(model.Aranan)));


            model.RowCount = q.Count(); 
            q = !model.Sort.IsNullOrWhiteSpace() ? q.OrderBy(model.Sort) : q.OrderByDescending(o => o.AktarimTarihi).ThenByDescending(t => t.IslemTarihi); 
            model.Data = q.Skip(model.PagingStartRowIndex).Take(model.PageSize).ToList(); ;
            model.GTVeriGirisDurumlaris = db.GTVeriGirisDurumlaris.ToList();
            #region export
            if (export && model.RowCount > 0)
            {

                var detay = q.SelectMany(s => s.GTVerigirisiDetayList).OrderBy(o => o.HesapKod).ToList();
                var qData = q.OrderBy(o => o.HesapNo).ThenBy(t => t.AktarimTarihi).ToList();
                qData.Add(new FrGTVeriGirisi { GTVeriGirisiID = -53, GelenTutar = detay.Sum(s => s.Tutar) });
                var data = qData.Select((s, inx) => new { s, inx }).ToList();

                detay.Add(new FrGTVerigirisiDetay { GTVeriGirisID = -53, GelirNiteligiAdi = "Genel Toplam" });
                var qEData = (from s in data
                              join d in detay on s.s.GTVeriGirisiID equals d.GTVeriGirisID
                              select new
                              {
                                  SiraNo = s.s.GTVeriGirisiID == -53 ? (int?)null : (s.inx + 1),
                                  s.s.HesapNo,
                                  AktarimTarihi = s.s.GTVeriGirisiID == -53 ? "" : s.s.AktarimTarihi.ToFormatDate(),
                                  HesapKodu = s.s.GTVeriGirisiID == -53 ? "" : (d.HesapKod + "/" + d.HesapKodAdi),
                                  GelirNiteligi = d.GelirNiteligiAdi,
                                  GelenTutar = s.s.GelenTutar.ToString("n2"),
                                  Tutar = s.s.GTVeriGirisiID == -53 ? "" : d.Tutar.ToString("n2"),
                                  VergiNo = s.s.GTVeriGirisiID == -53 ? "" : d.GTVergiKimlikNoID.HasValue ? (d.VergiKimlikNo + " / " + d.AdSoyad) : "",
                                  d.Aciklama,
                                  YevmiyeAciklamasi = s.s.GTVeriGirisiID == -53 ? "" : (s.s.AktarimTarihi.ToFormatDate() + " - " + s.s.HesapNo + " - " + d.GelirNiteligiAdi + " - " + s.s.GelenTutar + " - " + d.Tutar + (d.GTVergiKimlikNoID.HasValue ? " - " + d.VergiKimlikNo + " " + d.AdSoyad : "") + (!d.Aciklama.IsNullOrWhiteSpace() ? " - " + d.Aciklama : ""))
                              }).ToList();

                var birimAdi = "Tüm Birimler";
                var yil = data.First().s.Yil.ToString();
                if (model.GTBirimID.HasValue)
                {
                    birimAdi = data.First().s.BirimAdi;
                }

                var gv = new GridView();
                gv.DataSource = qEData;
                gv.DataBind();
                var sw = new StringWriter();
                var htw = new HtmlTextWriter(sw);
                Response.ContentType = "application/ms-excel";
                Response.ContentEncoding = System.Text.Encoding.UTF8;
                Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());
                gv.RenderControl(htw);
                return File(System.Text.Encoding.UTF8.GetBytes(sw.ToString()), Response.ContentType, birimAdi + "_Birimi_" + yil + "_Yılı_GelirTakibiListesi.xls");
            }
            #endregion



            ViewBag.GTDonemID = new SelectList(GtDonemlerBus.CmbGtDonemler(false), "Value", "Caption", model.GTDonemID);
            ViewBag.GTBirimID = new SelectList(UserBus.CmbYetkiliGtBirimlerKullanici(), "Value", "Caption", model.GTBirimID);
            ViewBag.GTHesapNoID = new SelectList(GtVeriGirisiBus.CmbHesapNumaralari(model.GTBirimID), "Value", "Caption", model.GTHesapNoID);
            ViewBag.GTHesapKodID = new SelectList(UserBus.CmbYetkiliGtHesapKodKullanici(model.GTBirimID), "Value", "Caption", model.GTHesapKodID);
            ViewBag.GTVeriGirisDurumID = new SelectList(GtVeriGirisiBus.CmbGtVeriGirisDurumlari(), "Value", "Caption", model.GTHesapKodID);

            UserIdentity.SetPageSelectedTableId(RoleNames.GTVeriGirisi, RollTableIdName.GtBirimId, model.GTBirimID);
            UserIdentity.SetPageSelectedTableId(RoleNames.GTVeriGirisi, RollTableIdName.DonemId, model.GTDonemID);
            return View(model);
        }


        public ActionResult Kayit(int? id, int? gtDonemid)
        {
            var mmMessage = new MmMessage();
            ViewBag.MmMessage = mmMessage;
            var model = new GTVeriGirisi();

            if (id.HasValue)
            {
                var data = db.GTVeriGirisis.FirstOrDefault(p => p.GTVeriGirisiID == id);
                if (data != null)
                {
                    if (data.GTVeriGirisDurumID != GTVeriGirisDurumu.Beklemede)
                    {
                        mmMessage.Messages.Add("Düzeltme işlemleri durumu Beklemede olan kayıtlar üzerinde yapılabilir.");
                        MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, mmMessage.Messages.ToArray());
                        return RedirectToAction("Index");
                    }
                    model = data;
                }
            }
            else
            {
                model.GTDonemID = gtDonemid ?? (GtDonemlerBus.GetAktifGtDonemId() ?? 0);
            }
            ViewBag.GTBirimID = new SelectList(UserBus.CmbYetkiliGtBirimlerKullanici(), "Value", "Caption", model.GTBirimID);
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.GTVeriGirisiKayitYetkisi)]
        public ActionResult Kayit(GTVeriGirisi kModel, List<int?> gtVeriGirisDetayIDs, List<int?> gtHesapKodIDs, List<int?> gtHesapKodlariGelirNiteligiIDs, List<decimal?> tutars, List<int?> gtVergiKimlikNoIDs, List<string> aciklamas)
        {
            var mmMessage = new MmMessage();
            gtVeriGirisDetayIDs = gtVeriGirisDetayIDs ?? new List<int?>();
            gtHesapKodIDs = gtHesapKodIDs ?? new List<int?>();
            gtHesapKodlariGelirNiteligiIDs = gtHesapKodlariGelirNiteligiIDs ?? new List<int?>();
            tutars = tutars ?? new List<decimal?>();
            gtVergiKimlikNoIDs = gtVergiKimlikNoIDs ?? new List<int?>();
            aciklamas = aciklamas ?? new List<string>();
            var qVgIds = gtVeriGirisDetayIDs.Select((s, inx) => new { s, inx }).ToList();
            var qHkIds = gtHesapKodIDs.Select((s, inx) => new { s, inx }).ToList();
            var qGnIds = gtHesapKodlariGelirNiteligiIDs.Select((s, inx) => new { s, inx }).ToList();
            var qTutars = tutars.Select((s, inx) => new { s, inx }).ToList();
            var qVkIds = gtVergiKimlikNoIDs.Select((s, inx) => new { s, inx }).ToList();
            var qAcklmas = aciklamas.Select((s, inx) => new { s, inx }).ToList();
            var qDetays = (from vg in qVgIds
                           join hk in qHkIds on vg.inx equals hk.inx
                           join gn in qGnIds on vg.inx equals gn.inx
                           join tt in qTutars on vg.inx equals tt.inx
                           join vk in qVkIds on vg.inx equals vk.inx
                           join ac in qAcklmas on vg.inx equals ac.inx
                           select new
                           {
                               GTVeriGirisDetayID = vg.s,
                               GTHesapKodID = hk.s,
                               GTHesapKodlariGelirNiteligiID = gn.s,
                               Tutar = tt.s,
                               GTVergiKimlikNoID = vk.s,
                               Aciklama = ac.s
                           }).ToList();

            if (kModel.GTVeriGirisiID > 0)
            {
                var vgKontrol = db.GTVeriGirisis.First(p => p.GTVeriGirisiID == kModel.GTVeriGirisiID);
                if (vgKontrol.GTVeriGirisDurumID != GTVeriGirisDurumu.Beklemede)
                {
                    mmMessage.Messages.Add("Düzeltme işlemleri durumu Beklemede olan kayıtlar üzerinde yapılabilir.");
                    MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, mmMessage.Messages.ToArray());
                    return RedirectToAction("Index");
                }
            }


            var gtDonem = GtDonemlerBus.GetGtDonemKontrol(kModel.GTDonemID);
            #region Kontrol
            if (gtDonem == null) mmMessage.Messages.Add("Dönem bilgisi aktif değildir. Kayıt işlemi yapılamaz!");
            else if (!gtDonem.IsAktif || !gtDonem.AktifSurec) mmMessage.Messages.Add(gtDonem.DonemYilAdi + " Aktif değildir. Kayıt işlemi yapılamaz!");
            else
            {
                if (kModel.GTBirimID <= 0)
                {
                    mmMessage.Messages.Add("Birim seçiniz.");
                    mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "GTBirimID" });
                }
                else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "GTBirimID" });
                if (kModel.GTHesapNoID <= 0)
                {
                    mmMessage.Messages.Add("Hesap Numarası seçiniz.");
                    mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "GTHesapNoID" });
                }
                else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "GTHesapNoID" });
                if (kModel.AktarimTarihi == DateTime.MinValue)
                {
                    mmMessage.Messages.Add("Aktarım Tarihi boş bırakılamaz.");
                    mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "AktarimTarihi" });
                }
                else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "AktarimTarihi" });

                if (kModel.GelenTutar <= 0)
                {
                    mmMessage.Messages.Add("Gelen Tutar bilgisi 0 dan büyük olmalıdır.");
                    mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "GelenTutar" });
                }
                else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "GelenTutar" });



                if (qDetays.Count == 0)
                {
                    mmMessage.Messages.Add("Kayıt işlemini yapabilmeniz için en az bir Detay eklemeniz gerekmektedir!");
                    mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "GTHesapKodIDs" });
                }
                if (mmMessage.Messages.Count == 0)
                {
                    int inx = 0;
                    foreach (var item in qDetays)
                    {
                        inx++;
                        var lMsg = new List<string>();
                        if (!item.GTHesapKodID.HasValue) lMsg.Add("Hesap Kodu");
                        if (!item.GTHesapKodlariGelirNiteligiID.HasValue) lMsg.Add("Gelir Niteliği");
                        if (!item.Tutar.HasValue) lMsg.Add("Tutar");
                        if (lMsg.Any())
                        {
                            mmMessage.Messages.Add(inx + ". satırdaki " + string.Join(", ", lMsg) + " bilgileri boş bırakımaz.");
                        }
                    }


                    if (mmMessage.Messages.Count == 0)
                    {
                        if (kModel.GelenTutar != qDetays.Sum(s => s.Tutar))
                        {
                            mmMessage.Messages.Add("Gelen tutar bilgisi ile girilen detaylardaki tutar toplamı bilgisi uyuşmamaktadır!");
                            mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Tutars" });
                            mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "GelenTutar" });
                        }
                    }
                }
            }
            #endregion

            if (mmMessage.Messages.Count == 0)
            {
                kModel.IslemTarihi = DateTime.Now;
                kModel.GTVeriGirisDurumID = GTVeriGirisDurumu.Beklemede;
                kModel.OnayYapanID = UserIdentity.Current.Id;
                kModel.OnayTarihi = DateTime.Now;
                kModel.IslemYapanID = UserIdentity.Current.Id;
                kModel.IslemYapanIP = UserIdentity.Ip;
                GTVeriGirisi table;

                if (kModel.GTVeriGirisiID <= 0)
                {
                    table = db.GTVeriGirisis.Add(kModel);
                    table.GTVerigirisiDetays = qDetays.Select(s => new GTVerigirisiDetay
                    {
                        GTHesapKodID = s.GTHesapKodID.Value,
                        GTHesapKodlariGelirNiteligiID = s.GTHesapKodlariGelirNiteligiID.Value,
                        Tutar = s.Tutar.Value,
                        GTVergiKimlikNoID = s.GTVergiKimlikNoID,
                        Aciklama = s.Aciklama,

                        IslemTarihi = DateTime.Now,
                        IslemYapanID = UserIdentity.Current.Id,
                        IslemYapanIP = UserIdentity.Ip

                    }).ToList();
                }
                else
                {
                    table = db.GTVeriGirisis.First(p => p.GTVeriGirisiID == kModel.GTVeriGirisiID);
                    table.GTBirimID = kModel.GTBirimID;
                    table.GTHesapNoID = kModel.GTHesapNoID;
                    table.AktarimTarihi = kModel.AktarimTarihi;
                    table.GelenTutar = kModel.GelenTutar;
                    table.IslemTarihi = kModel.IslemTarihi;
                    table.IslemYapanID = kModel.IslemYapanID;
                    table.IslemYapanIP = kModel.IslemYapanIP;

                    var varolanGnt = qDetays.Where(p => table.GTVerigirisiDetays.Any(a => a.GTVeriGirisDetayID == p.GTVeriGirisDetayID)).ToList();
                    var silinecekGnt = table.GTVerigirisiDetays.Where(p => qDetays.All(a => a.GTVeriGirisDetayID != p.GTVeriGirisDetayID)).ToList();
                    var eklenecekGnt = qDetays.Where(p => p.GTVeriGirisDetayID == 0).ToList();

                    db.GTVerigirisiDetays.RemoveRange(silinecekGnt);
                    foreach (var item in eklenecekGnt)
                        table.GTVerigirisiDetays.Add(new GTVerigirisiDetay
                        {
                            GTHesapKodID = item.GTHesapKodID.Value,
                            GTHesapKodlariGelirNiteligiID = item.GTHesapKodlariGelirNiteligiID.Value,
                            Tutar = item.Tutar.Value,
                            GTVergiKimlikNoID = item.GTVergiKimlikNoID,
                            Aciklama = item.Aciklama,
                            IslemTarihi = DateTime.Now,
                            IslemYapanID = UserIdentity.Current.Id,
                            IslemYapanIP = UserIdentity.Ip
                        });

                    foreach (var item in varolanGnt)
                    {
                        var gtVerigirisiDetay = table.GTVerigirisiDetays.First(p => p.GTVeriGirisDetayID == item.GTVeriGirisDetayID);
                        gtVerigirisiDetay.GTHesapKodID = item.GTHesapKodID.Value;
                        gtVerigirisiDetay.GTHesapKodlariGelirNiteligiID = item.GTHesapKodlariGelirNiteligiID.Value;
                        gtVerigirisiDetay.Tutar = item.Tutar.Value;
                        gtVerigirisiDetay.GTVergiKimlikNoID = item.GTVergiKimlikNoID;
                        gtVerigirisiDetay.Aciklama = item.Aciklama;
                        gtVerigirisiDetay.IslemTarihi = DateTime.Now;
                        gtVerigirisiDetay.IslemYapanID = UserIdentity.Current.Id;
                        gtVerigirisiDetay.IslemYapanIP = UserIdentity.Ip;
                    }

                }
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                if (kModel.GTHesapNoID > 0) kModel.GTHesapNumaralari = db.GTHesapNumaralaris.First(p => p.GTHesapNoID == kModel.GTHesapNoID);
                kModel.GTVerigirisiDetays = qDetays.Select(s => new GTVerigirisiDetay
                {
                    GTVeriGirisDetayID = s.GTVeriGirisDetayID ?? 0,
                    GTHesapKodID = s.GTHesapKodID ?? 0,
                    GTHesapKodlariGelirNiteligiID = s.GTHesapKodlariGelirNiteligiID ?? 0,
                    Tutar = s.Tutar ?? 0,
                    GTVergiKimlikNoID = s.GTVergiKimlikNoID ?? 0,
                    Aciklama = s.Aciklama,

                }).ToList();
                foreach (var item in kModel.GTVerigirisiDetays)
                {
                    if (item.GTHesapKodID > 0) item.GTHesapKodlari = db.GTHesapKodlaris.First(p => p.GTHesapKodID == item.GTHesapKodID);
                    if (item.GTHesapKodlariGelirNiteligiID > 0) item.GTHesapKodlariGelirNitelikleri = db.GTHesapKodlariGelirNitelikleris.First(p => p.GTHesapKodlariGelirNiteligiID == item.GTHesapKodlariGelirNiteligiID);
                    if (item.GTVergiKimlikNoID > 0) item.GTVergiKimlikNumaralari = db.GTVergiKimlikNumaralaris.First(p => p.GTVergiKimlikNoID == item.GTVergiKimlikNoID);
                }
                MessageBox.Show("Uyarı", MessageBox.MessageType.Warning, mmMessage.Messages.ToArray());
            }
            ViewBag.MmMessage = mmMessage;
            ViewBag.GTBirimID = new SelectList(UserBus.CmbYetkiliGtBirimlerKullanici(), "Value", "Caption", kModel.GTBirimID);
            return View(kModel);
        }
        public ActionResult GetHesapKodlari(int? gtBirimId, string term)
        {
            var hesapKodIDs = UserIdentity.GetSelectedTableIDs(RollTableIdName.GtHesapKodId, gtBirimId ?? -1).SelectMany(s => s.RefTableIDs).ToList();
            var hesapKods = (from s in db.GTHesapKodlaris.Where(p => p.GTBirimHesapKodlaris.Any(a => a.GTBirimID == (gtBirimId ?? -1) && hesapKodIDs.Contains(a.GTHesapKodID)))
                             orderby s.HesapKod
                             where (s.HesapKod + " " + s.HesapKodAdi).Contains(term)
                             select new
                             {
                                 id = s.GTHesapKodID,
                                 text = s.HesapKod + " / " + s.HesapKodAdi

                             }).OrderBy(o => o.text).Take(20).ToList();

            return hesapKods.ToJsonResult();
        }
        public ActionResult GetHesapKodGelirNitelikleri(int gtHesapKodId)
        {


            var gelirNiteliks = (from s in db.GTHesapKodlariGelirNitelikleris.Where(p => p.GTHesapKodID == gtHesapKodId)
                                 orderby s.GelirNiteligiAdi
                                 select new
                                 {
                                     id = s.GTHesapKodlariGelirNiteligiID,
                                     text = s.GelirNiteligiAdi

                                 }).OrderBy(o => o.text).ToList();

            return gelirNiteliks.ToJsonResult();
        }
        public ActionResult GetVergiKimlikNumaralari(string term)
        {


            var gtVergiKimlikNumaralaris = (from s in db.GTVergiKimlikNumaralaris.Where(p => p.VergiKimlikNo.StartsWith(term) || p.AdSoyad.Contains(term))
                                            orderby s.AdSoyad
                                            select new
                                            {
                                                id = s.GTVergiKimlikNoID,
                                                text = (s.VergiKimlikNo + " / " + s.AdSoyad)
                                            }).Take(20).ToList();

            return gtVergiKimlikNumaralaris.ToJsonResult();
        }
        public ActionResult GetHesapNumaralari(string term, int? gtBirimId)
        {
            var gtHesapNoIDs = UserIdentity.GetSelectedTableIDs(RollTableIdName.GtHesapNoId, gtBirimId ?? -1).SelectMany(s => s.RefTableIDs).ToList();


            var gtHesapNumaralaris = (from s in db.GTHesapNumaralaris.Where(p => p.GTBirimHesapNumaralaris.Any(a => a.GTBirimID == (gtBirimId ?? -1) && gtHesapNoIDs.Contains(a.GTHesapNoID)) && (p.HesapNo.StartsWith(term) || p.HesapNoAdi.Contains(term)))
                                      orderby s.HesapNoAdi
                                      select new
                                      {
                                          id = s.GTHesapNoID,
                                          text = s.HesapNo + " / " + s.HesapNoAdi
                                      }).Take(20).ToList();

            return gtHesapNumaralaris.ToJsonResult();
        }
        public ActionResult VknEkle(string vergiKimlikNo, string adSoyad)
        {
            var mmMessage = new MmMessage();
            #region Kontrol
            if (vergiKimlikNo.IsNullOrWhiteSpace())
            {
                mmMessage.Messages.Add("Vergi Kimlik No bilgisi boş bırakılamaz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "VergiKimlikNo" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "VergiKimlikNo" });
            if (adSoyad.IsNullOrWhiteSpace())
            {
                mmMessage.Messages.Add("Ad Soyad boş bırakılamaz.");
                mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "AdSoyad" });
            }
            else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "AdSoyad" });



            if (mmMessage.Messages.Count == 0)
            {
                if (db.GTVergiKimlikNumaralaris.Any(a => a.VergiKimlikNo == vergiKimlikNo))
                {
                    mmMessage.Messages.Add("Girilen Vergi Kimlik Numarası bilgisi daha önceden girilen bir Vergi Kimlik Numarası ile çakışmaktadır.");
                    mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "VergiKimlikNo" });
                }
                else mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Success, PropertyName = "VergiKimlikNo" });

            }

            #endregion
            string message;
            var success = true;
            if (mmMessage.Messages.Count == 0)
            {
                var kModel = new GTVergiKimlikNumaralari
                {
                    AdSoyad = adSoyad,
                    VergiKimlikNo = vergiKimlikNo,
                    IslemTarihi = DateTime.Now,
                    IslemYapanID = UserIdentity.Current.Id,
                    IslemYapanIP = UserIdentity.Ip,
                    IsAktif = true
                };
                db.GTVergiKimlikNumaralaris.Add(kModel);
                db.SaveChanges();
                message = vergiKimlikNo + " numaralı VKN sisteme eklendi.";
            }
            else
            {
                message = string.Join("</br>", mmMessage.Messages);
                success = false;
            }

            return Json(new { success, message }, "application/json", JsonRequestBehavior.AllowGet);
        }
        public ActionResult Onayla(int id, int gtVeriGirisDurumId)
        {
            var kayit = db.GTVeriGirisis.FirstOrDefault(p => p.GTVeriGirisiID == id);
            var message = "";
            var success = true;
            if (kayit != null)
            {
                var gtDonemId = kayit.GTDonemID;
                var birimYetkileri = UserIdentity.Current.TableRollId[RollTableIdName.GtBirimId];
                var mMessage = new MmMessage
                {
                    Title = "Gelir Takip Veri Girişi Onayı İşlemi",
                    MessageType = Msgtype.Warning
                };


                var onayYetkisi = RoleNames.GTVeriGirisiOnayYetkisi.InRole();
                var yevmiyeEklendiYetkisi = RoleNames.GTVeriGirisiYevmiyeEklendiYetkisi.InRole();
                var yetkiGtVeriGirisDurumIDs = new List<int>();
                var tamYetki = false;
                var durum = db.GTVeriGirisDurumlaris.First(p => p.GTVeriGirisDurumID == gtVeriGirisDurumId);
                if (onayYetkisi && yevmiyeEklendiYetkisi)
                {
                    yetkiGtVeriGirisDurumIDs.AddRange(new List<int> { 1, 2, 3 });
                    tamYetki = true;
                }
                else if (onayYetkisi) yetkiGtVeriGirisDurumIDs.Add(2);
                else if (yevmiyeEklendiYetkisi)
                {
                    yetkiGtVeriGirisDurumIDs.AddRange(new List<int> { 1, 2, 3 }); tamYetki = true;
                }

                var birimYetki = birimYetkileri.Contains(kayit.GTBirimID);
                var surec = GtDonemlerBus.GetGtDonemKontrol(gtDonemId);
                if (!surec.IsAktif || !surec.AktifSurec)
                {
                    mMessage.Messages.Add("İşlem yapmaya çalıştığınız Süreç aktif olmadığından herhangi bir işlem yapamazsınız.");
                }
                else if (!birimYetki)
                {
                    mMessage.Messages.Add("İşlem yapmaya çalıştığınız birim yetkiniz dahilinde değildir.");
                }
                else if (!yetkiGtVeriGirisDurumIDs.Contains(gtVeriGirisDurumId))
                {
                    mMessage.Messages.Add("Kayıt durumunu '" + durum.DurumAdi + "' durumuna çevirecek yetkiye sahip değilsiniz.");
                }
                else if (!tamYetki)
                {
                    if (kayit.GTVeriGirisDurumID == GTVeriGirisDurumu.Onaylandi && gtVeriGirisDurumId == GTVeriGirisDurumu.Beklemede)
                    {
                        mMessage.Messages.Add("Kayıt durumunu 'Beklemede' durumuna çevirecek yetkiye sahip değilsiniz.");
                    }
                }
                if (mMessage.Messages.Count == 0)
                {
                    try
                    {
                        message = "'" + kayit.AktarimTarihi.ToShortDateString() + "' aktarım tarihli ve " + kayit.GTHesapNumaralari.HesapNo + "  Hesap numarası ile yapılan veri girişi durumu '" + durum.DurumAdi + "' şeklinde güncellendi";

                        kayit.GTVeriGirisDurumID = gtVeriGirisDurumId;
                        kayit.OnayYapanID = UserIdentity.Current.Id;
                        kayit.OnayTarihi = DateTime.Now;
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        success = false;
                        message = "'" + kayit.AktarimTarihi.ToShortDateString() + "' aktarım tarihli ve " + kayit.GTHesapNumaralari.HesapNo + "  Hesap numarası ile yapılan veri girişi bilgisi Onay işlemi yapılamadı! <br/> Bilgi:" + ex.ToExceptionMessage();
                        SistemBilgilendirmeBus.SistemBilgisiKaydet(message, "GTVeriGirisi/Onayla<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
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
                message = "Onay işlemi yapmak istediğiniz veri girişi bilgisi sistemde bulunamadı!";
            }

            return Json(new
            {
                success,
                message,
                kayit.GTVeriGirisDurumID,
                kayit.GTVeriGirisDurumlari.DurumColor
            }, "application/json", JsonRequestBehavior.AllowGet);
        }
        public ActionResult Sil(int id)
        {
            var kayit = db.GTVeriGirisis.FirstOrDefault(p => p.GTVeriGirisiID == id);
            string message = "";
            bool success = true;
            if (kayit != null)
            {
                var gtDonemId = kayit.GTDonemID;
                var mMessage = new MmMessage
                {
                    Title = "Gelir Takip Veri Girişi Silme İşlemi",
                    MessageType = Msgtype.Warning
                };
                var gtHesapNoIDs = UserIdentity.GetSelectedTableIDs(RollTableIdName.GtHesapNoId, kayit.GTBirimID).SelectMany(s => s.RefTableIDs).ToList();


                var kayitYetki = RoleNames.GTVeriGirisiKayitYetkisi.InRole();
                var surec = GtDonemlerBus.GetGtDonemKontrol(gtDonemId);
                if (!surec.IsAktif || !surec.AktifSurec)
                {
                    mMessage.Messages.Add("İşlem yapmaya çalıştığınız Süreç aktif olmadığından herhangi bir işlem yapamazsınız.");
                }
                else if (!kayitYetki)
                {
                    mMessage.Messages.Add("Veri girişi için yetkiniz bulunmamaktadır.");
                }
                else if (!gtHesapNoIDs.Contains(kayit.GTHesapNoID))
                {
                    mMessage.Messages.Add("İşlem yapmaya çalıştığınız Hesap Numarası yetkiniz dahilinde değildir.");
                }
                else if (kayit.GTVeriGirisDurumID != GTVeriGirisDurumu.Beklemede)
                {
                    mMessage.Messages.Add("Onaylanan kayıt üzerinde silme işlemi yapılamaz.");
                }
                else
                {
                    try
                    {
                        message = "'" + kayit.AktarimTarihi.ToShortDateString() + "' aktarım tarihli ve " + kayit.GTHesapNumaralari.HesapNo + "  Hesap numarası ile yapılan veri girişi bilgisi silindi!";
                        db.GTVeriGirisis.Remove(kayit);
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        success = false;
                        message = "'" + kayit.AktarimTarihi.ToShortDateString() + "' aktarım tarihli ve " + kayit.GTHesapNumaralari.HesapNo + "  Hesap numarası ile yapılan veri girişi bilgisi silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage();
                        SistemBilgilendirmeBus.SistemBilgisiKaydet(message, "GTVeriGirisi/Sil<br/><br/>" + ex.ToExceptionStackTrace(), BilgiTipi.OnemsizHata);
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
                message = "Silmek istediğiniz veri girişi bilgisi sistemde bulunamadı!";
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