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
        public int iter { get; set; }
        [Display(Name = "Leave ID: ")]
        public int leaveID { get; set; }
        public int employeeID { get; set; }

        [Display(Name = "Name")]
        public string staffName { get; set; }

        [Display(Name = "Leave Type")]
        // [DisplayName("Select Leave Type")]
        [Required(ErrorMessage = "Please select leave type")]
        public string leaveType { get; set; }

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
        public TimeSpan? shortStartTime { get; set; }

        [DisplayName("End Time")]
        public TimeSpan? shortEndTime { get; set; }

        [DisplayName("Line Manager Comment")]
        public string lmComment { get; set; }

        [DisplayName("HR Comment")]
        public string hrComment { get; set; }

        [DisplayName("Leave Status")]
        public int leaveStatus { get; set; }

        [DisplayName("Comments")]
        public string comments { get; set; }

        [DisplayName("Supporting Docs")]
        public HttpPostedFileBase supportingDocs { get; set; }

        [DisplayName("Book Air Ticket?")]
        public bool bookAirTicket { get; set; }

        [DisplayName("Phone Number")]
        [Required(ErrorMessage = "Phone Number is required.")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "You must provide a phone number in international format, starting with a + symbol.")]
        [RegularExpression(@"^\+[1-9]{1}[0-9]{3,14}$", ErrorMessage = "You must provide a phone number in international format, starting with a + symbol.")]
        public string contactDetails { get; set; }
    }
}