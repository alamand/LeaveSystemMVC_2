using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class hrCreateLeave
    {
        public List <string> existingLeaveType { set; get; }

        [Display(Name = "New Leave Type")]
        [Required(ErrorMessage = "The New Leave Type is required")]
        public string newLeaveType { set; get; }

        public int leaveID { set; get; }

        [Display(Name = "Duration of Leave")]
        [Required(ErrorMessage = "The New Leave Type Duration is required")]
        public int holidayDuration { get; set; }

    }
}