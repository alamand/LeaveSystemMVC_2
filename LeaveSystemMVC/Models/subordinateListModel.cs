using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class subordinateListModel
    {
        public List<minEmployee> employeeList { get; set; }
    }

    public class minEmployee
    {
        public int empID;
        public string empName;
    }
}