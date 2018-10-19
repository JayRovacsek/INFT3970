using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using INFT3970Project.Helpers;
using INFT3970Project.Models;
using INFT3970Project.Models.ApplicationModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using DB = INFT3970Project.Models.Database_Entities;

namespace INFT3970Project.Controllers
{
    [Authorize]
    public class DashboardController : BaseController
    {
        public DashboardController(IConfiguration configuration, DatabaseHelper databaseHelper, IHttpContextAccessor httpContextAccessor) : base(configuration, databaseHelper, httpContextAccessor)
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> AllTemperature()
        {
            if (Convert.ToBoolean(GetCookie("IsAdmin"))){
                return RedirectToAction("Temperature", ApplicationMode.Admin);

            }
            return RedirectToAction("Temperature", ApplicationMode.User);
        }

        public async Task<IActionResult> Temperature(ApplicationMode mode = ApplicationMode.User)
        {
            var userId = Convert.ToInt32(Request.Cookies["UserId"]);

            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var models = (mode == ApplicationMode.Admin) 
                    ? await _databaseHelper.QueryAllTemperature() 
                    : (mode == ApplicationMode.User)
                    ? await _databaseHelper.QueryUserTemperature(userId)
                    : await _databaseHelper.QueryUserTemperature(userId);

                var chartData = new List<Dictionary<int, ChartData>>();

                foreach(var model in models)
                {
                    chartData.Add(new Dictionary<int, ChartData> { { model.SensorID, new ChartData { x = model.Date.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds, y = model.Temp } } });
                }

                //var chartData = models.Select(x => new ChartData
                //{
                //    x = x.Date.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds,
                //    y = x.Temp
                //});

                return View(chartData);
                // NEEED TO FIX THE ABOVE FOR DEMO MODE.
            }
        }

        public async Task<IActionResult> AllHumidity()
        {
            if (Convert.ToBoolean(GetCookie("IsAdmin")))
            {
                return RedirectToAction("Humidity", ApplicationMode.Admin);
            }
            return RedirectToAction("Humidity", ApplicationMode.User);
        }

        public async Task<IActionResult> Humidity(ApplicationMode mode = ApplicationMode.User)
        {
            var userId = Convert.ToInt32(Request.Cookies["UserId"]);

            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var models = (mode == ApplicationMode.Admin) 
                    ? await _databaseHelper.QueryAllHumidity() 
                    : (mode == ApplicationMode.User) 
                    ? await _databaseHelper.QueryUserHumidity(userId)
                    : await _databaseHelper.QueryUserHumidity(userId);
                return View(models);
                // NEEED TO FIX THE ABOVE FOR DEMO MODE.
            }
        }

        public async Task<IActionResult> AllMotion()
        {
            if (Convert.ToBoolean(GetCookie("IsAdmin")))
            {
                return RedirectToAction("Motion", ApplicationMode.Admin);
            }
            return RedirectToAction("Motion", ApplicationMode.User);
        }

        public async Task<IActionResult> Motion(ApplicationMode mode = ApplicationMode.User)
        {
            var userId = Convert.ToInt32(Request.Cookies["UserId"]);

            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var models = (mode == ApplicationMode.Admin) 
                    ? await _databaseHelper.QueryAllMotion() 
                    : (mode == ApplicationMode.User) 
                    ? await _databaseHelper.QueryUserMotion(userId) 
                    : await _databaseHelper.QueryUserMotion(userId);
                // NEEED TO FIX THE ABOVE FOR DEMO MODE.

                return View(models);
            }
        }
    }
}