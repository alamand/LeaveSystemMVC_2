using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class hrCreateLeave
    {
        public List <string> existingLeaveType { set; get; } //to display leave types in the database
        public string newLeaveType { set; get; } //to prompt user to enter a new leave type name
        public int holidayDuration { get; set; } // to prompt user to enter no. of days for new leave type 

    }
}