using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Project.Models.Database_Entities
{
    public class HumidityModel
    {
        public int HumidityId { get; set; }
        public int SensorID { get; set; }
        public decimal Humidity { get; set; }
        public DateTime Date { get; set; }
    }
}
