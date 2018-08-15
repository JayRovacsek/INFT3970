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

namespace INFT3970Project.Controllers
{
    [Produces("application/json")]
    [Route("api/Database")]
    public class DatabaseController : Controller
    {
        private static IOptions<ConnectionStrings> connectionStrings;
        private DatabaseHelper _databaseHelper = new DatabaseHelper(connectionStrings);

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