using System;
using System.Collections.Generic;
using System.Linq;
using WebApp.Models;
using WebApp.Utilities.Dtos;

namespace WebApp.Utilities.SystemData
{
    public static class ComboData
    {

        public static IEnumerable<CmbBoolDto> GetCmbAktifPasifData(bool bosSecimVar = true)
        {
            var dct = new List<CmbBoolDto>();
            if (bosSecimVar) dct.Add(new CmbBoolDto { Value = null, Caption = "" });
            dct.Add(new CmbBoolDto { Value = true, Caption = "Aktif" });
            dct.Add(new CmbBoolDto { Value = false, Caption = "Pasif" });
            return dct;

        }
        public static IEnumerable<CmbBoolDto> GetCmbAcikKapaliData(bool bosSecimVar = true)
        {
            var dct = new List<CmbBoolDto>();
            if (bosSecimVar) dct.Add(new CmbBoolDto { Value = null, Caption = "" });
            dct.Add(new CmbBoolDto { Value = true, Caption = "Kapalı" });
            dct.Add(new CmbBoolDto { Value = false, Caption = "Açık" });
            return dct;

        }

        public static IEnumerable<CmbBoolDto> GetCmbDosyaEkiDurumData(bool bosSecimVar = true)
        {
            var dct = new List<CmbBoolDto>();
            if (bosSecimVar) dct.Add(new CmbBoolDto { Value = null, Caption = "" });
            dct.Add(new CmbBoolDto { Value = true, Caption = "Dosya Eki Olanlar" });
            dct.Add(new CmbBoolDto { Value = false, Caption = "Dosya Eki Olmayanlar" });
            return dct;

        }

        public static IEnumerable<ComboModelBool> CmbEslesenEslesmeyenData(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelBool>();
            if (bosSecimVar) dct.Add(new ComboModelBool { Value = null, Caption = "" });
            dct.Add(new ComboModelBool { Value = true, Caption = "Birim Eşleştirmesi Yapılan" });
            dct.Add(new ComboModelBool { Value = false, Caption = "Birim Eşleştirmesi Yapılmayan" });
            return dct;

        }

        public static IEnumerable<ComboModelInt> CmbMaddeDurum(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt());
            dct.Add(new ComboModelInt { Value = 1, Caption = "Veri Girişi Tamamlananlar" });
            dct.Add(new ComboModelInt { Value = 0, Caption = "Veri Girişi Tamamlanmayanlar" });

            return dct;

        }

        public static IEnumerable<ComboModelInt> CmbSurecBirimDurum()
        {
            var dct = new List<ComboModelInt>
            {
                new ComboModelInt { Caption = "Tüm Birimler" },
                new ComboModelInt { Value = 1, Caption = "Veri Girişini Tamamlayan" },
                new ComboModelInt { Value = 0, Caption = "Veri Girişini Tamamlamayan" },
                new ComboModelInt { Value = 2, Caption = "Veri Onayını Tamamlayan" },
                new ComboModelInt { Value = 3, Caption = "Veri Onayını Tamamlamayan" }
            };
            return dct;

        }

        public static IEnumerable<ComboModelInt> CmbSurecMaddeDurum()
        {
            var dct = new List<ComboModelInt>
            {
                new ComboModelInt { Caption = "Tüm Maddeler" },
                new ComboModelInt { Value = 1, Caption = "Veri Girişin Tamamlanan" },
                new ComboModelInt { Value = 0, Caption = "Veri Girişin Tamamlanmayan" },
                new ComboModelInt { Value = 2, Caption = "Veri Onayı Tamamlanan" },
                new ComboModelInt { Value = 3, Caption = "Veri Onayı Tamamlanmayan" }
            };
            return dct;

        }

        public static IEnumerable<ComboModelBool> CmbVeriGirisOnayDurum(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelBool>();
            if (bosSecimVar) dct.Add(new ComboModelBool { Caption = "" });
            dct.Add(new ComboModelBool { Value = true, Caption = "Onaylandı" });
            dct.Add(new ComboModelBool { Value = false, Caption = "Onaylanmadı" });
            return dct;

        }

        public static IEnumerable<ComboModelBool> CmbFormYuklemeDurum(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelBool>();
            if (bosSecimVar) dct.Add(new ComboModelBool { Caption = "" });
            dct.Add(new ComboModelBool { Value = true, Caption = "Yüklenenler" });
            dct.Add(new ComboModelBool { Value = false, Caption = "Yüklenmeyenler" });
            return dct;

        }

        public static IEnumerable<ComboModelBool> CmbIsActiveDirectoryUserData(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelBool>();
            if (bosSecimVar) dct.Add(new ComboModelBool { Value = null, Caption = "" });
            dct.Add(new ComboModelBool { Value = true, Caption = "Active Directory" });
            dct.Add(new ComboModelBool { Value = false, Caption = "Lokal Şifre" });
            return dct;

        }


        public static IEnumerable<ComboModelBool> CmbAcikKapaliData(bool bosSecimVar = true)
        {
            var dct = new List<ComboModelBool>();
            if (bosSecimVar) dct.Add(new ComboModelBool { Value = null, Caption = "" });
            dct.Add(new ComboModelBool { Value = true, Caption = "Kapalı" });
            dct.Add(new ComboModelBool { Value = false, Caption = "Açık" });
            return dct;

        }

        public static IEnumerable<ComboModelInt> CmbSurecKayitYillari(bool bosSecimVar = true, int baslangicYil = 2018)
        {
            var dct = new List<ComboModelInt>();
            if (bosSecimVar) dct.Add(new ComboModelInt { Value = null, Caption = "" });

            for (var i = 2018; i <= DateTime.Now.Year + 1; i++)
            {
                dct.Add(new ComboModelInt { Value = i, Caption = i + " Yılı Süreci" });
            }

            return dct.OrderByDescending(o => o.Value ?? int.MaxValue).ToList();

        }


        public static IEnumerable<CmbBoolDto> GetCmbVarYokData(bool bosSecimVar = true)
        {
            var dct = new List<CmbBoolDto>();
            if (bosSecimVar) dct.Add(new CmbBoolDto { Value = null, Caption = "" });
            dct.Add(new CmbBoolDto { Value = true, Caption = "Var" });
            dct.Add(new CmbBoolDto { Value = false, Caption = "Yok" });
            return dct;

        }

        public static IEnumerable<CmbBoolDto> GetCmbEvetHayirData(bool bosSecimVar = true)
        {
            var dct = new List<CmbBoolDto>();
            if (bosSecimVar) dct.Add(new CmbBoolDto { Value = null, Caption = "" });
            dct.Add(new CmbBoolDto { Value = true, Caption = "Evet" });
            dct.Add(new CmbBoolDto { Value = false, Caption = "Hayir" });
            return dct;

        }
    }
}