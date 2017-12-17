using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class sLeaveModel
    {
        [Display(Name = "Leave Application ID")]
        public int leaveAppID { get; set; }

        [Display(Name = "Leave Type ID")]
        public int leaveTypeID { get; set; }

        [Display(Name = "Leave Type Name")]
        public string leaveTypeName { get; set; }

        [Display(Name = "Employee ID")]
        public int employeeID { get; set; }

        [Display(Name = "Employee Name")]
        public string employeeName { get; set; }

        //[DisplayName("Select Application Date")]
        public DateTime applicationDate { get; set; } //Date of application creation

        [DisplayName("Start Date")]
        [Required(ErrorMessage = "Start Date is required.")]
        public DateTime startDate { get; set; }

        // @TODO: Remove on of the two (end date or return date)
        [DisplayName("End Date")]
        [Required(ErrorMessage = "Reporting Back Date is required.")]
        public DateTime endDate { get; set; }

        [Display(Name = "Return Date")]
        [Required(ErrorMessage = "Reporting Back Date is required.")]
        public DateTime returnDate { get; set; }

        public decimal leaveDuration { get; set; }

        [DisplayName("Start Time")]
        [Required(ErrorMessage = "Start Time is required.")]
        public TimeSpan shortStartTime { get; set; }

        [DisplayName("End Time")]
        [Required(ErrorMessage = "End Time is required.")]
        public TimeSpan shortEndTime { get; set; }

        [DisplayName("Line Manager Comment")]
        public string lmComment { get; set; }

        [DisplayName("HR Comment")]
        public string hrComment { get; set; }

        [DisplayName("Leave Status ID")]
        public int leaveStatusID { get; set; }

        [DisplayName("Leave Status ID")]
        public string leaveStatusName { get; set; }

        [DisplayName("Comments")]
        public string comments { get; set; }

        [DisplayName("Documentation")]
        public string documentation { get; set; }

        [DisplayName("Book Air Ticket?")]
        public bool bookAirTicket { get; set; }

        [DisplayName("Phone Number")]
        [Required(ErrorMessage = "Phone Number is required.")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "You must provide a phone number in international format, starting with a + symbol.")]
        [RegularExpression(@"^\+[1-9]{1}[0-9]{3,14}$", ErrorMessage = "You must provide a phone number in international format, starting with a + symbol.")]
        public string contactDetails { get; set; }
    }
}