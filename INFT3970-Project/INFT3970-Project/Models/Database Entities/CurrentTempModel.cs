using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace INFT3970Project.Models
{
    public class CurrentTempModel
    {
        public string Temp { get; set; }
        public string Humidity { get; set; }
        // Room Name
        public string Expr1 { get; set; }
        // Sensor Name
        public string Name { get; set; }
    }
}
