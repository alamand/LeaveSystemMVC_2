using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LeaveSystemMVC.Models
{
    public class sleaveBalanceModel
    {

        public int empId { get; set; } = 0;
        [Required]
        public int annual { get; set; } = 0;
        public int annualID { get; set; } = 0;
        [Required]
        public int sick { get; set; } = 0;
        public int sickID { get; set; } = 0;
        [Required]
        public int compassionate { get; set; } = 0;
        public int compassionateID { get; set; } = 0;
        [Required]
        public int maternity { get; set; } = 0;
        public int maternityID { get; set; } = 0;
        [Required]
        public int daysInLieue { get; set; } = 0;
        public int daysInLieueID { get; set; } = 0;
        public int unpaidTotal { get; set; } = 0;
        public int unpaidID { get; set; } = 0;
        [Required]
        public int shortLeaveHours { get; set; } = 0;
        public int shortID { get; set; } = 0;
    }

}