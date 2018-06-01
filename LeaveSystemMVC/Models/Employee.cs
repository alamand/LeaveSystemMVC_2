using System;
using System.ComponentModel.DataAnnotations;

namespace LeaveSystemMVC.Models
{
    public class Employee
    {
        [Display(Name = "Staff ID")]
        [Required(ErrorMessage = "5-digit Staff ID is required.")]
        [DataType(DataType.Text)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Staff ID must be numeric.")]
        [Range(10000, 99999, ErrorMessage = "Staff ID must be 5 digits")]
        public int? staffID { get;set; }

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(25, MinimumLength = 1, ErrorMessage = "First Name is too long.")]
        public string firstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(25, MinimumLength = 1, ErrorMessage = "Last Name is too long.")]
        public string lastName { get; set; }

        [Display(Name = "User Name")]
        [Required(ErrorMessage = "User Name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "User Name is too long")]
        public string userName { get; set; }

        [StringLength(20, MinimumLength = 1, ErrorMessage = "User Name is too long")]
        public string password { get; set; }

        [Display(Name = "Designation")]
        [Required(ErrorMessage = "Designation is required.")]
        [StringLength(50, MinimumLength = 0, ErrorMessage = "Designation is too long.")]
        public string designation{ get; set; }

        [Display(Name = "Email address")]
        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [StringLength(100, MinimumLength = 0, ErrorMessage = "Email Address is too long.")]
        public string email { get; set; }

        [Display(Name = "Gender")]
        public char gender { get; set; }

        public int? deptID { get; set; }

        [Display(Name = "Phone Number")]
        [Required(ErrorMessage = "Phone Number is required.")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "Invalid Phone Number.")]
        [RegularExpression(@"^(?:\+971|00971|0)?(?:50|51|52|55|56|2|3|4|6|7|9)\d{7}$", ErrorMessage = "Invalid Phone Number.")]
        public string phoneNo{ get; set; }

        [Display(Name = "Start Date")]
        [Required(ErrorMessage = "Start Date is required.")]
        [DataType(DataType.Date)]
        public DateTime empStartDate { get; set; }

        [Display(Name = "End Date")]
        [Required(ErrorMessage = "End Date is required.")]
        [DataType(DataType.Date)]
        public DateTime empEndDate { get; set; }

        [Display(Name = "Account Status")]
        public bool accountStatus { get; set; } = true;

        [Display(Name = "Staff Role")]
        public bool isStaff { get; set; } = false;

        [Display(Name = "Line Manager Role")]
        public bool isLM { get; set; } = false;

        [Display(Name = "Human Resources Role")]
        public bool isHR { get; set; } = false;

        [Display(Name = "Administrator Role")]
        public bool isAdmin { get; set; } = false;

        [Display(Name = "Human Resources Responsible Role")]
        public bool isHRResponsible { get; set; } = false;

        [Display(Name = "Probation Status")]
        public bool onProbation { get; set; } = true;

        public int? reportsToLineManagerID { get; set; }

        [Display(Name = "Religion")]
        [Required(ErrorMessage = "Religion is required.")]
        public int religionID { get; set; }

        [Display(Name = "Nationality")]
        [Required(ErrorMessage = "Nationality is required.")]
        public int nationalityID { get; set; }

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Date of Birth is required.")]
        public DateTime dateOfBirth { get; set; }

    }
}