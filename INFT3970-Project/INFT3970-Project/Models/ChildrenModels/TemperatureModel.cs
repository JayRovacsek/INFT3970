﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Project.Models
{
    public class TemperatureModel : MasterModel
    {
        [Required]
        public double Temperature { get; set; }
    }
}
