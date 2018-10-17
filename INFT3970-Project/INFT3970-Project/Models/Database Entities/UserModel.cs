using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Project.Models
{
    public class UserModel
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public string fName { get; set; }
        [Required]
        public string lName { get; set; }
        [Required]
        public int ContactNumber { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Status { get; set; }
        [Required]
        public string IsAdmin { get; set; }
    }
}