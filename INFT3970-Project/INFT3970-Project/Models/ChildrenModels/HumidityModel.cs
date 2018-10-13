using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Project.Models
{
    public class HumidityModel : MasterModel
    {
        [Required]
        public decimal Humidity { get; set; }
    }
}
