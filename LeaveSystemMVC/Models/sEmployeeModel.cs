using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LeaveSystemMVC.CustomLibraries;

namespace LeaveSystemMVC.Models
{
    public class sEmployeeModel
    {
        [Display(Name = "Staff ID")]
        [Required(ErrorMessage = "The 5-digit Staff ID is required")]
        public int staffID { get;set; }

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "The First Name is required")]
        public string firstName { get; set; }
        public string lastName { get; set; }

        [Display(Name = "Username")]
        [Required(ErrorMessage = "Username is required")]
        public string userName { get; set; }
        public string password { get; set; }
        public string designation{ get; set; }

        [Display(Name = "Email address")]
        [Required(ErrorMessage = "The email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address, please check if " + "@" + " symbol exists")]
        public string email { get; set; }
        public string gender { get; set; }
        public int deptId { get; set; }
        //DepartmentList will hold a list of departments, it will be used
        //to for display purposes, to give users a list of departments to
        //choose from
        public Dictionary<int, string> departmentList { get; set; } = new Dictionary<int, string>();
        public string deptName { get; set; }
        public string phoneNo{ get; set; }

        [DataType(DataType.Date)]
        public DateTime empStartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime empEndDate { get; set; }


        public string secondLineManager { get; set; }
        public Dictionary<int, string> SecondLMSelectionOptions { get; set; } = new Dictionary<int, string>();
        //Would make more sense for the below to be
        //staffRole -- not staffType since in db it's
        //staff roles, not types
        //
        /*Holds the actual staff type/role. In the case of
         views specifically, these will hold the user's selection
         of staff type/role in dropdown lists*/
        public string staffType { get; set; }
        public string optionalStaffType { get; set; }
        public string optional2ndStaffType { get; set; }
        /*Will hold a list of options of staff roles/types for the 
         user to choose from*/
        public Dictionary<int, string> staffTypeSelectionOptions { get; set; } = new Dictionary<int, string>();
        //public IEnumerable<SelectListItem> staffTypeSelectionOptions { get; set; }
        //

        public bool accountStatus { get; set; }

        [Display(Name = "Administrator User")]
        public bool isAdmin { get; set; }
    }
}