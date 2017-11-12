
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Models
{
    public class EmpLeaveBalModel
    {
        public sleaveBalanceModel leaveBalance { get; set; }
        public sEmployeeModel employee { get; set; }
    }
}