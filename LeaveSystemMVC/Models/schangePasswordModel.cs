using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace LeaveSystemMVC.Models
{
    public class sChangePasswordModel
    {

        [Required]
        [DisplayName("Enter old Password")]
        public string oldPassword { get; set; }

        [Required]
        [DisplayName("Enter New Password")]
        public string newPassword { get; set; }

        [DisplayName("Re-enter New Password")]
        [Required]
        public string confirmPassword { get; set;  }
    }
}