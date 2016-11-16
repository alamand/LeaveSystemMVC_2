using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class aStaffRole
    {
        [Display(Name = "Staff Role")]
        [Required(ErrorMessage = "The New Staff Role is required before submitting form")]
        public string staffRoleName { get; set;  }
        public List <string> existingStaffRole {set;get;}

    }
}