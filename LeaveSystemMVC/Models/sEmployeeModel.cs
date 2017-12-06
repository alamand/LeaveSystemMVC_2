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
        [Required(ErrorMessage = "5-digit Staff ID is required")]
        public int staffID { get;set; }

        [Display(Name = "Staff ID")]
        [Required(ErrorMessage = "5-digit Staff ID is required")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Staff ID must be numeric")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "Staff ID must be 5 digits")]
        public string staffIDInString { get; set; }

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "First Name is required")]
        [StringLength(25, MinimumLength = 1, ErrorMessage = "First Name is too long")]
        public string firstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "Last Name is required")]
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
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address, please check if " + "@" + " symbol exists")]
        [StringLength(100, MinimumLength = 0, ErrorMessage = "E-mail address is too long")]
        public string email { get; set; }

        public char gender { get; set; }

        public int deptId { get; set; }

        public Dictionary<int, string> departmentList { get; set; } = new Dictionary<int, string>();

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
 
        [Display(Name = "Staff Role")]
        public string staffType { get; set; }

        [Display(Name = "Optional Staff Role")]
        public string optionalStaffType { get; set; }

        public string optional2ndStaffType { get; set; }

        public Dictionary<int, string> staffTypeSelectionOptions { get; set; } = new Dictionary<int, string>();

        [Display(Name = "Account Status")]
        public bool accountStatus { get; set; }

        [Display(Name = "Administrator User")]
        public bool isAdmin { get; set; }

        [Display(Name = "Probation Status")]
        public bool onProbation { get; set; }

        public Dictionary<int, string> lineManagerSelectionOptions { get; set; } = new Dictionary<int, string>();

        public string reportsToLineManagerString { get; set; }

        public string religionString { get; set; }

        public string nationalityString { get; set; }

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Date of Birth is required.")]
        public DateTime empDateOfBirth { get; set; }

    }
}