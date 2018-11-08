using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace INFT3970Project.Models
{
    public class CurrentTempModel
    {
        public decimal Temperature { get; set; }
        public decimal Humidity { get; set; }
        // Room Name
        public string RoomName { get; set; }
        // Sensor Name
        public string SensorName { get; set; }
        public string MostRecentMovement { get; set; }
    }
}
