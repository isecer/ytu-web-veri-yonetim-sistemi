using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Web;
using BiskaUtil;
using Database;

namespace WebApp.Models
{
   
 
    public static class ExcelDataImportModel
    {
        private static string ToConnectionStringXls(this string Path)
        {
            return "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Path + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
        }

        private static string ToConnectionStringXlsx(this string Path)
        {
            return "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Path + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
        }

     

    }
}