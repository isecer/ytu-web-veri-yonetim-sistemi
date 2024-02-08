using BiskaUtil;
using Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApp.Models
{

    public class URoles
    {

        public int? YetkiGrupID { get; set; }
        public string YetkiGrupAdi { get; set; }
        public List<Roller> EklenenRoller { get; set; }
        public List<Roller> YetkiGrupRolleri { get; set; }
        public List<Roller> TumRoller { get; set; }
        public URoles()
        {
            EklenenRoller = new List<Roller>();
            YetkiGrupRolleri = new List<Roller>();
            TumRoller = new List<Roller>();
        }
    }

    public class FmSurecIslemleri : PagerOption
    {
        public int? ShowDetailVaSurecId { get; set; }
        public int? Yıl { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<FrSurecIslemleri> Data { get; set; }
    }

    public class FrSurecIslemleri : VASurecleri
    {
        public string SurecYilAdi { get; set; }
        public string IslemYapan { get; set; }
        public bool AktifSurec { get; set; }
        public int SelectedTabIndex { get; set; }


        public List<VaSurecleriMaddeTurDto> MaddeTurleris = new List<VaSurecleriMaddeTurDto>();


        public List<FrBirimler> BirimData = new List<FrBirimler>();
        public SelectList SListBirimDurum { get; set; }
        public int? SBirimDurumId { get; set; }
        public List<FrMaddeler> MaddeData = new List<FrMaddeler>();
        public SelectList SListMaddeDurum { get; set; }
        public int? SMaddeDurumId { get; set; }

    }

    public class VaSurecleriMaddeTurDto : VASurecleriMaddeTur
    {
        public string MaddeTurAdi { get; set; }
        public int ToplamMaddeCount { get; set; } 
    }
    public class FmVeriBilgisi : PagerOption
    {
        public int? BirimID { get; set; }

        public List<int> VeriYillari { get; set; }
        public IEnumerable<FrVeriBilgisi> Data { get; set; }
    }
    public class FrVeriBilgisi
    {
        public int MaddeID { get; set; }
        public string MaddeKod { get; set; }
        public string MaddeAdi { get; set; }
        public List<VASurecleriMadde> Veriler { get; set; }
    }





    public class FRFaaliyetDetayModel
    {
        public Vw_BirimlerTree BirimBilgi { get; set; }
        public FrFrFormYukle FrFaaliyetler { get; set; }
        public int FRDonemID { get; set; }
        public int BirimID { get; set; }
        public int FRFormID { get; set; }
    }
    public class FRFaaliyetDetayModelBF
    {
        public Vw_BirimlerTree BirimBilgi { get; set; }
        public FrBfrFormYukle BFrFaaliyetler { get; set; }
        public int BFRDonemID { get; set; }
        public int BirimID { get; set; }
        public int BFRFormID { get; set; }
    }


    public class KmVeriGiris : VASurecleriMadde
    {
        public decimal? PlanlananDeger { get; set; }
        public decimal? PlanlananDegerGelecekYil { get; set; }
        public decimal? HesaplananSonucDegeri { get; set; }
        public string YilSonuDegerHesaplamaAdi { get; set; }
        public int GirilecekVeriSayisi { get; set; }
        public int GirilenVeriSayisi { get; set; }
        public int DosyaCount { get; set; }
        public bool VeriGirisiOnaylandi { get; set; }
        public string VeriGirisiOnaylayan { get; set; }
        public DateTime? OnayIslemTarihi { get; set; }
        public VASurecleriMaddeBirim MaddeBirim { get; set; }
        public string Aciklama { get; set; }
    }
    public class VgModel
    {
        public int VASurecID { get; set; }
        public int BirimID { get; set; }
        public int MaddeID { get; set; }
        public int? VaCokluVeriDonemId { get; set; }
        public bool? IsVeriVar { get; set; }
        public bool VeriGirisiOnaylandi { get; set; }
        public decimal? GirilenDeger { get; set; }
    }
    public class KmSurecIslemleri : VASurecleri
    {
        public DateTime? BaslangicTarihi { get; set; }
        public DateTime? BitisTarihi { get; set; }
        public bool BirimleriKopyala { get; set; }
        public bool MaddeleriKopyala { get; set; }
        public List<int> MaddeTurIds { get; set; } = new List<int>();
    }

    public class FmKullanicilar : PagerOption
    {

        public int KullaniciID { get; set; }

        public int? BirimID { get; set; }
        public int? YetkiliBirimID { get; set; }
        public int? UnvanID { get; set; }
        public bool? IsAktif { get; set; }
        public int? YetkiGrupID { get; set; }
        public int? Cinsiyet { get; set; }
        public string KullaniciAdi { get; set; }
        public string AdSoyad { get; set; }
        public string EMail { get; set; }
        public string Telefon { get; set; }
        public bool? IsActiveDirectoryUser { get; set; }
        public bool? IsAdmin { get; set; }
        public string Aciklama { get; set; }
        public IEnumerable<FrKullanicilar> Data { get; set; }
        public FmKullanicilar()
        {
            Data = new FrKullanicilar[0];
        }
    }
    public class RollTableIDName
    {
        public static string BirimID => "BirimID";
        public static string DonemID => "DonemID";
        public static string MaddeTurID => "MaddeTurID";
        public static string GTBirimID => "GTBirimID";
        public static string GTHesapKodID => "GTHesapKodID";
        public static string GTHesapNoID => "GTHesapNoID";
    }

    public class FrKullanicilar : Kullanicilar
    {
        public string YetkiGrupAdi { get; set; }
        public string EgitmenTipAdi { get; set; }
        public string AlternatifKadroTipAdi { get; set; }
        public string UnvanAdi { get; set; }
        public string BirimAdi { get; set; }
        public string BirimTreeAdi { get; set; }

        public Dictionary<string, List<int>> TableRollID { get; set; }
        public List<SelectedTableRoll> SelectedTableRoll { get; set; }


        public FrKullanicilar()
        {
            TableRollID = new Dictionary<string, List<int>>();
            SelectedTableRoll = new List<SelectedTableRoll>();
        }

    }


    public class FmYetkiGruplari : PagerOption
    {
        public int YetkiGrupID { get; set; }
        public string YetkiGrupAdi { get; set; }
        public IEnumerable<FrYetkiGruplari> Data { get; set; }
    }
    public class FrYetkiGruplari
    {
        public int YetkiGrupID { get; set; }
        public string YetkiGrupAdi { get; set; }
        public string EnstituKod { get; set; }
        public string EnstituAdi { get; set; }
        public int YetkiSayisi { get; set; }
    }

    public class FmUnvanlar : PagerOption
    {
        public string UnvanAdi { get; set; }
        public int? Yuk { get; set; }
        public decimal? YukKatsayisi { get; set; }
        public decimal? GostergePuani { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<Unvanlar> Data { get; set; }

    }

    public class FmBirimler : PagerOption
    {
        public string BirimKod { get; set; }
        public string BirimAdi { get; set; }
        public bool? EslestirmeDurum { get; set; }
        public bool? IsAktif { get; set; }
        public bool? IsMaddeEklenebilir { get; set; }
        public IEnumerable<FrBirimler> Data { get; set; }

    }
    public class FrBirimler : Birimler
    {
        public string IslemYapan { get; set; }
        public string BirimTreeAdi { get; set; }
        public List<FrMaddeler> BirimMaddeleri { get; set; }
        public int ToplamCount { get; set; }
        public int TamamlananCount { get; set; }
        public int OnaylananCount { get; set; }
       
    }
    public class FmMaddeTurleri : PagerOption
    {
        public string MaddeTurAdi { get; set; }
        public bool? IsPlanlananDegerOlacak { get; set; }
        public bool? IsPlanlananDegerOlacakGelecekYil { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<FrMaddeTurleri> Data { get; set; }

    }
    public class FrMaddeTurleri : MaddeTurleri
    {
        public string IslemYapan { get; set; }

    }
    public class FmMaddeler : PagerOption
    {
        public string MaddeKod { get; set; }
        public string MaddeAdi { get; set; }
        public int? OgretimCount { get; set; }
        public int? VeriGirisSekliID { get; set; }
        public int? MaddeYilSonuDegerHesaplamaTipID { get; set; }
        public int? MaddeTurID { get; set; }
        public bool? IsAktif { get; set; }
        public bool? IsCokluVeriGiris { get; set; }
        public bool? IsDosyaYuklensin { get; set; }
        public bool? EslestirmeDurum { get; set; }
        public string Aranan { get; set; }
        public IEnumerable<FrMaddeler> Data { get; set; }

        public FmMaddeler()
        {
            this.PageSizes.Add(100);
            this.PageSizes.Add(150);
            this.PageSizes.Add(200);
            this.PageSizes.Add(300);
            this.PageSizes.Add(400);
            this.PageSizes.Add(500);
        }

    }

    public class FrMaddeler : Maddeler
    {
        public string IslemYapan { get; set; }
        public string MaddeTreeAdi { get; set; }
        public string MaddeTurAdi { get; set; }
        public double GirilenDeger { get; set; }
        public int ToplamCount { get; set; }
        public int TamamlananCount { get; set; }
        public int OnaylananCount { get; set; }
        public bool VeriGirisiOnaylandi { get; set; }
        public bool IsPlanlananDegerOlacak { get; set; }
        public bool IsPlanlananDegerOlacakGelecekYil { get; set; }
        public decimal? PlanlananDeger { get; set; }
        public decimal? PlanlananDegerGelecekYil { get; set; }
        public string YilSonuDegerHesaplamaAdi { get; set; }
        public decimal? HesaplananSonucDegeri { get; set; }
        public int GirilecekVeriSayisi { get; set; }
        public int GirilenVeriSayisi { get; set; }
        public int OnaylananVeriSayisi { get; set; }
        public int MaddeVeriGirisDurumID { get; set; }
        public List<FrBirimler> MaddeBirimleri { get; set; }
        public int VaSurecleriBirimId { get; set; }
        public int VaSurecleriMaddeId { get; set; }
        public int Yil { get; set; }
        public string BirimAdi { get; set; }
        public string VeriGirisSekliAdi { get; set; }
        public VASurecleriMaddeBirim MaddeBirim { get; set; }
        public ICollection<VASurecleriMaddeGirilenDeger> VaSurecleriMaddeGirilenDegers { get; set; }
        public ICollection<VASurecleriMaddeVeriGirisDonemleri> VaSurecleriMaddeVeriGirisDonemleris { get; set; }
        public int DosyaCount { get; set; }

    }


    public class FmFaaliyetler : PagerOption
    {
        public string FaaliyetKod { get; set; }
        public string FaaliyetAdi { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<FrFaaliyetler> Data { get; set; }

        public FmFaaliyetler()
        {
            this.PageSizes.Add(100);
            this.PageSizes.Add(150);
            this.PageSizes.Add(200);
            this.PageSizes.Add(300);
            this.PageSizes.Add(400);
            this.PageSizes.Add(500);
        }

    }

    public class FrFaaliyetler : Faaliyetler
    {
        public string IslemYapan { get; set; }
        public List<Vw_MaddelerTree> FaaliyetMaddeleri { get; set; }
        public List<Aylar> FaaliyetAylari { get; set; }
        public List<Kaynaklar> FaaliyetKaynaklari { get; set; }
        public FrFaaliyetler()
        {

        }
    }
    public class FmRaporTipleri : PagerOption
    {
        public string RaporTipAdi { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<FrRaporTipleri> Data { get; set; }

        public FmRaporTipleri()
        {

        }

    }

    public class FrRaporTipleri : RaporTipleri
    {
        public string IslemYapan { get; set; }
        public List<Vw_MaddelerTree> RaporMaddeleri { get; set; }

        public FrRaporTipleri()
        {

        }
    }
    public class FmSistemBilgilendirme : PagerOption
    {
        public Nullable<byte> BilgiTipi { get; set; }
        public string Kategori { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public Nullable<System.DateTime> IslemZamani { get; set; }
        public string IpAdresi { get; set; }
        public string AdSoyad { get; set; }

        public IEnumerable<FrSistemBilgilendirme> Data { get; set; }

    }
    public class FrSistemBilgilendirme
    {
        public int SistemBilgiID { get; set; }
        public Nullable<byte> BilgiTipi { get; set; }
        public string Kategori { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public Nullable<System.DateTime> IslemZamani { get; set; }
        public string IpAdresi { get; set; }
        public string AdSoyad { get; set; }
        public string KullaniciAdi { get; set; }
    }
    public class FmDuyurular : PagerOption
    {
        public string Baslik { get; set; }
        public DateTime? Tarih { get; set; }
        public string Aciklama { get; set; }
        public bool? IsAktif { get; set; }
        public string DuyuruYapan { get; set; }
        public IEnumerable<FrDuyurular> Data { get; set; }
    }

    public class FrDuyurular : Duyurular
    {

        public string DuyuruYapan { get; set; }
        public int EkSayisi { get; set; }
        public List<DuyuruEkleri> Ekler { get; set; }
    }
    public class FmMailSablonlari : PagerOption
    {
        public int? MailSablonTipID { get; set; }
        public string SablonAdi { get; set; }
        public DateTime? Tarih { get; set; }
        public string Sablon { get; set; }
        public bool? IsAktif { get; set; }
        public string DuyuruYapan { get; set; }
        public IEnumerable<FrMailSablonlari> Data { get; set; }
    }

    public class FrMailSablonlari : MailSablonlari
    {
        public string SablonTipAdi { get; set; }
        public string Parametreler { get; set; }
        public string IslemYapan { get; set; }
        public int EkSayisi { get; set; }
    }
    public class FmMesajKategorileri : PagerOption
    {
        public string KategoriAdi { get; set; }
        public string KategoriAciklamasi { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<MesajKategorileri> Data { get; set; }
    }
    public class FrMesajKategorileri : MesajKategorileri
    {
        public string IslemYapan { get; set; }
    }

    public class FmMesajlar : PagerOption
    {
        public int? MesajKategoriID { get; set; }
        public string Konu { get; set; }
        public DateTime? Tarih { get; set; }
        public string Aciklama { get; set; }
        public bool? IsAktif { get; set; }
        public string AdSoyad { get; set; }
        public IEnumerable<FrMesajlar> Data { get; set; }
    }


    public class FrMesajlar : Mesajlar
    {
        public string KategoriAdi { get; set; }
        public string BirimAdi { get; set; }
        public Kullanicilar Kullanici { get; set; }
        public int EkSayisi { get; set; }
        public List<SubMessages> SubMesajList { get; set; }


    }

    public class SubMessages
    {
        public int KullaniciID { get; set; }
        public string EMail { get; set; }
        public DateTime Tarih { get; set; }
        public string ResimYolu { get; set; }
        public string AdSoyad { get; set; }
        public int MesajID { get; set; }
        public string Icerik { get; set; }
        public string IslemYapanIP { get; set; }
        public List<MesajEkleri> Ekler { get; set; }
        public List<GonderilenMailKullanicilar> Gonderilenler { get; set; }

    }

    public class FmMailGonderme : PagerOption
    {
        public string Konu { get; set; }
        public DateTime? Tarih { get; set; }
        public string Aciklama { get; set; }
        public string MailGonderen { get; set; }
        public IEnumerable<FrMailGonderme> Data { get; set; }
    }
    public class FrMailGonderme : GonderilenMailler
    {
        public string MailGonderen { get; set; }
        public int EkSayisi { get; set; }
        public int KisiSayisi { get; set; }

    }

    public class MailKullaniciBilgi
    {

        public bool Checked { get; set; }
        public int KullaniciID { get; set; }
        public string AdSoyad { get; set; }
        public string BirimAdi { get; set; }
        public string Email { get; set; }

    }

    public class UrlInfoModel
    {
        public string Root { get; set; }
        public string AbsolutePath { get; set; }
        public string LastPath { get; set; }
        public string Query { get; set; }
    }

    public class ChkListModel
    {
        public string PanelTitle { get; set; }
        public string TableId { get; set; }
        public string InputName { get; set; }
        public int ScrollMaxHeight { get; set; }
        public bool ShowIsDosyaYuklensin { get; set; }
        public IEnumerable<CheckObject<ChkListDataModel>> Data { get; set; }
        public bool AllDataChecked
        {
            get
            {

                return Data.Any() && Data.Select(s => s.Value).Count() == Data.Where(p => p.Checked == true).Select(s => s.Value).Count();

            }
        }
        public ChkListModel(string inputName = "")
        {
            this.ScrollMaxHeight = 135;
            this.InputName = inputName;
            TableId = Guid.NewGuid().ToString().Substring(0, 4);
        }
    }
    public class ChkListDataModel
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string Caption { get; set; }
        public string Detail { get; set; }
        public bool IsDosyaYuklensin { get; set; }
    }
    public class RaporSendPdfModel
    {
        public MemoryStream RaporMemoryStream { get; set; }
        public string DisplayName { get; set; }

    }




    public class DosyaYuklePartialModel
    {
        public bool KayitYetkisi { get; set; }
        public int FRFormID { get; set; }
        public FRDonemlerFormGirisleri EklenenDosya { get; set; }
    }
    public class DosyaYuklePartialModelBFR
    {
        public bool KayitYetkisi { get; set; }
        public int BFRFormID { get; set; }
        public BFRDonemlerFormGirisleri EklenenDosya { get; set; }
    }
    public class FmGTDonemleri : PagerOption
    {
        public int? VeriAktarilacakSurecID { get; set; }
        public int? Yıl { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<FrGTDonemleri> Data { get; set; }
    }
    public class KmGTDonemleri : GTDonemleri
    {
        public DateTime? BaslangicTarihi { get; set; }
        public DateTime? BitisTarihi { get; set; }
        public bool BirimleriKopyala { get; set; }
        public bool MaddeleriKopyala { get; set; }
    }
    public class FrGTDonemleri : GTDonemleri
    {
        public string DonemYilAdi { get; set; }
        public string IslemYapan { get; set; }
        public bool AktifSurec { get; set; }

    }
    public class FmGTVeriGirisi : PagerOption
    {
        public int? GTDonemID { get; set; }
        public int? GTBirimID { get; set; }
        public int? GTHesapNoID { get; set; }
        public int? GTHesapKodID { get; set; }
        public int? GTVeriGirisDurumID { get; set; }
        public DateTime? BasTar { get; set; }
        public DateTime? BitTar { get; set; }
        public string Aranan { get; set; }
        public IEnumerable<GTVeriGirisDurumlari> GTVeriGirisDurumlaris { get; set; }
        public IEnumerable<FrGTVeriGirisi> Data { get; set; }

    }
    public class FrGTVeriGirisi : GTVeriGirisi
    {
        public int Yil { get; set; }
        public string DurumAdi { get; set; }
        public string DurumColor { get; set; }
        public string BirimAdi { get; set; }
        public string HesapNo { get; set; }
        public string HesapNoAdi { get; set; }
        public string IslemYapan { get; set; }
        public string OnayYapan { get; set; }
        public int DetayCount { get; set; }
        public bool IsAktifSurec { get; set; }
        public List<FrGTVerigirisiDetay> GTVerigirisiDetayList { get; set; }

    }
    public class FrGTVerigirisiDetay : GTVerigirisiDetay
    {
        public string HesapKod { get; set; }
        public string HesapKodAdi { get; set; }
        public string GelirNiteligiAdi { get; set; }
        public string VergiKimlikNo { get; set; }
        public string AdSoyad { get; set; }
    }
    public class FmGTBirimler : PagerOption
    {
        public IEnumerable<FrGTBirimler> Data { get; set; }

    }
    public class FrGTBirimler : GTBirimler
    {
        public List<GTHesapKodlari> GTHesapKodlariList { get; set; }
        public List<GTHesapNumaralari> GTHesapNumaralariList { get; set; }
        public string IslemYapan { get; set; }

    }

    public class FmGTHesapKodlari : PagerOption
    {
        public string HesapKod { get; set; }
        public string HesapKodAdi { get; set; }
        public string GelirNiteligi { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<FrGTHesapKodlari> Data { get; set; }

    }

    public class FrGTHesapKodlari : GTHesapKodlari
    {

        public string IslemYapan { get; set; }

    }
    public class FmGTHesapNumaralari : PagerOption
    {
        public string HesapNo { get; set; }
        public string HesapNoAdi { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<FrGTHesapNumaralari> Data { get; set; }

    }

    public class FrGTHesapNumaralari : GTHesapNumaralari
    {
        public string IslemYapan { get; set; }

    }
    public class FmGTVergiKimlikNumaralari : PagerOption
    {
        public string VergiKimlikNo { get; set; }
        public string AdSoyad { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<FrGTVergiKimlikNumaralari> Data { get; set; }

    }

    public class FrGTVergiKimlikNumaralari : GTVergiKimlikNumaralari
    {
        public string IslemYapan { get; set; }

    }
    #region ComboModels 

    public class ComboModelBool
    {
        public bool? Value { get; set; }
        public string Caption { get; set; }
    }
    public class ComboModelBoolDatetime
    {
        public int GroupID { get; set; }
        public bool? Value { get; set; }
        public DateTime? Caption { get; set; }
    }
    public class ComboModelInt
    {
        public int? Value { get; set; }
        public string Caption { get; set; }
    }


    public class ComboModelIntChecked
    {
        public bool Checked { get; set; }
        public int? Value { get; set; }
        public string Caption { get; set; }
    }
    public class ComboModelLong
    {
        public long? Value { get; set; }
        public string Caption { get; set; }
    }
    public class ComboModelInt_multi
    {
        public int Inx { get; set; }
        public int Key { get; set; }
        public int Value { get; set; }
        public string ValueS { get; set; }
        public string ValueS2 { get; set; }
        public string ValueS3 { get; set; }
        public bool ValueB { get; set; }
        public bool ValueB2 { get; set; }
        public double ValueDouble { get; set; }
        public double ValueDouble2 { get; set; }
        public DateTime? DateTime1 { get; set; }
        public DateTime? DateTime2 { get; set; }
    }
    public class ComboModelIntInt
    {
        public int Value { get; set; }
        public int Caption { get; set; }
    }
    public class ComboModelPageIndex
    {
        public int StartRowIndex { get; set; }
        public int PageIndex { get; set; }
    }

    public class ComboModelString
    {
        public string Value { get; set; }
        public string Caption { get; set; }
    }
    public class ComboModelDatetime
    {
        public int Value { get; set; }
        public DateTime? Caption { get; set; }
    }
    public class ComboModelIntBool
    {
        public int Value { get; set; }
        public bool Caption { get; set; }
    }
    #endregion

    #region SabitTanimlar


    public static class MaddeYilSonuDegerHesaplamaTip
    {
        public const byte Ortalama = 1;
        public const byte Kümülatif = 2;
        public const byte SonAy = 3;
    }


    public static class Cinsiyet
    {
        public const byte Erkek = 1;
        public const byte Kadın = 2;

    }
    public static class MedeniHal
    {
        public const byte Bekar = 1;
        public const byte Evli = 2;

    }

    public static class ModalSizeClass
    {

        public const string Small = "modal-dialog modal-sm";
        public const string Basic = "modal-dialog";
        public const string Large = "modal-dialog modal-lg";
    }

    public enum HaftaGunleriEnum
    {
        Pazar = 0,
        Pazartesi = 1,
        Salı = 2,
        Çarşamba = 3,
        Perşembe = 4,
        Cuma = 5,
        Cumartesi = 6
    }
    public static class HaftaGunleri
    {
        public static int Pazartesi { get; set; } = 1;
        public static int Salı { get; set; } = 2;
        public static int Çarşamba { get; set; } = 3;
        public static int Perşembe { get; set; } = 4;
        public static int Cuma { get; set; } = 5;
        public static int Cumartesi { get; set; } = 6;
        public static int Pazar { get; set; } = 0;
    }

    public static class BilgiTipi
    {
        public const byte Hata = 1;
        public const byte Uyarı = 2;
        public const byte Kritik = 3;
        public const byte OnemsizHata = 4;
        public const byte Saldırı = 5;
        public const byte LoginHatalari = 6;
        public const byte Bilgi = 7;
    }


    public static class VeriGirisSekli
    {
        public const byte VeriGirisiYok = 0;
        public const byte ElleGirilecek = 1;
        public const byte FormulleHesaplanacak = 2;
        public const byte WebServisi = 3;
    }

    public static class ZamanTipi
    {
        public const byte Yil = 1;
        public const byte Ay = 2;
        public const byte Gun = 3;
        public const byte Saat = 4;
        public const byte Dakika = 5;
    }
    public static class DenklikTipi
    {
        public const byte Kucuk = 1;
        public const byte Buyuk = 2;
        public const byte Esit = 3;
    }
    public static class RenkTiplier
    {
        public static string Primary = "#33414e";
        public static string Info = "#3fbae4";
        public static string Warning = "#fea223";
        public static string Success = "#95b75d";
        public static string Danger = "#b64645";
    }

    public static class GTVeriGirisDurumu
    {
        public static int Beklemede = 1;
        public static int Onaylandi = 2;
        public static int YevmiyeIslendi = 3;

    }
    public static class HttpDurumKod
    {
        public const int Continue = 100;
        public const int SwitchingProtocols = 101;
        public const int Processing = 102;
        public const int OK = 200;
        public const int Created = 201;
        public const int Accepted = 202;
        public const int NonAuthoritativeInformation = 203;
        public const int NoContent = 204;
        public const int ResetContent = 205;
        public const int PartialContent = 206;
        public const int MultiStatus = 207;
        public const int ContentDifferent = 210;
        public const int MultipleChoices = 300;
        public const int MovedPermanently = 301;
        public const int MovedTemporarily = 302;
        public const int SeeOther = 303;
        public const int NotModified = 304;
        public const int UseProxy = 305;
        public const int TemporaryRedirect = 307;
        public const int BadRequest = 400;
        public const int Unauthorized = 401;
        public const int PaymentRequired = 402;
        public const int Forbidden = 403;
        public const int NotFound = 404;
        public const int NotAccessMethod = 405;
        public const int NotAcceptable = 406;
        public const int UnLoginToProxyServer = 407;
        public const int RequestTimeOut = 408;
        public const int Conflict = 409;
        public const int Gone = 410;
        public const int LengthRequired = 411;
        public const int PreconditionAiled = 412;
        public const int RequestEntityTooLarge = 413;
        public const int RequestURITooLong = 414;
        public const int UnsupportedMediaType = 415;
        public const int RequestedrangeUnsatifiable = 416;
        public const int Expectationfailed = 417;
        public const int Unprocessableentity = 422;
        public const int Locked = 423;
        public const int Methodfailure = 424;
        public const int InternalServerError = 500;
        public const int Uygulanmamış = 501;
        public const int GeçersizAğGeçidi = 502;
        public const int HizmetYok = 503;
        public const int GatewayTimeout = 504;
        public const int HTTPVersionNotSupported = 505;
        public const int InsufficientStorage = 507;

    }


    public class BilgiTipleri
    {
        public List<BilgiRow> BilgiTip { get; set; }
        public BilgiTipleri()
        {

            var dct = new List<BilgiRow>();
            dct.Add(new BilgiRow { BilgiTipID = BilgiTipi.Hata, BilgiTipAdi = "Hata", BilgiTipCls = "primary" });
            dct.Add(new BilgiRow { BilgiTipID = BilgiTipi.Uyarı, BilgiTipAdi = "Uyarı", BilgiTipCls = "warning" });
            dct.Add(new BilgiRow { BilgiTipID = BilgiTipi.Kritik, BilgiTipAdi = "Kritik Durum", BilgiTipCls = "danger" });
            dct.Add(new BilgiRow { BilgiTipID = BilgiTipi.OnemsizHata, BilgiTipAdi = "Önemsiz Hata", BilgiTipCls = "default" });
            dct.Add(new BilgiRow { BilgiTipID = BilgiTipi.Saldırı, BilgiTipAdi = "Saldırı", BilgiTipCls = "danger" });
            dct.Add(new BilgiRow { BilgiTipID = BilgiTipi.LoginHatalari, BilgiTipAdi = "loginHatalari", BilgiTipCls = "info" });
            dct.Add(new BilgiRow { BilgiTipID = BilgiTipi.Bilgi, BilgiTipAdi = "Bilgi", BilgiTipCls = "success" });
            BilgiTip = dct;



        }


    }



    [Serializable()]
    public static class TreeExt
    {
        [Serializable()]
        public class TreeExtRow<T> where T : class
        {
            public string ParentLevel { get; set; }
            public int Level { get; set; }
            public bool HasChild { get; set; }
            public T Value { get; set; }
        }
        [Serializable()]
        public class TreeExtNode<T> where T : class
        {
            public string ParentLevel { get; set; }
            public int Level { get; set; }
            public T Value { get; set; }
            public bool Checked { get; set; }
            public TreeExtNode<T> Parent { get; set; }
            public List<TreeExtNode<T>> Children { get; set; }
        }

        public static IEnumerable<TreeExtRow<T>> CastToTree<T>(this IEnumerable<T> source, string RootPropertyField, string ParentPropertyField) where T : class
        {
            List<TreeExtRow<T>> resultList = new List<TreeExtRow<T>>();

            if (source == null || source.Count() == 0) return resultList.AsEnumerable();
            var type = typeof(T);
            var ids = source.Select(s => s.GetType().GetProperty(RootPropertyField).GetValue(s, null).ToString()).Distinct().ToArray();

            //List<T> roots = new List<T>(); 
            //foreach (var l in source) 
            //{ 
            //    if (type.GetProperty(ParentPropertyField).GetValue(l, null) == null) roots.Add(l); 
            //    else 
            //    { 
            //        var bid = type.GetProperty(ParentPropertyField).GetValue(l, null).ToString(); 
            //        if (ids.Contains(bid) == false) roots.Add(l); 
            //    } 
            //} 
            var roots = source.Where(p => type.GetProperty(ParentPropertyField).GetValue(p, null) == null ||
                                      ids.Contains(type.GetProperty(ParentPropertyField).GetValue(p, null).ToString()) == false);
            Func<TreeExtRow<T>, int> fxDetail = null;
            fxDetail = new Func<TreeExtRow<T>, int>
                (
                   (parent) =>
                   {
                       object parentid = type.GetProperty(RootPropertyField).GetValue(parent.Value, null);
                       var details = source.Where(p =>
                           type.GetProperty(ParentPropertyField).GetValue(p, null) != null && //it is root 
                           type.GetProperty(ParentPropertyField).GetValue(p, null).ToString() == parentid.ToString()).ToArray();
                       parent.HasChild = details.Length > 0;
                       foreach (var detail in details)
                       {
                           TreeExtRow<T> row = new TreeExtRow<T>();
                           row.Value = detail;
                           row.Level = parent.Level + 1;
                           row.ParentLevel = parent.ParentLevel + "_" + type.GetProperty(RootPropertyField).GetValue(detail, null).ToString();
                           resultList.Add(row);
                           fxDetail(row);
                       }
                       return 0;
                   }
                );

            foreach (var root in roots)
            {
                TreeExtRow<T> row = new TreeExtRow<T>();
                row.ParentLevel = type.GetProperty(RootPropertyField).GetValue(root, null).ToString();
                row.Level = 0;
                row.Value = root;
                row.HasChild = false;
                resultList.Add(row);
                fxDetail(row);
            }
            return resultList.AsEnumerable();
        }


        public static TreeExtNode<T> HasChildNode<T>(this TreeExtNode<T> Parent, object ID, string IDField) where T : class
        {
            var type = typeof(T);
            TreeExtNode<T> result = null;
            Func<TreeExtNode<T>, bool> fxDetail = null;
            fxDetail = new Func<TreeExtNode<T>, bool>((parent) =>
            {
                if (parent != null && parent.Children != null && parent.Children.Count > 0)
                {
                    foreach (var child in parent.Children)
                    {
                        object idValue = type.GetProperty(IDField).GetValue(child.Value, null);
                        if ((idValue == null && ID == null) || (idValue.ToString() == ID.ToString()))
                        {
                            result = child;
                            break;
                        }
                        else
                        {
                            fxDetail(child);
                        }
                    }
                }
                return false;
            });
            fxDetail(Parent);
            return result;
        }

        public static bool InChildNode<T>(this TreeExtNode<T> Parent, object ID, string IDField, string ParentIDField) where T : class
        {
            var type = typeof(T);

            bool result = false;
            Func<TreeExtNode<T>, bool> InSub = null;

            TreeExtNode<T> toNode = null;
            object ParentID = type.GetProperty(IDField).GetValue(Parent.Value, null);
            #region insub
            InSub = (TreeExtNode<T> parent) =>
            {
                foreach (var item in parent.Children)
                {
                    object id = type.GetProperty(IDField).GetValue(item.Value, null);
                    if (id == ID)
                    {
                        toNode = item;
                        result = InChildNode(Parent, toNode, IDField, ParentIDField);
                        #region  aa
                        /*
                        #region go to root
                        var toNodeParent = toNode;
                        while( (toNodeParent=toNode.Parent) !=null)
                        {
                            var toNodeParentID = type.GetProperty(ParentIDField).GetValue(toNodeParent.Value, null);
                            if (toNodeParentID == ParentID) 
                            {
                                result = true; break;
                            }
                        }
                        #endregion
                        */
                        #endregion
                        break;
                    }
                    else
                    {
                        InSub(item);
                    }
                }
                return false;
            };
            InSub(Parent);
            #endregion
            return result;
        }
        public static bool InChildNode<T>(this TreeExtNode<T> thisNode, TreeExtNode<T> toNode, string IDField, string ParentIDField) where T : class
        {
            var type = typeof(T);
            bool result = false;
            object ParentID = type.GetProperty(IDField).GetValue(thisNode.Value, null);
            #region go to root
            var toNodeParent = toNode;
            while ((toNodeParent = toNode.Parent) != null)
            {
                var toNodeParentID = type.GetProperty(ParentIDField).GetValue(toNodeParent.Value, null);
                if (toNodeParentID == ParentID)
                {
                    result = true; break;
                }
            }
            #endregion
            return result;
        }



        public static IEnumerable<TreeExtNode<T>> CastToTreeNode<T>(this IEnumerable<T> source, string RootPropertyField, string ParentPropertyField, params int[] notThisNodeChildNodes) where T : class
        {
            List<TreeExtNode<T>> resultList = new List<TreeExtNode<T>>();

            if (source == null || source.Count() == 0) return resultList.AsEnumerable();
            var type = typeof(T);
            var ids = source.Select(s => s.GetType().GetProperty(RootPropertyField).GetValue(s, null).ToString()).Distinct().ToArray();

            var roots = source.Where(p => type.GetProperty(ParentPropertyField).GetValue(p, null) == null ||
                                      ids.Contains(type.GetProperty(ParentPropertyField).GetValue(p, null).ToString()) == false);
            Func<TreeExtNode<T>, int> fxDetail = null;

            fxDetail = new Func<TreeExtNode<T>, int>
                (
                   (parent) =>
                   {

                       object parentid = type.GetProperty(RootPropertyField).GetValue(parent.Value, null);
                       var details = source.Where(p =>
                           type.GetProperty(ParentPropertyField).GetValue(p, null) != null && //it is root 
                           type.GetProperty(ParentPropertyField).GetValue(p, null).ToString() == parentid.ToString()).ToArray();

                       foreach (var detail in details)
                       {

                           #region Not This Child Nodes
                           if (notThisNodeChildNodes != null && notThisNodeChildNodes.Length > 0)
                           {
                               object detailParentID = type.GetProperty(ParentPropertyField).GetValue(detail, null);
                               if (detailParentID != null)
                               {
                                   if (notThisNodeChildNodes.Contains((int)detailParentID))
                                   {
                                       continue;
                                   }
                               }
                           }

                           #endregion
                           TreeExtNode<T> node = new TreeExtNode<T>();
                           node.Value = detail;
                           node.Parent = parent;
                           if (parent.Children == null) parent.Children = new List<TreeExtNode<T>>();
                           parent.Children.Add(node);

                           node.Value = detail;
                           node.Level = parent.Level + 1;
                           node.ParentLevel = parent.ParentLevel + "_" + type.GetProperty(RootPropertyField).GetValue(detail, null).ToString();

                           resultList.Add(node);
                           fxDetail(node);
                       }
                       return 0;
                   }
                );
            foreach (var root in roots)
            {
                TreeExtNode<T> node = new TreeExtNode<T>();
                node.Value = root;
                node.Parent = null;
                node.Children = new List<TreeExtNode<T>>();
                node.ParentLevel = type.GetProperty(RootPropertyField).GetValue(root, null).ToString();
                node.Level = 0;
                resultList.Add(node);
                fxDetail(node);
            }
            return resultList.AsEnumerable();
        }

    }


    public class BilgiRow
    {
        public int BilgiTipID { get; set; }
        public string BilgiTipAdi { get; set; }
        public string BilgiTipCls { get; set; }
    }

    public static class DuyuruPopupTipleri
    {
        public const byte AnaSayfa = 1;

    }
    public class AjaxLoginModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
        public string ReturnUrl { get; set; }
        public string NewGuid { get; set; }
        public string NewSrc { get; set; }
    }



    public class EkAciklamaContent
    {
        public string Baslik { get; set; }
        public List<ComboModelString> Detay { get; set; }
        public EkAciklamaContent()
        {
            Detay = new List<ComboModelString>();
        }
    }
    public class FmFrDonemleri : PagerOption
    {
        public int? VeriAktarilacakSurecId { get; set; }
        public int? Yıl { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<FrFrDonemleri> Data { get; set; }
    }
    public class KmFrDonemleri : FRDonemleri
    {
        public new DateTime? BaslangicTarihi { get; set; }
        public new DateTime? BitisTarihi { get; set; }
        public bool BirimleriKopyala { get; set; }
        public bool MaddeleriKopyala { get; set; }
    }
    public class FrFrDonemleri : FRDonemleri
    {
        public string DonemYilAdi { get; set; }
        public string IslemYapan { get; set; }
        public bool AktifSurec { get; set; }
        public int SelectedTabIndex { get; set; }

    }
    public class FmFrFormOlsur : PagerOption
    {
        public string FormAdi { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<FrFrFormOlsur> Data { get; set; }
    }
    public class FrFrFormOlsur : FRFormlar
    {
        public string IslemYapan { get; set; }
        public List<Vw_BirimlerTree> FormBirimleri { get; set; }
    }
    public class FmFrFormYukle : PagerOption
    {
        public bool? FormYuklemeDurumId { get; set; }
        public bool IsAktif { get; set; }
        public int? FrDonemId { get; set; }
        public int? BirimId { get; set; }
        public string Aranan { get; set; }
        public FrFrDonemleri SurecBilgi { get; set; }
        public List<FrFrFormYukle> Data { get; set; } = new List<FrFrFormYukle>();
    }


    public class FrFrFormYukle : FRDonemlerFormGirisleri
    {
        public string FormAdi { get; set; }
        public string Aciklama { get; set; }
        public string FormDosyaAdi { get; set; }
        public string FormDosyaYolu { get; set; }
        public string IslemYapan { get; set; }
        public bool FormYuklendi { get; set; }
        public FRDonemlerFormGirisleri FrDonemlerFormGirisleri { get; set; }
    }


    public class FmBfrDonemleri : PagerOption
    {
        public int? VeriAktarilacakSurecId { get; set; }
        public int? Yıl { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<FrBfrDonemleri> Data { get; set; }
    }
    public class KmBfrDonemleri : BFRDonemleri
    {
        public new DateTime? BaslangicTarihi { get; set; }
        public new DateTime? BitisTarihi { get; set; }
        public bool BirimleriKopyala { get; set; }
        public bool MaddeleriKopyala { get; set; }
    }
    public class FrBfrDonemleri : BFRDonemleri
    {
        public string DonemYilAdi { get; set; }
        public string IslemYapan { get; set; }
        public bool AktifSurec { get; set; }
        public int SelectedTabIndex { get; set; }

    }
    public class FmBfrFormOlsur : PagerOption
    {
        public string FormAdi { get; set; }
        public bool? IsAktif { get; set; }
        public IEnumerable<FrBfrFormOlsur> Data { get; set; }


    }
    public class FrBfrFormOlsur : BFRFormlar
    {
        public string IslemYapan { get; set; }
        public List<Vw_BirimlerTree> FormBirimleri { get; set; }
    }
    public class FmBfrFormYukle : PagerOption
    {
        public bool? FormYuklemeDurumId { get; set; }
        public bool IsAktif { get; set; }
        public int? BfrDonemId { get; set; }
        public int? BirimId { get; set; }
        public string Aranan { get; set; }
        public FrBfrDonemleri SurecBilgi { get; set; }
        public List<FrBfrFormYukle> Data { get; set; } = new List<FrBfrFormYukle>();
    }


    public class FrBfrFormYukle : BFRDonemlerFormGirisleri
    {
        public string FormAdi { get; set; }
        public string Aciklama { get; set; }
        public string FormDosyaAdi { get; set; }
        public string FormDosyaYolu { get; set; }
        public string IslemYapan { get; set; }
        public bool FormYuklendi { get; set; }
        public BFRDonemlerFormGirisleri BfrDonemlerFormGirisleri { get; set; }
    }
    #endregion


}