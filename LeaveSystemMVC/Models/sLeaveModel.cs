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
        public string leaveID { get; set; }
        public string employeeID { get; set; }

        [Display(Name ="Name")]
        public string staffName { get; set; }

        [Display(Name = "Leave Type")]
        // [DisplayName("Select Leave Type")]
        [Required(ErrorMessage = "Please select leave type")]
        public string leaveType { get; set; }

        //[DisplayName("Select Application Date")]
        public DateTime applicationDate { get; set; } //Date of application creation

        [DisplayName("Start Date")]
        [Required(ErrorMessage = "Please select leave start date")]
        public DateTime startDate { get; set; } 

        [DisplayName("End Date")]
        [Required(ErrorMessage = "Please select leave end date")]
        public DateTime endDate { get; set; } 

        [Display(Name ="Return Date")]
        public DateTime returnDate { get; set; }

        public int leaveDuration { get; set; } 

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
        public string supportingDocs { get; set; } 

        [DisplayName("Book Air Ticket?")]
        public bool bookAirTicket { get; set; } 

        [DisplayName("Phone Number")]
        [Required(ErrorMessage = "You must provide your home phone number.")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "You must provide a proper phone number.")]
        [RegularExpression(@"^(?:\+971|00971|0)?(?:50|51|52|55|56|2|3|4|6|7|9)\d{7}$", ErrorMessage = "You must provide a proper phone number.")]
        public string contactDetails { get; set; } 
    }
}
