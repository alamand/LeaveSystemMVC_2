using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace LeaveSystemMVC.Models
{
    public class minStaff
    {
        public int deptID { get; set; }
        [Required(ErrorMessage = "Employee selection is required")]
        public int empID { get; set; }
        [Required(ErrorMessage = "Employee selection is required")]
        public string empIDAsString { get; set; }
        public string empName { get; set; }
    }
    
}