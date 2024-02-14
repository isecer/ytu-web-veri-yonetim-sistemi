using BiskaUtil;
using Database;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using WebApp.Models;

namespace WebApp.Utilities.Dtos
{
    public class FmVeriGiris : PagerOption
    {
        public bool Export { get; set; }
        public bool IsAktif { get; set; }
        public int? VaSurecId { get; set; }
        public int? BirimId { get; set; }
        public string Aranan { get; set; }
        public int? MaddeVeriGirisDurumId { get; set; }
        public int? MaddeTurId { get; set; }
        public bool? VeriGirisiOnaylandi { get; set; }
        public List<FrVgMaddeler> Data { get; set; }
        public List<int> FilteredMaddeIds { get; set; } = new List<int>();
    }
    public class FrVgMaddeler : Vw_MaddeVeriGirisDurum
    {
        public bool VeriGirisiOnaylandi { get; set; }
        public int MaddeVeriGirisDurumId { get; set; }
        public string BirimAdi { get; set; }
        public bool MaddeTurIsVeriGirisiAcik { get; set; }
        public List<VgMaddeVerileri> VgMaddeVerileris { get; set; }
        public List<VASurecleriMaddeGirilenDeger> VaSurecleriMaddeGirilenDegers { get; set; } = new List<VASurecleriMaddeGirilenDeger>();
        public List<VASurecleriMaddeVeriGirisDonemleri> VaSurecleriMaddeVeriGirisDonemleris { get; set; } = new List<VASurecleriMaddeVeriGirisDonemleri>();
        public int DosyaCount { get; set; }
    }

    public class VgMaddeVerileri
    {
        public int? VaCokluVeriDonemId { get; set; }
        public string CokluVeriDonemAdi { get; set; }
        public bool IsDosyaYuklensin { get; set; }
        public int DosyaCount { get; set; }
        public bool? IsVeriVar { get; set; }
        public decimal? GirilenDeger { get; set; }
        public string VeriGirenAdSoyad { get; set; }
        public string OnayYapanAdSoyad { get; set; }

        public bool? VeriGirisiOnaylandi { get; set; }
    }
    public class VeriGirisDetailDto
    {
        public int SurecYil { get; set; }
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }
        public bool SurecIsAktif { get; set; }
        public int VASurecID { get; set; }
        public int BirimID { get; set; }
        public int MaddeID { get; set; }
        public bool IsAktifYilPlanlananVeriGirisiAcik { get; set; }
        public bool IsGelecekYilPlanlananVeriGirisiAcik { get; set; }
        public bool VeriGirisiOnaylandi { get; set; }

        public int AciklamaCount { get; set; }
        public FrVgMaddeler VeriGirisi = new FrVgMaddeler();
        public List<FrFaaliyetler> FaaliyetData = new List<FrFaaliyetler>();
        public List<VASurecleriMaddeEklenenAciklama> EklenenAciklamas = new List<VASurecleriMaddeEklenenAciklama>();

    }


    public class VeriKanitModel
    {
        public int VASurecID { get; set; }
        public bool SurecAktif { get; set; }
        public bool VeriGirisiOnaylandi { get; set; }
        public int Yil { get; set; }
        public int BirimID { get; set; }
        public string BirimAdi { get; set; }
        public int MaddeID { get; set; }
        public string MaddeAdi { get; set; }
        public SelectList SelectListVaCokluVeriDonemId { get; set; }
        public int? VaCokluVeriDonemId { get; set; }

        public VeriKanitDosyaListModel VeriKanitDosyaListModel { get; set; }
    }
    public class VeriKanitDosyaListModel
    {
        public bool KayitYetki { get; set; }
        public int? VaCokluVeriDonemId { get; set; }
        public List<VeriKanitDosyaListRw> Data = new List<VeriKanitDosyaListRw>();
    }
    public class VeriKanitDosyaListRw : VASurecleriMaddeEklenenDosya
    {
        public string BirimAdi { get; set; }
        public string DonemAdi { get; set; }
    }
    public class AciklamaGirisModel : VASurecleriMaddeEklenenAciklama
    {

        public VASurecleri SurecBilgi { get; set; }
        public Birimler BirimBilgi { get; set; }
        public int Yil { get; set; }
        public int VASurecID { get; set; }
        public int BirimID { get; set; }
        public string BirimAdi { get; set; }
        public int MaddeID { get; set; }
        public string MaddeAdi { get; set; }
        public bool VeriGirisiOnaylandi { get; set; }
        public bool SurecAktif { get; set; }

        public SelectList SListAylar { get; set; }

        public AciklamaListModel AciklamaData = new AciklamaListModel();
    }
    public class AciklamaListModel
    {
        public bool KayitYetki { get; set; }
        public List<VASurecleriMaddeEklenenAciklama> Data = new List<VASurecleriMaddeEklenenAciklama>();
    }
}