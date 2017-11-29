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
        [Display(Name = "Staff ID")]
        [Required(ErrorMessage = "The 5-digit Staff ID is required")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Staff ID must be numeric")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "The Staff ID must be 5 digits")]
        public string staffIDInString { get; set; }

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "The First Name is required")]
        [StringLength(25, MinimumLength = 1, ErrorMessage = "First Name is too long")]
        public string firstName { get; set; }
        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "The Last Name is required")]
        [StringLength(25, MinimumLength = 1, ErrorMessage = "Last Name is too long")]
        public string lastName { get; set; }

        [Display(Name = "Username")]
        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "User Name is too long")]
        public string userName { get; set; }
        public string password { get; set; }
        [StringLength(100, MinimumLength = 0, ErrorMessage = "Designation name is too long")]
        public string designation{ get; set; }

        [Display(Name = "Email address")]
        [Required(ErrorMessage = "The email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address, please check if " + "@" + " symbol exists")]
        [StringLength(100, MinimumLength = 0, ErrorMessage = "E-mail address is too long")]
        public string email { get; set; }
        public char gender { get; set; }
        public int deptId { get; set; }
        //DepartmentList will hold a list of departments, it will be used
        //to for display purposes, to give users a list of departments to
        //choose from
        public Dictionary<int, string> departmentList { get; set; } = new Dictionary<int, string>();
        //[Required(ErrorMessage = "A Department is required")]
        public string deptName { get; set; }
        [Display(Name = "Phone Number")]
        [Required(ErrorMessage = "You must provide your home phone number.")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "You must provide a proper phone number.")]
        [RegularExpression(@"^(?:\+971|00971|0)?(?:50|51|52|55|56|2|3|4|6|7|9)\d{7}$", ErrorMessage = "You must provide a proper phone number.")]
        public string phoneNo{ get; set; }

        [Required(ErrorMessage = "The Start Date is required")]
        [DataType(DataType.Date)]
        public DateTime empStartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime empEndDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime empOldStartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime empOldEndDate { get; set; }

        public string secondLineManager { get; set; }
        public Dictionary<int, string> SecondLMSelectionOptions { get; set; } = new Dictionary<int, string>();
        //Would make more sense for the below to be
        //staffRole -- not staffType since in db it's
        //staff roles, not types
        //
        /*Holds the actual staff type/role. In the case of
         views specifically, these will hold the user's selection
         of staff type/role in dropdown lists*/
        [Display(Name = "Staff Role")]
        public string staffType { get; set; }
        [Display(Name = "Optional Staff Role")]
        public string optionalStaffType { get; set; }
        public string optional2ndStaffType { get; set; }
        /*Will hold a list of options of staff roles/types for the 
         user to choose from*/
        public Dictionary<int, string> staffTypeSelectionOptions { get; set; } = new Dictionary<int, string>();

        public bool accountStatus { get; set; }

        [Display(Name = "Administrator User")]
        public bool isAdmin { get; set; }

        [Display(Name = "Probation Status")]
        public bool onProbation { get; set; }

        public Dictionary<int, string> lineManagerSelectionOptions { get; set; } = new Dictionary<int, string>();

        public string reportsToLineManagerString { get; set; }
    }
}