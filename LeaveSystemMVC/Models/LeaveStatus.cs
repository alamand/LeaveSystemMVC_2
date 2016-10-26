using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class LeaveStatus
    {
        public DateTime formSubmitDate { get; set; }
        public string status { get; set; }
        public DateTime leaveStartDate { get; set; }
        public DateTime leaveEndDate { get; set; }
        public string Form { get; set; }
        public string supportingDocument { get; set; }
    }
}