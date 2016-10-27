using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class BalancesModel
    {
        public int annualBalance { get; set; }
        public int sickBalance { get; set; }
        public int compassionateBalance { get; set; }
        public int shortHours { get; set; }
        public int shortMinutes { get; set; }
        public int daysInLieu { get; set; }
        public int unpaidBalance { get; set; }

    }
}