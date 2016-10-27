using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class leaveHistory
    {
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string applicationDate { get; set; }
        public int leaveDuration { get; set; }
        public string leaveType { get; set; }
        public string leaveStatus { get; set;  }
        public string supportingDoc { get; set; }
        public string form { get; set; }
        public int unpaidLeave { get; set; }

    }
}