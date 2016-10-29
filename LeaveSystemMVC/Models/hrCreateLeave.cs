using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class hrCreateLeave
    {
        public List <string> existingLeaveType { set; get; }
        public string newLeaveType { set; get; }
        public int leaveID { set; get; }
        public int holidayDuration { get; set; }

    }
}