using BiskaUtil;
using Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebApp.Models;
using WebApp.Utilities.Extensions;
using WebApp.Utilities.Helpers;
using WebApp.Utilities.MessageBox;
using WebApp.Utilities.Results;
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
                    mdl.MaddeTurleris = (from vaSurecleriMaddeTur in entities.VASurecleriMaddeTurs.Where(p => p.VASurecID == id)
                                         select new VaSurecleriMaddeTurDto
                                         {
                                             VASurecleriMaddeTurID = vaSurecleriMaddeTur.VASurecleriMaddeTurID,
                                             MaddeTurID = vaSurecleriMaddeTur.MaddeTurID,
                                             MaddeTurAdi = vaSurecleriMaddeTur.MaddeTurleri.MaddeTurAdi,
                                             IslemTarihi = vaSurecleriMaddeTur.IslemTarihi,
                                             IsVeriGirisiAcik = vaSurecleriMaddeTur.IsVeriGirisiAcik,
                                             ToplamMaddeCount = entities.VASurecleriMaddes.Count(p => p.VASurecID == id && p.MaddeTurID == vaSurecleriMaddeTur.MaddeTurID && p.IsAktif),

                                         }).OrderBy(o => o.MaddeTurAdi).ToList();

                    return ViewRenderHelper.RenderPartialView("SurecIslemleri", "ViewMaddeTurleri", mdl);
                }
                if (tbInx == 2)
                {
                    mdl.SBirimDurumId = selectDurumId;
                    var mBirimIDs = entities.VASurecleriMaddeBirims.Where(p => p.VASurecleriMadde.VASurecID == id && p.VASurecleriMadde.IsAktif).Select(s => s.BirimID).Distinct().ToList();
                    var birimDataQuery =
                        (from s in entities.Vw_BirimlerTree.Where(p => mBirimIDs.Contains(p.BirimID))
                         select new FrBirimler
                         {
                             BirimID = s.BirimID,
                             BirimKod = s.BirimKod,
                             BirimAdi = s.BirimTreeAdi,
                             IslemTarihi = s.IslemTarihi,
                             ToplamCount = entities.VASurecleriMaddeBirims.Count(p => p.VASurecleriMadde.VASurecID == id && p.BirimID == s.BirimID && p.VASurecleriMadde.IsAktif),
                             TamamlananCount = entities.VASurecleriMaddeBirims.Count(p => p.VASurecleriMadde.VASurecID == id && p.BirimID == s.BirimID && p.VASurecleriMadde.IsAktif && p.VASurecleriMadde.VASurecleriMaddeGirilenDegers.Count(p2 => p2.BirimID == s.BirimID && p2.IsVeriVar.HasValue) == p.VASurecleriMadde.VASurecleriMaddeVeriGirisDonemleris.Count),
                             OnaylananCount = entities.VASurecleriMaddeBirims.Count(p => p.VASurecleriMadde.VASurecID == id && p.BirimID == s.BirimID && p.VASurecleriMadde.IsAktif && p.VASurecleriMadde.VASurecleriMaddeGirilenDegers.Count(p2 => p2.BirimID == s.BirimID && p2.IsVeriVar.HasValue && p2.VeriGirisiOnaylandi) == p.VASurecleriMadde.VASurecleriMaddeVeriGirisDonemleris.Count)

                         });
                    if (mdl.SBirimDurumId == 0) birimDataQuery = birimDataQuery.Where(p => p.ToplamCount != p.TamamlananCount);
                    else if (mdl.SBirimDurumId == 1) birimDataQuery = birimDataQuery.Where(p => p.ToplamCount == p.TamamlananCount);
                    else if (mdl.SBirimDurumId == 2) birimDataQuery = birimDataQuery.Where(p => p.ToplamCount == p.OnaylananCount);
                    else if (mdl.SBirimDurumId == 3) birimDataQuery = birimDataQuery.Where(p => p.ToplamCount != p.OnaylananCount);

                    mdl.BirimData = birimDataQuery.OrderBy(o => o.BirimAdi).ToList();
                    mdl.SListBirimDurum = new SelectList(ComboData.CmbSurecBirimDurum(), "Value", "Caption", mdl.SBirimDurumId);
                    return ViewRenderHelper.RenderPartialView("SurecIslemleri", "ViewBirimler", mdl);
                }
                mdl.SMaddeDurumId = selectDurumId;
                var maddeDataQuery =
                                (from s in entities.VASurecleriMaddes.Where(p => p.VASurecID == id && p.IsAktif)
                                 join bt in entities.Maddelers on s.MaddeID equals bt.MaddeID
                                 select new FrMaddeler
                                 {
                                     MaddeID = s.MaddeID,
                                     MaddeKod = s.MaddeKod,
                                     MaddeAdi = bt.MaddeAdi,
                                     ToplamCount = s.VASurecleriMaddeBirims.Count(p => p.VASurecleriMadde.IsAktif),
                                     TamamlananCount = s.VASurecleriMaddeBirims.Count(p => p.VASurecleriMadde.IsAktif && p.VASurecleriMadde.VASurecleriMaddeGirilenDegers.Count(p2 => p2.BirimID == p.BirimID && p2.IsVeriVar.HasValue) == p.VASurecleriMadde.VASurecleriMaddeVeriGirisDonemleris.Count),
                                     OnaylananCount = s.VASurecleriMaddeBirims.Count(p => p.VASurecleriMadde.IsAktif && p.VASurecleriMadde.VASurecleriMaddeGirilenDegers.Count(p2 => p2.BirimID == p.BirimID && p2.IsVeriVar.HasValue && p2.VeriGirisiOnaylandi) == p.VASurecleriMadde.VASurecleriMaddeVeriGirisDonemleris.Count)

                                 });
                if (mdl.SMaddeDurumId == 0) maddeDataQuery = maddeDataQuery.Where(p => p.ToplamCount != p.TamamlananCount);
                else if (mdl.SMaddeDurumId == 1) maddeDataQuery = maddeDataQuery.Where(p => p.ToplamCount == p.TamamlananCount);
                else if (mdl.SMaddeDurumId == 2) maddeDataQuery = maddeDataQuery.Where(p => p.ToplamCount == p.OnaylananCount);
                else if (mdl.SMaddeDurumId == 3) maddeDataQuery = maddeDataQuery.Where(p => p.ToplamCount != p.OnaylananCount);
                mdl.MaddeData = maddeDataQuery.OrderBy(o => o.MaddeAdi).ToList();
                mdl.SListMaddeDurum = new SelectList(ComboData.CmbSurecMaddeDurum(), "Value", "Caption", mdl.SMaddeDurumId);

                return ViewRenderHelper.RenderPartialView("SurecIslemleri", "ViewMaddeler", mdl);

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
                                          GirilenVeriSayisi = vsmd.VASurecleriMaddeGirilenDegers.Count(p => p.BirimID == birimId && p.IsVeriVar.HasValue),
                                          OnaylananVeriSayisi = vsmd.VASurecleriMaddeGirilenDegers.Count(p => p.BirimID == birimId && p.IsVeriVar.HasValue && p.VeriGirisiOnaylandi),
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
                                          TamamlananCount = s2.VASurecleriMadde.VASurecleriMaddeBirims.Count(p => p.VASurecleriMadde.VASurecleriMaddeGirilenDegers.Count(p2 => p2.BirimID == br.BirimID && p2.IsVeriVar.HasValue) == p.VASurecleriMadde.VASurecleriMaddeVeriGirisDonemleris.Count),
                                          OnaylananCount = s2.VASurecleriMadde.VASurecleriMaddeBirims.Count(p => p.VASurecleriMadde.VASurecleriMaddeGirilenDegers.Count(p2 => p2.BirimID == br.BirimID && p2.IsVeriVar.HasValue && p2.VeriGirisiOnaylandi) == p.VASurecleriMadde.VASurecleriMaddeVeriGirisDonemleris.Count)
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

                if (kModel.VASurecID <= 0)
                {
                    if (!kModel.MaddeTurIds.Any())
                    {
                        mmMessage.Messages.Add("Sürecin kayıt edilebilmesi için en az 1 Madde Türü seçilmesi gerekmektedir.");
                    }
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
                            IsAktifYilPlanlananVeriGirisiAcik = kModel.IsAktifYilPlanlananVeriGirisiAcik,
                            IsGelecekYilPlanlananVeriGirisiAcik = kModel.IsGelecekYilPlanlananVeriGirisiAcik,
                            IsAktif = kModel.IsAktif,
                            VASurecleriMaddeTurs = kModel.MaddeTurIds.Select(s => new VASurecleriMaddeTur { MaddeTurID = s, IsVeriGirisiAcik = true, IslemTarihi = DateTime.Now, IslemYapanID = UserIdentity.Current.Id, IslemYapanIP = UserIdentity.Ip }).ToList(),
                            IslemTarihi = DateTime.Now,
                            IslemYapanID = UserIdentity.Current.Id,
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
                    if (kModel.VASurecID <= 0)
                    {
                        SurecMaddeSenkronizasyonu(vaSureci.VASurecID, kModel.MaddeTurIds);
                    }
                    kModel.VASurecID = vaSureci.VASurecID;
                    return new DataResult<KmSurecIslemleri>(kModel, true, mmMessage);

                }
                return new DataResult<KmSurecIslemleri>(kModel, false, mmMessage);
            }
        }

        public static IResult SurecMaddeSenkronizasyonu(int vaSurecId, List<int> maddeTurIds)
        {
            var mMessage = new MmMessage
            {
                IsSuccess = false,
                Title = "Süreç Maddeleri Güncelleme ve Kopyalama İşlemi",
                MessageType = Msgtype.Warning
            };
            try
            {
                if (!maddeTurIds.Any())
                {
                    mMessage.Messages.Add("Süreç maddeleri güncellenebilmesi için en az 1 Madde türü seçilmelidir.");
                }
                else
                {
                    SurecMaddeGuncellemeleri(vaSurecId, maddeTurIds);
                    SurecEslesenMaddeUpdate(vaSurecId);
                    SurecMaddeTuruUpdate(vaSurecId);
                    mMessage.IsSuccess = true;
                    mMessage.Messages.Add("Süreç maddeleri güncellendi.");
                    mMessage.MessageType = Msgtype.Success;
                }

            }
            catch (Exception ex)
            {
                mMessage.Messages.Add("Veriler Aktarılırken bir hata oluştu! Hata:" + ex.ToExceptionMessage());
            }
            return new Result(mMessage.IsSuccess, mMessage);

        }



        private static void SurecMaddeGuncellemeleri(int vaSurecId, List<int> maddeTurIds)
        {
            using (var entities = new VysDBEntities())
            {

                var bulkMergeVaSurecMaddes = new List<VASurecleriMadde>();
                var bulkDeleteVaSurecMaddes = new List<VASurecleriMadde>();
                var bulkDeleteVaSurecleriMaddeVeriGirisDonemleris = new List<VASurecleriMaddeVeriGirisDonemleri>();
                var bulkDeleteVaSurecleriMaddeBirims = new List<VASurecleriMaddeBirim>();

                var aktarilacakMaddelers = entities.Maddelers.Where(p => maddeTurIds.Contains(p.MaddeTurID ?? 0) && p.IsAktif && p.MaddeTurleri.IsAktif).ToList();
                var vaSurecleriMaddelers = entities.VASurecleriMaddes.Where(p => p.VASurecID == vaSurecId).ToList();
                var vaSurecleriSecilenMaddeTuruMaddelers = vaSurecleriMaddelers.Where(p => maddeTurIds.Contains(p.MaddeTurID)).ToList();

                bulkDeleteVaSurecMaddes.AddRange(vaSurecleriSecilenMaddeTuruMaddelers.Where(p => aktarilacakMaddelers.All(a => a.MaddeID != p.MaddeID)).ToList());

                var vaSureci = entities.VASurecleris.Where(f => f.VASurecID == vaSurecId)
                  .Select(s => new { s.VASurecID, s.Yil, s.VASurecleriMaddeTurs }).First();
                var oncekiDonemGirilenHedefler = entities.VASurecleriMaddeBirims.Where(p => p.VASurecleriMadde.IsPlanlananDegerOlacak && p.VASurecleriMadde.VASurecleri.Yil == (vaSureci.Yil - 1) && maddeTurIds.Contains(p.VASurecleriMadde.MaddeTurID)).Select(s => new
                {
                    s.BirimID,
                    s.VASurecleriMadde.MaddeID,
                    s.PlanlananDegerGelecekYil,

                }).ToList();

                foreach (var itemAktarilacakMadde in aktarilacakMaddelers)
                {
                    var surecMadde = vaSurecleriMaddelers.FirstOrDefault(f => f.MaddeID == itemAktarilacakMadde.MaddeID);
                    if (surecMadde != null)
                    {
                        surecMadde.MaddeKod = itemAktarilacakMadde.MaddeKod;
                        surecMadde.MaddeTurID = itemAktarilacakMadde.MaddeTurID ?? 0;
                        surecMadde.VeriGirisSekliID = itemAktarilacakMadde.VeriGirisSekliID;
                        surecMadde.VeriTipID = itemAktarilacakMadde.VeriTipID;
                        surecMadde.HesaplamaFormulu = itemAktarilacakMadde.HesaplamaFormulu;
                        surecMadde.MaddeYilSonuDegerHesaplamaTipID = itemAktarilacakMadde.MaddeYilSonuDegerHesaplamaTipID ?? 0;
                        surecMadde.IsPlanlananDegerOlacak = itemAktarilacakMadde.MaddeTurleri.IsPlanlananDegerOlacak;
                        surecMadde.IsPlanlananDegerOlacakGelecekYil = itemAktarilacakMadde.MaddeTurleri.IsPlanlananDegerOlacakGelecekYil;
                        surecMadde.IsAktif = true;
                        surecMadde.IslemTarihi = DateTime.Now;
                        surecMadde.IslemYapanID = UserIdentity.Current.Id;
                        surecMadde.IslemYapanIP = UserIdentity.Ip;


                        bulkDeleteVaSurecleriMaddeBirims.AddRange(surecMadde.VASurecleriMaddeBirims.Where(p => itemAktarilacakMadde.BirimMaddeleris.All(a => a.BirimID != p.BirimID)).ToList());
                        bulkDeleteVaSurecleriMaddeVeriGirisDonemleris.AddRange(surecMadde.VASurecleriMaddeVeriGirisDonemleris.Where(p => itemAktarilacakMadde.MaddelerVeriGirisDonemleris.All(a => a.VACokluVeriDonemID != p.VACokluVeriDonemID)).ToList());

                        surecMadde.VASurecleriMaddeBirims = (from aktarilacakMaddeBirim in itemAktarilacakMadde.BirimMaddeleris
                                                             join surecMaddeBirim in surecMadde.VASurecleriMaddeBirims on new { aktarilacakMaddeBirim.BirimID, aktarilacakMaddeBirim.MaddeID } equals new { surecMaddeBirim.BirimID, surecMadde.MaddeID } into defSmb
                                                             from surecMaddeBirimDef in defSmb.DefaultIfEmpty()
                                                             join oncekiSurecHedef in oncekiDonemGirilenHedefler on new { aktarilacakMaddeBirim.BirimID, aktarilacakMaddeBirim.MaddeID } equals new { oncekiSurecHedef.BirimID, oncekiSurecHedef.MaddeID } into defOsh
                                                             from oncekiSurecHedefDef in defOsh.DefaultIfEmpty()
                                                             select new VASurecleriMaddeBirim
                                                             {
                                                                 VASurecleriMaddeBirimID = surecMaddeBirimDef?.VASurecleriMaddeBirimID ?? 0,
                                                                 VASurecleriMaddeID = surecMadde.VASurecleriMaddeID,
                                                                 BirimID = aktarilacakMaddeBirim.BirimID,
                                                                 PlanlananDeger = surecMaddeBirimDef?.PlanlananDeger ?? oncekiSurecHedefDef?.PlanlananDegerGelecekYil,
                                                                 PlanlananDegerGelecekYil = surecMaddeBirimDef?.PlanlananDegerGelecekYil,
                                                                 IslemTarihi = surecMaddeBirimDef?.IslemTarihi ?? DateTime.Now,
                                                                 IslemYapanID = surecMaddeBirimDef?.IslemYapanID ?? UserIdentity.Current.Id,
                                                                 IslemYapanIP = surecMaddeBirimDef?.IslemYapanIP ?? UserIdentity.Ip
                                                             }).ToList();
                        surecMadde.VASurecleriMaddeVeriGirisDonemleris = (from aktarilacakMaddeDonem in itemAktarilacakMadde.MaddelerVeriGirisDonemleris
                                                                          join surecMaddeDonem in surecMadde.VASurecleriMaddeVeriGirisDonemleris on new { aktarilacakMaddeDonem.VACokluVeriDonemID, aktarilacakMaddeDonem.MaddeID } equals new { surecMaddeDonem.VACokluVeriDonemID, surecMadde.MaddeID } into defSmb
                                                                          from surecMaddeDonemDef in defSmb.DefaultIfEmpty()
                                                                          select new VASurecleriMaddeVeriGirisDonemleri
                                                                          {
                                                                              VASurecleriMaddeVeriGirisDonemID = surecMaddeDonemDef?.VASurecleriMaddeVeriGirisDonemID ?? 0,
                                                                              VASurecleriMaddeID = surecMadde.VASurecleriMaddeID,
                                                                              VACokluVeriDonemID = aktarilacakMaddeDonem.VACokluVeriDonemID,
                                                                              IsDosyaYuklensin = aktarilacakMaddeDonem.IsDosyaYuklensin
                                                                          }).ToList();

                        bulkMergeVaSurecMaddes.Add(surecMadde);


                    }
                    else
                    {

                        bulkMergeVaSurecMaddes.Add(new VASurecleriMadde
                        {
                            VASurecID = vaSurecId,
                            MaddeID = itemAktarilacakMadde.MaddeID,
                            MaddeKod = itemAktarilacakMadde.MaddeKod,
                            MaddeTurID = itemAktarilacakMadde.MaddeTurID ?? 0,
                            VeriGirisSekliID = itemAktarilacakMadde.VeriGirisSekliID,
                            VeriTipID = itemAktarilacakMadde.VeriTipID,
                            HesaplamaFormulu = itemAktarilacakMadde.HesaplamaFormulu,
                            MaddeYilSonuDegerHesaplamaTipID = itemAktarilacakMadde.MaddeYilSonuDegerHesaplamaTipID ?? 0,
                            IsPlanlananDegerOlacak = itemAktarilacakMadde.MaddeTurleri.IsPlanlananDegerOlacak,
                            IsPlanlananDegerOlacakGelecekYil = itemAktarilacakMadde.MaddeTurleri.IsPlanlananDegerOlacakGelecekYil,
                            IsAktif = true,
                            IslemTarihi = DateTime.Now,
                            IslemYapanID = UserIdentity.Current.Id,
                            IslemYapanIP = UserIdentity.Ip,
                            VASurecleriMaddeVeriGirisDonemleris = itemAktarilacakMadde.MaddelerVeriGirisDonemleris.Select(s => new VASurecleriMaddeVeriGirisDonemleri { VACokluVeriDonemID = s.VACokluVeriDonemID, IsDosyaYuklensin = s.IsDosyaYuklensin }).ToList(),
                            VASurecleriMaddeBirims = (from aktarilacakMaddeBirim in itemAktarilacakMadde.BirimMaddeleris
                                                      join oncekiSurecHedef in oncekiDonemGirilenHedefler on new { aktarilacakMaddeBirim.BirimID, aktarilacakMaddeBirim.MaddeID } equals new { oncekiSurecHedef.BirimID, oncekiSurecHedef.MaddeID } into defOsh
                                                      from oncekiSurecHedefDef in defOsh.DefaultIfEmpty()
                                                      select new VASurecleriMaddeBirim
                                                      {
                                                          BirimID = aktarilacakMaddeBirim.BirimID,
                                                          PlanlananDeger = oncekiSurecHedefDef?.PlanlananDegerGelecekYil,
                                                          IslemTarihi = DateTime.Now,
                                                          IslemYapanID = UserIdentity.Current.Id,
                                                          IslemYapanIP = UserIdentity.Ip
                                                      }).ToList()

                        });
                    }

                }



                if (bulkMergeVaSurecMaddes.Any())
                {

                    //Süreç madde eklemeleri ve güncellemeleri
                    entities.BulkMerge(bulkMergeVaSurecMaddes);

                    var insertedMaddeIds = bulkMergeVaSurecMaddes.Select(s => s.MaddeID).ToList();
                    var insertedMaddes = entities.VASurecleriMaddes.Where(p => p.VASurecID == vaSurecId && insertedMaddeIds.Contains(p.MaddeID))
                        .Select(s => new { s.VASurecleriMaddeID, s.MaddeID });

                    foreach (var itemInsertVaSurecMadde in bulkMergeVaSurecMaddes)
                    {
                        var vasMadde = insertedMaddes.First(f => f.MaddeID == itemInsertVaSurecMadde.MaddeID);
                        itemInsertVaSurecMadde.VASurecleriMaddeVeriGirisDonemleris.ForEach(f => f.VASurecleriMaddeID = vasMadde.VASurecleriMaddeID);
                        itemInsertVaSurecMadde.VASurecleriMaddeBirims.ForEach(f => f.VASurecleriMaddeID = vasMadde.VASurecleriMaddeID);
                    }
                    //Süreç madde birimleri ve dönemleri eklemeleri ve güncellemeleri
                    entities.BulkMerge(bulkMergeVaSurecMaddes.SelectMany(s => s.VASurecleriMaddeVeriGirisDonemleris));
                    entities.BulkMerge(bulkMergeVaSurecMaddes.SelectMany(s => s.VASurecleriMaddeBirims));


                }
                if (bulkDeleteVaSurecMaddes.Any())
                {
                    //Süreç madde silme işlemleri
                    if (bulkDeleteVaSurecMaddes.Any(a => a.VASurecleriMaddeGirilenDegers.Any() || a.VASurecleriMaddeEklenenAciklamas.Any() || a.VASurecleriMaddeEklenenDosyas.Any()))
                    {
                        foreach (var silinecekVaSurecleriMadde in bulkDeleteVaSurecMaddes)
                        {
                            silinecekVaSurecleriMadde.IsAktif = false;
                            silinecekVaSurecleriMadde.IslemTarihi = DateTime.Now;
                            silinecekVaSurecleriMadde.IslemYapanID = UserIdentity.Current.Id;
                            silinecekVaSurecleriMadde.IslemYapanIP = UserIdentity.Ip;
                        }
                    }
                    else
                    {
                        entities.BulkDelete(bulkDeleteVaSurecMaddes);
                    }
                }
                if (bulkDeleteVaSurecleriMaddeVeriGirisDonemleris.Any())
                {
                    //Süreç madde güncelleme işlemlerinde dönem bilgileri kaldırılmışsa db den silme işlemi
                    entities.BulkDelete(bulkDeleteVaSurecleriMaddeVeriGirisDonemleris);
                }
                if (bulkDeleteVaSurecleriMaddeBirims.Any())
                {
                    //Süreç madde güncelleme işlemlerinde birim bilgileri kaldırılmışsa db den silme işlemi
                    entities.BulkDelete(bulkDeleteVaSurecleriMaddeBirims);
                }
            }

        }

        private static void SurecMaddeTuruUpdate(int vaSurecId)
        {
            using (var entities = new VysDBEntities())
            {

                var surecMaddeTurIds =
                    entities.VASurecleriMaddes.Where(p => p.VASurecID == vaSurecId).Select(s => s.MaddeTurID).Distinct().ToList();

                var maddeTurleris = entities.MaddeTurleris.Where(p => surecMaddeTurIds.Contains(p.MaddeTurID)).ToList();

                var surecMaddeTurs = entities.VASurecleriMaddeTurs.Where(p => p.VASurecID == vaSurecId).ToList();


                var bulkMergeSurecMaddeTurs = (from maddeTur in maddeTurleris
                                               join surecMaddeTur in surecMaddeTurs on maddeTur.MaddeTurID equals surecMaddeTur.MaddeTurID into defMt
                                               from surecMaddeTurDef in defMt.DefaultIfEmpty()
                                               select new VASurecleriMaddeTur
                                               {
                                                   VASurecleriMaddeTurID = surecMaddeTurDef?.VASurecleriMaddeTurID ?? 0,
                                                   MaddeTurID = maddeTur.MaddeTurID,
                                                   IsVeriGirisiAcik = surecMaddeTurDef?.IsVeriGirisiAcik ?? true,
                                                   VASurecID = vaSurecId,
                                                   IslemTarihi = surecMaddeTurDef?.IslemTarihi ?? DateTime.Now,
                                                   IslemYapanIP = surecMaddeTurDef?.IslemYapanIP ?? UserIdentity.Ip,
                                                   IslemYapanID = surecMaddeTurDef?.IslemYapanID ?? UserIdentity.Current.Id

                                               }).ToList();
                entities.BulkMerge(bulkMergeSurecMaddeTurs);
                var buldkDeleteSurecMaddeTurs = surecMaddeTurs.Where(p => !surecMaddeTurIds.Contains(p.MaddeTurID)).ToList();
                if (buldkDeleteSurecMaddeTurs.Any())
                    entities.BulkDelete(buldkDeleteSurecMaddeTurs);
            }

        }
        private static void SurecEslesenMaddeUpdate(int vaSurecId)
        {
            using (var entities = new VysDBEntities())
            {



                var surecMaddeleri = entities.VASurecleriMaddes
                    .Where(p => p.VASurecID == vaSurecId).Select(s => new
                    {
                        isFormulVar = (s.HesaplamaFormulu != null &&
                        s.HesaplamaFormulu != ""),
                        s.MaddeID,
                        s.VASurecleriMaddeID,
                        s.VASurecleriMaddeFormulEslesenMaddes,
                        SurecMadde = s
                    }).ToList();
                var formulMaddeleri = surecMaddeleri.Where(p => p.isFormulVar).ToList();
                var maddeIds = formulMaddeleri.Select(s => s.MaddeID).Distinct().ToList();
                var maddeler = entities.Maddelers.Where(p => maddeIds.Contains(p.MaddeID)).ToList();
                foreach (var itemFormulMadde in formulMaddeleri)
                {
                    var madde = maddeler.First(f => f.MaddeID == itemFormulMadde.MaddeID);
                    if (madde.MaddelerFormulEslesenMaddelers.Count == madde.MaddelerFormulEslesenMaddelers.Count(a =>
                            surecMaddeleri.Any(p => p.MaddeID == a.EslesenMaddeID)))
                    {
                        entities.VASurecleriMaddeFormulEslesenMaddes.RemoveRange(itemFormulMadde
                            .VASurecleriMaddeFormulEslesenMaddes);

                        var vaSurecleriMaddeFormulEslesen = madde.MaddelerFormulEslesenMaddelers.Select(s => new VASurecleriMaddeFormulEslesenMadde
                        {

                            VASurecleriMaddeID = itemFormulMadde.VASurecleriMaddeID,
                            EslesenVASurecleriMaddeID = surecMaddeleri.Where(p => p.MaddeID == s.EslesenMaddeID).Select(se => se.VASurecleriMaddeID).First()

                        });
                        entities.VASurecleriMaddeFormulEslesenMaddes.AddRange(vaSurecleriMaddeFormulEslesen);
                    }
                    else
                    {
                        entities.VASurecleriMaddes.Remove(surecMaddeleri.First(p => p.MaddeID == madde.MaddeID).SurecMadde);
                    }


                }
                var formulMaddesiOlmayanlar = entities.VASurecleriMaddes
                    .Where(p => p.VASurecID == vaSurecId && (p.HesaplamaFormulu == null || p.HesaplamaFormulu == "")).ToList();
                foreach (var item in formulMaddesiOlmayanlar)
                {
                    entities.VASurecleriMaddeFormulEslesenMaddes.RemoveRange(item.VASurecleriMaddeFormulEslesenMaddes);
                }
                entities.SaveChanges();
            }
        }
        public static IResult MaddeTurIsVeriGirisDurumKayit(int vaSurecId, int maddeTurId, bool isVeriGirisiAcik)
        {
            using (var entities = new VysDBEntities())
            {
                var mmMessage = new MmMessage
                {
                    Title = "Madde Türü Veri Girişi Durum Değişikliği",
                    MessageType = Msgtype.Error
                };
                var vaSurecleriMaddeTur = entities.VASurecleriMaddeTurs.First(p => p.VASurecID == vaSurecId && p.MaddeTurID == maddeTurId);

                try
                {

                    vaSurecleriMaddeTur.IsVeriGirisiAcik = isVeriGirisiAcik;
                    vaSurecleriMaddeTur.IslemTarihi = DateTime.Now;
                    vaSurecleriMaddeTur.IslemYapanID = UserIdentity.Current.Id;
                    vaSurecleriMaddeTur.IslemYapanIP = UserIdentity.Ip;
                    entities.SaveChanges();
                    mmMessage.IsSuccess = true;
                    mmMessage.MessageType = Msgtype.Success;
                    mmMessage.Messages.Add("'" + vaSurecleriMaddeTur.MaddeTurleri.MaddeTurAdi + "' madde türü veri giriş işlemine " + (isVeriGirisiAcik ? "Açık" : "Kapalı") + " durumuna geririldir.!");
                    return new Result(true, mmMessage);
                }
                catch (Exception ex)
                {
                    mmMessage.Messages.Add("'" + vaSurecleriMaddeTur.MaddeTurleri.MaddeTurAdi + "' madde türü için veri giriş durumu güncellenirken bir hata oluştu! </br> Hata:" + ex.ToExceptionMessage());
                    SistemBilgilendirmeBus.SistemBilgisiKaydet(mmMessage.Messages[0], ObjectExtensions.GetCurrentMethodPath(), BilgiTipi.OnemsizHata);
                    mmMessage.IsSuccess = false;
                    return new Result(false, mmMessage);
                }

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
                return new Result(true, mmMessage);

            }
        }
    }
}