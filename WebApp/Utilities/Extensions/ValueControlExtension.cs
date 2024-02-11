using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace WebApp.Utilities.Extensions
{
    public static class ValueControlExtension
    {
        public static bool ToIsValidEmail(this string email)
        {
            var isSuccess = !Regex.IsMatch(email,
                @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,24}))$",
                RegexOptions.IgnoreCase);
            if (!isSuccess) isSuccess = !email.IsASCII();
            return isSuccess;
        }

        public static List<string> ToIkiKarakterArasiKodBul(this string body, string start, string end)
        {
            var matched = new List<string>(); 
            var exit = false;
            while (!exit)
            {
                var indexStart = body.IndexOf(start, StringComparison.Ordinal);
                var indexEnd = body.IndexOf(end, StringComparison.Ordinal);
                if (indexStart != -1 && indexEnd != -1)
                {
                    matched.Add(body.Substring(indexStart + start.Length, indexEnd - indexStart - start.Length));
                    body = body.Substring(indexEnd + end.Length);
                }
                else
                {
                    exit = true;
                }
            }

            return matched;
        }



        public static List<string> ToMaddeKodlariniBul(this string input)
        {
            var codes = new List<string>();

            // Regex deseni tanımlama: @ ile başlayan ve ardından bir veya daha fazla rakam veya harf içeren bloklar
            var pattern = @"@(\w+)";

            // Girdi içindeki tüm eşleşmeleri bulma
            var matches = Regex.Matches(input, pattern);

            // Her eşleşmenin değerini listeye ekleme
            foreach (Match match in matches)
            {
                codes.Add(match.Groups[1].Value);
            }

            return codes;
        }
        //public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);

        //public static bool IsNullOrWhiteSpace(this string str) => string.IsNullOrWhiteSpace(str);

        //public static bool ToIsValidEmail(this string email)
        //{
        //    var isSuccess = !Regex.IsMatch(email,
        //        @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
        //        @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,24}))$",
        //        RegexOptions.IgnoreCase);
        //    if (!isSuccess) isSuccess = !email.IsASCII();
        //    return isSuccess;
        //}
        //public static bool IsSpecialCharacterCheck(this string gelenStr)
        //{
        //    var regexItem = new Regex("^[a-zA-Z0-9 ]*$");
        //    return regexItem.IsMatch(gelenStr);
        //}
        //public static bool IsImage(this string uzanti)
        //{
        //    var imagesTypes = new List<string>
        //    {
        //        "Png",
        //        "Jpg",
        //        "Bmp",
        //        "Tif",
        //        "Gif"
        //    };
        //    return imagesTypes.Contains(uzanti);
        //}
    }
}
