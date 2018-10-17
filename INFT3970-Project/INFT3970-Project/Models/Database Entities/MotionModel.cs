using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Project.Models.Database_Entities
{
    public class MotionModel
    {
        public int MotionId { get; set; }
        public int SensorID { get; set; }
        public Boolean Motion { get; set; }
        public DateTime Date { get; set; }
    }
}
