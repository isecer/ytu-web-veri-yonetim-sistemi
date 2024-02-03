using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Utilities.MessageBox
{
    public enum Msgtype
    {
        Success, Error, Warning, Information, Nothing
    }
    public class MrMessage
    {
         
        public bool IsSucces { get; set; } 
        public string PropertyName { get; set; }
        public bool AddIcon { get; set; }
        public string HtmlData { get; set; }
        public List<int> ReturnIds { get; set; }
        public Msgtype MessageType { get; set; }
        public MrMessage()
        {
            AddIcon = true;
            MessageType = Msgtype.Nothing;
        }
    }
    public class MmMessage
    {

        public bool IsSuccess { get; set; }
        public Msgtype MessageType { get; set; } = Msgtype.Nothing;

        public string Title { get; set; }
        public string ReturnUrl { get; set; }
        public string ReturnHtml { get; set; }
        public int ReturnUrlTimeOut { get; set; } = 400;
        public List<string> Messages { get; set; } = new List<string>();
        public List<MrMessage> MessagesDialog { get; set; } = new List<MrMessage>();

    }


}