using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Project.Models
{
    public class TemperatureModel
    {
        public Guid Id { get; set; }
        public double Temperature { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
