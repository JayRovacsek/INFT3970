using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Project.Models
{
    public class UserPasswordModel
    {
        public int UserID { get; set; }
        public string Password { get; set; }
        public string HashedPassword { get; set; }
        public int Salt { get; set; }
    }
}