using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class LeaveApplicationModel
    {
        public String leaves { get; set; }

        public DateTime date { get; set; }
        public DateTime endDate { get; set; }
        public DateTime returnDate { get; set; }
        public int lShortStartHour { get; set; }
        public int lShortStartMinute { get; set; }
        public int lShortEndHour { get; set; }
        public int lShortEndMinute { get; set; }

        public string comments { get; set; }
        public string supportingDocs { get; set; }
        public bool bookAirTicket { get; set; }
    }
}