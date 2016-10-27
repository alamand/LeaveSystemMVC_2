using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class lmSubordinateLeaveBalance
    {
        public int staffID { get; set; }
        public string name { get; set; }
        public int annualBalance { get; set; }
        public int sickBalance { get; set; }
        public int commpassionate { get; set; }
        public string shortLeaveBalance { get; set; }
        public int maternityBalance { get; set; }
        public int daysInLieuTotal { get; set; }
    }
}