﻿using System;
using System.ComponentModel.DataAnnotations;

namespace INFT3970Project.Models
{
    public class RegisterModel
    {
        [Required]
        public string fName { get; set; }
        [Required]
        public string lName { get; set; }
        [Required]
        public string ContactNumber { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string StreetNum { get; set; }
        [Required]
        public string StreetName { get; set; }
        [Required]
        public string Postcode { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string Country { get; set; }                     

        [Required]
        public string Password { get; set; }

        public bool SuccessfulRegister { get; set; } = false;
    }
}
