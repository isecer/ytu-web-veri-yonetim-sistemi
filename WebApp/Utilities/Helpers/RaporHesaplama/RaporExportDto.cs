using System.Collections.Generic;

namespace WebApp.Utilities.Helpers.RaporHesaplama
{
    public class RaporExportDto
    { 
        public int Yil { get; set; }
        public string BirimAdi { get; set; } 
        public string MaddeKod { get; set; }
        public string MaddeTurAdi { get; set; }
        public string MaddeAdi { get; set; }
        public decimal? Ocak { get; set; }
        public decimal? Subat { get; set; }
        public decimal? Mart { get; set; }
        public decimal? Nisan { get; set; }
        public decimal? Mayis { get; set; }
        public decimal? Haziran { get; set; }
        public decimal? Temmuz { get; set; }
        public decimal? Agustos { get; set; }
        public decimal? Eylul { get; set; }
        public decimal? Ekim { get; set; }
        public decimal? Kasim { get; set; }
        public decimal? Aralik { get; set; }
        public decimal? Guz { get; set; }
        public decimal? Bahar { get; set; }
        public decimal? Yaz { get; set; }
        public decimal? YillikVeri { get; set; }
        public string HesaplamaSekli { get; set; }
        public string HesaplamaFormulu { get; set; }
        public decimal? HesaplamaSonucu { get; set; }
        public decimal? PlanlananHedef { get; set; }
        public decimal? PlanlananVeriGelecekYil { get; set; }
        public string BilgiMesaji { get; set; }
        public List<GirilenDegerler> VaSurecleriMaddeGirilenDegers { get; set; } = new List<GirilenDegerler>();

    }

    public class GirilenDegerler
    { 
        public string MaddeKod { get; set; }
        public int? VaCokluVeriDonemId { get; set; } 
        public decimal? GirilenDeger { get; set; }
    }
}