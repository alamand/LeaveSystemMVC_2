using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LeaveSystemMVC.Models
{
    public class Holiday
    {
        public int holidayID { get; set; }

        [DisplayName("Holiday Name ")]
        [Required(ErrorMessage = "Holiday name is required.")]
        [StringLength(30, MinimumLength = 0, ErrorMessage = "Holiday name is too long.")]
        public string holidayName { set; get; }

        [Required(ErrorMessage = "Holiday date is required.")]
        [DisplayName("Date")]
        [DataType(DataType.Date)]
        public DateTime date { set; get; }
    }
}