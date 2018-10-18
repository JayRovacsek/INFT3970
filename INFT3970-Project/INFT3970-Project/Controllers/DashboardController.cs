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

        public string GetCookie(string key)
        {
            return Request.Cookies[key];
        }

        public async Task<IActionResult> AllTemperature()
        {
            if (Convert.ToBoolean(GetCookie("IsAdmin"))){
                return RedirectToAction("Temperature", true);

            }
            return RedirectToAction("Temperature", false);
        }

        public async Task<IActionResult> Temperature(bool all = false)
        {
            var userId = Convert.ToInt32(Request.Cookies["UserId"]);

            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var models = (all) ? await _databaseHelper.QueryAllTemperature() : await _databaseHelper.QueryUserTemperature(userId);
                return View(models);
            }
        }

        public async Task<IActionResult> AllHumidity()
        {
            if (Convert.ToBoolean(GetCookie("IsAdmin")))
            {
                return RedirectToAction("Humidity", true);
            }
            return RedirectToAction("Humidity", false);
        }

        public async Task<IActionResult> Humidity(bool all = false)
        {
            var userId = Convert.ToInt32(Request.Cookies["UserId"]);

            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var models = (all) ? await _databaseHelper.QueryAllHumidity() : await _databaseHelper.QueryUserHumidity(userId);
                return View(models);
            }
        }

        public async Task<IActionResult> AllMotion()
        {
            if (Convert.ToBoolean(GetCookie("IsAdmin")))
            {
                return RedirectToAction("Motion", true);
            }
            return RedirectToAction("Motion", false);
        }

        public async Task<IActionResult> Motion(bool all = false)
        {
            var userId = Convert.ToInt32(Request.Cookies["UserId"]);

            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var models = (all) ? await _databaseHelper.QueryAllMotion() : await _databaseHelper.QueryUserMotion(userId);

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