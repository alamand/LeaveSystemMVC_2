using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class sleaveBalanceModel
    {


        public int annual { get; set; } = 0;
        public int sick { get; set; } = 0;
        public int compassionate { get; set; } = 0;
        public int maternity { get; set; } = 0;
        public int daysInLieue { get; set; } = 0;
        public int unpaidTotal { get; set; } = 0;
        public double shortLeaveHours { get; set; } = 0;
    }
}