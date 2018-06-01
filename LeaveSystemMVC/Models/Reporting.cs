using System;
using System.ComponentModel;

namespace LeaveSystemMVC.Models
{
    public class Reporting
    {
        [DisplayName("Reporting ID ")]
        public int reportToID { get; set; }

        [DisplayName("Employee ID ")]
        public int employeeID { get; set; }

        [DisplayName("From Reporting ID ")]
        public int? fromID { get; set; }

        [DisplayName("To Reporting ID ")]
        public int? toID { get; set; }

        [DisplayName("Substitution Level ")]
        public int? subLevel { get; set; }

        [DisplayName("Is Active? ")]
        public bool? isActive { get; set; }

        [DisplayName("Employee Name ")]
        public String employeeName { get; set; }
        
    }
}