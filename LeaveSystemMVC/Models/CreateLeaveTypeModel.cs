using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class CreateLeaveTypeModel
    {
        public string existingLeaveTypes { get; set; }
        public string addLeaveType { get; set; }
        public int leaveTypeBalance { get; set; } //how many days/hours of this leave type everyone will be allowed.
    }
}