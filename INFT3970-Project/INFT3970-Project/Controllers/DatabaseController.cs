using System;
using Dapper;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using INFT3970Project.Helpers;
using INFT3970Project.Models;
using System.Text;

namespace INFT3970Project.Controllers
{
    [Produces("application/json")]
    [Route("api/Database")]
    public class DatabaseController : Controller
    {
        private DatabaseHelper _databaseHelper;

        /// <summary>
        /// Implementation of method to create a record async via post request
        /// </summary>
        /// <param name="temperatureModel"></param>
        [Route("api/Database/CreateTemperature")]
        [HttpPost]
        public void CreateRecord(TemperatureModel temperatureModel)
        {
            _databaseHelper.CreateRecord(temperatureModel);
        }

        /// <summary>
        /// Implementation of method to test working status
        /// </summary>
        /// <param name="temperatureModel"></param>
        [Route("api/Database/Test")]
        [HttpPost]
        public void CreateRecord(string test)
        {
            Console.WriteLine(test);
        }

    }
}