using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LeaveSystemMVC.Models
{
    public class Users
    {
        [Required(ErrorMessage = "You forgot to enter a username.")]
        public int UserID { get; set; }

        [Required(ErrorMessage = "You forgot to enter a password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
    }


}