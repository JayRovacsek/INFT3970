using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using INFT3970Project.Helpers;
using INFT3970Project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace INFT3970Project.Controllers
{
    [Produces("application/json")]
    [Route("api/Temperature")]
    public class TemperatureController : Controller
    {
        private readonly IConfiguration configuration;
        private DatabaseHelper _databaseHelper;

        public TemperatureController(IConfiguration configuration, DatabaseHelper databaseHelper)
        {
            this.configuration = configuration;
            _databaseHelper = new DatabaseHelper(configuration);
        }

        /// <summary>
        /// Implementation of method to create a record async via post request
        /// </summary>
        /// <param name="temperatureModel"></param>
        [HttpPost]
        [Route("Create")]
        public IActionResult Create([FromBody] TemperatureModel model)
        {
            try
            {
                model.Timestamp = DateTime.Now;
                _databaseHelper.CreateRecord(model);
            }
            catch(Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            return null;
        }
    }
}