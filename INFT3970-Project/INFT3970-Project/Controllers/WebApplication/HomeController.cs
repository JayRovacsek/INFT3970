using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using INFT3970Project.Helpers;
using Microsoft.AspNetCore.Mvc;
using INFT3970Project.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using INFT3970Project.Models.ApplicationModels;

namespace INFT3970Project.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(IConfiguration configuration, DatabaseHelper databaseHelper, IHttpContextAccessor httpContextAccessor) : base(configuration, databaseHelper, httpContextAccessor)
        {
        }

        /// <summary>
        /// Asynchronous method to return index page
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = Convert.ToInt32(Request.Cookies["UserId"]);
            var models = await _databaseHelper.QueryCurrentAsync(userId);
            return View(models);
        }

        /// <summary>
        /// Synchronous method to return about page
        /// </summary>
        /// <returns></returns>
        public IActionResult About()
        {
            return View();
        }

        /// <summary>
        /// Asynchronous method to return demo page
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Demo()
        {
            try
            {
                using (var _databaseHelper = new DatabaseHelper(configuration))
                {
                    var chartData = new ChartDataModel
                    {
                        datasets = new List<DataSetModel>()
                    };

                    var dashboardController = new DashboardController(configuration, _databaseHelper, _httpContextAccessor);

                    var temperatureTask = dashboardController.GetTemperatureModels(false, true, 12, DateTime.Now.Subtract(new TimeSpan(0, 5, 0)));
                    var humidityTask = dashboardController.GetHumidityModels(false, true, 12, DateTime.Now.Subtract(new TimeSpan(0, 5, 0)));

                    var temperature = await temperatureTask;
                    var humidity = await humidityTask;

                    var temperatureChartData = dashboardController.ConvertTemperatureToChart(temperature);
                    var humidityChartData = dashboardController.ConvertHumidityToChart(humidity);

                    if (temperatureChartData.datasets.Count > 0)
                    {
                        chartData.datasets.AddRange(temperatureChartData.datasets);
                    }

                    if (humidityChartData.datasets.Count > 0)
                    {
                        chartData.datasets.AddRange(humidityChartData.datasets);
                    }

                    return View(chartData);
                }

            }
            catch (Exception exception)
            {
                return View(null);
            }
        }
    }
}
