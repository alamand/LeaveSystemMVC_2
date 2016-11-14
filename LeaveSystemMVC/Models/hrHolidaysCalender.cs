using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class hrHolidaysCalender
    {
        [DisplayName("Holiday Name ")]
        public string holidayName { set; get; }
        [DisplayName("Start Date")]
        [DataType(DataType.Date)]
        //[DisplayFormat(DataFormatString ="dd/MM/yyyy")]
        public DateTime startDate { set; get; }
        [DisplayName("End Date")]
        [DataType(DataType.Date)]
        public DateTime endDate { set; get; }
        [DisplayName("Descriptioin")]
        public string description { set; get; }

    }
}