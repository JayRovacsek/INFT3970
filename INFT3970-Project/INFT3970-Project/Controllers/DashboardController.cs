using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using INFT3970Project.Helpers;
using INFT3970Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using DB = INFT3970Project.Models.Database_Entities;

namespace INFT3970Project.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IConfiguration configuration;
        private DatabaseHelper _databaseHelper;

        public DashboardController(IConfiguration configuration)
        {
            this.configuration = configuration;
            _databaseHelper = new DatabaseHelper(configuration);
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Temperature()
        {
            var userId = Convert.ToInt32(Request.Cookies["UserId"]);

            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var models = await _databaseHelper.QueryUserTemperature(userId);
                return View(models);
            }
        }

        public async Task<IActionResult> Humidity()
        {
            var userId = Convert.ToInt32(Request.Cookies["UserId"]);

            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var models = await _databaseHelper.QueryUserHumidity(userId);
                return View(models);
            }
        }

        public async Task<IActionResult> Motion()
        {
            var userId = Convert.ToInt32(Request.Cookies["UserId"]);

            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var models = await _databaseHelper.QueryUserMotion(userId);
                return View(models);
            }
        }

        //public async Task<IEnumerable<DB.TemperatureModel>> GetUserTemperatureModelsAsync(int userId)
        //{
        //    using (var _databaseHelper = new DatabaseHelper(configuration))
        //    {
        //        var result = await _databaseHelper.QueryUserTemperatures(userId);
        //        return result;
        //    }
        //}
    }
}