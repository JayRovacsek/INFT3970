using INFT3970Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Project.Models
{
    public class UserSensorAndPasswordViewModel
    {
        public UserModel User { get; set; }
        public IEnumerable<SensorModel> Sensors { get; set; }
        public UserPasswordModel Password { get; set; }
    }
}
