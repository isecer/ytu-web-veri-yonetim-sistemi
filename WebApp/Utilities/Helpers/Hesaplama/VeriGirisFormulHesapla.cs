using Database;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Data.Entity;
using WebApp.Models;
using WebApp.Utilities.Extensions;

namespace WebApp.Utilities.Helpers.Hesaplama
{
    public class VeriGirisFormulHesapla
    {
        private List<sp_BirimAgaci_Result> Birimlers { get; set; }

        private List<VASurecleriMadde> VaSurecleriMaddes { get; set; }
        private List<VASurecleriMaddeGirilenDeger> VaSurecleriMaddeGirilenDegers { get; set; }
        private List<int> FilteredMaddeIds { get; set; } = new List<int>();

        public VeriGirisFormulHesapla(int vaSurecId, List<int> maddeIds)
        {
            using (var entities = new VysDBEntities())
            {
                maddeIds = maddeIds ?? new List<int>();
                FilteredMaddeIds.AddRange(maddeIds);

                var formulMaddes = (from vaSurecleriMadde in entities.VASurecleriMaddes
                                    join vaSurecleriMaddeFormulEslesen in entities.VASurecleriMaddeFormulEslesenMaddes on
                                        vaSurecleriMadde.VASurecleriMaddeID equals vaSurecleriMaddeFormulEslesen.VASurecleriMaddeID
                                    where maddeIds.Contains(vaSurecleriMadde.MaddeID)
                                    select new
                                    {
                                        vaSurecleriMadde.MaddeID,
                                        EslesenMaddeId = vaSurecleriMaddeFormulEslesen.VASurecleriMadde1.MaddeID,

                                    }).Distinct().ToList();
                foreach (var formulMadde in formulMaddes)
                {
                    maddeIds.Add(formulMadde.MaddeID);
                    maddeIds.Add(formulMadde.EslesenMaddeId);
                }
                VaSurecleriMaddes = entities.VASurecleriMaddes.Where(p => maddeIds.Contains(p.MaddeID) && p.VASurecID == vaSurecId && p.IsAktif)
                    .Include(inc => inc.VASurecleri)
                    .Include(inc => inc.Maddeler)
                    .Include(inc => inc.MaddeTurleri)
                    .Include(inc => inc.MaddeYilSonuDegerHesaplamaTipleri)
                    .Include(inc => inc.VASurecleriMaddeBirims)
                    .Include(inc => inc.VASurecleriMaddeVeriGirisDonemleris)
                    .Include(inc => inc.VASurecleriMaddeFormulEslesenMaddes).ToList();
                VaSurecleriMaddeGirilenDegers = entities.VASurecleriMaddeGirilenDegers.Where(p => p.VASurecleriMadde.VASurecID == vaSurecId).ToList();
                Birimlers = entities.sp_BirimAgaci().ToList();
            }
        }

        private List<RaporExportDto> KurumsalRaporExportDtos { get; set; } = new List<RaporExportDto>();

        public IEnumerable<RaporExportDto> HesaplaFormulMaddeleri()
        {

            var formulMaddeHesaplamalari = FormulMaddeleriHesapla();
            KurumsalRaporExportDtos.AddRange(formulMaddeHesaplamalari);
            return KurumsalRaporExportDtos;
        }
        private IEnumerable<RaporExportDto> FormulMaddeleriHesapla()
        {
            var returnData = new List<RaporExportDto>();

            var hesaplanacakMaddeler = VaSurecleriMaddes.Where(p => FilteredMaddeIds.Contains(p.MaddeID) && p.VASurecleriMaddeFormulEslesenMaddes.Any()).ToList();
            foreach (var itemMadde in hesaplanacakMaddeler)
            {

                var hesaplananFormulMaddeleri = new List<RaporExportDto>();
                foreach (var itemFmadde in itemMadde.VASurecleriMaddeFormulEslesenMaddes)
                {
                    var formuMaddesi = VaSurecleriMaddes.First(f =>
                        f.VASurecleriMaddeID == itemFmadde.EslesenVASurecleriMaddeID);
                    var hesaplananMadde = MaddeHesap(formuMaddesi);
                    hesaplananFormulMaddeleri.Add(hesaplananMadde);
                }
                var hesaplananFormumMaddesi = FormulMaddesiHesapla(itemMadde, hesaplananFormulMaddeleri);
                returnData.Add(hesaplananFormumMaddesi);
            }

            return returnData;
        }




        private RaporExportDto FormulMaddesiHesapla(VASurecleriMadde surecMadde, List<RaporExportDto> hesaplananFormulMaddeleri)
        {
            var surecMaddeHesaplanan = new RaporExportDto
            {
                Yil = surecMadde.VASurecleri.Yil,
                MaddeKod = surecMadde.MaddeKod,
                MaddeTurAdi = surecMadde.MaddeTurleri.MaddeTurAdi,
                MaddeAdi = surecMadde.Maddeler.MaddeAdi,
                HesaplamaFormulu = surecMadde.HesaplamaFormulu,
                HesaplamaSekli = surecMadde.MaddeYilSonuDegerHesaplamaTipleri.YilSonuDegerHesaplamaAdi
            };
            var vaSurecleriDonemIds = hesaplananFormulMaddeleri.SelectMany(sm => sm.VaSurecleriMaddeGirilenDegers.Select(s => s.VaCokluVeriDonemId)).Distinct().ToList();
            foreach (var itemDonem in vaSurecleriDonemIds)
            {
                var girilenDeger = surecMaddeHesaplanan.VaSurecleriMaddeGirilenDegers.FirstOrDefault(f => f.VaCokluVeriDonemId == itemDonem);
                if (girilenDeger == null)
                {
                    surecMaddeHesaplanan.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMadde.MaddeKod, VaCokluVeriDonemId = itemDonem });
                    girilenDeger = surecMaddeHesaplanan.VaSurecleriMaddeGirilenDegers.First(f => f.VaCokluVeriDonemId == itemDonem);
                }
                var formul = surecMadde.HesaplamaFormulu.ToLower();
                foreach (var itemHesaplananMaddeGirilenDeger in hesaplananFormulMaddeleri.SelectMany(s => s.VaSurecleriMaddeGirilenDegers.Where(p => p.VaCokluVeriDonemId == itemDonem.Value)))
                {
                    formul = formul.Replace("@" + itemHesaplananMaddeGirilenDeger.MaddeKod.ToLower(), itemHesaplananMaddeGirilenDeger.GirilenDeger.Value.ToString());
                }
                try
                {
                    girilenDeger.VeriGirisiOnaylandi= hesaplananFormulMaddeleri.SelectMany(s=>s.VaSurecleriMaddeGirilenDegers).Where(p=>p.VaCokluVeriDonemId==itemDonem).All(a=>a.VeriGirisiOnaylandi);
                    var hesaplanmayanMaddeKodlari = formul.ToMaddeKodlariniBul();
                    if (!hesaplanmayanMaddeKodlari.Any())
                        girilenDeger.GirilenDeger = (decimal?)formul.EvaluateExpression();
                    else surecMaddeHesaplanan.BilgiMesaji = string.Join(",", hesaplanmayanMaddeKodlari) + " madde verileri bulunamadı! ";
                }
                catch (Exception e)
                {
                    surecMaddeHesaplanan.BilgiMesaji = (surecMaddeHesaplanan.BilgiMesaji.IsNullOrEmpty() ? e.ToExceptionMessage() : "," + e.ToExceptionMessage());
                }
            }

            if (surecMaddeHesaplanan.BilgiMesaji.IsNullOrWhiteSpace())
            {
                if (surecMadde.IsPlanlananDegerOlacak)
                {
                    if (hesaplananFormulMaddeleri.All(a => a.PlanlananHedef.HasValue))
                    {
                        var formul = surecMadde.HesaplamaFormulu;
                        foreach (var itemHesaplananMadde in hesaplananFormulMaddeleri)
                        {
                            formul = formul.Replace("@" + itemHesaplananMadde.MaddeKod,
                                itemHesaplananMadde.PlanlananHedef.Value.ToString());
                        }

                        try
                        {
                            surecMaddeHesaplanan.PlanlananHedef = (decimal?)formul.EvaluateExpression();
                        }
                        catch (Exception)
                        {
                            surecMaddeHesaplanan.BilgiMesaji += "PlanlananHedef bilgisi için (" + formul + ") formülü hesaplanamadı";
                        }

                    }
                    else surecMaddeHesaplanan.BilgiMesaji += "PlanlananHedef bilgisi eksik olduğu için hesaplanmadı";
                }
                else surecMaddeHesaplanan.BilgiMesaji += "PlanlananHedef istenmiyor";

                if (surecMadde.IsPlanlananDegerOlacakGelecekYil)
                {
                    if (hesaplananFormulMaddeleri.All(a => a.PlanlananVeriGelecekYil.HasValue))
                    {

                        var formul = surecMadde.HesaplamaFormulu;
                        foreach (var itemHesaplananMadde in hesaplananFormulMaddeleri)
                        {
                            formul = formul.Replace("@" + itemHesaplananMadde.MaddeKod,
                                itemHesaplananMadde.PlanlananVeriGelecekYil.Value.ToString());
                        }
                        try
                        {
                            surecMaddeHesaplanan.PlanlananVeriGelecekYil = (decimal?)formul.EvaluateExpression();
                        }
                        catch (Exception)
                        {
                            surecMaddeHesaplanan.BilgiMesaji += "PlanlananVeriGelecekYil bilgisi için (" + formul + ") formülü hesaplanamadı";
                        }

                    }
                    else surecMaddeHesaplanan.BilgiMesaji += (surecMaddeHesaplanan.BilgiMesaji.IsNullOrWhiteSpace() ? "" : ",") + "PlanlananVeriGelecekYil bilgisi eksik olduğu için hesaplanmadı";
                }
                else surecMaddeHesaplanan.BilgiMesaji += (surecMaddeHesaplanan.BilgiMesaji.IsNullOrWhiteSpace() ? "" : ",") + "PlanlananVeriGelecekYil istenmiyor";

                surecMaddeHesaplanan.Ocak = surecMaddeHesaplanan.VaSurecleriMaddeGirilenDegers.FirstOrDefault(f => f.VaCokluVeriDonemId == VaCokluVeriDonemEnum.Ocak)?.GirilenDeger;
                surecMaddeHesaplanan.Subat = surecMaddeHesaplanan.VaSurecleriMaddeGirilenDegers.FirstOrDefault(f => f.VaCokluVeriDonemId == VaCokluVeriDonemEnum.Subat)?.GirilenDeger;
                surecMaddeHesaplanan.Mart = surecMaddeHesaplanan.VaSurecleriMaddeGirilenDegers.FirstOrDefault(f => f.VaCokluVeriDonemId == VaCokluVeriDonemEnum.Mart)?.GirilenDeger;
                surecMaddeHesaplanan.Nisan = surecMaddeHesaplanan.VaSurecleriMaddeGirilenDegers.FirstOrDefault(f => f.VaCokluVeriDonemId == VaCokluVeriDonemEnum.Nisan)?.GirilenDeger;
                surecMaddeHesaplanan.Mayis = surecMaddeHesaplanan.VaSurecleriMaddeGirilenDegers.FirstOrDefault(f => f.VaCokluVeriDonemId == VaCokluVeriDonemEnum.Mayis)?.GirilenDeger;
                surecMaddeHesaplanan.Haziran = surecMaddeHesaplanan.VaSurecleriMaddeGirilenDegers.FirstOrDefault(f => f.VaCokluVeriDonemId == VaCokluVeriDonemEnum.Haziran)?.GirilenDeger;
                surecMaddeHesaplanan.Temmuz = surecMaddeHesaplanan.VaSurecleriMaddeGirilenDegers.FirstOrDefault(f => f.VaCokluVeriDonemId == VaCokluVeriDonemEnum.Temmuz)?.GirilenDeger;
                surecMaddeHesaplanan.Agustos = surecMaddeHesaplanan.VaSurecleriMaddeGirilenDegers.FirstOrDefault(f => f.VaCokluVeriDonemId == VaCokluVeriDonemEnum.Agustos)?.GirilenDeger;
                surecMaddeHesaplanan.Eylul = surecMaddeHesaplanan.VaSurecleriMaddeGirilenDegers.FirstOrDefault(f => f.VaCokluVeriDonemId == VaCokluVeriDonemEnum.Eylul)?.GirilenDeger;
                surecMaddeHesaplanan.Ekim = surecMaddeHesaplanan.VaSurecleriMaddeGirilenDegers.FirstOrDefault(f => f.VaCokluVeriDonemId == VaCokluVeriDonemEnum.Ekim)?.GirilenDeger;
                surecMaddeHesaplanan.Kasim = surecMaddeHesaplanan.VaSurecleriMaddeGirilenDegers.FirstOrDefault(f => f.VaCokluVeriDonemId == VaCokluVeriDonemEnum.Kasim)?.GirilenDeger;
                surecMaddeHesaplanan.Aralik = surecMaddeHesaplanan.VaSurecleriMaddeGirilenDegers.FirstOrDefault(f => f.VaCokluVeriDonemId == VaCokluVeriDonemEnum.Aralik)?.GirilenDeger;
                surecMaddeHesaplanan.Guz = surecMaddeHesaplanan.VaSurecleriMaddeGirilenDegers.FirstOrDefault(f => f.VaCokluVeriDonemId == VaCokluVeriDonemEnum.Guz)?.GirilenDeger;
                surecMaddeHesaplanan.Bahar = surecMaddeHesaplanan.VaSurecleriMaddeGirilenDegers.FirstOrDefault(f => f.VaCokluVeriDonemId == VaCokluVeriDonemEnum.Bahar)?.GirilenDeger;
                surecMaddeHesaplanan.Yaz = surecMaddeHesaplanan.VaSurecleriMaddeGirilenDegers.FirstOrDefault(f => f.VaCokluVeriDonemId == VaCokluVeriDonemEnum.Yaz)?.GirilenDeger;
                surecMaddeHesaplanan.YillikVeri = surecMaddeHesaplanan.VaSurecleriMaddeGirilenDegers.FirstOrDefault(f => f.VaCokluVeriDonemId == VaCokluVeriDonemEnum.Yillik)?.GirilenDeger;
                surecMaddeHesaplanan.HesaplamaSonucu = surecMaddeHesaplanan.VaSurecleriMaddeGirilenDegers.FirstOrDefault(f => f.VaCokluVeriDonemId == VaCokluVeriDonemEnum.HesaplamaSonucu)?.GirilenDeger;
            }

            var birimIds = surecMadde.VASurecleriMaddeBirims.Select(s => s.BirimID).ToList();
            surecMaddeHesaplanan.BirimAdi = string.Join(",\r\n",
                Birimlers.Where(p => birimIds.Contains(p.BirimID ?? 0)).Select(s => s.BirimTreeAdi).Distinct().ToList());


            return surecMaddeHesaplanan;

        }

        private RaporExportDto MaddeHesap(VASurecleriMadde surecMadde)
        {
            var returnData = new RaporExportDto
            {
                Yil = surecMadde.VASurecleri.Yil,
                MaddeKod = surecMadde.MaddeKod,
                MaddeTurAdi = surecMadde.MaddeTurleri.MaddeTurAdi,
                MaddeAdi = surecMadde.Maddeler.MaddeAdi,
                Ocak = MaddeDonemVerisiHesap(surecMadde, VaCokluVeriDonemEnum.Ocak),
                Subat = MaddeDonemVerisiHesap(surecMadde, VaCokluVeriDonemEnum.Subat),
                Mart = MaddeDonemVerisiHesap(surecMadde, VaCokluVeriDonemEnum.Mart),
                Nisan = MaddeDonemVerisiHesap(surecMadde, VaCokluVeriDonemEnum.Nisan),
                Mayis = MaddeDonemVerisiHesap(surecMadde, VaCokluVeriDonemEnum.Mayis),
                Haziran = MaddeDonemVerisiHesap(surecMadde, VaCokluVeriDonemEnum.Haziran),
                Temmuz = MaddeDonemVerisiHesap(surecMadde, VaCokluVeriDonemEnum.Temmuz),
                Agustos = MaddeDonemVerisiHesap(surecMadde, VaCokluVeriDonemEnum.Agustos),
                Eylul = MaddeDonemVerisiHesap(surecMadde, VaCokluVeriDonemEnum.Eylul),
                Ekim = MaddeDonemVerisiHesap(surecMadde, VaCokluVeriDonemEnum.Ekim),
                Kasim = MaddeDonemVerisiHesap(surecMadde, VaCokluVeriDonemEnum.Kasim),
                Aralik = MaddeDonemVerisiHesap(surecMadde, VaCokluVeriDonemEnum.Aralik),
                Guz = MaddeDonemVerisiHesap(surecMadde, VaCokluVeriDonemEnum.Guz),
                Bahar = MaddeDonemVerisiHesap(surecMadde, VaCokluVeriDonemEnum.Bahar),
                Yaz = MaddeDonemVerisiHesap(surecMadde, VaCokluVeriDonemEnum.Yaz),
                YillikVeri = MaddeDonemVerisiHesap(surecMadde, VaCokluVeriDonemEnum.Yillik),
                HesaplamaSekli = surecMadde.MaddeYilSonuDegerHesaplamaTipleri.YilSonuDegerHesaplamaAdi,
                PlanlananHedef = MaddePlanlananDegerHesap(surecMadde, true),
                PlanlananVeriGelecekYil = MaddePlanlananDegerHesap(surecMadde, false)
            };
            if (returnData.Ocak.HasValue)
            {
                returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Ocak, GirilenDeger = returnData.Ocak.Value, VeriGirisiOnaylandi = VaSurecleriMaddeGirilenDegers.Where(p => p.VASurecleriMaddeID == surecMadde.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Ocak && p.VeriGirisiOnaylandi).All(a => a.VeriGirisiOnaylandi) });
            }
            if (returnData.Subat.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Subat, GirilenDeger = returnData.Subat.Value, VeriGirisiOnaylandi = VaSurecleriMaddeGirilenDegers.Where(p => p.VASurecleriMaddeID == surecMadde.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Ocak).All(a => a.VeriGirisiOnaylandi) }); }
            if (returnData.Mart.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Mart, GirilenDeger = returnData.Mart.Value, VeriGirisiOnaylandi = VaSurecleriMaddeGirilenDegers.Where(p => p.VASurecleriMaddeID == surecMadde.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Ocak).All(a => a.VeriGirisiOnaylandi) }); }
            if (returnData.Nisan.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Nisan, GirilenDeger = returnData.Nisan.Value, VeriGirisiOnaylandi = VaSurecleriMaddeGirilenDegers.Where(p => p.VASurecleriMaddeID == surecMadde.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Ocak).All(a => a.VeriGirisiOnaylandi) }); }
            if (returnData.Mayis.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Mayis, GirilenDeger = returnData.Mayis.Value, VeriGirisiOnaylandi = VaSurecleriMaddeGirilenDegers.Where(p => p.VASurecleriMaddeID == surecMadde.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Ocak).All(a => a.VeriGirisiOnaylandi) }); }
            if (returnData.Haziran.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Haziran, GirilenDeger = returnData.Haziran.Value, VeriGirisiOnaylandi = VaSurecleriMaddeGirilenDegers.Where(p => p.VASurecleriMaddeID == surecMadde.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Ocak).All(a => a.VeriGirisiOnaylandi) }); }
            if (returnData.Temmuz.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Temmuz, GirilenDeger = returnData.Temmuz.Value, VeriGirisiOnaylandi = VaSurecleriMaddeGirilenDegers.Where(p => p.VASurecleriMaddeID == surecMadde.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Ocak).All(a => a.VeriGirisiOnaylandi) }); }
            if (returnData.Agustos.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Agustos, GirilenDeger = returnData.Agustos.Value, VeriGirisiOnaylandi = VaSurecleriMaddeGirilenDegers.Where(p => p.VASurecleriMaddeID == surecMadde.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Ocak).All(a => a.VeriGirisiOnaylandi) }); }
            if (returnData.Eylul.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Eylul, GirilenDeger = returnData.Eylul.Value, VeriGirisiOnaylandi = VaSurecleriMaddeGirilenDegers.Where(p => p.VASurecleriMaddeID == surecMadde.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Ocak).All(a => a.VeriGirisiOnaylandi) }); }
            if (returnData.Ekim.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Ekim, GirilenDeger = returnData.Ekim.Value, VeriGirisiOnaylandi = VaSurecleriMaddeGirilenDegers.Where(p => p.VASurecleriMaddeID == surecMadde.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Ocak).All(a => a.VeriGirisiOnaylandi) }); }
            if (returnData.Kasim.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Kasim, GirilenDeger = returnData.Kasim.Value, VeriGirisiOnaylandi = VaSurecleriMaddeGirilenDegers.Where(p => p.VASurecleriMaddeID == surecMadde.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Ocak).All(a => a.VeriGirisiOnaylandi) }); }
            if (returnData.Aralik.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Aralik, GirilenDeger = returnData.Aralik.Value, VeriGirisiOnaylandi = VaSurecleriMaddeGirilenDegers.Where(p => p.VASurecleriMaddeID == surecMadde.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Ocak).All(a => a.VeriGirisiOnaylandi) }); }
            if (returnData.Guz.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Guz, GirilenDeger = returnData.Guz.Value, VeriGirisiOnaylandi = VaSurecleriMaddeGirilenDegers.Where(p => p.VASurecleriMaddeID == surecMadde.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Ocak).All(a => a.VeriGirisiOnaylandi) }); }
            if (returnData.Bahar.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Bahar, GirilenDeger = returnData.Bahar.Value, VeriGirisiOnaylandi = VaSurecleriMaddeGirilenDegers.Where(p => p.VASurecleriMaddeID == surecMadde.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Ocak).All(a => a.VeriGirisiOnaylandi) }); }
            if (returnData.Yaz.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Yaz, GirilenDeger = returnData.Yaz.Value, VeriGirisiOnaylandi = VaSurecleriMaddeGirilenDegers.Where(p => p.VASurecleriMaddeID == surecMadde.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Ocak).All(a => a.VeriGirisiOnaylandi) }); }
            if (returnData.YillikVeri.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Yillik, GirilenDeger = returnData.YillikVeri.Value, VeriGirisiOnaylandi = VaSurecleriMaddeGirilenDegers.Where(p => p.VASurecleriMaddeID == surecMadde.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Ocak).All(a => a.VeriGirisiOnaylandi) }); }

            if (surecMadde.MaddeYilSonuDegerHesaplamaTipID == MaddeYilSonuDegerHesaplamaTip.Kümülatif)
                returnData.HesaplamaSonucu = returnData.VaSurecleriMaddeGirilenDegers.Sum(s => s.GirilenDeger.Value);

            else if (surecMadde.MaddeYilSonuDegerHesaplamaTipID == MaddeYilSonuDegerHesaplamaTip.SonAy)
                returnData.HesaplamaSonucu = returnData.VaSurecleriMaddeGirilenDegers.LastOrDefault()?.GirilenDeger;
            else
            {
                var toplamGirilenDeger = returnData.VaSurecleriMaddeGirilenDegers.Sum(s => s.GirilenDeger ?? 0);

                returnData.HesaplamaSonucu = toplamGirilenDeger == 0 ? 0 : (toplamGirilenDeger / returnData.VaSurecleriMaddeGirilenDegers.Count);
            }
            returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.HesaplamaSonucu, GirilenDeger = returnData.HesaplamaSonucu.Value });

            var birimIds = VaSurecleriMaddeGirilenDegers
                .Where(p => p.IsVeriVar == true && p.GirilenDeger.HasValue &&
                            p.VASurecleriMaddeID == surecMadde.VASurecleriMaddeID).Select(s => s.BirimID).Distinct()
                .ToList();
            returnData.BirimAdi = string.Join(",\r\n",
                Birimlers.Where(p => birimIds.Contains(p.BirimID ?? 0)).Select(s => s.BirimTreeAdi).Distinct().ToList());

            if (returnData.BirimAdi.IsNullOrWhiteSpace())
            {

            }
            return returnData;
        }

        private decimal? MaddePlanlananDegerHesap(VASurecleriMadde surecMadde, bool isBuYilOrGelecelYil)
        {
            // planlanan değer bilgisi olmayacaksa null dön
            if (isBuYilOrGelecelYil ? !surecMadde.IsPlanlananDegerOlacak : !surecMadde.IsPlanlananDegerOlacakGelecekYil) return null;

            var planlananDegerlers = surecMadde.VASurecleriMaddeBirims.Where(p => isBuYilOrGelecelYil ? p.PlanlananDeger.HasValue : p.PlanlananDegerGelecekYil.HasValue).ToList();
            var toplamGirilenDeger = planlananDegerlers.Sum(s =>
                isBuYilOrGelecelYil ? s.PlanlananDeger.Value : s.PlanlananDegerGelecekYil.Value);
            if (surecMadde.MaddeYilSonuDegerHesaplamaTipID == MaddeYilSonuDegerHesaplamaTip.Kümülatif ||
                surecMadde.MaddeYilSonuDegerHesaplamaTipID == MaddeYilSonuDegerHesaplamaTip.SonAy)
            {
                return toplamGirilenDeger;
            }

            if (toplamGirilenDeger == 0) return 0;
            return toplamGirilenDeger / planlananDegerlers.Select(s => s.BirimID).Distinct().Count();

        }
        private decimal? MaddeDonemVerisiHesap(VASurecleriMadde surecMadde, int? vaCokluVeriDonemId)
        {
            // ay bazında veri varsa tüm birimleride ay bazında veriyi hesaplama tipine göre hesapla
            if (surecMadde.VASurecleriMaddeVeriGirisDonemleris.All(a => a.VACokluVeriDonemID != vaCokluVeriDonemId)) return null;

            var maddeDonemindeTumGirilenDegers = VaSurecleriMaddeGirilenDegers.Where(p => p.IsVeriVar == true && p.GirilenDeger.HasValue && p.VASurecleriMaddeID == surecMadde.VASurecleriMaddeID && p.VACokluVeriDonemID == vaCokluVeriDonemId).ToList();

            if (surecMadde.MaddeYilSonuDegerHesaplamaTipID == MaddeYilSonuDegerHesaplamaTip.Kümülatif ||
                surecMadde.MaddeYilSonuDegerHesaplamaTipID == MaddeYilSonuDegerHesaplamaTip.SonAy)
            {
                return maddeDonemindeTumGirilenDegers.Sum(s => s.GirilenDeger.Value);
            }

            var toplamGirilenDeger = maddeDonemindeTumGirilenDegers.Sum(s => s.GirilenDeger.Value);
            if (toplamGirilenDeger == 0) return 0;
            return toplamGirilenDeger / maddeDonemindeTumGirilenDegers.Select(s => s.BirimID).Distinct().Count();

        }

    }
}