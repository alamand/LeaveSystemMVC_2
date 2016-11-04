using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class subordinateListModel
    {
        public List<minEmployee> employeeList;
    }

    public struct minEmployee
    {
        public int empID { get; set; }
        public string empName { get; set; }
}