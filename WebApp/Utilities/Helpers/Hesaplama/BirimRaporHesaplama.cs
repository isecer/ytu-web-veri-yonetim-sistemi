using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Database;
using WebApp.Models;
using WebApp.Utilities.Extensions;

namespace WebApp.Utilities.Helpers.Hesaplama
{
    public class BirimRaporHesaplama
    {
        private int VaSurecId { get; set; }
        private ICollection<int> RaporTipIds { get; set; }
        private List<sp_BirimAgaci_Result> Birimlers { get; set; }

        private List<VASurecleriMaddeBirim> VaSurecleriMaddeBirims { get; set; }
        private List<VASurecleriMaddeGirilenDeger> VaSurecleriMaddeGirilenDegers { get; set; }
        private readonly List<int> _filteredMaddeIds;
        private readonly int? _birimId;
        public BirimRaporHesaplama(int vaSurecId, ICollection<int> raporTipIds, int? birimId)
        {
            VaSurecId = vaSurecId;
            RaporTipIds = raporTipIds;
            _birimId = birimId;
            using (var entities = new VysDBEntities())
            {
                _filteredMaddeIds = entities.RaporTipleriSecilenMaddelers.Where(p => raporTipIds.Contains(p.RaporTipID)).Select(s => s.MaddeID).ToList();
                VaSurecleriMaddeBirims = entities.VASurecleriMaddeBirims.Where(p => p.VASurecleriMadde.VASurecID == vaSurecId && p.VASurecleriMadde.IsAktif && p.BirimID == (birimId ?? p.BirimID))
                   .Include(inc => inc.VASurecleriMadde)
                   .Include(inc => inc.VASurecleriMadde.VASurecleri)
                   .Include(inc => inc.VASurecleriMadde.Maddeler)
                   .Include(inc => inc.VASurecleriMadde.MaddeTurleri)
                   .Include(inc => inc.VASurecleriMadde.MaddeYilSonuDegerHesaplamaTipleri)
                   .Include(inc => inc.VASurecleriMadde.VASurecleriMaddeVeriGirisDonemleris)
                   .Include(inc => inc.VASurecleriMadde.VASurecleriMaddeFormulEslesenMaddes).ToList();
                VaSurecleriMaddeGirilenDegers = entities.VASurecleriMaddeGirilenDegers.Where(p => p.VASurecleriMadde.VASurecID == vaSurecId).ToList();
                Birimlers = entities.sp_BirimAgaci().ToList();
            }
        }
        public BirimRaporHesaplama(int vaSurecId, int? birimId, List<int> filteredMaddeIds)
        {
            filteredMaddeIds = filteredMaddeIds ?? new List<int>();
            VaSurecId = vaSurecId;
            _birimId = birimId;
            using (var entities = new VysDBEntities())
            {
                VaSurecleriMaddeBirims = entities.VASurecleriMaddeBirims.Where(p => p.VASurecleriMadde.VASurecID == vaSurecId && p.VASurecleriMadde.IsAktif && p.BirimID == (birimId ?? p.BirimID))
                  .Include(inc => inc.VASurecleriMadde)
                  .Include(inc => inc.VASurecleriMadde.VASurecleri)
                  .Include(inc => inc.VASurecleriMadde.Maddeler)
                  .Include(inc => inc.VASurecleriMadde.MaddeTurleri)
                  .Include(inc => inc.VASurecleriMadde.MaddeYilSonuDegerHesaplamaTipleri)
                  .Include(inc => inc.VASurecleriMadde.VASurecleriMaddeVeriGirisDonemleris)
                  .Include(inc => inc.VASurecleriMadde.VASurecleriMaddeFormulEslesenMaddes).ToList();
                VaSurecleriMaddeGirilenDegers = entities.VASurecleriMaddeGirilenDegers.Where(p => p.VASurecleriMadde.VASurecID == vaSurecId).ToList();
                Birimlers = entities.sp_BirimAgaci().ToList();
            }

            if (filteredMaddeIds.Any())
            {
                _filteredMaddeIds = filteredMaddeIds;
            }
        }

        private List<RaporExportDto> BirimRaporExportDtos { get; set; } = new List<RaporExportDto>();
        public IEnumerable<RaporExportDto> Hesapla()
        {

            var normalMaddeHesaplamalari = NormalMaddeleriHesapla();
            BirimRaporExportDtos.AddRange(normalMaddeHesaplamalari);
            var formulMaddeHesaplamalari = FormulMaddeleriHesapla();
            BirimRaporExportDtos.AddRange(formulMaddeHesaplamalari);
            return BirimRaporExportDtos.OrderBy(o => o.BirimAdi).ThenBy(t => t.MaddeAdi);
        }

        private IEnumerable<RaporExportDto> NormalMaddeleriHesapla()
        {
            var returnData = new List<RaporExportDto>();
            foreach (var itemMadde in VaSurecleriMaddeBirims.Where(p => p.BirimID == (_birimId ?? p.BirimID) && _filteredMaddeIds.Contains(p.VASurecleriMadde.MaddeID) && !p.VASurecleriMadde.VASurecleriMaddeFormulEslesenMaddes.Any()))
            {
                returnData.Add(MaddeVeriDoldur(itemMadde));
            }
            return returnData;
        }

        private IEnumerable<RaporExportDto> FormulMaddeleriHesapla()
        {
            if (RaporTipIds != null)
            {
                var krHesap = new KurumsalRaporHesaplama(VaSurecId, RaporTipIds, _birimId);
                var formulMaddeleri = krHesap.HesaplaFormulMaddeleri(_filteredMaddeIds);

                return formulMaddeleri;
            }
            if (_birimId != null && _filteredMaddeIds.Any())
            {
                var krHesap = new KurumsalRaporHesaplama(VaSurecId, _birimId.Value, _filteredMaddeIds);
                var formulMaddeleri = krHesap.HesaplaFormulMaddeleri(_filteredMaddeIds);

                return formulMaddeleri;
            }

            return new List<RaporExportDto>();
        }
        private RaporExportDto MaddeVeriDoldur(VASurecleriMaddeBirim surecMaddeBirim)
        {


            var returnData = new RaporExportDto
            {
                Yil = surecMaddeBirim.VASurecleriMadde.VASurecleri.Yil,
                MaddeKod = surecMaddeBirim.VASurecleriMadde.MaddeKod,
                MaddeTurAdi = surecMaddeBirim.VASurecleriMadde.MaddeTurleri.MaddeTurAdi,
                MaddeAdi = surecMaddeBirim.VASurecleriMadde.Maddeler.MaddeAdi,
                Ocak = VaSurecleriMaddeGirilenDegers.FirstOrDefault(p => p.BirimID == surecMaddeBirim.BirimID && p.IsVeriVar == true && p.GirilenDeger.HasValue && p.VASurecleriMaddeID == surecMaddeBirim.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Ocak)?.GirilenDeger,
                Subat = VaSurecleriMaddeGirilenDegers.FirstOrDefault(p => p.BirimID == surecMaddeBirim.BirimID && p.IsVeriVar == true && p.GirilenDeger.HasValue && p.VASurecleriMaddeID == surecMaddeBirim.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Subat)?.GirilenDeger,
                Mart = VaSurecleriMaddeGirilenDegers.FirstOrDefault(p => p.BirimID == surecMaddeBirim.BirimID && p.IsVeriVar == true && p.GirilenDeger.HasValue && p.VASurecleriMaddeID == surecMaddeBirim.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Mart)?.GirilenDeger,
                Nisan = VaSurecleriMaddeGirilenDegers.FirstOrDefault(p => p.BirimID == surecMaddeBirim.BirimID && p.IsVeriVar == true && p.GirilenDeger.HasValue && p.VASurecleriMaddeID == surecMaddeBirim.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Nisan)?.GirilenDeger,
                Mayis = VaSurecleriMaddeGirilenDegers.FirstOrDefault(p => p.BirimID == surecMaddeBirim.BirimID && p.IsVeriVar == true && p.GirilenDeger.HasValue && p.VASurecleriMaddeID == surecMaddeBirim.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Mayis)?.GirilenDeger,
                Haziran = VaSurecleriMaddeGirilenDegers.FirstOrDefault(p => p.BirimID == surecMaddeBirim.BirimID && p.IsVeriVar == true && p.GirilenDeger.HasValue && p.VASurecleriMaddeID == surecMaddeBirim.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Haziran)?.GirilenDeger,
                Temmuz = VaSurecleriMaddeGirilenDegers.FirstOrDefault(p => p.BirimID == surecMaddeBirim.BirimID && p.IsVeriVar == true && p.GirilenDeger.HasValue && p.VASurecleriMaddeID == surecMaddeBirim.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Temmuz)?.GirilenDeger,
                Agustos = VaSurecleriMaddeGirilenDegers.FirstOrDefault(p => p.BirimID == surecMaddeBirim.BirimID && p.IsVeriVar == true && p.GirilenDeger.HasValue && p.VASurecleriMaddeID == surecMaddeBirim.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Agustos)?.GirilenDeger,
                Eylul = VaSurecleriMaddeGirilenDegers.FirstOrDefault(p => p.BirimID == surecMaddeBirim.BirimID && p.IsVeriVar == true && p.GirilenDeger.HasValue && p.VASurecleriMaddeID == surecMaddeBirim.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Eylul)?.GirilenDeger,
                Ekim = VaSurecleriMaddeGirilenDegers.FirstOrDefault(p => p.BirimID == surecMaddeBirim.BirimID && p.IsVeriVar == true && p.GirilenDeger.HasValue && p.VASurecleriMaddeID == surecMaddeBirim.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Ekim)?.GirilenDeger,
                Kasim = VaSurecleriMaddeGirilenDegers.FirstOrDefault(p => p.BirimID == surecMaddeBirim.BirimID && p.IsVeriVar == true && p.GirilenDeger.HasValue && p.VASurecleriMaddeID == surecMaddeBirim.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Kasim)?.GirilenDeger,
                Aralik = VaSurecleriMaddeGirilenDegers.FirstOrDefault(p => p.BirimID == surecMaddeBirim.BirimID && p.IsVeriVar == true && p.GirilenDeger.HasValue && p.VASurecleriMaddeID == surecMaddeBirim.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Aralik)?.GirilenDeger,
                Guz = VaSurecleriMaddeGirilenDegers.FirstOrDefault(p => p.BirimID == surecMaddeBirim.BirimID && p.IsVeriVar == true && p.GirilenDeger.HasValue && p.VASurecleriMaddeID == surecMaddeBirim.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Guz)?.GirilenDeger,
                Bahar = VaSurecleriMaddeGirilenDegers.FirstOrDefault(p => p.BirimID == surecMaddeBirim.BirimID && p.IsVeriVar == true && p.GirilenDeger.HasValue && p.VASurecleriMaddeID == surecMaddeBirim.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Bahar)?.GirilenDeger,
                Yaz = VaSurecleriMaddeGirilenDegers.FirstOrDefault(p => p.BirimID == surecMaddeBirim.BirimID && p.IsVeriVar == true && p.GirilenDeger.HasValue && p.VASurecleriMaddeID == surecMaddeBirim.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Yaz)?.GirilenDeger,
                YillikVeri = VaSurecleriMaddeGirilenDegers.FirstOrDefault(p => p.BirimID == surecMaddeBirim.BirimID && p.IsVeriVar == true && p.GirilenDeger.HasValue && p.VASurecleriMaddeID == surecMaddeBirim.VASurecleriMaddeID && p.VACokluVeriDonemID == VaCokluVeriDonemEnum.Yillik)?.GirilenDeger,
                HesaplamaSekli = surecMaddeBirim.VASurecleriMadde.MaddeYilSonuDegerHesaplamaTipleri.YilSonuDegerHesaplamaAdi,
                PlanlananHedef = surecMaddeBirim.VASurecleriMadde.IsPlanlananDegerOlacak ? surecMaddeBirim.PlanlananDeger : null,
                PlanlananVeriGelecekYil = surecMaddeBirim.VASurecleriMadde.IsPlanlananDegerOlacakGelecekYil ? surecMaddeBirim.PlanlananDegerGelecekYil : null
            };
            if (returnData.Ocak.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMaddeBirim.VASurecleriMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Ocak, GirilenDeger = returnData.Ocak.Value }); }
            if (returnData.Subat.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMaddeBirim.VASurecleriMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Subat, GirilenDeger = returnData.Subat.Value }); }
            if (returnData.Mart.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMaddeBirim.VASurecleriMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Mart, GirilenDeger = returnData.Mart.Value }); }
            if (returnData.Nisan.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMaddeBirim.VASurecleriMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Nisan, GirilenDeger = returnData.Nisan.Value }); }
            if (returnData.Mayis.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMaddeBirim.VASurecleriMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Mayis, GirilenDeger = returnData.Mayis.Value }); }
            if (returnData.Haziran.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMaddeBirim.VASurecleriMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Haziran, GirilenDeger = returnData.Haziran.Value }); }
            if (returnData.Temmuz.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMaddeBirim.VASurecleriMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Temmuz, GirilenDeger = returnData.Temmuz.Value }); }
            if (returnData.Agustos.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMaddeBirim.VASurecleriMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Agustos, GirilenDeger = returnData.Agustos.Value }); }
            if (returnData.Eylul.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMaddeBirim.VASurecleriMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Eylul, GirilenDeger = returnData.Eylul.Value }); }
            if (returnData.Ekim.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMaddeBirim.VASurecleriMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Ekim, GirilenDeger = returnData.Ekim.Value }); }
            if (returnData.Kasim.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMaddeBirim.VASurecleriMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Kasim, GirilenDeger = returnData.Kasim.Value }); }
            if (returnData.Aralik.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMaddeBirim.VASurecleriMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Aralik, GirilenDeger = returnData.Aralik.Value }); }
            if (returnData.Guz.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMaddeBirim.VASurecleriMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Guz, GirilenDeger = returnData.Guz.Value }); }
            if (returnData.Bahar.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMaddeBirim.VASurecleriMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Bahar, GirilenDeger = returnData.Bahar.Value }); }
            if (returnData.Yaz.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMaddeBirim.VASurecleriMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Yaz, GirilenDeger = returnData.Yaz.Value }); }
            if (returnData.YillikVeri.HasValue) { returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMaddeBirim.VASurecleriMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.Yillik, GirilenDeger = returnData.YillikVeri.Value }); }

            if (surecMaddeBirim.VASurecleriMadde.MaddeYilSonuDegerHesaplamaTipID == MaddeYilSonuDegerHesaplamaTip.Kümülatif)
                returnData.HesaplamaSonucu = returnData.VaSurecleriMaddeGirilenDegers.Sum(s => s.GirilenDeger.Value);

            else if (surecMaddeBirim.VASurecleriMadde.MaddeYilSonuDegerHesaplamaTipID == MaddeYilSonuDegerHesaplamaTip.SonAy)
                returnData.HesaplamaSonucu = returnData.VaSurecleriMaddeGirilenDegers.LastOrDefault()?.GirilenDeger;
            else
            {
                var toplamGirilenDeger = returnData.VaSurecleriMaddeGirilenDegers.Sum(s => s.GirilenDeger ?? 0);

                returnData.HesaplamaSonucu = toplamGirilenDeger == 0 ? 0 : (toplamGirilenDeger / returnData.VaSurecleriMaddeGirilenDegers.Count);
            }
            returnData.VaSurecleriMaddeGirilenDegers.Add(new GirilenDegerler { MaddeKod = surecMaddeBirim.VASurecleriMadde.MaddeKod, VaCokluVeriDonemId = VaCokluVeriDonemEnum.HesaplamaSonucu, GirilenDeger = returnData.HesaplamaSonucu });


            returnData.BirimAdi = Birimlers.Where(p => p.BirimID == surecMaddeBirim.BirimID).Select(s => s.BirimTreeAdi).FirstOrDefault();
            if (returnData.BirimAdi.IsNullOrWhiteSpace())
            {

            }


            return returnData;
        }



    }
}