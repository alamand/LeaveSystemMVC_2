using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class lmChangeApplicationStatus
    {
        public string name { get; set; }
        //public string applicationDate { get; set; }
        public string form { get; set; }
        //public string supportingDocs { get; set; }
        public string comments { get; set; }
        public string currentStatus { get; set; }
        public string newStatus { get; set; }
    }
}