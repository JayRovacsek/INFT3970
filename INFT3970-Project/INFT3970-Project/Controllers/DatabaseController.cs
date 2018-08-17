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
using System.Net.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace INFT3970Project.Controllers
{
    [Produces("application/json")]
    [Route("api/Database")]
    public class DatabaseController : Controller
    {
        private readonly IConfiguration configuration;
        private DatabaseHelper _databaseHelper;

        public DatabaseController(IConfiguration configuration)
        {
            this.configuration = configuration;
            _databaseHelper = new DatabaseHelper(configuration.GetConnectionString("AzureConnectionString").ToString());
        }

        /// <summary>
        /// Implementation of method to create a record async via post request
        /// </summary>
        /// <param name="temperatureModel"></param>
        [HttpPost]
        [Route("Create")]
        public IActionResult Create([FromBody] object model)
        {
            _databaseHelper.CreateRecord(model);
            return null;
        }
    }
}