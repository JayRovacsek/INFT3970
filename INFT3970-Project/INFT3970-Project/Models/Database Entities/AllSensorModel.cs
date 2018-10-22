﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Project.Models
{
    public class AllSensorModel
    {
        public int SensorId { get; set; }
        public int UserId { get; set; }
        public string fName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SensorDescription { get; set; }
        public string SensorName { get; set; }
        public int RoomId { get; set; }
    }
}
