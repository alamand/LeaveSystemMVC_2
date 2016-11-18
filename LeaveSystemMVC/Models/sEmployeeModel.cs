using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

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
        public string deptName { get; set; }
        public string phoneNo{ get; set; }

        [DataType(DataType.Date)]
        public DateTime empStartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime empEndDate { get; set; }


        public string secondLineManager { get; set; }
        public string staffType { get; set; }
        public string optionalStaffType { get; set; }
        public string optional2ndStaffType { get; set; }
        public bool accountStatus { get; set; }
    }
}