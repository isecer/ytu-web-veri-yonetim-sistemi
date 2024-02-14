using BiskaUtil;
using Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using WebApp.Business;
using WebApp.Utilities.Extensions;
using WebApp.Utilities.Helpers;
using WebApp.Utilities.SystemSetting;

namespace WebApp.Models
{
    public class MdlMailMainContent
    {
        public string LogoPath { get; set; }
        public string UniversiteAdi { get; set; }
        public string Content { get; set; }

    }
    public class MailTableContent
    {
        public string AciklamaBasligi { get; set; }
        public string AciklamaDetayi { get; set; }
        public bool AciklamaTextAlingCenter { get; set; } = false;
        public string GrupBasligi { get; set; }
        public int CaptTdWidth { get; set; } = 200;
        public List<MailTableRow> Detaylar { get; set; } = new List<MailTableRow>();
        public bool Success { get; set; }
    }
    public class MailTableRow
    {
        public bool Colspan2 { get; set; } = false;
        public int SiraNo { get; set; }
        public string Baslik { get; set; }
        public string Aciklama { get; set; }
    }
    public static class MailManager
    {
        public static void SendMail(int gonderilenMailId, string konu, string icerik, List<string> emails, List<Attachment> attach, bool toOrBcc = true)
        {

            #region sendMail
            var uid = UserIdentity.Current.Id;
            var uIp = UserIdentity.Ip;
            new System.Threading.Thread(() =>
            {
                try
                {
                    using (var dbb = new VysDBEntities())
                    {
                        var qeklenen = dbb.GonderilenMaillers.First(p => p.GonderilenMailID == gonderilenMailId);
                        try
                        {
                            sendMail(konu, icerik, emails, attach, toOrBcc);
                            qeklenen.Gonderildi = true;
                            qeklenen.IslemTarihi = DateTime.Now;
                        }
                        catch (Exception ex)
                        {
                            qeklenen.HataMesaji = ex.ToExceptionMessage();
                            SistemBilgilendirmeBus.SistemBilgisiKaydet("Mail gönderim işlemi yapılamadı! Hata: " + ex.ToExceptionMessage(), ex.ToExceptionStackTrace(), BilgiTipi.Hata, uid, uIp);
                        }
                        dbb.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    SistemBilgilendirmeBus.SistemBilgisiKaydet("Mail gönderim işlemi sırasında bir hata oluştu! Hata: " + ex.ToExceptionMessage(), ex.ToExceptionStackTrace(), BilgiTipi.Hata, uid, uIp);
                }
            }).Start();

            #endregion
        }

        private static Exception SendMailRetVal(string konu, string icerik, string email, List<Attachment> attach, bool toOrBcc = true)
        {
            Exception exRet = null;

            try
            {
                MailManager.sendMail(konu, icerik, email, attach, toOrBcc);

            }
            catch (Exception ex)
            {
                exRet = ex;
            }

            return exRet;
        }
        public static Exception sendMailRetVal(string konu, string icerik, List<string> email, List<Attachment> attach, bool toOrBcc = true)
        {
            Exception exRet = null;

            try
            {
                MailManager.sendMail(konu, icerik, email, attach, toOrBcc);

            }
            catch (Exception ex)
            {
                exRet = ex;
            }

            return exRet;
        }
        public static bool sendMail(string Baslik, string Body, string GonderilecekMailAdresi, List<Attachment> attach, bool ToOrBcc = true)
        {
            var EmailAdresi = SistemAyar.GetAyar(SistemAyar.AyarSmtpMail);
            var Name = SistemAyar.GetAyar(SistemAyar.AyarSmtpUser);
            var Sifre = SistemAyar.GetAyar(SistemAyar.AyarSmtpPassword);
            var Port = SistemAyar.GetAyar(SistemAyar.AyarSmtpPort);
            var Host = SistemAyar.GetAyar(SistemAyar.AyarSmtpHost);
            var SSL = SistemAyar.GetAyar(SistemAyar.AyarSmtpSsl);

            using (var ePosta = new MailMessage())
            {
                ePosta.From = new MailAddress(EmailAdresi, Name, System.Text.Encoding.UTF8);
                ePosta.IsBodyHtml = true;
                ePosta.To.Add(GonderilecekMailAdresi);

                ePosta.Subject = Baslik;
                ePosta.Body = Body;
                ePosta.BodyEncoding = System.Text.Encoding.UTF8;
                ePosta.Priority = MailPriority.High;


                if (attach != null)
                    foreach (var item in attach)
                        ePosta.Attachments.Add(item);
                var smtp = new SmtpClient();
                smtp.Credentials = new System.Net.NetworkCredential(EmailAdresi, Sifre);
                smtp.Port = Port.ToInt().Value;
                smtp.Host = Host;
                smtp.EnableSsl = SSL.ToBoolean(false);
                smtp.Timeout = 5 * 60 * 1000;
                smtp.Send(ePosta);
            }
            return true;



        }
        public static bool sendMail(string Baslik, string Body, List<string> GonderilecekMailAdresleri, List<Attachment> attach, bool ToOrBcc)
        {

            var EmailAdresi = SistemAyar.GetAyar(SistemAyar.AyarSmtpMail);
            var Name = SistemAyar.GetAyar(SistemAyar.AyarSmtpUser);
            var Sifre = SistemAyar.GetAyar(SistemAyar.AyarSmtpPassword);
            var Port = SistemAyar.GetAyar(SistemAyar.AyarSmtpPort);
            var Host = SistemAyar.GetAyar(SistemAyar.AyarSmtpHost);
            var SSL = SistemAyar.GetAyar(SistemAyar.AyarSmtpSsl);

            using (var ePosta = new MailMessage())
            {
                ePosta.From = new MailAddress(EmailAdresi, Name, System.Text.Encoding.UTF8);
                ePosta.IsBodyHtml = true;
                if (GonderilecekMailAdresleri.Count == 1 || ToOrBcc)
                    foreach (var item in GonderilecekMailAdresleri)
                        ePosta.To.Add(item);
                else
                    foreach (var item in GonderilecekMailAdresleri)
                        ePosta.Bcc.Add(item);
                ePosta.Subject = Baslik;
                ePosta.Body = Body;
                ePosta.BodyEncoding = System.Text.Encoding.UTF8;
                ePosta.Priority = MailPriority.High;
                if (attach != null)
                    foreach (var item in attach)
                        ePosta.Attachments.Add(item);
                var smtp = new SmtpClient();
                smtp.Credentials = new System.Net.NetworkCredential(EmailAdresi, Sifre);
                smtp.Port = Port.ToInt().Value;
                smtp.Host = Host;
                smtp.EnableSsl = SSL.ToBoolean(false);
                smtp.Timeout = 5 * 60 * 1000;
                smtp.Send(ePosta);

            }
            return true;


        }


        public static Exception YeniHesapMailGonder(Kullanicilar kModel, string sfr)
        {

            using (var db = new VysDBEntities())
            {
                var _ea = SistemAyar.GetAyar(SistemAyar.AyarSistemErisimAdresi);

                var mRowModel = new List<MailTableRow>();

                var WurlAddr = _ea.Split('/').ToList();
                if (_ea.Contains("//"))
                    _ea = WurlAddr[0] + "//" + WurlAddr.Skip(2).Take(1).First();
                else
                    _ea = "http://" + WurlAddr.First();
                mRowModel.Add(new MailTableRow { Baslik = "Ad Soyad", Aciklama = kModel.Ad + " " + kModel.Soyad });

                //if (kModel.BirimKod.IsNullOrWhiteSpace() == false)
                //{
                //    var birim = db.Birimlers.Where(p => p.BirimKod == kModel.BirimKod).First();
                //    mRowModel.Add(new mailTableRow { Baslik = "Birim Adı", Aciklama = birim.BirimAdi });
                //}
                //if (kModel.UnvanID.HasValue)
                //{
                //    var unvan = db.Unvanlars.Where(p => p.UnvanID == kModel.UnvanID).First();
                //    mRowModel.Add(new mailTableRow { Baslik = "Ünvan", Aciklama = unvan.UnvanAdi });
                //}
                if (kModel.Tel.IsNullOrWhiteSpace() == false) mRowModel.Add(new MailTableRow { Baslik = "Cep Tel", Aciklama = kModel.Tel });

                mRowModel.Add(new MailTableRow { Baslik = "Kullanıcı Adı", Aciklama = kModel.KullaniciAdi });
                mRowModel.Add(new MailTableRow { Baslik = "Şifre", Aciklama = kModel.IsActiveDirectoryUser ? "Yıldız Email şifreniz ile aynı" : sfr });
                mRowModel.Add(new MailTableRow { Baslik = "Sisteme Erişim Adresi", Aciklama = "<a href='" + _ea + "' target='_blank'>" + _ea + "</a>" });
                var mmmC = new MdlMailMainContent();
                var mtc = new MailTableContent() { AciklamaBasligi = "Kullanıcı hesabınız oluşturuldu. Sisteme Giriş Bilgisi Aşağıdaki Gibidir.", Detaylar = mRowModel };
                var tavleContent = ViewRenderHelper.RenderPartialView("Ajax", "getMailTableContent", mtc);
                mmmC.Content = tavleContent;
                mmmC.LogoPath = _ea + "/Content/assets/images/ytu_logo_tr.png";
                mmmC.UniversiteAdi = "YILDIZ TEKNİK ÜNİVERSİTESİ";
                string htmlMail = ViewRenderHelper.RenderPartialView("Ajax", "getMailContent", mmmC);
                var User = SistemAyar.GetAyar(SistemAyar.AyarSmtpUser);
                var snded = MailManager.SendMailRetVal(User, htmlMail, kModel.EMail, null);
                return snded;


            }
        }
    }



}