using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class aDepartment
    {
        public string departmentName { get; set; }
        public List<string> existingDeparment { get; set; }
        public int departmentID { get; set; }
        public int primaryLMID { get; set; }
        public int secondaryLMID { get; set; }

    }
}