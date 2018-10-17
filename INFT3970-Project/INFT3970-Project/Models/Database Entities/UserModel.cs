using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Project.Models
{
    public class UserModel
    {
        public int UserId { get; set; }
        public string fName { get; set; }
        public string lName { get; set; }
        public int ContactNumber { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public string IsAdmin { get; set; }
    }
}