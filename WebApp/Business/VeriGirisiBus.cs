using BiskaUtil;
using Database;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;
using WebApp.Utilities.Dtos;
using WebApp.Utilities.Extensions;
using WebApp.Utilities.Helpers;
using WebApp.Utilities.Helpers.Hesaplama;
using WebApp.Utilities.MenuAndRoles;
using WebApp.Utilities.MessageBox;
using WebApp.Utilities.Results;

namespace WebApp.Business
{
    public static class VeriGirisiBus
    {
        public static List<ComboModelInt> CmbGetVgMaddeTurleri(int? vaSurecId, int? birimId, bool bosSecimVar = true)
        {
            return CmbGetVgMaddeTurleriData(vaSurecId, birimId, false, bosSecimVar);

        }
        public static List<ComboModelInt> CmbGetVgMaddeTurleri(int? vaSurecId, int? birimId, bool isVeriGirisineAcikFiltresiEkle, bool bosSecimVar)
        {
            return CmbGetVgMaddeTurleriData(vaSurecId, birimId, bosSecimVar, isVeriGirisineAcikFiltresiEkle);

        }
        private static List<ComboModelInt> CmbGetVgMaddeTurleriData(int? vaSurecId, int? birimId, bool isVeriGirisineAcikFiltresiEkle = false, bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt());
            if (isVeriGirisineAcikFiltresiEkle) dct.Add(new ComboModelInt { Value = -1, Caption = "Veri Girişine Açık Olan Maddeleri Listele" });
            using (var db = new VysDBEntities())
            {
                var data = db.MaddeTurleris.Where(p => p.VASurecleriMaddes.Any(a => a.VASurecID == (vaSurecId ?? a.VASurecID) && a.VASurecleriMaddeBirims.Any(a2 => a2.BirimID == (birimId ?? a2.BirimID)))).OrderBy(o => o.MaddeTurAdi).ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.MaddeTurID, Caption = item.MaddeTurAdi });
                }
            }
            return dct;

        }
        public static Birimler[] VaSurecBirimlerTreeList(int vaSurecId)
        {
            var birimIDs = UserIdentity.Current.TableRollId[RollTableIdName.BirimId];
            using (var db = new VysDBEntities())
            {
                var surecBirimIds = db.VASurecleriMaddeBirims
                    .Where(p => birimIDs.Contains(p.BirimID) && p.VASurecleriMadde.IsAktif &&
                                p.VASurecleriMadde.VASurecID == vaSurecId).Select(s => s.BirimID).Distinct().ToList();
                var seciliBirimlers = db.Birimlers.Where(p => surecBirimIds.Contains(p.BirimID)).ToList();

                foreach (var itemBirim in seciliBirimlers)
                {
                    var birim = itemBirim;
                    while (birim.UstBirimID.HasValue)
                    {
                        surecBirimIds.Add(birim.UstBirimID.Value);
                        birim = birim.Birimler2;
                    }
                }
                var data = db.Birimlers.Where(p => surecBirimIds.Contains(p.BirimID)).OrderBy(o => o.BirimAdi).ToList().ToOrderedList("BirimID", "UstBirimID", "BirimAdi");
                return data;
            }


        }
        public static List<ComboModelInt> CmbYetkiliVaSurecBirimlerKullanici(int vaSurecId, bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt());
            using (var db = new VysDBEntities())
            {
                var birimIDs = UserIdentity.Current.TableRollId[RollTableIdName.BirimId];

                var surecBirimIds = db.VASurecleriMaddeBirims
                    .Where(p => birimIDs.Contains(p.BirimID) && p.VASurecleriMadde.IsAktif &&
                                p.VASurecleriMadde.VASurecID == vaSurecId).Select(s => s.BirimID).Distinct().ToList();
                var data = (from s in db.Vw_BirimlerTree.Where(p => surecBirimIds.Contains(p.BirimID))
                            select new
                            {
                                s.BirimID,
                                s.BirimTreeAdi
                            }).OrderBy(o => o.BirimTreeAdi).ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.BirimID, Caption = item.BirimTreeAdi });
                }
            }
            return dct;

        }

        public static List<ComboModelInt> CmbVaCokluVeriDonemleri(bool bosSecimVar = true, string bosSecimAdi = "")
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Value = null, Caption = bosSecimAdi });

            using (var db = new VysDBEntities())
            {
                var data = db.VACokluVeriDonemleris.ToList();
                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.VACokluVeriDonemID, Caption = item.CokluVeriDonemAdi });
                }
            }
            return dct;

        }


        public static int? GetLastVaSurecId(int? vaSurecId)
        {

            using (var entities = new VysDBEntities())
            {
                if (!entities.VASurecleris.Any(a => a.VASurecID == vaSurecId))
                    vaSurecId = entities.VASurecleris.OrderByDescending(o => o.Yil).ThenByDescending(t => t.BaslangicTarihi).Select(s => s.VASurecID).FirstOrDefault();
            }

            return vaSurecId;

        }

        public static FmVeriGiris GetVerigirisDataModel(FmVeriGiris model)
        {
            using (var db = new VysDBEntities())
            {
                var birimIDs = UserIdentity.Current.TableRollId[RollTableIdName.BirimId];

                var data = db.Vw_MaddeVeriGirisDurum.Where(p => p.VASurecID == model.VaSurecId && p.IsAktif && birimIDs.Contains(p.BirimID) && p.BirimID == (model.BirimId ?? p.BirimID)).ToList();
                var vaSurecBirimIds = data.Select(s => s.BirimID).Distinct().ToList();
                var vaSurecMaddeIds = data.Select(s => s.VASurecleriMaddeID).Distinct().ToList();
                var vaSurecMaddeTur = db.VASurecleriMaddeTurs.Where(p => p.VASurecID == model.VaSurecId);
                var datavm = db.VASurecleriMaddes.Where(p => vaSurecMaddeIds.Contains(p.VASurecleriMaddeID)  && p.VASurecleriMaddeBirims.Any(a => vaSurecBirimIds.Contains(a.BirimID))).ToList();
                var q = (from s in data
                         join vm in datavm on s.VASurecleriMaddeID equals vm.VASurecleriMaddeID
                         join vmt in vaSurecMaddeTur on vm.MaddeTurID equals vmt.MaddeTurID
                         select new FrVgMaddeler
                         {
                             VASurecID = s.VASurecID,
                             Yil = s.Yil,
                             BirimID = s.BirimID,
                             BirimAdi = s.BirimAdi,
                             BirimTreeAdi = s.BirimTreeAdi,
                             MaddeTurID = s.MaddeTurID,
                             MaddeTurIsVeriGirisiAcik = vmt.IsVeriGirisiAcik,
                             MaddeTurAdi = s.MaddeTurAdi,
                             MaddeID = s.MaddeID,
                             MaddeKod = s.MaddeKod,
                             MaddeAdi = s.MaddeAdi,
                             MaddeTreeAdi = s.MaddeTreeAdi,
                             VeriGirisiOnaylandi = s.GirilecekVeriSayisi == s.OnaylananVeriSayisi,
                             OnaylananVeriSayisi = s.OnaylananVeriSayisi ?? 0,
                             IsPlanlananDegerOlacak = s.IsPlanlananDegerOlacak,
                             IsPlanlananDegerOlacakGelecekYil = s.IsPlanlananDegerOlacakGelecekYil,
                             PlanlananDeger = s.PlanlananDeger,
                             PlanlananDegerGelecekYil = s.PlanlananDegerGelecekYil,
                             GirilecekVeriSayisi = s.GirilecekVeriSayisi ?? 0,
                             GirilenVeriSayisi = s.GirilenVeriSayisi ?? 0,
                             YilSonuDegerHesaplamaAdi = s.YilSonuDegerHesaplamaAdi,
                             HesaplamaFormulu = s.HesaplamaFormulu,
                             VeriGirisSekliID = s.VeriGirisSekliID,
                             MaddeYilSonuDegerHesaplamaTipID = s.MaddeYilSonuDegerHesaplamaTipID,
                             HesaplananSonucDegeri = s.HesaplananSonucDegeri,
                             MaddeVeriGirisDurumId = s.OnaylananVeriSayisi > 0 && s.GirilecekVeriSayisi == s.OnaylananVeriSayisi ? 1 : 0,
                             VASurecleriMaddeID = s.VASurecleriMaddeID,
                             Aciklama = s.Aciklama,
                             VeriGirisSekliAdi = s.VeriGirisSekliAdi,
                             VaSurecleriMaddeGirilenDegers = db.VASurecleriMaddeGirilenDegers.Where(p => p.VASurecleriMaddeID == s.VASurecleriMaddeID && p.BirimID == s.BirimID).ToList(),
                             VaSurecleriMaddeVeriGirisDonemleris = vm.VASurecleriMaddeVeriGirisDonemleris.ToList()
                         }).AsQueryable();
                if (!model.Aranan.IsNullOrWhiteSpace()) q = q.Where(p => p.MaddeTreeAdi.Contains(model.Aranan) || p.MaddeKod.ToLower() == model.Aranan.ToLower().Trim());
                if (model.MaddeVeriGirisDurumId.HasValue)
                {
                    q = model.MaddeVeriGirisDurumId == 1 ? q.Where(p => p.GirilecekVeriSayisi == p.GirilenVeriSayisi) : q.Where(p => p.GirilecekVeriSayisi != p.GirilenVeriSayisi);
                }

                if (model.MaddeTurId.HasValue)
                {
                    q = model.MaddeTurId == -1 ? q.Where(p => p.MaddeTurIsVeriGirisiAcik) : q.Where(p => p.MaddeTurID == model.MaddeTurId);
                }
                if (model.VeriGirisiOnaylandi.HasValue) q = q.Where(p => p.VeriGirisiOnaylandi == model.VeriGirisiOnaylandi);
                model.RowCount = q.Count();
                model.AktifCount = q.Count(c => c.MaddeVeriGirisDurumId == 1);
                q = !model.Sort.IsNullOrWhiteSpace() ? q.OrderBy(model.Sort) : q.OrderBy(o => o.MaddeAdi);

                if (model.Export)
                {
                    model.FilteredMaddeIds = q.Select(s => s.MaddeID).Distinct().ToList();
                    return model;
                }
                model.Data = q.Skip(model.PagingStartRowIndex).Take(model.PageSize).ToList();


                var formulleHesaplanacakMaddeIds = model.Data
                    .Where(p => p.VeriGirisSekliID == VeriGirisSekli.FormulleHesaplanacak).Select(s => s.MaddeID)
                    .ToList();
                if (formulleHesaplanacakMaddeIds.Any())
                {
                    var formulHesapla = new VeriGirisFormulHesapla(model.VaSurecId ?? 0, formulleHesaplanacakMaddeIds);
                    var hesaplananlar = formulHesapla.HesaplaFormulMaddeleri();
                    foreach (var itemHesaplanan in hesaplananlar)
                    {
                        var row = model.Data.FirstOrDefault(p => p.MaddeKod == itemHesaplanan.MaddeKod);
                        row.PlanlananDeger = itemHesaplanan.PlanlananHedef;
                        row.PlanlananDegerGelecekYil = itemHesaplanan.PlanlananVeriGelecekYil;
                        row.HesaplananSonucDegeri = itemHesaplanan.HesaplamaSonucu;
                        row.GirilenVeriSayisi = itemHesaplanan.VaSurecleriMaddeGirilenDegers.Count(c => c.GirilenDeger.HasValue && c.VaCokluVeriDonemId != VaCokluVeriDonemEnum.HesaplamaSonucu);
                        row.OnaylananVeriSayisi = itemHesaplanan.VaSurecleriMaddeGirilenDegers.Count(c => c.VeriGirisiOnaylandi && c.VaCokluVeriDonemId != VaCokluVeriDonemEnum.HesaplamaSonucu);
                        row.VeriGirisiOnaylandi = row.GirilenVeriSayisi > 0;
                    }
                }

                return model;
            }
        }

        public static VeriGirisDetailDto GetDetailModel(int vaSurecId, int birimId, int maddeId)
        {
            var vaSurecKontrol = SurecIslemleriBus.GetVaSurecKontrol(vaSurecId);

            using (var entities = new VysDBEntities())
            {
                var birimIDs = UserIdentity.Current.TableRollId[RollTableIdName.BirimId];
                var mdl = new VeriGirisDetailDto
                {
                    VASurecID = vaSurecId,
                    BirimID = birimId,
                    MaddeID = maddeId,
                    SurecAktifYilPlanlananVeriGirisiAcik = vaSurecKontrol.IsAktifYilPlanlananVeriGirisiAcik,
                    SurecGelecekYilPlanlananVeriGirisiAcik = vaSurecKontrol.IsGelecekYilPlanlananVeriGirisiAcik,
                    BaslangicTarihi = vaSurecKontrol.BaslangicTarihi,
                    BitisTarihi = vaSurecKontrol.BitisTarihi,
                    SurecIsAktif = vaSurecKontrol.AktifSurec,
                    SurecYil = vaSurecKontrol.Yil
                };

                var vaSurecleriEklenenDosyalar = entities.VASurecleriMaddeEklenenDosyas.Where(p =>
                    p.VASurecleriMadde.VASurecID == mdl.VASurecID && p.VASurecleriMadde.MaddeID == maddeId && p.BirimID == birimId).ToList();
                mdl.VeriGirisi = (from maddeVeriGirisDurum in entities.Vw_MaddeVeriGirisDurum.Where(p => p.VASurecID == mdl.VASurecID && p.IsAktif)
                                  join vaSurecleriMadde in entities.VASurecleriMaddes on maddeVeriGirisDurum.VASurecleriMaddeID equals vaSurecleriMadde.VASurecleriMaddeID
                                  join vaSurecleriMaddeTur in entities.VASurecleriMaddeTurs.Where(p => p.VASurecID == mdl.VASurecID) on maddeVeriGirisDurum.MaddeTurID equals vaSurecleriMaddeTur.MaddeTurID
                                  where birimIDs.Contains(maddeVeriGirisDurum.BirimID) && maddeVeriGirisDurum.BirimID == mdl.BirimID && maddeVeriGirisDurum.MaddeID == mdl.MaddeID
                                  select new FrVgMaddeler
                                  {

                                      Yil = maddeVeriGirisDurum.Yil,
                                      BirimAdi = maddeVeriGirisDurum.BirimAdi,
                                      MaddeTurID = maddeVeriGirisDurum.MaddeTurID,
                                      MaddeTurAdi = maddeVeriGirisDurum.MaddeTurAdi,
                                      MaddeTurIsVeriGirisiAcik = vaSurecleriMaddeTur.IsVeriGirisiAcik,
                                      MaddeID = maddeVeriGirisDurum.MaddeID,
                                      MaddeKod = maddeVeriGirisDurum.MaddeKod,
                                      MaddeAdi = maddeVeriGirisDurum.MaddeAdi,
                                      MaddeTreeAdi = maddeVeriGirisDurum.MaddeTreeAdi,
                                      VeriGirisiOnaylandi = maddeVeriGirisDurum.GirilecekVeriSayisi == maddeVeriGirisDurum.OnaylananVeriSayisi,
                                      OnaylananVeriSayisi = maddeVeriGirisDurum.OnaylananVeriSayisi ?? 0,
                                      DosyaCount = vaSurecleriEklenenDosyalar.Count,
                                      IsPlanlananDegerOlacak = maddeVeriGirisDurum.IsPlanlananDegerOlacak,
                                      IsPlanlananDegerOlacakGelecekYil = maddeVeriGirisDurum.IsPlanlananDegerOlacakGelecekYil,
                                      PlanlananDeger = maddeVeriGirisDurum.PlanlananDeger,
                                      PlanlananDegerGelecekYil = maddeVeriGirisDurum.PlanlananDegerGelecekYil,
                                      GirilecekVeriSayisi = maddeVeriGirisDurum.GirilecekVeriSayisi ?? 0,
                                      GirilenVeriSayisi = maddeVeriGirisDurum.GirilenVeriSayisi ?? 0,
                                      YilSonuDegerHesaplamaAdi = maddeVeriGirisDurum.YilSonuDegerHesaplamaAdi,
                                      HesaplamaFormulu = maddeVeriGirisDurum.HesaplamaFormulu,
                                      VeriGirisSekliID = maddeVeriGirisDurum.VeriGirisSekliID,
                                      MaddeYilSonuDegerHesaplamaTipID = maddeVeriGirisDurum.MaddeYilSonuDegerHesaplamaTipID,
                                      HesaplananSonucDegeri = maddeVeriGirisDurum.HesaplananSonucDegeri,
                                      MaddeVeriGirisDurumId = maddeVeriGirisDurum.OnaylananVeriSayisi > 0 && maddeVeriGirisDurum.GirilecekVeriSayisi == maddeVeriGirisDurum.OnaylananVeriSayisi ? 1 : 0,
                                      VASurecleriMaddeID = maddeVeriGirisDurum.VASurecleriMaddeID,
                                      Aciklama = maddeVeriGirisDurum.Aciklama,
                                      VeriGirisSekliAdi = maddeVeriGirisDurum.VeriGirisSekliAdi,
                                      VaSurecleriMaddeVeriGirisDonemleris = vaSurecleriMadde.VASurecleriMaddeVeriGirisDonemleris.ToList(),
                                      VaSurecleriMaddeGirilenDegers = vaSurecleriMadde.VASurecleriMaddeGirilenDegers.Where(p => p.BirimID == maddeVeriGirisDurum.BirimID).ToList()

                                  }).First();
                var kullaniciIds = mdl.VeriGirisi.VaSurecleriMaddeGirilenDegers.Where(p => p.IslemYapanID.HasValue).Select(s => s.IslemYapanID.Value).Distinct().ToList();
                kullaniciIds.AddRange(mdl.VeriGirisi.VaSurecleriMaddeGirilenDegers.Where(p => p.OnayIslemYapanID.HasValue).Select(s => s.OnayIslemYapanID.Value).Distinct().ToList());

                var kullanicilar = entities.Kullanicilars.Where(p => kullaniciIds.Contains(p.KullaniciID))
                    .Select(s => new { s.KullaniciID, AdSoyad = s.Ad + " " + s.Soyad }).ToList();
                mdl.VeriGirisi.VgMaddeVerileris = (from vaSurecleriMaddeVeriGirisDonemleri in mdl.VeriGirisi.VaSurecleriMaddeVeriGirisDonemleris
                                                   let vaSurecleriMaddeGirilenDeger = mdl.VeriGirisi.VaSurecleriMaddeGirilenDegers.FirstOrDefault(p => p.VACokluVeriDonemID == (vaSurecleriMaddeVeriGirisDonemleri.VACokluVeriDonemID == 0 ? (int?)null : vaSurecleriMaddeVeriGirisDonemleri.VACokluVeriDonemID))
                                                   let veriGiren = kullanicilar.FirstOrDefault(p => p.KullaniciID == vaSurecleriMaddeGirilenDeger?.IslemYapanID)
                                                   let veriOnaylayan = kullanicilar.FirstOrDefault(p => p.KullaniciID == vaSurecleriMaddeGirilenDeger?.OnayIslemYapanID)
                                                   select new VgMaddeVerileri
                                                   {
                                                       VaCokluVeriDonemId = vaSurecleriMaddeVeriGirisDonemleri.VACokluVeriDonemID.ToNullIntZero(),
                                                       CokluVeriDonemAdi = vaSurecleriMaddeVeriGirisDonemleri.VACokluVeriDonemleri.CokluVeriDonemAdi,
                                                       IsDosyaYuklensin = vaSurecleriMaddeVeriGirisDonemleri.IsDosyaYuklensin,
                                                       DosyaCount = vaSurecleriEklenenDosyalar.Count(p => p.VACokluVeriDonemID == (vaSurecleriMaddeVeriGirisDonemleri.VACokluVeriDonemID == 0 ? p.VACokluVeriDonemID : vaSurecleriMaddeVeriGirisDonemleri.VACokluVeriDonemID)),
                                                       IsVeriVar = vaSurecleriMaddeGirilenDeger?.IsVeriVar,
                                                       GirilenDeger = vaSurecleriMaddeGirilenDeger?.GirilenDeger,
                                                       VeriGirisiOnaylandi = vaSurecleriMaddeGirilenDeger?.VeriGirisiOnaylandi,
                                                       VeriGirenAdSoyad = veriGiren?.AdSoyad,
                                                       OnayYapanAdSoyad = veriOnaylayan?.AdSoyad,
                                                   }).OrderBy(o => o.VaCokluVeriDonemId).ToList();

                if (mdl.VeriGirisi.VeriGirisSekliID == VeriGirisSekli.FormulleHesaplanacak)
                {

                    mdl.VeriGirisi = VeriGirisRowSet(mdl.VeriGirisi, mdl.VASurecID, mdl.MaddeID);

                    var surecMaddesi = entities.VASurecleriMaddes.First(p => p.VASurecID == vaSurecId && p.MaddeID == maddeId);
                    mdl.VeriGirisi.DosyaCount = surecMaddesi.VASurecleriMaddeFormulEslesenMaddes.Sum(s =>
                        s.VASurecleriMadde1.VASurecleriMaddeEklenenDosyas.Count);
                    var eklenenAciklamlar = surecMaddesi.VASurecleriMaddeFormulEslesenMaddes.Select(s => s.VASurecleriMadde1)
                        .SelectMany(sm => sm.VASurecleriMaddeEklenenAciklamas).ToList();

                    foreach (var eklenenAciklamaItem in eklenenAciklamlar)
                    {
                        eklenenAciklamaItem.Aciklama = "<b>" + eklenenAciklamaItem.Birimler.BirimAdi + "</b>:<br/>" + eklenenAciklamaItem.Aciklama;
                    }
                    mdl.EklenenAciklamas = eklenenAciklamlar;

                }
                else
                {
                    mdl.EklenenAciklamas = entities.VASurecleriMaddeEklenenAciklamas
                        .Where(p => p.VASurecleriMaddeID == mdl.VeriGirisi.VASurecleriMaddeID &&
                                    p.BirimID == mdl.BirimID)
                        .Include(inc => inc.VACokluVeriDonemleri).ToList();
                }


                mdl.VeriGirisiOnaylandi = mdl.VeriGirisi.VgMaddeVerileris.All(a => a.VeriGirisiOnaylandi == true);
                mdl.FaaliyetData = (from s in entities.VASurecleriMaddes.Where(p => p.VASurecID == vaSurecId && p.IsAktif)
                                    join bt in entities.Maddelers on s.MaddeID equals bt.MaddeID
                                    join fm in entities.FaaliyetlerMaddes on bt.MaddeID equals fm.MaddeID
                                    join f in entities.Faaliyetlers.Where(p => p.IsAktif) on fm.FaaliyetID equals f.FaaliyetID
                                    where bt.MaddeID == maddeId

                                    select new FrFaaliyetler
                                    {
                                        FaaliyetID = f.FaaliyetID,
                                        FaaliyetKod = f.FaaliyetKod,
                                        FaaliyetAdi = f.FaaliyetAdi,
                                        TakipGostergesi = f.TakipGostergesi,
                                        IsAktif = s.IsAktif,
                                        IslemTarihi = s.IslemTarihi,
                                        FaaliyetAylari = f.FaaliyetlerAys.Select(s2 => s2.Aylar).OrderBy(o => o.AyID).ToList(),
                                        FaaliyetKaynaklari = f.FaaliyetlerKaynaks.Select(s2 => s2.Kaynaklar).OrderBy(o => o.KaynakID).ToList(),
                                    }).OrderBy(o => o.FaaliyetAdi).ToList();

                mdl.AciklamaCount = mdl.EklenenAciklamas.Count;// entities.VASurecleriMaddeEklenenAciklamas.Count(p =>
                                                               //p.VASurecleriMadde.VASurecID == vaSurecId && p.BirimID == birimId &&
                                                               // p.VASurecleriMadde.MaddeID == maddeId);
                return mdl;
            }
        }
        public static VeriKanitModel GetVeriKanitModel(int vaSurecId, int birimId, int maddeId, int? vaCokluVeriDonemId)
        {
            using (var db = new VysDBEntities())
            {
                var model = new VeriKanitModel();
                var vaSurecMadde = db.VASurecleriMaddes.First(p => p.VASurecID == vaSurecId && p.MaddeID == maddeId && p.VASurecleriMaddeBirims.Any(a => a.BirimID == birimId));
                var birimMadde = vaSurecMadde.VASurecleriMaddeBirims.First(p => p.BirimID == birimId);
                var birim = db.Birimlers.First(p => p.BirimID == birimId);
                var veriGirisDonemleris = vaSurecMadde.VASurecleriMaddeVeriGirisDonemleris
                    .Where(p => p.VACokluVeriDonemID == (vaCokluVeriDonemId ?? p.VACokluVeriDonemID)).ToList();


                var isFormulMaddesi = !vaSurecMadde.HesaplamaFormulu.IsNullOrWhiteSpace();
                var birimMaddeGirilenDegerlers = isFormulMaddesi
                    ? vaSurecMadde.VASurecleriMaddeFormulEslesenMaddes
                        .SelectMany(s => s.VASurecleriMadde1.VASurecleriMaddeGirilenDegers).ToList()
                    : vaSurecMadde.VASurecleriMaddeGirilenDegers
                        .Where(p => p.BirimID == birimMadde.BirimID).ToList();

                birimMaddeGirilenDegerlers = birimMaddeGirilenDegerlers
                    .Where(p => p.VACokluVeriDonemID == (vaCokluVeriDonemId ?? p.VACokluVeriDonemID)).ToList();
                var girilenDegerlerOnayDurumList = (from vaDonem in veriGirisDonemleris
                                                    join girilenDeger in birimMaddeGirilenDegerlers on vaDonem.VACokluVeriDonemID.ToNullIntZero() equals girilenDeger
                                                        .VACokluVeriDonemID into defGirilenDeger
                                                    from girilenDegerDef in defGirilenDeger.DefaultIfEmpty()
                                                    select new
                                                    {
                                                        vaDonem.VACokluVeriDonemID,
                                                        vaDonem.IsDosyaYuklensin,
                                                        girilenDegerDef?.VeriGirisiOnaylandi
                                                    }).ToList();

                model.VASurecID = vaSurecId;
                model.Yil = vaSurecMadde.VASurecleri.Yil;
                model.BirimID = birimId;
                model.BirimAdi = birim.BirimAdi;
                model.MaddeID = maddeId;
                model.MaddeAdi = "[" + vaSurecMadde.MaddeKod + "] " + vaSurecMadde.Maddeler.MaddeAdi;
                model.VaCokluVeriDonemId = vaCokluVeriDonemId;
                model.SurecAktif = vaSurecMadde.VASurecleri.IsAktif && (vaSurecMadde.VASurecleri.BaslangicTarihi <= DateTime.Now.Date && vaSurecMadde.VASurecleri.BitisTarihi >= DateTime.Now.Date);
                model.VeriKanitDosyaListModel = GetMaddeEklenenDosyalar(vaSurecId, birimId, maddeId, vaCokluVeriDonemId);

                model.VeriGirisiOnaylandi = girilenDegerlerOnayDurumList.All(a => a.VeriGirisiOnaylandi == true);
                var kanitEklenecekVaCokluVeriDonemIDs = girilenDegerlerOnayDurumList.Where(p => p.IsDosyaYuklensin && (vaCokluVeriDonemId.HasValue || p.VeriGirisiOnaylandi != true)).Select(s => s.VACokluVeriDonemID).Distinct().ToList();
                var cmbVaCokluVeriDonemleri = CmbVaCokluVeriDonemleri(false, "Genel").Where(p => !p.Value.HasValue || kanitEklenecekVaCokluVeriDonemIDs.Contains(p.Value ?? 0)).ToList();
                model.SelectListVaCokluVeriDonemId = new SelectList(cmbVaCokluVeriDonemleri, "Value", "Caption");


                return model;
            }
        }
        public static VeriKanitDosyaListModel GetMaddeEklenenDosyalar(int vaSurecId, int birimId, int maddeId, int? vaCokluVeriDonemId)
        {
            using (var db = new VysDBEntities())
            {
                var model = new VeriKanitDosyaListModel
                {
                    VaCokluVeriDonemId = vaCokluVeriDonemId,

                };
                var vaSurecMadde = db.VASurecleriMaddes.First(p => p.VASurecID == vaSurecId && p.MaddeID == maddeId && p.VASurecleriMaddeBirims.Any(a => a.BirimID == birimId));


                if (vaSurecMadde.VASurecleriMaddeFormulEslesenMaddes.Any())
                {
                    var eklenenDosyalar = vaSurecMadde.VASurecleriMaddeFormulEslesenMaddes.Select(s => s.VASurecleriMadde1)
                           .SelectMany(sm => sm.VASurecleriMaddeEklenenDosyas).ToList();
                    model.Data = eklenenDosyalar.Where(p => p.VACokluVeriDonemID == (vaCokluVeriDonemId ?? p.VACokluVeriDonemID)).Select(s => new VeriKanitDosyaListRw
                    {
                        VASurecleriMaddeEklenenDosyaID = s.VASurecleriMaddeEklenenDosyaID,
                        VACokluVeriDonemID = s.VACokluVeriDonemID,
                        BirimAdi = s.Birimler.BirimAdi,
                        DonemAdi = s.VACokluVeriDonemID.HasValue ? s.VACokluVeriDonemleri.CokluVeriDonemAdi : "Genel",
                        DosyaAdi = "<b>" + s.Birimler.BirimAdi + "</b>:<br/>" + s.DosyaAdi,
                        DosyaYolu = s.DosyaYolu,
                        IslemTarihi = s.IslemTarihi,
                    }).OrderBy(o => o.VACokluVeriDonemID).ThenBy(t => t.BirimAdi).ThenByDescending(t => t.IslemTarihi).ToList();
                    model.KayitYetki = false;
                }
                else
                {
                    var birimMadde = vaSurecMadde.VASurecleriMaddeBirims.First(p => p.BirimID == birimId);




                    model.Data = vaSurecMadde.VASurecleriMaddeEklenenDosyas.Where(p => p.VACokluVeriDonemID == (vaCokluVeriDonemId ?? p.VACokluVeriDonemID) && p.VASurecleriMaddeID == birimMadde.VASurecleriMaddeID && p.BirimID == birimMadde.BirimID).Select(s => new VeriKanitDosyaListRw
                    {
                        VASurecleriMaddeEklenenDosyaID = s.VASurecleriMaddeEklenenDosyaID,
                        VACokluVeriDonemID = s.VACokluVeriDonemID,
                        DonemAdi = s.VACokluVeriDonemID.HasValue ? s.VACokluVeriDonemleri.CokluVeriDonemAdi : "Genel",
                        DosyaAdi = s.DosyaAdi,
                        DosyaYolu = s.DosyaYolu,
                        IslemTarihi = s.IslemTarihi,
                    }).OrderBy(o => o.VACokluVeriDonemID).ThenByDescending(t => t.IslemTarihi).ToList();
                    var nowDate = DateTime.Now.Date;
                    var girilecekVeriCnt = vaSurecMadde.VASurecleriMaddeVeriGirisDonemleris.Count;
                    var veriGirisiOnaylandi =
                        girilecekVeriCnt == birimMadde.VASurecleriMadde.VASurecleriMaddeGirilenDegers.Count(c => c.BirimID == birimId && c.VeriGirisiOnaylandi)
                        ||
                        girilecekVeriCnt == vaSurecMadde.VASurecleriMaddeFormulEslesenMaddes.Select(s => s.VASurecleriMadde1.VASurecleriMaddeGirilenDegers.Count(p => p.BirimID == birimId && p.VeriGirisiOnaylandi)).OrderBy(o => o).FirstOrDefault();

                    model.KayitYetki = vaSurecMadde.VASurecleri.IsAktif &&
                                       (vaSurecMadde.VASurecleri.BaslangicTarihi <= nowDate &&
                                        vaSurecMadde.VASurecleri.BitisTarihi >= nowDate) &&
                                       RoleNames.VeriGirisiKayitYetkisi.InRole() && !veriGirisiOnaylandi;

                }




                return model;
            }
        }

        private static FrVgMaddeler VeriGirisRowSet(FrVgMaddeler model, int vaSurecId, int maddeId)
        {

            var formulMaddeHesap = new VeriGirisFormulHesapla(vaSurecId, new List<int> { maddeId });
            var hesaplanan = formulMaddeHesap.HesaplaFormulMaddeleri();
            foreach (var hesaplananFormulMaddesi in hesaplanan)
            {
                foreach (var formulGirilenDeger in hesaplananFormulMaddesi.VaSurecleriMaddeGirilenDegers)
                {
                    if (formulGirilenDeger.VaCokluVeriDonemId == VaCokluVeriDonemEnum.HesaplamaSonucu)
                    {

                        model.HesaplananSonucDegeri = formulGirilenDeger.GirilenDeger;
                    }
                    else
                    {

                        var vgMaddeVerisi = model.VgMaddeVerileris.FirstOrDefault(f => f.VaCokluVeriDonemId == formulGirilenDeger.VaCokluVeriDonemId);
                        if (vgMaddeVerisi != null)
                        {
                            vgMaddeVerisi.IsVeriVar = formulGirilenDeger.GirilenDeger.HasValue;
                            vgMaddeVerisi.GirilenDeger = formulGirilenDeger.GirilenDeger;
                            vgMaddeVerisi.VeriGirisiOnaylandi = formulGirilenDeger.VeriGirisiOnaylandi;
                        }
                        model.VaSurecleriMaddeGirilenDegers.Add(new VASurecleriMaddeGirilenDeger
                        {
                            IsVeriVar = formulGirilenDeger.GirilenDeger.HasValue,
                            GirilenDeger = formulGirilenDeger.GirilenDeger,
                            VeriGirisiOnaylandi = formulGirilenDeger.VeriGirisiOnaylandi,
                            VACokluVeriDonemID = formulGirilenDeger.VaCokluVeriDonemId,

                        });
                    }
                }

                model.PlanlananDeger = hesaplananFormulMaddesi.PlanlananHedef;
                model.PlanlananDegerGelecekYil = hesaplananFormulMaddesi.PlanlananVeriGelecekYil;
                model.MaddeVeriGirisDurumId = model.GirilecekVeriSayisi == model.VaSurecleriMaddeGirilenDegers.Count ? 1 : 0;
                model.GirilenVeriSayisi = hesaplananFormulMaddesi.VaSurecleriMaddeGirilenDegers.Count(c => c.GirilenDeger.HasValue && c.VaCokluVeriDonemId != VaCokluVeriDonemEnum.HesaplamaSonucu);
                model.OnaylananVeriSayisi = hesaplananFormulMaddesi.VaSurecleriMaddeGirilenDegers.Count(c => c.VeriGirisiOnaylandi && c.VaCokluVeriDonemId != VaCokluVeriDonemEnum.HesaplamaSonucu);

                model.VeriGirisiOnaylandi = model.VaSurecleriMaddeGirilenDegers.Any(p => p.GirilenDeger.HasValue);
            }


            //foreach (var item in model.VaSurecleriMaddeVeriGirisDonemleris)
            //{
            //    var girilenVeri = madde.VaSurecleriMaddeGirilenDegers.FirstOrDefault(f =>
            //          f.VaCokluVeriDonemId == item.VACokluVeriDonemID);
            //    model.VaSurecleriMaddeGirilenDegers.Add(new VASurecleriMaddeGirilenDeger
            //    {
            //        BirimID = birimId,
            //        VASurecleriMaddeID = model.VASurecleriMaddeID,
            //        IsVeriVar = girilenVeri?.GirilenDeger != null,
            //        GirilenDeger = girilenVeri.GirilenDeger,
            //        VACokluVeriDonemID = item.VACokluVeriDonemID
            //    });
            //}
            //using (var db = new VysDBEntities())
            //{
            //    var surecBirimMadde = db.VASurecleriMaddeBirims.First(p => p.VASurecleriMadde.MaddeID == maddeId && p.BirimID == birimId && p.VASurecleriMadde.VASurecID == vaSurecId);
            //    var surecMadde = surecBirimMadde.VASurecleriMadde;
            //    var formulMaddeleri = surecBirimMadde.VASurecleriMadde.VASurecleriMaddeFormulEslesenMaddes;
            //    var formul = surecBirimMadde.VASurecleriMadde.HesaplamaFormulu.ToLower();

            //    var herAyIcinOncekiAylarToplansin = formulMaddeleri.Count(p => p.VASurecleriMadde1.MaddeYilSonuDegerHesaplamaTipID == MaddeYilSonuDegerHesaplamaTip.Kümülatif) != formulMaddeleri.Count;

            //    if (formulMaddeleri.Count(a => formul.Contains(a.VASurecleriMadde1.MaddeKod.ToLower())) == formulMaddeleri.Count)
            //    {
            //        var vaCokluVeriDonemIds = surecMadde.VASurecleriMaddeVeriGirisDonemleris.Select(s => s.VACokluVeriDonemID).ToList();


            //        foreach (var vaCokluVeriDonemId in vaCokluVeriDonemIds)
            //        {
            //            var vaSurecleriMaddeGirilenDegers = formulMaddeleri.SelectMany(s => s.VASurecleriMadde1.VASurecleriMaddeGirilenDegers.Where(p => p.IsVeriVar == true)).ToList();
            //            var cokluVeriDonemVerileri = vaSurecleriMaddeGirilenDegers.Where(p => p.VACokluVeriDonemID == vaCokluVeriDonemId && p.IsVeriVar == true).ToList();

            //            var vgOnays = new List<bool>();
            //            foreach (var item in cokluVeriDonemVerileri)
            //            {
            //                var secilenMadde = item.VASurecleriMadde;
            //                var hesaplananSonucDegeri =
            //                      secilenMadde.MaddeYilSonuDegerHesaplamaTipID == MaddeYilSonuDegerHesaplamaTip.SonAy ?
            //                          secilenMadde.VASurecleriMaddeGirilenDegers.Where(p => p.BirimID == item.BirimID && p.VACokluVeriDonemID == item.VACokluVeriDonemID && p.IsVeriVar == true).OrderByDescending(o => o.VACokluVeriDonemID).Select(s2 => s2.GirilenDeger).FirstOrDefault()
            //                          :
            //                          secilenMadde.MaddeYilSonuDegerHesaplamaTipID == MaddeYilSonuDegerHesaplamaTip.Kümülatif ?
            //                              secilenMadde.VASurecleriMaddeGirilenDegers.Where(p => (herAyIcinOncekiAylarToplansin ? p.VACokluVeriDonemID <= item.VACokluVeriDonemID : p.VACokluVeriDonemID == item.VACokluVeriDonemID) && p.IsVeriVar == true && p.BirimID == item.BirimID).Sum(s2 => s2.GirilenDeger)
            //                              :
            //                              secilenMadde.VASurecleriMaddeGirilenDegers.Where(p => p.BirimID == item.BirimID && p.VACokluVeriDonemID == item.VACokluVeriDonemID && p.IsVeriVar == true).Sum(s2 => s2.GirilenDeger) / secilenMadde.VASurecleriMaddeGirilenDegers.Count(p => p.BirimID == item.BirimID && p.VACokluVeriDonemID == item.VACokluVeriDonemID && p.IsVeriVar == true);
            //                vgOnays.Add(item.VeriGirisiOnaylandi);
            //                formul = formul.Replace(secilenMadde.MaddeKod.ToLower(), hesaplananSonucDegeri.ToString());
            //            }
            //            try
            //            {
            //                formul = formul.Replace("@", "");
            //                model.VaSurecleriMaddeGirilenDegers.Add(new VASurecleriMaddeGirilenDeger
            //                {
            //                    BirimID = surecBirimMadde.BirimID,
            //                    VASurecleriMaddeID = surecMadde.VASurecleriMaddeID,
            //                    VASurecleriMadde = surecMadde,
            //                    IsVeriVar = true,
            //                    GirilenDeger = (decimal)formul.EvaluateExpression(),
            //                    VACokluVeriDonemID = vaCokluVeriDonemId,
            //                    VeriGirisiOnaylandi = vgOnays.All(a => a)
            //                });
            //            }
            //            catch (Exception)
            //            {
            //                // ignored
            //            }
            //            formul = surecBirimMadde.VASurecleriMadde.HesaplamaFormulu.ToLower();
            //        }

            //    }

            //    var birimFormulMaddeleri = formulMaddeleri.Where(p => p.VASurecleriMadde.VASurecleriMaddeBirims.Any(a => a.BirimID == birimId)).SelectMany(p => p.VASurecleriMadde1.VASurecleriMaddeBirims).ToList();
            //    if (surecMadde.IsPlanlananDegerOlacak && birimFormulMaddeleri.Count > 0 && birimFormulMaddeleri.Count == birimFormulMaddeleri.Count(c => c.PlanlananDeger.HasValue))
            //    {
            //        formul = surecBirimMadde.VASurecleriMadde.HesaplamaFormulu.ToLower();
            //        birimFormulMaddeleri.ForEach(f =>
            //        {
            //            formul = formul.Replace(f.VASurecleriMadde.MaddeKod.ToLower(), f.PlanlananDeger.Value.ToString());
            //        });
            //        formul = formul.Replace("@", "");
            //        model.PlanlananDeger = (decimal)formul.EvaluateExpression();
            //    }
            //    if (surecMadde.IsPlanlananDegerOlacakGelecekYil && birimFormulMaddeleri.Count > 0 && birimFormulMaddeleri.Count == birimFormulMaddeleri.Count(c => c.PlanlananDegerGelecekYil.HasValue))
            //    {
            //        formul = surecBirimMadde.VASurecleriMadde.HesaplamaFormulu.ToLower();
            //        birimFormulMaddeleri.ForEach(f =>
            //        {
            //            formul = formul.Replace(f.VASurecleriMadde.MaddeKod.ToLower(), f.PlanlananDegerGelecekYil.Value.ToString());
            //        });

            //        formul = formul.Replace("@", "");
            //        model.PlanlananDegerGelecekYil = (decimal)formul.EvaluateExpression();
            //    }
            //    model.MaddeVeriGirisDurumId = model.GirilecekVeriSayisi == model.VaSurecleriMaddeGirilenDegers.Count ? 1 : 0;
            //    model.VeriGirisiOnaylandi = model.GirilecekVeriSayisi == model.VaSurecleriMaddeGirilenDegers.Count(p => p.VeriGirisiOnaylandi);

            //    if (model.VeriGirisiOnaylandi && model.VaSurecleriMaddeGirilenDegers.Any(a => a.IsVeriVar == true))
            //        model.HesaplananSonucDegeri = (
            //            model.MaddeYilSonuDegerHesaplamaTipID == MaddeYilSonuDegerHesaplamaTip.SonAy
            //                ? model.VaSurecleriMaddeGirilenDegers.Where(p => p.IsVeriVar == true)
            //                    .OrderByDescending(o => o.VACokluVeriDonemID).Select(s2 => s2.GirilenDeger)
            //                    .FirstOrDefault()
            //                : (
            //                    model.MaddeYilSonuDegerHesaplamaTipID == MaddeYilSonuDegerHesaplamaTip.Kümülatif
            //                        ? model.VaSurecleriMaddeGirilenDegers.Where(p => p.IsVeriVar == true)
            //                            .Sum(s2 => s2.GirilenDeger)
            //                        : model.VaSurecleriMaddeGirilenDegers.Where(p => p.IsVeriVar == true)
            //                              .Sum(s2 => s2.GirilenDeger) /
            //                          model.VaSurecleriMaddeGirilenDegers.Count(p => p.IsVeriVar == true)
            //                )
            //        );
            //    else model.HesaplananSonucDegeri = 0;

            //}
            return model;
        }

        public static HtmlString VeriGirisOnayDurumHtml(this FrVgMaddeler row)
        {
            var html = ViewRenderHelper.RenderPartialView("VeriGirisi", "ViewVeriGirisOnayDurum", row);
            return new HtmlString(html);
        }


        public static IResult PlanlananDegerKayit(int vaSurecId, int maddeId, int birimId, bool isBuYilOrGelecekYil, decimal? planlananDeger)
        {
            using (var db = new VysDBEntities())
            {
                var birimIDs = UserIdentity.Current.TableRollId[RollTableIdName.BirimId];
                var messageModel = new MmMessage
                {
                    Title = "Veri Giriş İşlemi",
                };
                var birimYetki = birimIDs.Contains(birimId);
                var phKayitYetki = RoleNames.VeriGirisiPlanlananHedefKayitYetkisi.InRole();
                var surec = SurecIslemleriBus.GetVaSurecKontrol(vaSurecId);
                if (!surec.IsAktif || !surec.AktifSurec)
                {
                    messageModel.Messages.Add("İşlem yapmaya çalıştığınız Süreç aktif olmadığından veriler üstünden herhangi bir işlem yapamazsınız.");
                }
                else if (!phKayitYetki)
                {
                    messageModel.Messages.Add("Veri girişi için yetkiniz bulunmamaktadır.");
                }
                else if (!birimYetki)
                {
                    messageModel.Messages.Add("İşlem yapmaya çalıştığınız birim yetkiniz dahilinde değildir.");
                }
                else if (!planlananDeger.HasValue)
                {
                    var name = (surec.Yil + (isBuYilOrGelecekYil ? 0 : 1)) + " Yılı Planlanan Hedef";
                    messageModel.Messages.Add(name + " bilgisi boş bırakılamaz.");
                }
                else
                {
                    var birimMadde = db.VASurecleriMaddeBirims.First(p => p.VASurecleriMadde.VASurecID == vaSurecId && p.BirimID == birimId && p.VASurecleriMadde.MaddeID == maddeId);

                    if (isBuYilOrGelecekYil)
                        birimMadde.PlanlananDeger = birimMadde.VASurecleriMadde.IsPlanlananDegerOlacak ? planlananDeger : null;
                    else
                        birimMadde.PlanlananDegerGelecekYil = birimMadde.VASurecleriMadde.IsPlanlananDegerOlacakGelecekYil ? planlananDeger : null;

                    birimMadde.IslemTarihi = DateTime.Now;
                    birimMadde.IslemYapanID = UserIdentity.Current.Id;
                    birimMadde.IslemYapanIP = UserIdentity.Ip;
                    db.SaveChanges();
                    messageModel.IsSuccess = true;

                }
                messageModel.MessageType = messageModel.IsSuccess ? Msgtype.Success : Msgtype.Warning;
                return new Result(true, messageModel);
            }
        }
        public static IResult VeriDurumKayit(VgModel kModel)
        {
            using (var db = new VysDBEntities())
            {
                var birimIDs = UserIdentity.Current.TableRollId[RollTableIdName.BirimId];
                var mMessage = new MmMessage
                {
                    Title = "Veri Giriş İşlemi",
                    MessageType = Msgtype.Warning
                };

                var kayitYetki = RoleNames.VeriGirisiKayitYetkisi.InRole();
                var birimYetki = birimIDs.Contains(kModel.BirimId);
                var surec = SurecIslemleriBus.GetVaSurecKontrol(kModel.VaSurecId);

                if (!surec.IsAktif || !surec.AktifSurec)
                {
                    mMessage.Messages.Add("İşlem yapmaya çalıştığınız Süreç aktif olmadığından veriler üstünden herhangi bir işlem yapamazsınız.");
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

                    var birimMadde = db.VASurecleriMaddeBirims.First(p => p.VASurecleriMadde.VASurecID == kModel.VaSurecId && p.BirimID == kModel.BirimId && p.VASurecleriMadde.MaddeID == kModel.MaddeId);
                    var vaSurecMadde = birimMadde.VASurecleriMadde;
                    var girilenDeger = vaSurecMadde.VASurecleriMaddeGirilenDegers.FirstOrDefault(p => p.BirimID == kModel.BirimId && p.VACokluVeriDonemID == kModel.VaCokluVeriDonemId);



                    if (vaSurecMadde.VeriGirisSekliID != VeriGirisSekli.ElleGirilecek)
                    {
                        mMessage.Messages.Add("İşlem yapmaya çalıştığınız madde için veri girişi yapılamaz. Bu madde verisi özel hesaplama formülü ile oluşturulur.");
                    }
                    else if (girilenDeger != null && girilenDeger.VeriGirisiOnaylandi)
                    {
                        mMessage.Messages.Add("İşlem yapmaya çalıştığınız madde verisi onaylandığından kayıt işlemi yapılamaz.");
                    }
                    if (mMessage.Messages.Count == 0)
                    {

                        if (girilenDeger == null)
                        {
                            vaSurecMadde.VASurecleriMaddeGirilenDegers.Add(new VASurecleriMaddeGirilenDeger
                            {
                                BirimID = birimMadde.BirimID,
                                VACokluVeriDonemID = kModel.VaCokluVeriDonemId,
                                IsVeriVar = kModel.IsVeriVar,
                                IslemTarihi = DateTime.Now,
                                IslemYapanID = UserIdentity.Current.Id,
                                IslemYapanIP = UserIdentity.Ip
                            });
                        }
                        else
                        {
                            girilenDeger.VACokluVeriDonemID = kModel.VaCokluVeriDonemId;
                            girilenDeger.IsVeriVar = kModel.IsVeriVar;
                            girilenDeger.GirilenDeger = kModel.IsVeriVar == true ? kModel.GirilenDeger : null;
                            girilenDeger.IslemTarihi = DateTime.Now;
                            girilenDeger.IslemYapanID = UserIdentity.Current.Id;
                            girilenDeger.IslemYapanIP = UserIdentity.Ip;

                        }


                        db.SaveChanges();
                        mMessage.IsSuccess = true;

                    }
                }
                mMessage.MessageType = mMessage.IsSuccess ? Msgtype.Success : Msgtype.Warning;
                return new Result(true, mMessage);
            }
        }

        public static IDataResult<bool> VeriGirisiKayit(VgModel kModel)
        {
            using (var db = new VysDBEntities())
            {
                var birimIDs = UserIdentity.Current.TableRollId[RollTableIdName.BirimId];
                var mMessage = new MmMessage
                {
                    Title = "Veri Giriş İşlemi",
                    MessageType = Msgtype.Warning
                };

                var kayitYetki = RoleNames.VeriGirisi.InRole();
                var birimYetki = birimIDs.Contains(kModel.BirimId);
                var surec = SurecIslemleriBus.GetVaSurecKontrol(kModel.VaSurecId);
                var showKanitDosyasi = false;

                if (!surec.IsAktif || !surec.AktifSurec)
                {
                    mMessage.Messages.Add("İşlem yapmaya çalıştığınız Süreç aktif olmadığından veriler üstünden herhangi bir işlem yapamazsınız.");
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

                    var vaSurecMadde = db.VASurecleriMaddes.First(p => p.VASurecID == kModel.VaSurecId && p.MaddeID == kModel.MaddeId && p.VASurecleriMaddeBirims.Any(a => a.BirimID == kModel.BirimId));
                    var vaSurecleriBirimMadde = vaSurecMadde.VASurecleriMaddeBirims.First(p => p.BirimID == kModel.BirimId);
                    var surecMaddeGirilenDeger = vaSurecMadde.VASurecleriMaddeGirilenDegers.FirstOrDefault(p => p.BirimID == kModel.BirimId && p.VASurecleriMadde.MaddeID == kModel.MaddeId && p.VACokluVeriDonemID == kModel.VaCokluVeriDonemId);
                    if (vaSurecMadde.VeriGirisSekliID != VeriGirisSekli.ElleGirilecek)
                    {
                        mMessage.Messages.Add("İşlem yapmaya çalıştığınız madde için veri girişi yapılamaz. Bu madde için özel hesaplama formülü ile veriler oluşturulur.");
                    }
                    else if (surecMaddeGirilenDeger != null && surecMaddeGirilenDeger.VeriGirisiOnaylandi)
                    {
                        mMessage.Messages.Add("İşlem yapmaya çalıştığınız madde verisi onaylandığından kayıt işlemi yapılamaz.");
                    }
                    else
                    {
                        var vaSurecleriDonem =
                            vaSurecMadde.VASurecleriMaddeVeriGirisDonemleris.First(f =>
                                f.VACokluVeriDonemID == kModel.VaCokluVeriDonemId);
                        if (vaSurecleriDonem.IsDosyaYuklensin && !vaSurecMadde.VASurecleriMaddeEklenenDosyas.Any(a => a.VACokluVeriDonemID == kModel.VaCokluVeriDonemId && a.BirimID == vaSurecleriBirimMadde.BirimID))
                        {
                            showKanitDosyasi = true;
                            mMessage.Messages.Add("Veri girişi yapılabilmesi için " + vaSurecleriDonem.VACokluVeriDonemleri.CokluVeriDonemAdi + " veri alım dönemine en az bir adet veri Kanıt Dosyası eklenmesi gerekmektedir!");
                        }
                        else if (surecMaddeGirilenDeger.IsVeriVar == true && !kModel.GirilenDeger.HasValue)
                        {
                            mMessage.Messages.Add("'Veri Var' seçeneği seçilen  " + vaSurecleriDonem.VACokluVeriDonemleri.CokluVeriDonemAdi + " verisi için veri girişi boş bırakılamaz.");

                        }

                    }

                    if (!mMessage.Messages.Any())
                    {
                        var vaSurecMaddeTur = db.VASurecleriMaddeTurs.First(p => p.VASurecID == kModel.VaSurecId && p.MaddeTurID == vaSurecMadde.MaddeTurID);
                        if (!vaSurecMaddeTur.IsVeriGirisiAcik)
                        {
                            mMessage.Messages.Add($"{vaSurecMadde.MaddeTurleri.MaddeTurAdi} madde türüne ait maddeler için veri giriş işlemleri kapalıdır. Detaylı bilgi için sistem yöneticisi ile iletişime geçiniz.");

                        }

                    }

                    if (mMessage.Messages.Count == 0)
                    {

                        if (surecMaddeGirilenDeger == null)
                        {
                            vaSurecMadde.VASurecleriMaddeGirilenDegers.Add(new VASurecleriMaddeGirilenDeger
                            {
                                BirimID = vaSurecleriBirimMadde.BirimID,
                                VACokluVeriDonemID = kModel.VaCokluVeriDonemId,
                                IsVeriVar = kModel.IsVeriVar,
                                GirilenDeger = kModel.GirilenDeger,
                                IslemTarihi = DateTime.Now,
                                IslemYapanID = UserIdentity.Current.Id,
                                IslemYapanIP = UserIdentity.Ip
                            });
                        }
                        else
                        {
                            surecMaddeGirilenDeger.VACokluVeriDonemID = kModel.VaCokluVeriDonemId;
                            surecMaddeGirilenDeger.GirilenDeger = surecMaddeGirilenDeger.IsVeriVar == true ? kModel.GirilenDeger : null;
                            surecMaddeGirilenDeger.IslemTarihi = DateTime.Now;
                            surecMaddeGirilenDeger.IslemYapanID = UserIdentity.Current.Id;
                            surecMaddeGirilenDeger.IslemYapanIP = UserIdentity.Ip;

                        }



                        db.SaveChanges();
                        var secilenVeriDonemi = db.VACokluVeriDonemleris.FirstOrDefault(p => p.VACokluVeriDonemID == kModel.VaCokluVeriDonemId);

                        mMessage.IsSuccess = true;
                        mMessage.MessageType = Msgtype.Success;
                        mMessage.Messages.Add(secilenVeriDonemi.CokluVeriDonemAdi + " veri alım dönemi için veri girişi yapıldı.");
                        return new DataResult<bool>(showKanitDosyasi, true, mMessage);
                    }
                }

                mMessage.MessageType = mMessage.IsSuccess ? Msgtype.Success : Msgtype.Warning;
                return new DataResult<bool>(showKanitDosyasi, false, mMessage);
            }
        }

        public static IResult VeriOnayiKayit(int vaSurecId, int birimId, int maddeId, int? vaCokluVeriDonemId, bool veriGirisiOnaylandi)
        {
            using (var db = new VysDBEntities())
            {
                var mMessage = new MmMessage
                {
                    Title = "Veri Giriş Onay İşlemi Aşağıdaki Sebeplerden Dolayı Yapılamadı",
                    MessageType = Msgtype.Warning
                };

                var birimIDs = UserIdentity.Current.TableRollId[RollTableIdName.BirimId];
                var birimYetki = birimIDs.Contains(birimId);
                var surec = SurecIslemleriBus.GetVaSurecKontrol(vaSurecId);
                var onayYetki = RoleNames.VeriGirisiOnayYetkisi.InRole();
                var birimMadde = db.VASurecleriMaddeBirims.First(p => p.VASurecleriMadde.VASurecID == vaSurecId && p.BirimID == birimId && p.VASurecleriMadde.MaddeID == maddeId);
                var maddeVeriBilgi = birimMadde.VASurecleriMadde.VASurecleriMaddeGirilenDegers.FirstOrDefault(p => p.BirimID == birimId && p.VACokluVeriDonemID == vaCokluVeriDonemId);
                var secilenVeriDonemi = db.VACokluVeriDonemleris.FirstOrDefault(p => p.VACokluVeriDonemID == vaCokluVeriDonemId);
                if (!surec.IsAktif || !surec.AktifSurec)
                {
                    mMessage.Messages.Add("İşlem yapmaya çalıştığınız Süreç aktif olmadığından onaylama işlemi yapamazsınız.");
                }
                else if (!onayYetki)
                {
                    mMessage.Messages.Add("Veri girişi onay yetkiniz bulunmamaktadır. Onay İşlemini Yapamazsınız.");
                }
                else if (!birimYetki)
                {
                    mMessage.Messages.Add("İşlem yapmaya çalıştığınız birim yetkiniz dahilinde değildir.");
                }
                else if (!maddeVeriBilgi.IsVeriVar.HasValue)
                {
                    mMessage.Messages.Add(secilenVeriDonemi.CokluVeriDonemAdi + " veri alım dönemi için veri durumu seçilmeli. (veri var veya veri yok seçeneklerinden biri seçilmeli)");
                }
                else if (veriGirisiOnaylandi && maddeVeriBilgi.IsVeriVar == true && !maddeVeriBilgi.GirilenDeger.HasValue)
                {
                    mMessage.Messages.Add(secilenVeriDonemi.CokluVeriDonemAdi + " veri alım dönemi için veri girişi yapılmalı.");
                }
                else if (veriGirisiOnaylandi)

                {
                    var madde = birimMadde.VASurecleriMadde;
                    var maddeAyCount = madde.VASurecleriMaddeVeriGirisDonemleris.Count;
                    var girilenOnayCount = birimMadde.VASurecleriMadde.VASurecleriMaddeGirilenDegers.Count(p =>p.BirimID==birimId && p.VASurecleriMadde.MaddeID == maddeId && p.VeriGirisiOnaylandi) + (maddeAyCount == 0 ? 0 : 1);

                    if (girilenOnayCount == maddeAyCount)
                    {
                        // IsSonOnayYapildi = GirilenOnayCount == MaddeAyCount;
                        //if (madde.IsPlanlananDegerOlacak)
                        //    if (!birimMadde.PlanlananDeger.HasValue) { mMessage.Messages.Add("Maddeye ait " + (surec.Yil) + " yılı planlanan değer bilgisi girilmedi."); }

                        if (madde.IsPlanlananDegerOlacakGelecekYil)
                            if (!birimMadde.PlanlananDegerGelecekYil.HasValue) { mMessage.Messages.Add("Maddeye ait " + (surec.Yil + 1) + " yılı planlanan değer bilgisi girilmedi."); }
                    }
                }

                if (!mMessage.Messages.Any())
                {

                    var vaSurecMaddeTur = db.VASurecleriMaddeTurs.First(p => p.VASurecID == vaSurecId && p.MaddeTurID == birimMadde.VASurecleriMadde.MaddeTurID);
                    if (!vaSurecMaddeTur.IsVeriGirisiAcik)
                    {
                        mMessage.Messages.Add($"{vaSurecMaddeTur.MaddeTurleri.MaddeTurAdi} madde türüne ait maddeler için veri giriş işlemleri kapalıdır. Detaylı bilgi için sistem yöneticisi ile iletişime geçiniz.");

                    }

                }
                if (mMessage.Messages.Count == 0)
                {

                    //BirimMadde.VeriGirisiOnaylandi = IsSonOnayYapildi;
                    //BirimMadde.OnayIslemTarihi = DateTime.Now;
                    //BirimMadde.OnayIslemYapanID = UserIdentity.Current.Id;
                    //BirimMadde.OnayIslemYapanIP = UserIdentity.Ip;
                    if (maddeVeriBilgi != null)
                    {
                        maddeVeriBilgi.VeriGirisiOnaylandi = veriGirisiOnaylandi;
                        maddeVeriBilgi.OnayIslemTarihi = DateTime.Now;
                        maddeVeriBilgi.OnayIslemYapanID = UserIdentity.Current.Id;
                        maddeVeriBilgi.OnayIslemYapanIP = UserIdentity.Ip;
                    }


                    db.SaveChanges();

                    mMessage.IsSuccess = true;
                    mMessage.MessageType = veriGirisiOnaylandi ? Msgtype.Success : Msgtype.Warning;

                    mMessage.Title = veriGirisiOnaylandi ? "Veri Giriş Onaylandı" : "Veri Girişi Onaylamadı";
                    if (vaCokluVeriDonemId.HasValue)
                    {
                        mMessage.Messages.Add((surec.Yil) + " yılı için " + secilenVeriDonemi.CokluVeriDonemAdi + " verisi " + (veriGirisiOnaylandi ? "Onaylandı" : "onayı kaldırıldı"));
                    }
                    else mMessage.Messages.Add((surec.Yil) + " yılı verisi " + (veriGirisiOnaylandi ? "Onaylandı" : "onayı kaldırıldı"));


                }
                else mMessage.MessageType = Msgtype.Warning;

                return new Result(true, mMessage);
            }
        }

        public static IResult VeriKanitDosyasiKayit(int vaSurecId, int birimId, int maddeId, int? vaCokluVeriDonemId, HttpPostedFileBase kanitDosyasi)
        {
            using (var db = new VysDBEntities())
            {
                var vaSurecMadde = db.VASurecleriMaddes.First(p => p.VASurecID == vaSurecId && p.MaddeID == maddeId && p.VASurecleriMaddeBirims.Any(a => a.BirimID == birimId));
                var birimMadde = vaSurecMadde.VASurecleriMaddeBirims.First(p => p.BirimID == birimId);
                var birimIDs = UserIdentity.Current.TableRollId[RollTableIdName.BirimId];

                var mMessage = new MmMessage
                {
                    Title = "Veri Kanıt Dosyası Yükleme İşlemi",
                    MessageType = Msgtype.Warning
                };

                var kayitYetki = RoleNames.VeriGirisi.InRole();
                var birimYetki = birimIDs.Contains(birimId);
                var surec = SurecIslemleriBus.GetVaSurecKontrol(vaSurecId);
                var girilecekVeriCnt = vaSurecMadde.VASurecleriMaddeVeriGirisDonemleris.Count;

                if (!surec.IsAktif || !surec.AktifSurec)
                {
                    mMessage.Messages.Add("İşlem yapmaya çalıştığınız Süreç aktif olmadığından herhangi bir işlem yapamazsınız");
                }
                else if (!kayitYetki)
                {
                    mMessage.Messages.Add("Veri girişi için yetkiniz bulunmamaktadır.");
                }
                else if (!birimYetki)
                {
                    mMessage.Messages.Add("İşlem yapmaya çalıştığınız birim yetkiniz dahilinde değildir.");
                }
                else if (
                          (girilecekVeriCnt == birimMadde.VASurecleriMadde.VASurecleriMaddeGirilenDegers.Count(c => c.BirimID == birimId && c.VeriGirisiOnaylandi)
                            ||
                          (girilecekVeriCnt == vaSurecMadde.VASurecleriMaddeFormulEslesenMaddes.Select(s => s.VASurecleriMadde1.VASurecleriMaddeGirilenDegers.Count(p => p.BirimID == birimId && p.VeriGirisiOnaylandi)).OrderBy(o => o).FirstOrDefault()))
                        )
                {
                    mMessage.Messages.Add("Veri girişi onaylandığından Veri kanıt dosyası yüklenemez.");
                }
                else if (kanitDosyasi == null)
                {
                    mMessage.Messages.Add("Veri Kanıt Dosyası seçilmeden dosya kaydetme işlemi yapılamaz.");
                }
                else
                {
                    var extension = Path.GetExtension(kanitDosyasi.FileName).Replace(".", "");
                    var gecerliUzantilar = new List<string> { "xls", "xlsx", "doc", "docx", "pdf", "jpg", "jpeg", "png", "bmp" };
                    if (!gecerliUzantilar.Contains(extension))
                        mMessage.Messages.Add(kanitDosyasi.FileName + " isimli doyasının (excel, word, pdf, jpeg, png, bmp) formatlarından herhangi birini sağlaması gerekmektedir. Aksi halde dosya yükleme işlemi yapılamaz.");
                    else if (kanitDosyasi.ContentLength > (10 * 1024 * 1024))
                    {
                        mMessage.Messages.Add(kanitDosyasi.FileName + " isimli doyasının boyutu en fazla 10 MB büyüklüğünde olmalıdır.");
                    }
                }

                if (!mMessage.Messages.Any())
                {

                    var vaSurecMaddeTur = db.VASurecleriMaddeTurs.First(p => p.VASurecID == vaSurecId && p.MaddeTurID == birimMadde.VASurecleriMadde.MaddeTurID);
                    if (!vaSurecMaddeTur.IsVeriGirisiAcik)
                    {
                        mMessage.Messages.Add($"{vaSurecMaddeTur.MaddeTurleri.MaddeTurAdi} madde türüne ait maddeler için veri giriş işlemleri kapalıdır. Detaylı bilgi için sistem yöneticisi ile iletişime geçiniz.");

                    }
                }
                if (mMessage.Messages.Count == 0)
                {
                    try
                    {
                        var fileName = kanitDosyasi.FileName.Replace("'", "");
                        var birim = db.Birimlers.First(p => p.BirimID == birimId);
                        var gd = Guid.NewGuid().ToString().Substring(0, 4);
                        var path = "/KanitDosyalari/" + vaSurecMadde.VASurecleri.Yil + "-" + birim.BirimKod + "-" + birim.BirimAdi.Trim() + "-" + vaSurecMadde.MaddeKod + "-" + gd + "-" + fileName;
                        var fPath = HttpContext.Current.Server.MapPath("~" + path);
                        kanitDosyasi.SaveAs(fPath);
                        vaSurecMadde.VASurecleriMaddeEklenenDosyas.Add(new VASurecleriMaddeEklenenDosya
                        {
                            BirimID = birimMadde.BirimID,
                            VASurecleriMaddeID = birimMadde.VASurecleriMaddeID,
                            VACokluVeriDonemID = vaCokluVeriDonemId,
                            DosyaAdi = fileName,
                            DosyaYolu = path,
                            IslemTarihi = DateTime.Now,
                            IslemYapanID = UserIdentity.Current.Id,
                            IslemYapanIP = UserIdentity.Ip,

                        });
                        db.SaveChanges();

                        mMessage.Messages.Add("Kanıt dosyası ekleme işlemi başarılı.");
                        mMessage.IsSuccess = true;
                        mMessage.MessageType = Msgtype.Success;

                        return new Result(true, mMessage);

                    }
                    catch (Exception ex)
                    {
                        var msg = "Kanıt dosyası eklenirken bir hata oluştu! Hata:" + ex.ToExceptionMessage();
                        mMessage.Messages.Add(msg);
                        SistemBilgilendirmeBus.SistemBilgisiKaydet(msg, ObjectExtensions.GetCurrentMethodPath(), BilgiTipi.Hata);
                        mMessage.IsSuccess = false;
                        mMessage.MessageType = Msgtype.Error;
                    }
                }

                return new Result(false, mMessage);
            }
        }

        public static IResult VeriKanitDosyasiSil(int id)
        {
            using (var db = new VysDBEntities())
            {
                var mMessage = new MmMessage
                {
                    Title = "Veri Kanıt Dosyası Silme İşlemi",
                    MessageType = Msgtype.Warning
                };

                var kayit = db.VASurecleriMaddeEklenenDosyas.FirstOrDefault(p => p.VASurecleriMaddeEklenenDosyaID == id);
                if (kayit != null)
                {
                    var maddeId = kayit.VASurecleriMadde.MaddeID;
                    var birimId = kayit.BirimID;
                    var vaSurecId = kayit.VASurecleriMadde.VASurecID;
                    var vaSurecMadde = db.VASurecleriMaddes.First(p => p.VASurecID == vaSurecId && p.MaddeID == maddeId && p.VASurecleriMaddeBirims.Any(a => a.BirimID == birimId));
                    var birimMadde = vaSurecMadde.VASurecleriMaddeBirims.First(p => p.BirimID == birimId);

                    var birimIDs = UserIdentity.Current.TableRollId[RollTableIdName.BirimId];


                    var kayitYetki = RoleNames.VeriGirisi.InRole();
                    var birimYetki = birimIDs.Contains(birimId);
                    var surec = SurecIslemleriBus.GetVaSurecKontrol(vaSurecId);
                    var girilecekVeriCnt = vaSurecMadde.VASurecleriMaddeVeriGirisDonemleris.Count;

                    if (!surec.IsAktif || !surec.AktifSurec)
                    {
                        mMessage.Messages.Add("İşlem yapmaya çalıştığınız Süreç aktif olmadığından herhangi bir işlem yapamazsınız");
                    }
                    else if (!kayitYetki)
                    {
                        mMessage.Messages.Add("Veri girişi için yetkiniz bulunmamaktadır.");
                    }
                    else if (!birimYetki)
                    {
                        mMessage.Messages.Add("İşlem yapmaya çalıştığınız birim yetkiniz dahilinde değildir.");
                    }
                    else if (
                                (girilecekVeriCnt == birimMadde.VASurecleriMadde.VASurecleriMaddeGirilenDegers.Count(c => c.BirimID == birimId && c.VeriGirisiOnaylandi)
                                  ||
                                (girilecekVeriCnt == vaSurecMadde.VASurecleriMaddeFormulEslesenMaddes.Select(s => s.VASurecleriMadde1.VASurecleriMaddeGirilenDegers.Count(p => p.BirimID == birimId && p.VeriGirisiOnaylandi)).OrderBy(o => o).FirstOrDefault()))
                            )
                    {
                        mMessage.Messages.Add("Veri girişi onaylandığından Veri kanıt dosyası yüklenemez.");
                    }
                    else if (db.VASurecleriMaddeTurs.Any(p => p.VASurecID == vaSurecId && p.MaddeTurID == birimMadde.VASurecleriMadde.MaddeTurID && !p.IsVeriGirisiAcik))
                    {
                        mMessage.Messages.Add($"{vaSurecMadde.MaddeTurleri.MaddeTurAdi} madde türüne ait maddeler için veri giriş işlemleri kapalıdır. Detaylı bilgi için sistem yöneticisi ile iletişime geçiniz.");

                    }
                    else
                    {
                        try
                        {
                            var fPath = HttpContext.Current.Server.MapPath("~" + kayit.DosyaYolu);
                            db.VASurecleriMaddeEklenenDosyas.Remove(kayit);
                            db.SaveChanges();
                            mMessage.Messages.Add("'" + kayit.DosyaAdi + "' isimli veri kanıt dosyası silindi!");
                            try
                            {
                                File.Delete(fPath);
                            }
                            catch (Exception ex)
                            {
                                SistemBilgilendirmeBus.SistemBilgisiKaydet("'" + fPath + "' dosyası silinirken bir hata oluştu! Hata:" + ex.ToExceptionMessage(), ObjectExtensions.GetCurrentMethodPath(), BilgiTipi.OnemsizHata);
                            }

                            return new Result(true, mMessage);

                        }
                        catch (Exception ex)
                        {
                            mMessage.Messages.Add("'" + kayit.DosyaAdi + "' isimli veri kanıt dosyası silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage());
                            SistemBilgilendirmeBus.SistemBilgisiKaydet(mMessage.Messages[0], ObjectExtensions.GetCurrentMethodPath(), BilgiTipi.OnemsizHata);
                        }

                    }

                }
                else
                {
                    mMessage.Messages.Add("Silinmek istenen dosya sistemde bulunamadı!");

                }

                return new Result(false, mMessage);
            }
        }

        public static AciklamaGirisModel GetVeriAciklamaDialogData(int? vaSurecleriMaddeEklenenAciklamaId, int vaSurecId, int birimId, int maddeId)
        {
            using (var db = new VysDBEntities())
            {
                var model = new AciklamaGirisModel();
                var vaSurecMadde = db.VASurecleriMaddes.First(p => p.VASurecID == vaSurecId && p.MaddeID == maddeId && p.VASurecleriMaddeBirims.Any(a => a.BirimID == birimId));
                var birimMadde = vaSurecMadde.VASurecleriMaddeBirims.First(p => p.BirimID == birimId);
                var birim = db.Birimlers.First(p => p.BirimID == birimId);

                var nowDate = DateTime.Now.Date;
                model.VASurecID = vaSurecId;
                model.Yil = vaSurecMadde.VASurecleri.Yil;
                model.BirimID = birimId;
                model.BirimAdi = birim.BirimAdi;
                model.MaddeID = maddeId;
                model.MaddeAdi = "[" + vaSurecMadde.MaddeKod + "] " + vaSurecMadde.Maddeler.MaddeAdi;
                model.SurecAktif = vaSurecMadde.VASurecleri.IsAktif && (vaSurecMadde.VASurecleri.BaslangicTarihi <= nowDate && vaSurecMadde.VASurecleri.BitisTarihi >= nowDate);

                if (vaSurecleriMaddeEklenenAciklamaId.HasValue)
                {
                    var kayit = vaSurecMadde.VASurecleriMaddeEklenenAciklamas.First(p => p.VASurecleriMaddeEklenenAciklamaID == vaSurecleriMaddeEklenenAciklamaId);

                    model.VASurecleriMaddeEklenenAciklamaID = vaSurecleriMaddeEklenenAciklamaId.Value;
                    model.Aciklama = kayit.Aciklama;
                }

                var vgAylar = vaSurecMadde.VASurecleriMaddeVeriGirisDonemleris.Select(s => s.VACokluVeriDonemID).Distinct();
                var cmbVaCokluVeriDonemleri = CmbVaCokluVeriDonemleri(true, "Genel");
                cmbVaCokluVeriDonemleri = cmbVaCokluVeriDonemleri.Where(p => !p.Value.HasValue || vgAylar.Contains(p.Value ?? 0)).ToList();

                var girilecekVeriCnt = vaSurecMadde.VASurecleriMaddeVeriGirisDonemleris.Count;

                model.VeriGirisiOnaylandi = (
                                                (girilecekVeriCnt == birimMadde.VASurecleriMadde.VASurecleriMaddeGirilenDegers.Count(c => c.BirimID == birimId && c.VeriGirisiOnaylandi)
                                                  ||
                                                (girilecekVeriCnt == vaSurecMadde.VASurecleriMaddeFormulEslesenMaddes.Select(s => s.VASurecleriMadde1.VASurecleriMaddeGirilenDegers.Count(p => p.BirimID == birimId && p.VeriGirisiOnaylandi)).OrderBy(o => o).FirstOrDefault()))
                                            );
                model.SListAylar = new SelectList(cmbVaCokluVeriDonemleri, "Value", "Caption", model.VACokluVeriDonemID);
                model.AciklamaData = new AciklamaListModel
                {
                    Data = db.VASurecleriMaddeEklenenAciklamas
                        .Where(p => p.VASurecleriMadde.VASurecID == vaSurecId && p.BirimID == birimId &&
                                    p.VASurecleriMadde.MaddeID == maddeId).OrderBy(o => o.VACokluVeriDonemID)
                        .ThenByDescending(t => t.IslemTarihi).ToList()
                };
                return model;
            }
        }
        public static AciklamaListModel GetVeriAciklamaModelData(int vaSurecId, int birimId, int maddeId)
        {
            using (var db = new VysDBEntities())
            {
                var vaSurecMadde = db.VASurecleriMaddes.First(p => p.VASurecID == vaSurecId && p.MaddeID == maddeId && p.VASurecleriMaddeBirims.Any(a => a.BirimID == birimId));
                var birimMadde = vaSurecMadde.VASurecleriMaddeBirims.First(p => p.BirimID == birimId);

                var model = new AciklamaListModel()
                {
                    Data = db.VASurecleriMaddeEklenenAciklamas
                        .Where(p => p.VASurecleriMadde.VASurecID == vaSurecId && p.BirimID == birimId &&
                                    p.VASurecleriMadde.MaddeID == maddeId).OrderBy(o => o.VACokluVeriDonemID)
                        .ThenByDescending(t => t.IslemTarihi).ToList()
                };
                var nowDate = DateTime.Now.Date;
                var girilecekVeriCnt = vaSurecMadde.VASurecleriMaddeVeriGirisDonemleris.Count;
                var veriGirisiOnaylandi = (
                                              (girilecekVeriCnt == birimMadde.VASurecleriMadde.VASurecleriMaddeGirilenDegers.Count(c => c.BirimID == birimId && c.VeriGirisiOnaylandi)
                                                ||
                                              (girilecekVeriCnt == vaSurecMadde.VASurecleriMaddeFormulEslesenMaddes.Select(s => s.VASurecleriMadde1.VASurecleriMaddeGirilenDegers.Count(p => p.BirimID == birimId && p.VeriGirisiOnaylandi)).OrderBy(o => o).FirstOrDefault()))
                                           );
                model.KayitYetki = vaSurecMadde.VASurecleri.IsAktif && (vaSurecMadde.VASurecleri.BaslangicTarihi <= nowDate && vaSurecMadde.VASurecleri.BitisTarihi >= nowDate) && RoleNames.VeriGirisiKayitYetkisi.InRole() && !veriGirisiOnaylandi;
                return model;
            }
        }
        public static IResult VeriAciklamasiKayit(int? vaSurecleriMaddeEklenenAciklamaId, int vaSurecId, int birimId, int maddeId, int? vaCokluVeriDonemId, string aciklama)
        {
            using (var db = new VysDBEntities())
            {
                var vaSurecMadde = db.VASurecleriMaddes.First(p => p.VASurecID == vaSurecId && p.MaddeID == maddeId && p.VASurecleriMaddeBirims.Any(a => a.BirimID == birimId));
                var birimMadde = vaSurecMadde.VASurecleriMaddeBirims.First(p => p.BirimID == birimId);
                var birimIDs = UserIdentity.Current.TableRollId[RollTableIdName.BirimId];
                var mMessage = new MmMessage
                {
                    Title = "Veri Girişi Açıklaması Ekleme İşlemi",
                    MessageType = Msgtype.Warning
                };

                var kayitYetki = RoleNames.VeriGirisi.InRole();
                var birimYetki = birimIDs.Contains(birimId);
                var surec = SurecIslemleriBus.GetVaSurecKontrol(vaSurecId);
                var girilecekVeriCnt = vaSurecMadde.VASurecleriMaddeVeriGirisDonemleris.Count;

                if (!surec.IsAktif || !surec.AktifSurec)
                {
                    mMessage.Messages.Add("İşlem yapmaya çalıştığınız Süreç aktif olmadığından herhangi bir işlem yapamazsınız");
                }
                else if (!kayitYetki)
                {
                    mMessage.Messages.Add("Veri girişi için yetkiniz bulunmamaktadır.");
                }
                else if (!birimYetki)
                {
                    mMessage.Messages.Add("İşlem yapmaya çalıştığınız birim yetkiniz dahilinde değildir.");
                }
                else if (db.VASurecleriMaddeTurs.Any(p => p.VASurecID == vaSurecId && p.MaddeTurID == birimMadde.VASurecleriMadde.MaddeTurID && !p.IsVeriGirisiAcik))
                {
                    mMessage.Messages.Add($"{vaSurecMadde.MaddeTurleri.MaddeTurAdi} madde türüne ait maddeler için veri giriş işlemleri kapalıdır. Detaylı bilgi için sistem yöneticisi ile iletişime geçiniz.");

                }
                else if (
                          (girilecekVeriCnt == birimMadde.VASurecleriMadde.VASurecleriMaddeGirilenDegers.Count(c => c.BirimID == birimId && c.VeriGirisiOnaylandi)
                            ||
                          (girilecekVeriCnt == vaSurecMadde.VASurecleriMaddeFormulEslesenMaddes.Select(s => s.VASurecleriMadde1.VASurecleriMaddeGirilenDegers.Count(p => p.BirimID == birimId && p.VeriGirisiOnaylandi)).OrderBy(o => o).FirstOrDefault()))
                       )
                {
                    mMessage.Messages.Add("Veri girişi onaylandığından açıklama ekleyemezsiniz.");
                }
                else if (aciklama.IsNullOrWhiteSpace())
                {
                    mMessage.Messages.Add("Açıklama bilgisi boş bırakılamaz.");
                }
                else if (aciklama.Length > 2048)
                {
                    mMessage.Messages.Add("Açıklama bilgisi 2048 karakterden daha uzun olamaz.");
                }
              

                if (mMessage.Messages.Count == 0)
                {
                    try
                    {
                        if (vaSurecleriMaddeEklenenAciklamaId.HasValue)
                        {
                            var kayit = vaSurecMadde.VASurecleriMaddeEklenenAciklamas.First(p => p.VASurecleriMaddeEklenenAciklamaID == vaSurecleriMaddeEklenenAciklamaId);
                            kayit.VACokluVeriDonemID = vaCokluVeriDonemId;
                            kayit.Aciklama = aciklama;
                            kayit.IslemTarihi = DateTime.Now;
                            kayit.IslemYapanID = UserIdentity.Current.Id;
                            kayit.IslemYapanIP = UserIdentity.Ip;

                        }
                        else
                        {
                            vaSurecMadde.VASurecleriMaddeEklenenAciklamas.Add(new VASurecleriMaddeEklenenAciklama
                            {
                                BirimID = birimMadde.BirimID,
                                VASurecleriMaddeID = birimMadde.VASurecleriMaddeID,
                                VACokluVeriDonemID = vaCokluVeriDonemId,
                                Aciklama = aciklama,
                                IslemTarihi = DateTime.Now,
                                IslemYapanID = UserIdentity.Current.Id,
                                IslemYapanIP = UserIdentity.Ip
                            });

                        }
                        db.SaveChanges();

                        mMessage.Messages.Add("Açıklama ekleme işlemi başarılı.");
                        mMessage.IsSuccess = true;
                        mMessage.MessageType = Msgtype.Success;

                        return new Result(true, mMessage);
                    }
                    catch (Exception ex)
                    {
                        var msg = "Açıklama eklenirken bir hata oluştu! Hata:" + ex.ToExceptionMessage();
                        mMessage.Messages.Add(msg);
                        SistemBilgilendirmeBus.SistemBilgisiKaydet(msg, ObjectExtensions.GetCurrentMethodPath(), BilgiTipi.Hata);
                        mMessage.IsSuccess = false;
                        mMessage.MessageType = Msgtype.Error;
                    }
                }

                return new Result(false, mMessage);
            }
        }

        public static IResult VeriAciklamasiSil(int vaSurecleriMaddeEklenenAciklamaId)
        {
            using (var db = new VysDBEntities())
            {
                var mMessage = new MmMessage
                {
                    Title = "Veri Girişi Açıklaması Silme İşlemi",
                    MessageType = Msgtype.Warning
                };

                var kayit = db.VASurecleriMaddeEklenenAciklamas.FirstOrDefault(p => p.VASurecleriMaddeEklenenAciklamaID == vaSurecleriMaddeEklenenAciklamaId);
                if (kayit != null)
                {
                    var maddeId = kayit.VASurecleriMadde.MaddeID;
                    var vaSurecId = kayit.VASurecleriMadde.VASurecID;
                    var vaSurecMadde = db.VASurecleriMaddes.First(p => p.VASurecID == vaSurecId && p.MaddeID == maddeId);
                    var birimMadde = vaSurecMadde.VASurecleriMaddeBirims.First(p => p.BirimID == kayit.BirimID);

                    var birimIDs = UserIdentity.Current.TableRollId[RollTableIdName.BirimId];
                    var girilecekVeriCnt = vaSurecMadde.VASurecleriMaddeVeriGirisDonemleris.Count;
                    var kayitYetki = RoleNames.VeriGirisi.InRole();
                    var birimYetki = birimIDs.Contains(kayit.BirimID);
                    var surec = SurecIslemleriBus.GetVaSurecKontrol(vaSurecId);
                    if (!surec.IsAktif || !surec.AktifSurec)
                    {
                        mMessage.Messages.Add("İşlem yapmaya çalıştığınız Süreç aktif olmadığından herhangi bir işlem yapamazsınız");
                    }
                    else if (!kayitYetki)
                    {
                        mMessage.Messages.Add("Veri girişi için yetkiniz bulunmamaktadır.");
                    }
                    else if (!birimYetki)
                    {
                        mMessage.Messages.Add("İşlem yapmaya çalıştığınız birim yetkiniz dahilinde değildir.");
                    }
                    else if (
                                 girilecekVeriCnt == birimMadde.VASurecleriMadde.VASurecleriMaddeGirilenDegers.Count(c => c.BirimID == kayit.BirimID && c.VeriGirisiOnaylandi)
                                 ||
                                 girilecekVeriCnt == vaSurecMadde.VASurecleriMaddeFormulEslesenMaddes.Select(s => s.VASurecleriMadde1.VASurecleriMaddeGirilenDegers.Count(p => p.BirimID == kayit.BirimID && p.VeriGirisiOnaylandi)).OrderBy(o => o).FirstOrDefault()
                              )
                    {
                        mMessage.Messages.Add("Veri girişi onaylandığından açıklama ekleyemezsiniz.");
                    }
                    if (db.VASurecleriMaddeTurs.Any(p => p.VASurecID == vaSurecId && p.MaddeTurID == birimMadde.VASurecleriMadde.MaddeTurID && !p.IsVeriGirisiAcik))
                    {
                        mMessage.Messages.Add($"{vaSurecMadde.MaddeTurleri.MaddeTurAdi} madde türüne ait maddeler için veri giriş işlemleri kapalıdır. Detaylı bilgi için sistem yöneticisi ile iletişime geçiniz.");
                    }
                    else
                    {
                        try
                        {
                            mMessage.Messages.Add("'" + kayit.Aciklama + "' açıklaması silindi!");
                            db.VASurecleriMaddeEklenenAciklamas.Remove(kayit);
                            db.SaveChanges();
                            mMessage.IsSuccess = true;
                            return new Result(true, mMessage);
                        }
                        catch (Exception ex)
                        {
                            mMessage.Messages.Add("'" + kayit.Aciklama + "' açıklaması silinemedi! <br/> Bilgi:" + ex.ToExceptionMessage());
                            SistemBilgilendirmeBus.SistemBilgisiKaydet(mMessage.Messages[0], ObjectExtensions.GetCurrentMethodPath(), BilgiTipi.OnemsizHata);
                        }

                    }

                }
                else
                {
                    mMessage.Messages.Add("Silmek istenen açıklama sistemde bulunamadı!");
                }

                return new Result(false, mMessage);
            }
        }
    }
}