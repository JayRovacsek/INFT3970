using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Project.Models.ApplicationModels
{
    public class DataSetModel
    {
        public string label { get; set; }
        public string backgroundColour { get; set; }
        public string borderColor { get; set; }
        public string fillColour { get; set; }
        public bool fill { get; set; }
        public int borderWidth { get; set; }
        public List<ValueModel> data { get; set; }
    }
}
