using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class aDepartment
    {

        public string existingDepartmentName { get; set; } //used to display existing department names in view do not delete
        public string departmentName { get; set; }
        public string departmentID { get; set; } //not required not being used in view or controller.. delete at your own risk
        public string primaryLMID { get; set; } //holds employee ID of the primary line manager
        public Dictionary<int, string> linemanagerSelectionListOptions { get; set; } = new Dictionary<int, string>();  //holds a list of names and IDs of line managers to choose from.

    }
}