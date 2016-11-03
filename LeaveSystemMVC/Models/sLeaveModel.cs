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

        public String staffName { get; set; } = "Dwayne";

        [Required]
        // [DisplayName("Select Leave Type")]
        public String leaveType { get; set; } = "Annual";

        //[DisplayName("Select Application Date")]
        public DateTime applicationDate { get; set; } = new DateTime(24-11-2016);

        [DisplayName("Select Leave Start Date")]
        public DateTime startDate { get; set; } = new DateTime(24 - 12 - 2016);

        [DisplayName("Select Leave End Date")]
        public DateTime endDate { get; set; } = new DateTime(24 - 1 - 2017);

        public DateTime returnDate { get; set; } = new DateTime(24 - 1 - 2017);

        public int leaveDuration { get; set; } = 30;

        [DisplayName("Select Leave Start Time")]
        public DateTime shortStartTime { get; set; } = new DateTime(24 - 1 - 2017);

        [DisplayName("Select Leave End Time")]
        public DateTime shortEndTime { get; set; } = new DateTime(24 - 1 - 2017);

        [DisplayName("Line Manager Comment")]
        public string lmComment { get; set; } = "Leave Approved";

        [DisplayName("HR Comment")]
        public string hrComment { get; set; } = "Leave Approved";

        [DisplayName("Leave Status")]
        public int leaveStatus { get; set; } = 1;

        [DisplayName("Enter Comments")]
        public string comments { get; set; } = "Taking Annual Leave";

        [DisplayName("Upload Supporting Docs")]
        public string supportingDocs { get; set; } = "/abc";

        [DisplayName("Book Air Ticket?")]
        public bool bookAirTicket { get; set; } = true;

        [DisplayName("Contact Details Outside UAE")]
        public string contactDetails { get; set; } = "052123344";
    }
}