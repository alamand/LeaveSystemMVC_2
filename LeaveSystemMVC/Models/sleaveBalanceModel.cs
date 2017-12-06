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
        public decimal annual { get; set; } = 0;
        public int annualID { get; set; } = 0;

        [Required]
        public decimal sick { get; set; } = 0;
        public int sickID { get; set; } = 0;

        [Required]
        public decimal compassionate { get; set; } = 0;
        public int compassionateID { get; set; } = 0;

        [Required]
        public decimal maternity { get; set; } = 0;
        public int maternityID { get; set; } = 0;

        [Required]
        public decimal daysInLieue { get; set; } = 0;
        public int daysInLieueID { get; set; } = 0;

        public decimal unpaidTotal { get; set; } = 0;
        public int unpaidID { get; set; } = 0;

        [Required]
        public decimal shortLeaveHours { get; set; } = 0;
        public int shortID { get; set; } = 0;

        public decimal pilgrimage { get; set; } = 0;
        public int pilgrimageID { get; set; } = 0;
    }

}