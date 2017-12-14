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

        [Display(Name = "Annual")]
        public decimal annual { get; set; } = 0;
        public int annualID { get; set; } = 0;

        [Display(Name = "Sick")]
        public decimal sick { get; set; } = 0;
        public int sickID { get; set; } = 0;

        [Display(Name = "Compassionate")]
        public decimal compassionate { get; set; } = 0;
        public int compassionateID { get; set; } = 0;

        [Display(Name = "Maternity")]
        public decimal maternity { get; set; } = 0;
        public int maternityID { get; set; } = 0;

        [Display(Name = "Days In Lieu")]
        public decimal daysInLieu { get; set; } = 0;
        public int daysInLieuID { get; set; } = 0;

        [Display(Name = "Unpaid")]
        public decimal unpaid { get; set; } = 0;
        public int unpaidID { get; set; } = 0;

        [Display(Name = "Short Hours")]
        public decimal shortHours { get; set; } = 0;
        public int shortHoursID { get; set; } = 0;

        [Display(Name = "Pilgrimage")]
        public decimal pilgrimage { get; set; } = 0;
        public int pilgrimageID { get; set; } = 0;
    }
}