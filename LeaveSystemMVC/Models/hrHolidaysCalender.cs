using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LeaveSystemMVC.Models
{
    public class hrHolidaysCalender
    {
        public int holidayID { get; set; }

        [Required]
        [DisplayName("Holiday Name ")]
        [StringLength(30, MinimumLength = 0, ErrorMessage = "Holiday name is too long.")]
        public string holidayName { set; get; }

        [Required]
        [DisplayName("Date")]
        [DataType(DataType.Date)]
        //[DisplayFormat(DataFormatString ="dd/MM/yyyy")]
        public DateTime date { set; get; }
    }
}