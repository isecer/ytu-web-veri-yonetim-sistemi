using System;
using System.Collections.Generic;
using System.Linq;
using WebApp.Models;
using Database;
using BiskaUtil;
using WebApp.Utilities.Extensions;
using WebApp.Utilities.Helpers;
using WebApp.Utilities.MessageBox;
using WebApp.Utilities.Results;
using System.Web.Mvc;
using WebApp.Utilities.SystemData;

namespace WebApp.Business
{
    public static class SurecIslemleriBus
    {
        public static int? GetAktifSurecId()
        {
            using (var db = new VysDBEntities())
            {
                var nowDate = DateTime.Now.Date;
                var donem = db.VASurecleris.FirstOrDefault(a => (a.BaslangicTarihi <= nowDate && a.BitisTarihi >= nowDate) && a.IsAktif);
                return donem?.VASurecID;
            }
        }
        public static List<ComboModelInt> CmbVaSurecler(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Value = null, Caption = "" });

            using (var db = new VysDBEntities())
            {
                var data = db.VASurecleris.OrderByDescending(o => o.Yil).ToList();

                foreach (var item in data)
                {
                    dct.Add(new ComboModelInt { Value = item.VASurecID, Caption = item.Yil + " Yılı Süreci" });
                }
            }
            return dct;

        }

        public static IDataResult<FmSurecIslemleri> GetSurecler(FmSurecIslemleri model)
        {
            using (var entities = new VysDBEntities())
            {
                var nowDate = DateTime.Now.Date;
                var q = from s in entities.VASurecleris
                        join k in entities.Kullanicilars on s.IslemYapanID equals k.KullaniciID
                        select new FrSurecIslemleri
                        {
                            VASurecID = s.VASurecID,
                            SurecYilAdi = s.Yil + " Yılı Süreci",
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
                if (model.IsAktif.HasValue) q = q.Where(p => p.IsAktif == model.IsAktif);
                model.RowCount = q.Count();
                q = !model.Sort.IsNullOrWhiteSpace() ? q.OrderBy(model.Sort) : q.OrderByDescending(t => t.Yil).ThenByDescending(t => t.BaslangicTarihi);
                model.Data = q.Skip(model.StartRowIndex).Take(model.PageSize).ToList();
                return new DataResult<FmSurecIslemleri>(model, true);
            }



        }
        public static KmSurecIslemleri GetSurec(int? id)
        {
            var model = new KmSurecIslemleri
            {
                IsAktif = true,
                IsAktifYilPlanlananVeriGirisiAcik = true,
                IsGelecekYilPlanlananVeriGirisiAcik = true
            };
            if (id > 0)
            {
                using (var entities = new VysDBEntities())
                {
                    var data = entities.VASurecleris.FirstOrDefault(p => p.VASurecID == id);

                    if (data != null)
                    {
                        model.VASurecID = data.VASurecID;
                        model.Yil = data.Yil;
                        model.BaslangicTarihi = data.BaslangicTarihi;
                        model.BitisTarihi = data.BitisTarihi;
                        model.IsAktifYilPlanlananVeriGirisiAcik = data.IsAktifYilPlanlananVeriGirisiAcik;
                        model.IsGelecekYilPlanlananVeriGirisiAcik = data.IsGelecekYilPlanlananVeriGirisiAcik;
                        model.IsAktif = data.IsAktif;
                        model.IslemTarihi = DateTime.Now;
                        model.IslemYapanID = data.IslemYapanID;
                        model.IslemYapanIP = data.IslemYapanIP;
                    }
                }


            }
            else
            {
                model.Yil = DateTime.Now.Year;
            }

            return model;
        }
        public static FrSurecIslemleri GetVaSurecKontrol(int vaSurecId)
        {
            using (var db = new VysDBEntities())
            {
                var nowDate = DateTime.Now.Date;
                var frSurecIslemleri = (from s in db.VASurecleris.Where(p => p.VASurecID == vaSurecId)
                                        join k in db.Kullanicilars on s.IslemYapanID equals k.KullaniciID
                                        select new FrSurecIslemleri
                                        {
                                            VASurecID = s.VASurecID,
                                            SurecYilAdi = s.Yil + " Yılı Süreci",
                                            Yil = s.Yil,
                                            BaslangicTarihi = s.BaslangicTarihi,
                                            BitisTarihi = s.BitisTarihi,
                                            IsAktifYilPlanlananVeriGirisiAcik = s.IsAktifYilPlanlananVeriGirisiAcik,
                                            IsGelecekYilPlanlananVeriGirisiAcik = s.IsGelecekYilPlanlananVeriGirisiAcik,
                                            IsAktif = s.IsAktif,
                                            IslemYapanID = s.IslemYapanID,
                                            IslemYapan = k.KullaniciAdi,
                                            IslemTarihi = s.IslemTarihi,
                                            IslemYapanIP = s.IslemYapanIP,
                                            AktifSurec = (s.BaslangicTarihi <= nowDate && s.BitisTarihi >= nowDate)
                                        }).FirstOrDefault();
                return frSurecIslemleri;
            }
        }

        public static string GetSubPage(int id, int? selectDurumId, int tbInx)
        {
            using (var entities = new VysDBEntities())
            {
                var mdl = GetVaSurecKontrol(id);

                if (tbInx == 1)
                {
                    mdl.SBirimDurumID = selectDurumId;
                    var mBirimIDs = entities.VASurecleriBirims.Where(p => p.VASurecID == id).Select(s => s.BirimID).Distinct().ToList();
                    mdl.BirimData = (from s in entities.VASurecleriBirims.Where(p => p.VASurecID == id && p.VASurecleriBirimMaddes.Any(a => a.VASurecleriMadde.IsAktif) && mBirimIDs.Contains(p.BirimID))
                                     join bt in entities.Vw_BirimlerTree on s.BirimID equals bt.BirimID
                                     select new FrBirimler
                                     {
                                         BirimID = s.BirimID,
                                         BirimKod = s.BirimKod,
                                         BirimAdi = bt.BirimTreeAdi,
                                         IslemTarihi = s.IslemTarihi,
                                         ToplamCount = s.VASurecleriBirimMaddes.Count(p => p.VASurecleriMadde.IsAktif),
                                         TamamlananCount = s.VASurecleriBirimMaddes.Count(p => p.VASurecleriMadde.IsAktif && p.VASurecleriMadde.VASurecleriMaddeGirilenDegers.Count(p2 => p2.VASurecleriBirimID == s.VASurecleriBirimID) == p.VASurecleriMadde.VASurecleriMaddeVeriGirisDonemleris.Count)

                                     }).Where(p => !mdl.SBirimDurumID.HasValue || (mdl.SBirimDurumID == 1 ? p.ToplamCount == p.TamamlananCount : p.ToplamCount != p.TamamlananCount)).OrderBy(o => o.BirimAdi).ToList();
                    mdl.SListBirimDurum = new SelectList(ComboData.CmbSurecBirimDurum(), "Value", "Caption", mdl.SBirimDurumID);

                    return ViewRenderHelper.RenderPartialView("SurecIslemleri", "GetBirimBilgileri", mdl);
                }
                mdl.SMaddeDurumID = selectDurumId;
                mdl.MaddeData = (from s in entities.VASurecleriMaddes.Where(p => p.VASurecID == id && p.IsAktif)
                                 join bt in entities.Maddelers on s.MaddeID equals bt.MaddeID
                                 select new FrMaddeler
                                 {
                                     MaddeID = s.MaddeID,
                                     MaddeKod = s.MaddeKod,
                                     MaddeAdi = bt.MaddeAdi,
                                     ToplamCount = s.VASurecleriBirimMaddes.Count(p => p.VASurecleriMadde.IsAktif),
                                     TamamlananCount = s.VASurecleriBirimMaddes.Count(p => p.VASurecleriMadde.IsAktif && p.VASurecleriMadde.VASurecleriMaddeGirilenDegers.Count(p2 => p2.VASurecleriBirimID == p.VASurecleriBirimID) == p.VASurecleriMadde.VASurecleriMaddeVeriGirisDonemleris.Count)

                                 }).Where(predicate: p => !mdl.SMaddeDurumID.HasValue || (mdl.SMaddeDurumID == 1 ? p.ToplamCount == p.TamamlananCount : p.ToplamCount != p.TamamlananCount)).OrderBy(o => o.MaddeAdi).ToList();
                mdl.SListMaddeDurum = new SelectList(ComboData.CmbSurecMaddeDurum(), "Value", "Caption", mdl.SMaddeDurumID);
                return ViewRenderHelper.RenderPartialView("SurecIslemleri", "GetMaddeBilgileri", mdl);

            }

        }

        public static List<FrMaddeler> GetSurecBirimMaddeleri(int vaSurecId, int birimId)
        {
            using (var db = new VysDBEntities())
            {
                var birimMaddeleri = (from s2 in db.VASurecleriBirimMaddes.Where(p => p.VASurecleriBirim.VASurecID == vaSurecId && p.VASurecleriBirim.BirimID == birimId)
                                      join vsmd in db.VASurecleriMaddes.Where(p => p.IsAktif) on s2.VASurecleriMaddeID equals vsmd.VASurecleriMaddeID
                                      join md in db.Maddelers on s2.VASurecleriMadde.MaddeID equals md.MaddeID
                                      select new FrMaddeler
                                      {
                                          MaddeID = s2.VASurecleriBirimMaddeID,
                                          MaddeKod = md.MaddeKod,
                                          MaddeAdi = md.MaddeAdi,
                                          IsPlanlananDegerOlacak = vsmd.IsPlanlananDegerOlacak,
                                          IsPlanlananDegerOlacakGelecekYil = vsmd.IsPlanlananDegerOlacakGelecekYil,
                                          PlanlananDeger = s2.PlanlananDeger,
                                          GirilecekVeriSayisi = s2.VASurecleriMadde.VASurecleriMaddeVeriGirisDonemleris.Count,
                                          GirilenVeriSayisi = vsmd.VASurecleriMaddeGirilenDegers.Count(p => p.VASurecleriBirim.BirimID == birimId),
                                      }).OrderBy(o => o.MaddeAdi).ToList();

                return birimMaddeleri;
            }
        }
        public static List<FrBirimler> GetSurecMaddeBirimleri(int vaSurecId, int maddeId)
        {

            using (var db = new VysDBEntities())
            {
                var maddeBirimleri = (from s2 in db.VASurecleriBirimMaddes.Where(p => p.VASurecleriMadde.VASurecID == vaSurecId && p.VASurecleriMadde.MaddeID == maddeId)
                                      join br in db.Vw_BirimlerTree on s2.VASurecleriBirim.BirimID equals br.BirimID
                                      select new FrBirimler
                                      {
                                          BirimID = s2.VASurecleriBirimMaddeID,
                                          BirimKod = br.BirimKod,
                                          BirimAdi = br.BirimTreeAdi,
                                          ToplamCount = s2.VASurecleriMadde.VASurecleriBirimMaddes.Count,
                                          TamamlananCount = s2.VASurecleriMadde.VASurecleriBirimMaddes.Count(p => p.VASurecleriMadde.VASurecleriMaddeGirilenDegers.Count(p2 => p2.VASurecleriBirim.BirimID == br.BirimID) == p.VASurecleriMadde.VASurecleriMaddeVeriGirisDonemleris.Count)
                                      }).OrderBy(o => o.BirimAdi).ToList();

                return maddeBirimleri;
            }
        }
        public static IDataResult<KmSurecIslemleri> Kayit(KmSurecIslemleri kModel)
        {
            using (var db = new VysDBEntities())
            {
                var mmMessage = new MmMessage();
                #region Kontrol  
                if (kModel.Yil <= 0)
                {
                    mmMessage.Messages.Add("Süreç Yılını Giriniz.");
                    mmMessage.MessagesDialog.Add(new MrMessage { MessageType = Msgtype.Warning, PropertyName = "Yil" });

                }
                else if (db.VASurecleris.Any(p => p.VASurecID != kModel.VASurecID && p.Yil == kModel.Yil))
                {
                    mmMessage.Messages.Add("Seçtiğiniz Süreç Yılı Daha Önceden Kayıt Edilmiştir. Tekrar Kayıt Edilemez.");
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

                #endregion

                if (mmMessage.Messages.Count == 0)
                {
                    kModel.IslemTarihi = DateTime.Now;
                    kModel.IslemYapanID = UserIdentity.Current.Id;
                    kModel.IslemYapanIP = UserIdentity.Ip;
                    kModel.Yil = kModel.Yil;
                    VASurecleri vaSureci;
                    if (kModel.VASurecID <= 0)
                    {
                        vaSureci = db.VASurecleris.Add(new VASurecleri
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
                        vaSureci = db.VASurecleris.First(p => p.VASurecID == kModel.VASurecID);
                        vaSureci.Yil = kModel.Yil;
                        vaSureci.BaslangicTarihi = kModel.BaslangicTarihi.Value;
                        vaSureci.BitisTarihi = kModel.BitisTarihi.Value;
                        vaSureci.IsAktifYilPlanlananVeriGirisiAcik = kModel.IsAktifYilPlanlananVeriGirisiAcik;
                        vaSureci.IsGelecekYilPlanlananVeriGirisiAcik = kModel.IsGelecekYilPlanlananVeriGirisiAcik;
                        vaSureci.IsAktif = kModel.IsAktif;
                        vaSureci.IslemTarihi = DateTime.Now;
                        vaSureci.IslemYapanID = kModel.IslemYapanID;
                        vaSureci.IslemYapanIP = kModel.IslemYapanIP;
                    }

                    db.SaveChanges();
                    kModel.VASurecID = vaSureci.VASurecID;
                    return new DataResult<KmSurecIslemleri>(kModel, true, mmMessage);

                }
                return new DataResult<KmSurecIslemleri>(kModel, false, mmMessage);
            }
        }

        private static void BirimleriVeMaddeleriKopyala(SurecVeriKopyalaModel fModel)
        {

            using (var entities = new VysDBEntities())
            {


                var maddeTurIDs = fModel.Data.Select(s => s.MaddeTurID).ToList();

                var eoDonem = entities.VASurecleris.First(p => p.VASurecID == fModel.VASurecID);
                var vaSurecleriMaddeler = eoDonem.VASurecleriMaddes.Where(p => maddeTurIDs.Contains(p.MaddeTurID)).ToList();
                var maddelers = entities.Maddelers.Where(p => p.IsAktif && p.BirimMaddeleris.Any() && maddeTurIDs.Contains(p.MaddeTurID.Value)).ToList();
                var eklenecekMaddelers = maddelers.Where(p => eoDonem.VASurecleriMaddes.All(a => a.MaddeID != p.MaddeID) && p.MaddeTurleri.IsAktif).ToList();
                var varolanMaddlers = vaSurecleriMaddeler.Where(p => maddelers.Any(a => a.MaddeID == p.MaddeID)).ToList();
                var silinecekMaddelers = vaSurecleriMaddeler.Where(p => maddelers.All(a => a.MaddeID != p.MaddeID)).ToList();
                if (eklenecekMaddelers.Any())
                {
                    foreach (var item in eklenecekMaddelers)
                    {
                        entities.VASurecleriMaddes.Add(new VASurecleriMadde
                        {
                            VASurecID = fModel.VASurecID,
                            MaddeID = item.MaddeID,
                            MaddeKod = item.MaddeKod,
                            MaddeTurID = item.MaddeTurID.Value,
                            VeriGirisSekliID = item.VeriGirisSekliID,
                            VeriTipID = item.VeriTipID,
                            MaddeYilSonuDegerHesaplamaTipID = item.MaddeYilSonuDegerHesaplamaTipID.Value,
                            HesaplamaFormulu = item.HesaplamaFormulu,
                            IsPlanlananDegerOlacak = item.MaddeTurleri.IsPlanlananDegerOlacak,
                            IsPlanlananDegerOlacakGelecekYil = item.MaddeTurleri.IsPlanlananDegerOlacakGelecekYil,
                            IslemTarihi = DateTime.Now,
                            IslemYapanID = UserIdentity.Current.Id,
                            IslemYapanIP = UserIdentity.Ip,
                            IsAktif = true,
                            VASurecleriMaddeVeriGirisDonemleris = item.MaddelerVeriGirisDonemleris.ToList().Select(s => new VASurecleriMaddeVeriGirisDonemleri { VACokluVeriDonemID = s.VACokluVeriDonemID, IsDosyaYuklensin = s.IsDosyaYuklensin }).ToList(),
                        });
                    }
                    entities.SaveChanges();
                    var donemMaddeler = eoDonem.VASurecleriMaddes.ToList();
                    foreach (var item in eklenecekMaddelers.Where(p => p.VeriGirisSekliID == VeriGirisSekli.FormulleHesaplanacak))
                    {
                        var eklenecekFhMadde = donemMaddeler.First(p => p.MaddeID == item.MaddeID);
                        foreach (var itemF in item.MaddelerFormulEslesenMaddelers)
                        {
                            var eslesenMadde = donemMaddeler.First(p => p.MaddeKod == itemF.EslesenMaddeKod);
                            eklenecekFhMadde.VASurecleriMaddeFormulEslesenMaddes.Add(new VASurecleriMaddeFormulEslesenMadde { EslesenVASurecleriMaddeID = eslesenMadde.VASurecleriMaddeID });
                        }
                    }
                    entities.SaveChanges();


                }
                if (varolanMaddlers.Any())
                {
                    foreach (var item in varolanMaddlers)
                    {
                        var mturKopyalamaBilgisi = fModel.Data.First(p => p.MaddeTurID == item.MaddeTurID);
                        var madde = maddelers.First(p => p.MaddeID == item.MaddeID);

                        item.VASurecID = fModel.VASurecID;
                        item.MaddeID = madde.MaddeID;
                        item.MaddeKod = madde.MaddeKod;
                        item.VeriGirisSekliID = madde.VeriGirisSekliID;
                        item.HesaplamaFormulu = madde.HesaplamaFormulu;
                        item.VeriTipID = madde.VeriTipID;
                        item.MaddeYilSonuDegerHesaplamaTipID = madde.MaddeYilSonuDegerHesaplamaTipID.Value;
                        item.IslemTarihi = DateTime.Now;
                        item.IslemYapanID = UserIdentity.Current.Id;
                        item.IslemYapanIP = UserIdentity.Ip;
                        item.IsAktif = item.MaddeTurleri.IsAktif;

                        if (mturKopyalamaBilgisi.MaddeTurOzellikleriniKopyala)
                        {
                            item.IsPlanlananDegerOlacak = madde.MaddeTurleri.IsPlanlananDegerOlacak;
                            item.IsPlanlananDegerOlacakGelecekYil = madde.MaddeTurleri.IsPlanlananDegerOlacakGelecekYil;
                            var eklenecekDonems = madde.MaddelerVeriGirisDonemleris.Where(p => item.VASurecleriMaddeVeriGirisDonemleris.Select(s => s.VACokluVeriDonemID).All(a => a != p.VACokluVeriDonemID)).ToList().Select(s => new VASurecleriMaddeVeriGirisDonemleri { VACokluVeriDonemID = s.VACokluVeriDonemID, IsDosyaYuklensin = s.IsDosyaYuklensin, VASurecleriMaddeID = item.VASurecleriMaddeID }).ToList();
                            var silinecekDonems = item.VASurecleriMaddeVeriGirisDonemleris.Where(p => madde.MaddelerVeriGirisDonemleris.All(a => a.VACokluVeriDonemID != p.VACokluVeriDonemID)).ToList();
                            if (silinecekDonems.Any())
                            {
                                entities.VASurecleriMaddeVeriGirisDonemleris.RemoveRange(silinecekDonems);
                            }
                            if (eklenecekDonems.Any())
                            {
                                entities.VASurecleriMaddeVeriGirisDonemleris.AddRange(eklenecekDonems);
                            }
                        }
                        if (item.VeriGirisSekliID == VeriGirisSekli.FormulleHesaplanacak)
                        {
                            entities.VASurecleriMaddeFormulEslesenMaddes.RemoveRange(item.VASurecleriMaddeFormulEslesenMaddes);
                            foreach (var itemF in madde.MaddelerFormulEslesenMaddelers)
                            {
                                var eslesenMadde = eoDonem.VASurecleriMaddes.First(p => p.MaddeKod == itemF.EslesenMaddeKod);
                                item.VASurecleriMaddeFormulEslesenMaddes.Add(new VASurecleriMaddeFormulEslesenMadde { EslesenVASurecleriMaddeID = eslesenMadde.VASurecleriMaddeID });
                            }
                        }
                        else
                        {
                            entities.VASurecleriMaddeFormulEslesenMaddes.RemoveRange(item.VASurecleriMaddeFormulEslesenMaddes);
                            item.VASurecleriMaddeFormulEslesenMaddes = null;
                            item.HesaplamaFormulu = null;
                        }
                    }
                }
                foreach (var item in silinecekMaddelers)
                    item.IsAktif = false;
                entities.SaveChanges();

                var birims = entities.Birimlers.Where(p => p.BirimMaddeleris.Any()).ToList();
                var eklenecekBirim = birims.Where(p => eoDonem.VASurecleriBirims.All(a => a.BirimID != p.BirimID)).ToList();
                var varolanBirim = eoDonem.VASurecleriBirims.Where(p => birims.Any(a => a.BirimID == p.BirimID)).ToList();
                var silinecekBirim = eoDonem.VASurecleriBirims.Where(p => birims.All(a => a.BirimID != p.BirimID)).ToList();

                var vaSurecleriMaddelers = entities.VASurecleriMaddes.Where(p => p.VASurecID == fModel.VASurecID && maddeTurIDs.Contains(p.MaddeTurID)).ToList();
                var pldegerMaddeTurIDs = fModel.Data.Where(p => p.PlanlananDegerleriKopyala).Select(s => s.MaddeTurID).Distinct().ToList();
                var oncekiDonemGirilenHedefler = entities.VASurecleriBirimMaddes.Where(p => p.VASurecleriMadde.VASurecleri.Yil == (eoDonem.Yil - 1) && pldegerMaddeTurIDs.Contains(p.VASurecleriMadde.MaddeTurID)).ToList();

                if (eklenecekBirim.Any())
                    entities.VASurecleriBirims.AddRange(eklenecekBirim.Select(item => new VASurecleriBirim
                    {
                        VASurecID = fModel.VASurecID,
                        BirimID = item.BirimID,
                        BirimKod = item.BirimKod,
                        IslemTarihi = DateTime.Now,
                        IslemYapanID = UserIdentity.Current.Id,
                        IslemYapanIP = UserIdentity.Ip,
                        VASurecleriBirimMaddes = (from s in vaSurecleriMaddelers.Where(p => item.BirimMaddeleris.Any(a => a.MaddeID == p.MaddeID))
                                                  join od in oncekiDonemGirilenHedefler on new { s.MaddeID, item.BirimID } equals new { od.VASurecleriMadde.MaddeID, od.VASurecleriBirim.BirimID } into defOd
                                                  from od in defOd.DefaultIfEmpty()
                                                  select new VASurecleriBirimMadde
                                                  {
                                                      IslemTarihi = DateTime.Now,
                                                      IslemYapanID = UserIdentity.Current.Id,
                                                      IslemYapanIP = UserIdentity.Ip,
                                                      VASurecleriMaddeID = s.VASurecleriMaddeID,
                                                      PlanlananDeger = (od != null && pldegerMaddeTurIDs.Contains(s.MaddeTurID) ? od.PlanlananDegerGelecekYil : null)
                                                  }).ToList()

                    }));
                // Maddetürü seçeneklerine göre planlanan hedefler seçime göre güncellenecek 
                foreach (var item in varolanBirim)
                {
                    var birim = birims.First(p => p.BirimID == item.BirimID);
                    var maddeIDs = birim.BirimMaddeleris.Select(s => s.MaddeID).ToList();
                    var vaSurecMaddeIDs = vaSurecleriMaddelers.Where(p => maddeIDs.Contains(p.MaddeID)).Select(s => s.VASurecleriMaddeID).ToList();
                    item.IslemTarihi = DateTime.Now;
                    item.IslemYapanID = UserIdentity.Current.Id;
                    item.IslemYapanIP = UserIdentity.Ip;

                    var eklenecekEBirims = vaSurecMaddeIDs.Where(p => !item.VASurecleriBirimMaddes.Any(a => maddeTurIDs.Contains(a.VASurecleriMadde.MaddeTurID) && a.VASurecleriMaddeID == p)).Select(s => new VASurecleriBirimMadde
                    {

                        VASurecleriMaddeID = s,
                        IslemTarihi = DateTime.Now,
                        IslemYapanID = UserIdentity.Current.Id,
                        IslemYapanIP = UserIdentity.Ip,
                    }).ToList();
                    var silinecekEBirims = item.VASurecleriBirimMaddes.Where(p => maddeTurIDs.Contains(p.VASurecleriMadde.MaddeTurID)).Where(p => vaSurecMaddeIDs.All(a => a != p.VASurecleriMaddeID)).ToList();
                    if (silinecekEBirims.Any()) entities.VASurecleriBirimMaddes.RemoveRange(silinecekEBirims);
                    var guncellenecekEBirims = item.VASurecleriBirimMaddes.Where(p => maddeTurIDs.Contains(p.VASurecleriMadde.MaddeTurID)).Where(p => vaSurecMaddeIDs.Any(a => a == p.VASurecleriMaddeID)).ToList();

                    foreach (var itemGb in guncellenecekEBirims)
                    {
                        if (pldegerMaddeTurIDs.Contains(itemGb.VASurecleriMadde.MaddeTurID))
                        {
                            itemGb.PlanlananDeger = oncekiDonemGirilenHedefler.Where(p => p.VASurecleriMadde.MaddeID == itemGb.VASurecleriMadde.MaddeID && p.VASurecleriBirim.BirimID == itemGb.VASurecleriBirim.BirimID).Select(s => s.PlanlananDegerGelecekYil).FirstOrDefault();
                        }
                    }
                    foreach (var itemMb in eklenecekEBirims)
                    {
                        var vaSMadde = vaSurecleriMaddelers.First(p => p.VASurecleriMaddeID == itemMb.VASurecleriMaddeID);
                        if (pldegerMaddeTurIDs.Contains(vaSMadde.MaddeTurID))
                        {
                            itemMb.PlanlananDeger = oncekiDonemGirilenHedefler.Where(p => p.VASurecleriMadde.MaddeID == vaSMadde.MaddeID && p.VASurecleriBirim.BirimID == itemMb.VASurecleriBirim.BirimID).Select(s => s.PlanlananDegerGelecekYil).FirstOrDefault();
                        }
                        item.VASurecleriBirimMaddes.Add(itemMb);
                    }

                }
                entities.VASurecleriBirims.RemoveRange(silinecekBirim);
                eoDonem.IslemTarihi = DateTime.Now;
                eoDonem.IslemYapanID = UserIdentity.Current.Id;
                eoDonem.IslemYapanIP = UserIdentity.Ip;
                entities.SaveChanges();
                var data = entities.VASurecleriMaddeGirilenDegers.Where(p => !p.VASurecleriBirim.VASurecleriBirimMaddes.Any(a => a.VASurecleriBirimID == p.VASurecleriBirimID && a.VASurecleriMaddeID == p.VASurecleriMaddeID)).ToList();
                entities.VASurecleriMaddeGirilenDegers.RemoveRange(data);
                entities.SaveChanges();
            }
        }

        public static IDataResult<SurecVeriKopyalaModel> VeriKopyala(SurecVeriKopyalaModel kModel)
        {
            var mMessage = new MmMessage
            {
                IsSuccess = false,
                Title = "Sürece Veri Kopyalama İşlemi Başarısız.",
                MessageType = Msgtype.Warning
            };
            var maddeSecildis = kModel.MaddeSecildi.Select((s, inx) => new { MaddeSecildi = s, Inx = inx }).ToList();
            var maddeTurIds = kModel.MaddeTurID.Select((s, inx) => new { MaddeTurID = s, Inx = inx }).ToList();
            var maddeTurOzellikleriniKopyalas = kModel.MaddeTurOzellikleriniKopyala.Select((s, inx) => new { MaddeTurOzellikleriniKopyala = s, Inx = inx }).ToList();
            var planlananDegerleriKopyalas = kModel.PlanlananDegerleriKopyala.Select((s, inx) => new { PlanlananDegerleriKopyala = s, Inx = inx }).ToList();
            kModel.Data = (from ms in maddeSecildis
                           join mt in maddeTurIds on ms.Inx equals mt.Inx
                           join mto in maddeTurOzellikleriniKopyalas on ms.Inx equals mto.Inx
                           join pdk in planlananDegerleriKopyalas on ms.Inx equals pdk.Inx
                           select new SurecVeriKopyalaRow
                           {
                               MaddeSecildi = ms.MaddeSecildi,
                               MaddeTurID = mt.MaddeTurID,
                               MaddeTurOzellikleriniKopyala = mto.MaddeTurOzellikleriniKopyala,
                               PlanlananDegerleriKopyala = pdk.PlanlananDegerleriKopyala

                           }).Where(p => p.MaddeSecildi).ToList();


            try
            {

                BirimleriVeMaddeleriKopyala(kModel);
                mMessage.Title = "Sürece Veri Kopyalama İşlemi Başarılı.";
                mMessage.IsSuccess = true;
                mMessage.Messages.Add("Veriler Aktarıldı.");
                mMessage.MessageType = Msgtype.Success;

                return new DataResult<SurecVeriKopyalaModel>(kModel, true, mMessage);
            }
            catch (Exception ex)
            {
                mMessage.Messages.Add("Veriler Aktarılırken bir hata oluştu! Hata:" + ex.ToExceptionMessage());
            }
            return new DataResult<SurecVeriKopyalaModel>(kModel, false, mMessage);

        }


        //public static IDataResult<SurecVeriKopyalaModel> SurecMaddeSenkronizasyonu()
        //{

        //}
        public static IResult SurecSil(int id)
        {
            using (var entities = new VysDBEntities())
            {
                var mmMessage = new MmMessage();
                var kayit = entities.VASurecleris.FirstOrDefault(p => p.VASurecID == id);
                string message;
                if (kayit != null)
                {
                    try
                    {
                        entities.VASurecleris.Remove(kayit);
                        entities.SaveChanges();

                        mmMessage.Title = "Uyarı";
                        message = "'" + kayit.Yil + "' Yılı Süreci Silindi!";
                        mmMessage.Messages.Add(message);
                        mmMessage.MessageType = Msgtype.Success;
                        mmMessage.IsSuccess = true;

                        return new Result(true, mmMessage);
                    }
                    catch (Exception ex)
                    {
                        message = "'" + kayit.Yil + "' Yılı Süreci Silinirken Bir Hata Oluştu! </br> Hata:" + ex.ToExceptionMessage();
                        SistemBilgilendirmeBus.SistemBilgisiKaydet(message, ObjectExtensions.GetCurrentMethodPath(), BilgiTipi.OnemsizHata);
                        mmMessage.Title = "Hata";
                        mmMessage.Messages.Add(message);
                        mmMessage.MessageType = Msgtype.Error;
                        mmMessage.IsSuccess = false;
                        return new Result(false, mmMessage);
                    }
                }

                message = "Silmek İstediğiniz Süreç Sistemde Bulunamadı!";
                mmMessage.Title = "Hata";
                mmMessage.Messages.Add(message);
                mmMessage.MessageType = Msgtype.Error;
                mmMessage.IsSuccess = true;
                return new Result(true, mmMessage);

            }
        }
    }
}