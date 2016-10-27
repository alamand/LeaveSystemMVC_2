using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class lmPendingApplications
    {
        public string name { get; set; }
        public string applicationDate { get; set;  }
        public string form { get; set; }
        public string supportingDocs { get; set; }
        public string comments { get; set; }
        public int annualBalance { get; set; }
        public int sickBalance { get; set; }
        public int commpassionate { get; set; }
        public string shortLeaveBalance { get; set; }
        public int maternityBalance { get; set; }
        public int daysInLieuTotal { get; set; }
        public string status { get; set; } 
    }
}