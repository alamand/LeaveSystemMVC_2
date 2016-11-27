using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class selectSubstitute
    {
        public string substituteStaffID { get; set; }
        public string substituteStaffName { get; set; }//not required
        public bool toTransferBack { get; set; }
        public Dictionary<int, string> substituteListOptions { get; set; } = new Dictionary<int, string>();  
    }
}