using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace LeaveSystemMVC.Models
{
    public class hrDaysInLieu
    {
        [Required(ErrorMessage = "Please select a duration.")]
        public decimal NumDays { set; get; }
        public int Employee_ID { set; get; }

        [Required(ErrorMessage = "Please select a date.")]
        public DateTime Date { set; get; }
        public string Comment { set; get; }

    }
}