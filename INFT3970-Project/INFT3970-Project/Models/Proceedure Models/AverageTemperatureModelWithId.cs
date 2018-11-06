using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Project.Models
{
    public class AverageTemperatureModelWithId
    {
        public int SensorId { get; set; }
        public decimal Temperature { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
