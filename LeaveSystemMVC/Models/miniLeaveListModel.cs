using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    /*This model will contain all the data required for
     selection lists of leaves. These selection lists will be used
     for the View Leave History use case which will be used by staff members
     to select which leave they want to view in their history or by HR or 
     line managers to select which of their subordinates' leave from the subordinate
     history they want to view the full details of.*/
    public class miniLeaveListModel
    {
        public int leaveID { get; set; }
        public string displayText { get; set; }
        public string leaveType { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
    }
    
}