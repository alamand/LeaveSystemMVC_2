using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class sChangePasswordModel
    {


        public string oldPassword { get; set; }
        public string newPassword { get; set; }
        public string confirmPassword { get; set;  }
    }
}