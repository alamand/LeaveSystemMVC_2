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
        [Required]
        public int UserID { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        public string Name { get; set; }
    }
}