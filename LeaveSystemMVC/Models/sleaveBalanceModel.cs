using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class sleaveBalanceModel
    {


        public int annual { get; set; }
        public int sick { get; set; }
        public int compassionate { get; set; }
        public int maternity { get; set; }
        public int daysInLieue { get; set; }
        public int unpaidTotal { get; set; }
        public double shortLeaveHours { get; set; }
    }
}