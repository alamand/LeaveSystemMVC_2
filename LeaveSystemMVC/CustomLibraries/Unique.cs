using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualBasic.CompilerServices;

namespace LeaveSystemMVC.CustomLibraries
{
    public class Unique : ValidationAttribute
    {
        public Type ObjectType { get; private set; }
        public Unique(Type type)
        {
            ObjectType = type;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if(ObjectType == typeof(string))
            {
                string username = (string)value;
                if (username.Equals("h.r"))
                    return new ValidationResult("Username already exists");
                else
                    return ValidationResult.Success;
            }
            /*
            if (ObjectType == typeof(Email))
            {
                // Here goes the code for creating DbContext, For testing I created List<string>
                // DbContext db = new DbContext();
                var emails = new List<string>();
                emails.Add("ra@ra.com");
                emails.Add("ve@ve.com");

                var email = emails.FirstOrDefault(u => u.Contains(((Email)value).EmailId));

                if (String.IsNullOrEmpty(email))
                    return ValidationResult.Success;
                else
                    return new ValidationResult("Mail already exists");
            }
            */

            return new ValidationResult("Generic Validation Fail");
        }

    }
}