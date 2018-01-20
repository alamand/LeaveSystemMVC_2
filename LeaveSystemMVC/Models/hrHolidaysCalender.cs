using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LeaveSystemMVC.Models
{
    public class hrHolidaysCalender
    {
        [Required]
        [DisplayName("Holiday Name ")]
        public string holidayName { set; get; }

        [Required]
        [DisplayName("Date")]
        [DataType(DataType.Date)]
        //[DisplayFormat(DataFormatString ="dd/MM/yyyy")]
        public DateTime startDate { set; get; }

        [Required]
        [DisplayName("End Date")]
        [DataType(DataType.Date)]
        public DateTime endDate { set; get; }

        [DisplayName("Description")]
        public string description { set; get; }

    }
}