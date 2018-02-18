using System;
using System.ComponentModel.DataAnnotations;

namespace LeaveSystemMVC.Models
{
    public class hrDaysInLieu
    {
        public int employeeID { set; get; }

        [Required(ErrorMessage = "Please select a duration.")]
        public decimal numOfDays { set; get; }

        [Required(ErrorMessage = "Please select a date.")]
        public DateTime date { set; get; }

        [StringLength(150, MinimumLength = 0, ErrorMessage = "Comment is too long.")]
        public string comment { set; get; }

    }
}