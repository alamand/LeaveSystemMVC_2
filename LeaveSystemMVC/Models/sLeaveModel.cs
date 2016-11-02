using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class sLeaveModel
    {

        public String staffName { get; set; }

        public String leaveType { get; set; }

        public DateTime applicationDate { get; set; }

        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }

        public DateTime returnDate { get; set; }

        public int leaveDuration { get; set; }

        public DateTime shortStartTime { get; set; }
        public DateTime shortEndTime { get; set; }
        
        public string lmComment { get; set; }
        public string hrComment { get; set; }

        public int leaveStatus { get; set; }

        public string comments { get; set; }
        public string supportingDocs { get; set; }
        public bool bookAirTicket { get; set; }

        public string contactDetails { get; set; }
    }
}