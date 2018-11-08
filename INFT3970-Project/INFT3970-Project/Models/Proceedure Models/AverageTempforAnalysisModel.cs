using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Project.Models.Proceedure_Models
{
    public class AverageTempforAnalysisModel
    {
        public int SensorId { get; set; }
        public decimal Temperature { get; set; }
        public DateTime StartTime { get; set; }

    }
}