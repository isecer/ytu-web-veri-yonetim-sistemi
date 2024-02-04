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
using WebGrease.Css.Extensions;

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
                    var mBirimIDs = entities.VASurecleriMaddeBirims.Where(p => p.VASurecleriMadde.VASurecID == id && p.VASurecleriMadde.IsAktif).Select(s => s.BirimID).Distinct().ToList();
                    mdl.BirimData = (from s in entities.Vw_BirimlerTree.Where(p => mBirimIDs.Contains(p.BirimID))
                                     select new FrBirimler
                                     {
                                         BirimID = s.BirimID,
                                         BirimKod = s.BirimKod,
                                         BirimAdi = s.BirimTreeAdi,
                                         IslemTarihi = s.IslemTarihi,
                                         ToplamCount = entities.VASurecleriMaddeBirims.Count(p => p.VASurecleriMadde.VASurecID == id && p.BirimID == s.BirimID && p.VASurecleriMadde.IsAktif),
                                         TamamlananCount = entities.VASurecleriMaddeBirims.Count(p => p.VASurecleriMadde.VASurecID == id && p.BirimID == s.BirimID && p.VASurecleriMadde.IsAktif && p.VASurecleriMadde.VASurecleriMaddeGirilenDegers.Count(p2 => p2.BirimID == s.BirimID) == p.VASurecleriMadde.VASurecleriMaddeVeriGirisDonemleris.Count)

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
                                     ToplamCount = s.VASurecleriMaddeBirims.Count(p => p.VASurecleriMadde.IsAktif),
                                     TamamlananCount = s.VASurecleriMaddeBirims.Count(p => p.VASurecleriMadde.IsAktif && p.VASurecleriMadde.VASurecleriMaddeGirilenDegers.Count(p2 => p2.BirimID == p.BirimID) == p.VASurecleriMadde.VASurecleriMaddeVeriGirisDonemleris.Count)

                                 }).Where(predicate: p => !mdl.SMaddeDurumID.HasValue || (mdl.SMaddeDurumID == 1 ? p.ToplamCount == p.TamamlananCount : p.ToplamCount != p.TamamlananCount)).OrderBy(o => o.MaddeAdi).ToList();
                mdl.SListMaddeDurum = new SelectList(ComboData.CmbSurecMaddeDurum(), "Value", "Caption", mdl.SMaddeDurumID);
                return ViewRenderHelper.RenderPartialView("SurecIslemleri", "GetMaddeBilgileri", mdl);

            }

        }

        public static List<FrMaddeler> GetSurecBirimMaddeleri(int vaSurecId, int birimId)
        {
            using (var db = new VysDBEntities())
            {
                var birimMaddeleri = (from s2 in db.VASurecleriMaddeBirims.Where(p => p.VASurecleriMadde.VASurecID == vaSurecId && p.BirimID == birimId)
                                      join vsmd in db.VASurecleriMaddes.Where(p => p.IsAktif) on s2.VASurecleriMaddeID equals vsmd.VASurecleriMaddeID
                                      join md in db.Maddelers on s2.VASurecleriMadde.MaddeID equals md.MaddeID
                                      select new FrMaddeler
                                      {
                                          MaddeID = s2.VASurecleriMadde.MaddeID,
                                          MaddeKod = md.MaddeKod,
                                          MaddeAdi = md.MaddeAdi,
                                          IsPlanlananDegerOlacak = vsmd.IsPlanlananDegerOlacak,
                                          IsPlanlananDegerOlacakGelecekYil = vsmd.IsPlanlananDegerOlacakGelecekYil,
                                          PlanlananDeger = s2.PlanlananDeger,
                                          GirilecekVeriSayisi = s2.VASurecleriMadde.VASurecleriMaddeVeriGirisDonemleris.Count,
                                          GirilenVeriSayisi = vsmd.VASurecleriMaddeGirilenDegers.Count(p => p.BirimID == birimId),
                                      }).OrderBy(o => o.MaddeAdi).ToList();

                return birimMaddeleri;
            }
        }
        public static List<FrBirimler> GetSurecMaddeBirimleri(int vaSurecId, int maddeId)
        {

            using (var db = new VysDBEntities())
            {
                var maddeBirimleri = (from s2 in db.VASurecleriMaddeBirims.Where(p => p.VASurecleriMadde.VASurecID == vaSurecId && p.VASurecleriMadde.MaddeID == maddeId)
                                      join br in db.Vw_BirimlerTree on s2.BirimID equals br.BirimID
                                      select new FrBirimler
                                      {
                                          BirimID = s2.BirimID,
                                          BirimKod = br.BirimKod,
                                          BirimAdi = br.BirimTreeAdi,
                                          ToplamCount = s2.VASurecleriMadde.VASurecleriMaddeBirims.Count,
                                          TamamlananCount = s2.VASurecleriMadde.VASurecleriMaddeBirims.Count(p => p.VASurecleriMadde.VASurecleriMaddeGirilenDegers.Count(p2 => p2.BirimID == br.BirimID) == p.VASurecleriMadde.VASurecleriMaddeVeriGirisDonemleris.Count)
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

                //var birims = entities.Birimlers.Where(p => p.BirimMaddeleris.Any()).ToList();
                //var eklenecekBirim = birims.Where(p => eoDonem.VASurecleriBirims.All(a => a.BirimID != p.BirimID)).ToList();
                //var varolanBirim = eoDonem.VASurecleriBirims.Where(p => birims.Any(a => a.BirimID == p.BirimID)).ToList();
                //var silinecekBirim = eoDonem.VASurecleriBirims.Where(p => birims.All(a => a.BirimID != p.BirimID)).ToList();

                //var vaSurecleriMaddelers = entities.VASurecleriMaddes.Where(p => p.VASurecID == fModel.VASurecID && maddeTurIDs.Contains(p.MaddeTurID)).ToList();
                //var pldegerMaddeTurIDs = fModel.Data.Where(p => p.PlanlananDegerleriKopyala).Select(s => s.MaddeTurID).Distinct().ToList();
                //var oncekiDonemGirilenHedefler = entities.VASurecleriBirimMaddes.Where(p => p.VASurecleriMadde.VASurecleri.Yil == (eoDonem.Yil - 1) && pldegerMaddeTurIDs.Contains(p.VASurecleriMadde.MaddeTurID)).ToList();

                //if (eklenecekBirim.Any())
                //    entities.VASurecleriBirims.AddRange(eklenecekBirim.Select(item => new VASurecleriBirim
                //    {
                //        VASurecID = fModel.VASurecID,
                //        BirimID = item.BirimID,
                //        BirimKod = item.BirimKod,
                //        IslemTarihi = DateTime.Now,
                //        IslemYapanID = UserIdentity.Current.Id,
                //        IslemYapanIP = UserIdentity.Ip,
                //        VASurecleriBirimMaddes = (from s in vaSurecleriMaddelers.Where(p => item.BirimMaddeleris.Any(a => a.MaddeID == p.MaddeID))
                //                                  join od in oncekiDonemGirilenHedefler on new { s.MaddeID, item.BirimID } equals new { od.VASurecleriMadde.MaddeID, od.VASurecleriBirim.BirimID } into defOd
                //                                  from od in defOd.DefaultIfEmpty()
                //                                  select new VASurecleriBirimMadde
                //                                  {
                //                                      IslemTarihi = DateTime.Now,
                //                                      IslemYapanID = UserIdentity.Current.Id,
                //                                      IslemYapanIP = UserIdentity.Ip,
                //                                      VASurecleriMaddeID = s.VASurecleriMaddeID,
                //                                      PlanlananDeger = (od != null && pldegerMaddeTurIDs.Contains(s.MaddeTurID) ? od.PlanlananDegerGelecekYil : null)
                //                                  }).ToList()

                //    }));
                // Maddetürü seçeneklerine göre planlanan hedefler seçime göre güncellenecek 
                //foreach (var item in varolanBirim)
                //{
                //    var birim = birims.First(p => p.BirimID == item.BirimID);
                //    var maddeIDs = birim.BirimMaddeleris.Select(s => s.MaddeID).ToList();
                //    var vaSurecMaddeIDs = vaSurecleriMaddelers.Where(p => maddeIDs.Contains(p.MaddeID)).Select(s => s.VASurecleriMaddeID).ToList();
                //    item.IslemTarihi = DateTime.Now;
                //    item.IslemYapanID = UserIdentity.Current.Id;
                //    item.IslemYapanIP = UserIdentity.Ip;

                //    var eklenecekEBirims = vaSurecMaddeIDs.Where(p => !item.VASurecleriBirimMaddes.Any(a => maddeTurIDs.Contains(a.VASurecleriMadde.MaddeTurID) && a.VASurecleriMaddeID == p)).Select(s => new VASurecleriBirimMadde
                //    {

                //        VASurecleriMaddeID = s,
                //        IslemTarihi = DateTime.Now,
                //        IslemYapanID = UserIdentity.Current.Id,
                //        IslemYapanIP = UserIdentity.Ip,
                //    }).ToList();
                //    var silinecekEBirims = item.VASurecleriBirimMaddes.Where(p => maddeTurIDs.Contains(p.VASurecleriMadde.MaddeTurID)).Where(p => vaSurecMaddeIDs.All(a => a != p.VASurecleriMaddeID)).ToList();
                //    if (silinecekEBirims.Any()) entities.VASurecleriBirimMaddes.RemoveRange(silinecekEBirims);
                //    var guncellenecekEBirims = item.VASurecleriBirimMaddes.Where(p => maddeTurIDs.Contains(p.VASurecleriMadde.MaddeTurID)).Where(p => vaSurecMaddeIDs.Any(a => a == p.VASurecleriMaddeID)).ToList();

                //    foreach (var itemGb in guncellenecekEBirims)
                //    {
                //        if (pldegerMaddeTurIDs.Contains(itemGb.VASurecleriMadde.MaddeTurID))
                //        {
                //            itemGb.PlanlananDeger = oncekiDonemGirilenHedefler.Where(p => p.VASurecleriMadde.MaddeID == itemGb.VASurecleriMadde.MaddeID && p.VASurecleriBirim.BirimID == itemGb.VASurecleriBirim.BirimID).Select(s => s.PlanlananDegerGelecekYil).FirstOrDefault();
                //        }
                //    }
                //    foreach (var itemMb in eklenecekEBirims)
                //    {
                //        var vaSMadde = vaSurecleriMaddelers.First(p => p.VASurecleriMaddeID == itemMb.VASurecleriMaddeID);
                //        if (pldegerMaddeTurIDs.Contains(vaSMadde.MaddeTurID))
                //        {
                //            itemMb.PlanlananDeger = oncekiDonemGirilenHedefler.Where(p => p.VASurecleriMadde.MaddeID == vaSMadde.MaddeID && p.VASurecleriBirim.BirimID == itemMb.VASurecleriBirim.BirimID).Select(s => s.PlanlananDegerGelecekYil).FirstOrDefault();
                //        }
                //        item.VASurecleriBirimMaddes.Add(itemMb);
                //    }

                //}
                //entities.VASurecleriBirims.RemoveRange(silinecekBirim);
                eoDonem.IslemTarihi = DateTime.Now;
                eoDonem.IslemYapanID = UserIdentity.Current.Id;
                eoDonem.IslemYapanIP = UserIdentity.Ip;
                //entities.SaveChanges();
                //var data = entities.VASurecleriMaddeGirilenDegers.Where(p => !p.VASurecleriBirim.VASurecleriBirimMaddes.Any(a => a.VASurecleriBirimID == p.VASurecleriBirimID && a.VASurecleriMaddeID == p.VASurecleriMaddeID)).ToList();
                //entities.VASurecleriMaddeGirilenDegers.RemoveRange(data);
                //entities.SaveChanges();
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
            try
            {
                SurecMaddeSenkronizasyonu(kModel.VASurecID);
                //BirimleriVeMaddeleriKopyala(kModel);
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


        public static IDataResult<bool> SurecMaddeSenkronizasyonu(int vaSurecId)
        {
            SurecMaddeGuncellemeleri(vaSurecId);
            SurecEslesenMaddeUpdate(vaSurecId);

            return new DataResult<bool>(true, true);
        }

        public class SurecMaddeGuncellemeBulkModel
        {
            public List<VASurecleriMadde> BulkInsertVaSurecMaddes { get; set; } = new List<VASurecleriMadde>();
            public List<VASurecleriMadde> BulkUpdateVaSurecMaddes { get; set; } = new List<VASurecleriMadde>();
            public List<VASurecleriMadde> BulkDeleteVaSurecMaddes { get; set; } = new List<VASurecleriMadde>();

            //public List<VASurecleriMaddeVeriGirisDonemleri> BulkInsertVASurecleriMaddeVeriGirisDonemleris { get; set; } = new List<VASurecleriMaddeVeriGirisDonemleri>();
            //public List<VASurecleriMaddeVeriGirisDonemleri> BulkUpdateVASurecleriMaddeVeriGirisDonemleris { get; set; } = new List<VASurecleriMaddeVeriGirisDonemleri>();
            //public List<VASurecleriMaddeVeriGirisDonemleri> BulkDeleteVASurecleriMaddeVeriGirisDonemleris { get; set; } = new List<VASurecleriMaddeVeriGirisDonemleri>();

            //public List<VASurecleriMaddeBirim> BulkInsertVASurecleriMaddeBirims { get; set; } = new List<VASurecleriMaddeBirim>();
            //public List<VASurecleriMaddeBirim> BulkUpdateVASurecleriMaddeBirims { get; set; } = new List<VASurecleriMaddeBirim>();
            //public List<VASurecleriMaddeBirim> BulkDeleteVASurecleriMaddeBirims { get; set; } = new List<VASurecleriMaddeBirim>();

        }
        private static void SurecMaddeGuncellemeleri(int vaSurecId)
        {
            using (var entities = new VysDBEntities())
            {

                var bulkInsertModel = new SurecMaddeGuncellemeBulkModel();

                var aktarilacakMaddelers = entities.Maddelers.Where(p => p.IsAktif && p.MaddeTurleri.IsAktif).ToList();
                var vaSurecleriMaddelers = entities.VASurecleriMaddes.Where(p => p.VASurecID == vaSurecId).ToList();

                bulkInsertModel.BulkDeleteVaSurecMaddes.AddRange(vaSurecleriMaddelers.Where(p => aktarilacakMaddelers.Any(a => a.MaddeID == p.MaddeID)).ToList());

                foreach (var itemMadde in aktarilacakMaddelers)
                {
                    var surecMadde = vaSurecleriMaddelers.FirstOrDefault(f => f.MaddeID == itemMadde.MaddeID);
                    if (surecMadde != null)
                    {
                        surecMadde.MaddeKod = itemMadde.MaddeKod;
                        surecMadde.MaddeTurID = itemMadde.MaddeTurID ?? 0;
                        surecMadde.VeriGirisSekliID = itemMadde.VeriGirisSekliID;
                        surecMadde.VeriTipID = itemMadde.VeriTipID;
                        surecMadde.HesaplamaFormulu = itemMadde.HesaplamaFormulu;
                        surecMadde.MaddeYilSonuDegerHesaplamaTipID = itemMadde.MaddeYilSonuDegerHesaplamaTipID ?? 0;
                        surecMadde.IsPlanlananDegerOlacak = itemMadde.MaddeTurleri.IsPlanlananDegerOlacak;
                        surecMadde.IsPlanlananDegerOlacakGelecekYil = itemMadde.MaddeTurleri.IsPlanlananDegerOlacakGelecekYil;
                        surecMadde.IsAktif = true;
                        surecMadde.IslemTarihi = DateTime.Now;
                        surecMadde.IslemYapanID = UserIdentity.Current.Id;
                        surecMadde.IslemYapanIP = UserIdentity.Ip;

                        var maddeBirimleri = itemMadde.BirimMaddeleris.ToList();
                        var surecMaddeBirimleris = surecMadde.VASurecleriMaddeBirims.ToList();

                        foreach (var itemMaddeBirim in maddeBirimleri)
                        {
                            var surecMaddeBirim = surecMaddeBirimleris.FirstOrDefault(f => f.BirimID == itemMaddeBirim.BirimID);

                            if (surecMaddeBirim != null)
                            {
                                //surecMaddeBirim.PlanlananDeger =  ;
                                //surecMaddeBirim.PlanlananDegerGelecekYil =  ;

                                surecMaddeBirim.IslemTarihi = DateTime.Now;
                                surecMaddeBirim.IslemYapanID = UserIdentity.Current.Id;
                                surecMaddeBirim.IslemYapanIP = UserIdentity.Ip;
                            }

                        }

                        var silinecekBirimler = surecMadde.VASurecleriMaddeBirims.Where(p => itemMadde.BirimMaddeleris.All(a => a.BirimID != p.BirimID)).ToList();

                    }
                    else
                    {

                        bulkInsertModel.BulkInsertVaSurecMaddes.Add(new VASurecleriMadde
                        {
                            VASurecID = vaSurecId,
                            MaddeID = itemMadde.MaddeID,
                            MaddeKod = itemMadde.MaddeKod,
                            MaddeTurID = itemMadde.MaddeTurID ?? 0,
                            VeriGirisSekliID = itemMadde.VeriGirisSekliID,
                            VeriTipID = itemMadde.VeriTipID,
                            HesaplamaFormulu = itemMadde.HesaplamaFormulu,
                            MaddeYilSonuDegerHesaplamaTipID = itemMadde.MaddeYilSonuDegerHesaplamaTipID ?? 0,
                            IsPlanlananDegerOlacak = itemMadde.MaddeTurleri.IsPlanlananDegerOlacak,
                            IsPlanlananDegerOlacakGelecekYil = itemMadde.MaddeTurleri.IsPlanlananDegerOlacakGelecekYil,
                            IsAktif = true,
                            IslemTarihi = DateTime.Now,
                            IslemYapanID = UserIdentity.Current.Id,
                            IslemYapanIP = UserIdentity.Ip,

                            VASurecleriMaddeVeriGirisDonemleris = itemMadde.MaddelerVeriGirisDonemleris.Select(s => new VASurecleriMaddeVeriGirisDonemleri { VACokluVeriDonemID = s.VACokluVeriDonemID, IsDosyaYuklensin = s.IsDosyaYuklensin }).ToList(),
                            VASurecleriMaddeBirims = itemMadde.BirimMaddeleris.Select(s => new VASurecleriMaddeBirim { BirimID = s.BirimID, IslemTarihi = DateTime.Now, IslemYapanID = UserIdentity.Current.Id, IslemYapanIP = UserIdentity.Ip }).ToList()

                        });
                    }

                }

                if (bulkInsertModel.BulkInsertVaSurecMaddes.Any())
                {
                    entities.BulkInsert(bulkInsertModel.BulkInsertVaSurecMaddes);

                    var insertedMaddeIds = bulkInsertModel.BulkInsertVaSurecMaddes.Select(s => s.MaddeID).ToList();
                    var insertedMaddes = entities.VASurecleriMaddes.Where(p => p.VASurecID == vaSurecId && insertedMaddeIds.Contains(p.MaddeID))
                        .Select(s => new { s.VASurecleriMaddeID, s.MaddeID });
                    foreach (var itemInsertVaSurecMadde in bulkInsertModel.BulkInsertVaSurecMaddes)
                    {
                        var vasMadde = insertedMaddes.First(f => f.MaddeID == itemInsertVaSurecMadde.MaddeID);
                        itemInsertVaSurecMadde.VASurecleriMaddeVeriGirisDonemleris.ForEach(f => f.VASurecleriMaddeID = vasMadde.VASurecleriMaddeID);
                        itemInsertVaSurecMadde.VASurecleriMaddeBirims.ForEach(f => f.VASurecleriMaddeID = vasMadde.VASurecleriMaddeID);
                    }
                    entities.BulkInsert(bulkInsertModel.BulkInsertVaSurecMaddes.SelectMany(s => s.VASurecleriMaddeVeriGirisDonemleris));
                    entities.BulkInsert(bulkInsertModel.BulkInsertVaSurecMaddes.SelectMany(s => s.VASurecleriMaddeBirims));
                }
                if (bulkInsertModel.BulkDeleteVaSurecMaddes.Any())
                {
                    if (bulkInsertModel.BulkDeleteVaSurecMaddes.Any(a => a.VASurecleriMaddeGirilenDegers.Any() || a.VASurecleriMaddeEklenenAciklamas.Any() || a.VASurecleriMaddeEklenenDosyas.Any()))
                    {
                        foreach (var silinecekVaSurecleriMadde in bulkInsertModel.BulkDeleteVaSurecMaddes)
                        {
                            silinecekVaSurecleriMadde.IsAktif = false;
                            silinecekVaSurecleriMadde.IslemTarihi = DateTime.Now;
                            silinecekVaSurecleriMadde.IslemYapanID = UserIdentity.Current.Id;
                            silinecekVaSurecleriMadde.IslemYapanIP = UserIdentity.Ip;
                        }
                    }
                    else
                    {
                        entities.BulkDelete(bulkInsertModel.BulkDeleteVaSurecMaddes);
                    }
                }

                //entities.SaveChanges();

            }

        }

        private static void SurecEslesenMaddeUpdate(int vaSurecId)
        {
            using (var entities = new VysDBEntities())
            {
                var formulMaddeleri = entities.VASurecleriMaddes
                    .Where(p => p.VASurecID == vaSurecId && p.HesaplamaFormulu != null && p.HesaplamaFormulu != "").ToList();
                var maddeIds = formulMaddeleri.Select(s => s.MaddeID).Distinct().ToList();
                var maddeler = entities.Maddelers.Where(p => maddeIds.Contains(p.MaddeID)).ToList();
                foreach (var itemFormulMadde in formulMaddeleri)
                {
                    var madde = maddeler.First(f => f.MaddeID == itemFormulMadde.MaddeID);
                    entities.VASurecleriMaddeFormulEslesenMaddes.RemoveRange(itemFormulMadde
                        .VASurecleriMaddeFormulEslesenMaddes);

                    var vaSurecleriMaddeFormulEslesen = madde.MaddelerFormulEslesenMaddelers.Select(s => new VASurecleriMaddeFormulEslesenMadde
                    {

                        VASurecleriMaddeID = itemFormulMadde.VASurecleriMaddeID,
                        EslesenVASurecleriMaddeID = formulMaddeleri.Where(p => p.MaddeID == s.EslesenMaddeID).Select(se => se.VASurecleriMaddeID).First()

                    });
                    entities.VASurecleriMaddeFormulEslesenMaddes.AddRange(vaSurecleriMaddeFormulEslesen);

                }

                entities.SaveChanges();
            }
        }
        public static IResult SurecSil(int id)
        {
            using (var entities = new VysDBEntities())
            {
                var mmMessage = new MmMessage
                {
                    Title = "Süreç Silme İşlemi",
                    MessageType = Msgtype.Error
                };
                var vaSureci = entities.VASurecleris.FirstOrDefault(p => p.VASurecID == id);
                if (vaSureci != null)
                {
                    try
                    {

                        var isVeriGirisiVar = entities.VASurecleriMaddes.Any(a => a.VASurecID == id && (
                            a.VASurecleriMaddeGirilenDegers.Any() || a.VASurecleriMaddeEklenenAciklamas.Any() ||
                            a.VASurecleriMaddeEklenenDosyas.Any()));

                        if (!isVeriGirisiVar)
                        {
                            entities.VASurecleris.Remove(vaSureci);
                            entities.SaveChanges();
                            mmMessage.IsSuccess = true;
                            mmMessage.MessageType = Msgtype.Success;
                            mmMessage.Messages.Add("'" + vaSureci.Yil + "' Yılı Süreci Silindi!");
                        }
                        else
                        {
                            mmMessage.IsSuccess = false;
                            mmMessage.Messages.Add("'" + vaSureci.Yil + "' Yılı Süreci için veri girişi yapılan maddeler mevcut. Silme işlemi yapılamaz!");
                        }





                        return new Result(true, mmMessage);
                    }
                    catch (Exception ex)
                    {
                        mmMessage.Messages.Add("'" + vaSureci.Yil + "' Yılı Süreci Silinirken Bir Hata Oluştu! </br> Hata:" + ex.ToExceptionMessage());
                        SistemBilgilendirmeBus.SistemBilgisiKaydet(mmMessage.Messages[0], ObjectExtensions.GetCurrentMethodPath(), BilgiTipi.OnemsizHata);
                        mmMessage.IsSuccess = false;
                        return new Result(false, mmMessage);
                    }
                }
                mmMessage.Messages.Add("Silmek İstediğiniz Süreç Sistemde Bulunamadı!");
                ;
                return new Result(true, mmMessage);

            }
        }
    }
}