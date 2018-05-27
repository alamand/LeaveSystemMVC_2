using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace LeaveSystemMVC.Models
{
    public class sLeaveModel
    {
        [Display(Name = "Leave Application ID")]
        public int leaveAppID { get; set; }

        [Display(Name = "Leave Type ID")]
        public int leaveTypeID { get; set; }

        [Display(Name = "Leave Type")]
        public string leaveTypeName { get; set; }

        [DisplayName("Leave Type Name")]
        public string leaveTypeDisplayName { get; set; }

        [Display(Name = "Employee ID")]
        public int employeeID { get; set; }

        [Display(Name = "Employee Name")]
        public string employeeName { get; set; }

        [DisplayName("Leave Application Date")]
        public DateTime applicationDate { get; set; }

        [DisplayName("Start Date")]
        [Required(ErrorMessage = "A valid Start Date is required.")]
        public DateTime startDate { get; set; }

        [Display(Name = "Reporting Back Date")]
        [Required(ErrorMessage = "Reporting Back Date is required.")]

        [DisplayName("Half Day")]
        public bool isStartDateHalfDay { get; set; }

        [DisplayName("Half Day")]
        public bool isReturnDateHalfDay { get; set; }

        [DisplayName("Return Date")]
        [Required(ErrorMessage = "A valid Return Date is required.")]
        public DateTime returnDate { get; set; }

        [Display(Name = "Duration")]
        public decimal leaveDuration { get; set; }

        [DisplayName("Start Time")]
        [Required(ErrorMessage = "Start Time is required.")]
        public TimeSpan shortStartTime { get; set; }

        [DisplayName("End Time")]
        [Required(ErrorMessage = "End Time is required.")]
        public TimeSpan shortEndTime { get; set; }

        [DisplayName("Line Manager Comment")]
        [StringLength(300, MinimumLength = 0, ErrorMessage = "Comment is too long.")]
        public string lmComment { get; set; }

        [DisplayName("HR Comment")]
        [StringLength(300, MinimumLength = 0, ErrorMessage = "Comment is too long.")]
        public string hrComment { get; set; }

        [DisplayName("Leave Status ID")]
        public int leaveStatusID { get; set; }

        [DisplayName("Leave Status")]
        public string leaveStatusName { get; set; }

        [DisplayName("Leave Status Name")]
        public string leaveStatusDisplayName { get; set; }

        [DisplayName("Comments")]
        [StringLength(300, MinimumLength = 0, ErrorMessage = "Comment is too long.")]
        public string comments { get; set; }

        [DisplayName("Documentation")]
        [StringLength(300, MinimumLength = 0, ErrorMessage = "File name is too long.")]
        public string documentation { get; set; }

        [DisplayName("Book Air Ticket?")]
        public bool bookAirTicket { get; set; }

        [DisplayName("Phone Number")]
        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(15, MinimumLength = 12, ErrorMessage = "Phone number is not the correct length.")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "You must provide a phone number in international format, starting with a + symbol.")]
        [RegularExpression(@"^\+[1-9]{1}[0-9]{3,14}$", ErrorMessage = "You must provide a phone number in international format, starting with a + symbol, and without spaces.")]
        public string contactDetails { get; set; }

        [Display(Name = "Personal email address")]
        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [StringLength(100, MinimumLength = 0, ErrorMessage = "Email Address is too long.")]
        public string email { get; set; }
    }
}