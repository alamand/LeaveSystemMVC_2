using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class sEmployeeModel
    {
        public int staffID { get;set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
        public string designation{ get; set; }
        public string email { get; set; }
        public string gender { get; set; }
        public int deptId { get; set; }
        public string deptName { get; set; }
        public string phoneNo{ get; set; }
        public DateTime empStartDate { get; set; }
        public DateTime empEndDate { get; set; }
        public int secondLineManager { get; set; }
        public string staffType { get; set; }
        public string optionalStaffType { get; set; }
        public string optional2ndStaffType { get; set; }
        public bool accountStatus { get; set; }
    }
}