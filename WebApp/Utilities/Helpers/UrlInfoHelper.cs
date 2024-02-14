using System;
using System.Linq;
using WebApp.Models;
using WebApp.Utilities.SystemSetting;

namespace WebApp.Utilities.Helpers
{
    public static class UrlInfoHelper
    {
        public static UrlInfoModel ToUrlInfo(this Uri uri)
        {
            var model = new UrlInfoModel
            {
                Root = GlobalSistemSetting.GetRoot(),
                AbsolutePath = uri.AbsolutePath.Replace("I", "i").ToLower()
            };
            var spl = model.AbsolutePath.Split('/').Where(p => p != "").ToList();

            if (spl.Count == 0)
            {
                model.LastPath = model.Root + "home" + "/index";
            }

            else if (spl.Count >= 1)
            {
                model.LastPath = model.Root + spl[0] + "/index";
            }
            model.Query = uri.Query;



            return model;
        }
    }
}