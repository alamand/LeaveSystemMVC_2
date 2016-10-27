using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class leaveForm
    {
        public string leaveType { get; set; }


       // [DataType(DataType.Date)]
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public DateTime returnDate { get; set; }
        public DateTime startShort { get; set; }
        public DateTime endShort { get; set; }
        public int contact { get; set; }
        public string comment { get; set; }
        public string document { get; set; }
        public Boolean airTicket { get; set; }

    }
}