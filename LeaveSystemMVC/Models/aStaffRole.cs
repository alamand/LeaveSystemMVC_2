using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class aStaffRole
    {
        public string staffRoleName { get; set;  } //used to get and store the new entered staff role name 
        public string tempStaffRoleName { get; set; } //used to display existing staff roles in view do not delete
       

    }
}