using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Project.Models
{
    public class LogsModel
    {

        public int id { get; set; }
        public string Serverity { get; set; }
        public string Message { get; set; }
        public int Userid { get; set; }
        public string Location { get; set; }
        public string Time { get; set; }
    }
}
