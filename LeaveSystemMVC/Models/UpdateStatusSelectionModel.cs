using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public struct SMDetails_LUSShort
    {
        public string name;
        public DateTime startDate;
        public DateTime endDate;
    }
    public class UpdateStatusSelectionModel
    {
        public SMDetails_LUSShort[] updatableStaffMembers;
    }
}