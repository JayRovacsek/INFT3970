using System;
using System.ComponentModel.DataAnnotations;

namespace INFT3970Project.Models
{
    public class LoginModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public bool SuccessfulLogin { get; set; } = false;
        public string admin { get; set; }
    }
}