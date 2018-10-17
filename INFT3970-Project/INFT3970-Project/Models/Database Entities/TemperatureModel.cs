using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Project.Models.Database_Entities
{
    public class TemperatureModel
    {
        public int TempId { get; set; }
        public int SensorID { get; set; }
        public decimal Temp { get; set; }
        public DateTime Date { get; set; }
    }
}
