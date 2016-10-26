using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class HolidaysCalendar
    {
        public DateTime[] currentHolidays { get; set; }
        public DateTime[] newHolidays { get; set; }
        public string newHolidaysDescription { get; set; }
    }
}