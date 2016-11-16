using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class aDepartment
    {
        [Display(Name = "Department Name")]
        [Required(ErrorMessage = "The New Department Name is required before submitting the form")]
        public string departmentName { get; set; }

        public List<string> existingDeparment { get; set; }

        public string departmentID { get; set; }
        public string primaryLMID { get; set; }
        public string secondaryLMID { get; set; }

    }
}