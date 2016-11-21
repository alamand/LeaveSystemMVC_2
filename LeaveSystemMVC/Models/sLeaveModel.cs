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

        public string staffName { get; set; } = "Dwayne";

        // [DisplayName("Select Leave Type")]
        [Required(ErrorMessage = "Please select leave type")]
        public String leaveType { get; set; } = "Annual";

        //[DisplayName("Select Application Date")]
        public DateTime applicationDate { get; set; } 

        [DisplayName("Select Leave Start Date")]
        [Required(ErrorMessage = "Please select leave start date")]
        public DateTime startDate { get; set; } 

        [DisplayName("Select Leave End Date")]
        [Required(ErrorMessage = "Please select leave end date")]
        public DateTime endDate { get; set; } 

        public DateTime returnDate { get; set; }

        public int leaveDuration { get; set; } 

        [DisplayName("Select Leave Start Time")]
        public TimeSpan shortStartTime { get; set; }

        [DisplayName("Select Leave End Time")]
        public TimeSpan shortEndTime { get; set; } 

        [DisplayName("Line Manager Comment")]
        public string lmComment { get; set; } 

        [DisplayName("HR Comment")]
        public string hrComment { get; set; } 

        [DisplayName("Leave Status")]
        public int leaveStatus { get; set; } 

        [DisplayName("Enter Comments")]
        public string comments { get; set; } 

        [DisplayName("Upload Supporting Docs")]
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
