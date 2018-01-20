using System.ComponentModel.DataAnnotations;

namespace LeaveSystemMVC.Models
{
    public class Users
    {
        [Display(Name = "Username or Employee ID")]
        [Required(ErrorMessage = "You forgot to enter a username.")]
        public string UserID { get; set; }

        [Display(Name = "Password")]
        [Required(ErrorMessage = "You forgot to enter a password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
    }
}