using System.Linq;
using Database;
using WebApp.Models;
using WebApp.Utilities.Extensions;

namespace WebApp.Business
{
    public static class MesajlarBus
    {
        public static ComboModelInt GetCevaplanmamisMesajCount()
        {
            var model = new ComboModelInt();
            using (var db = new VysDBEntities())
            {
                var mesajlars = db.Mesajlars.Where(p => p.UstMesajID.HasValue == false && !p.IsAktif && p.Silindi == false).OrderByDescending(o => (o.Mesajlar1.Any() ? o.Mesajlar1.Select(s => s.Tarih).Max() : o.Tarih)).ToList();
                var htmlContent = "";
                foreach (var item in mesajlars)
                {

                    var kul = item.Kullanicilar;
                    var birimAdi = item.KullaniciID.HasValue ? db.sp_BirimAgaciGetBr(kul.BirimID).FirstOrDefault().BirimTreeAdi : "";
                    htmlContent += "<a href='javascript:void(0);' class='list-group-item' style='padding-top:0px;padding-bottom:0px;padding-left:2px;padding-right:-1px;'>" +
                                   "<table style='table-layout:fixed;width:100%;'>" +
                                   "<tr>" +
                                   "<td width='40'><img style='width:40px;height:40px;' src ='" + ((item.KullaniciID.HasValue ? item.Kullanicilar.ResimAdi : "").ToKullaniciResim()) + "' class='pull-left' ></td>" +
                                   "<td><span class='contacts-title' title='" + (birimAdi) + "'>" + item.AdSoyad + "</span><span style='float:right;font-size:8pt;'><b>" + (item.Mesajlar1.Any() ? item.Mesajlar1.Select(s => s.Tarih).Max().ToFormatDateAndTime() : item.Tarih.ToFormatDateAndTime()) + "</b></span><p><b>Konu:</b> " + item.Konu + "</p></td>" +
                                   "</tr>" +
                                   "</table>" +
                                   "</a>";
                }
                model.Value = mesajlars.Count;
                model.Caption = htmlContent;
                return model;

            }
        }
    }
}