using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class addStaff
    {
        public int staffID { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
        public string joinDate { get; set; }
        public string gender { get; set; }
        public string designation { get; set; }
        public string department { get; set; }
        public string staffRole { get; set; }
        public string optionalStaffRole { get; set; }
        public string secondLM { get; set; }
        public string optionalSecdondStaffRole { get; set; }
        public string resignationDate { get; set; }
        public bool isActiveStaff { get; set; }
    }
}