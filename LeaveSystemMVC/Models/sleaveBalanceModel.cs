using System;
using System.ComponentModel.DataAnnotations;

namespace LeaveSystemMVC.Models
{
    public class sleaveBalanceModel
    {
        public int empId { get; set; } = 0;

        [Display(Name = "Annual")]
        public decimal annual { get; set; } = 0;
        public int annualID { get; set; } = 0;
        public String editCommentAnnual { get; set; } = "";

        [Display(Name = "Sick")]
        public decimal sick { get; set; } = 0;
        public int sickID { get; set; } = 0;
        public String editCommentSick { get; set; } = "";

        [Display(Name = "Compassionate")]
        public decimal compassionate { get; set; } = 0;
        public int compassionateID { get; set; } = 0;
        public String editCommentCompassionate { get; set; } = "";

        [Display(Name = "Maternity")]
        public decimal maternity { get; set; } = 0;
        public int maternityID { get; set; } = 0;
        public String editCommentMaternity { get; set; } = "";

        [Display(Name = "Days In Lieu")]
        public decimal daysInLieu { get; set; } = 0;
        public int daysInLieuID { get; set; } = 0;
        public String editCommentDIL { get; set; } = "";

        [Display(Name = "Unpaid")]
        public decimal unpaid { get; set; } = 0;
        public int unpaidID { get; set; } = 0;
        public String editCommentUnpaid { get; set; } = "";

        [Display(Name = "Short Hours")]
        public decimal shortHours { get; set; } = 0;
        public int shortHoursID { get; set; } = 0;
        public String editCommentShortHours { get; set; } = "";

        [Display(Name = "Pilgrimage")]
        public decimal pilgrimage { get; set; } = 0;
        public int pilgrimageID { get; set; } = 0;
        public String editCommentPilgrimage { get; set; } = "";
    }
}