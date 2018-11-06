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

        public async Task<IActionResult> Temperature(bool all)
        {
            var mode = ApplicationMode.User;

            var userId = Convert.ToInt32(Request.Cookies["UserId"]);

            var models = await GetTemperatureModels(Convert.ToBoolean(GetCookie("IsAdmin")), userId);

            var chartData = ConvertTemperatureToChart(models);

            return View(chartData);
            // NEEED TO FIX THE ABOVE FOR DEMO MODE.
        }

        public async Task<IActionResult> Humidity()
        {
            var userId = Convert.ToInt32(Request.Cookies["UserId"]);

            var models = await GetHumidityModels(Convert.ToBoolean(GetCookie("IsAdmin")), userId);

            var chartData = ConvertHumidityToChart(models);

            return View(chartData);
            // NEEED TO FIX THE ABOVE FOR DEMO MODE.
        }

        public async Task<IActionResult> Motion(ApplicationMode mode = ApplicationMode.User)
        {
            var userId = Convert.ToInt32(Request.Cookies["UserId"]);

            var models = await GetMotionModels(Convert.ToBoolean(GetCookie("IsAdmin")), userId);

            var chartData = ConvertMotionToChart(models);

            return View(chartData);
            
                // NEEED TO FIX THE ABOVE FOR DEMO MODE.

           
        }

        public async Task<IActionResult> Combined()
        {
            var userId = Convert.ToInt32(Request.Cookies["UserId"]);

            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var chartData = new ChartDataModel
                {
                    datasets = new List<DataSetModel>()
                };

                var temperature = await GetTemperatureModels(Convert.ToBoolean(GetCookie("IsAdmin")), userId);
                var temperatureChartData = ConvertTemperatureToChart(temperature);

                var humidity = await GetHumidityModels(Convert.ToBoolean(GetCookie("IsAdmin")), userId);
                var humidityChartData = ConvertHumidityToChart(humidity);

                var motion = await GetMotionModels(Convert.ToBoolean(GetCookie("IsAdmin")), userId);
                var motionChartData = ConvertMotionToChart(motion);

                if (temperatureChartData.datasets.Count > 0)
                {
                    chartData.datasets.AddRange(temperatureChartData.datasets);
                }

                if (humidityChartData.datasets.Count > 0)
                {
                    chartData.datasets.AddRange(humidityChartData.datasets);
                }

                if (motionChartData.datasets.Count > 0)
                {
                    chartData.datasets.AddRange(motionChartData.datasets);
                }

                return View(chartData);
                // NEEED TO FIX THE ABOVE FOR DEMO MODE.
            }
        }

        public ChartDataModel ConvertTemperatureToChart(IEnumerable<DB.TemperatureModel> models)
        {
            var chartData = new ChartDataModel
            {
                datasets = new List<DataSetModel>()
            };

            foreach (var sensorId in models.Select(x => x.SensorID).Distinct())
            {
                string colour;
                var location = _databaseHelper.QuerySensorLocation(sensorId).FirstOrDefault().Description;

                if (Request.Cookies.ContainsKey($"Sensor{sensorId}Colour"))
                {
                    colour = GetCookie($"Sensor{sensorId}Colour");
                }
                else
                {
                    colour = GetRandomColour();
                    SetCookie($"Sensor{sensorId}Colour", colour, 60);
                }

                var ds = models.Select(x => x)
                    .Where(x => x.SensorID == sensorId).ToList()
                    .ConvertAll(x => new DataSetModel
                    {
                        borderColor = colour,
                        backgroundColour = colour,
                        fill = false,
                        borderWidth = 1,
                        label = $"Sensor {x.SensorID}: {location}",
                        data = models.Select(y => y).Where(y => y.SensorID == sensorId).ToList().ConvertAll(y => new ValueModel
                        {
                            x = y.Date.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds,
                            y = y.Temp
                        })

                    });
                chartData.datasets.Add(ds.FirstOrDefault());
            }
            return chartData;
        }

        public ChartDataModel ConvertHumidityToChart(IEnumerable<DB.HumidityModel> models)
        {
            var chartData = new ChartDataModel
            {
                datasets = new List<DataSetModel>()
            };

            foreach (var sensorId in models.Select(x => x.SensorID).Distinct())
            {
                string colour;
                var location = _databaseHelper.QuerySensorLocation(sensorId).FirstOrDefault().Description;

                if (Request.Cookies.ContainsKey($"Sensor{sensorId}Colour"))
                {
                    colour = GetCookie($"Sensor{sensorId}Colour");
                }
                else
                {
                    colour = GetRandomColour();
                    SetCookie($"Sensor{sensorId}Colour", colour, 60);
                }

                var ds = models.Select(x => x)
                    .Where(x => x.SensorID == sensorId).ToList()
                    .ConvertAll(x => new DataSetModel
                    {
                        borderColor = colour,
                        backgroundColour = colour,
                        fill = false,
                        borderWidth = 1,
                        label = $"Sensor {x.SensorID}: {location}",
                        data = models.Select(y => y).Where(y => y.SensorID == sensorId).ToList().ConvertAll(y => new ValueModel
                        {
                            x = y.Date.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds,
                            y = y.Humidity
                        })

                    });
                chartData.datasets.Add(ds.FirstOrDefault());
            }
            return chartData;
        }

        public ChartDataModel ConvertMotionToChart(IEnumerable<DB.MotionModel> models)
        {
            var chartData = new ChartDataModel
            {
                datasets = new List<DataSetModel>()
            };

            foreach (var sensorId in models.Select(x => x.SensorID).Distinct())
            {
                string colour;
                var location = _databaseHelper.QuerySensorLocation(sensorId).FirstOrDefault().Description;

                if (Request.Cookies.ContainsKey($"Sensor{sensorId}Colour"))
                {
                    colour = GetCookie($"Sensor{sensorId}Colour");
                }
                else
                {
                    colour = GetRandomColour();
                    SetCookie($"Sensor{sensorId}Colour", colour, 60);
                }

                var ds = models.Select(x => x)
                    .Where(x => x.SensorID == sensorId).ToList()
                    .ConvertAll(x => new DataSetModel
                    {
                        borderColor = colour,
                        backgroundColour = colour,
                        fill = false,
                        borderWidth = 1,
                        label = $"Sensor {x.SensorID}: {location}",
                        data = models.Select(y => y).Where(y => y.SensorID == sensorId).ToList().ConvertAll(y => new ValueModel
                        {
                            x = y.Date.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds,
                            y = (y.Motion) ? 100 : 0
                        })

                    });
                chartData.datasets.Add(ds.FirstOrDefault());
            }
            return chartData;
        }

        public async Task<IEnumerable<DB.TemperatureModel>> GetTemperatureModels(bool all, int userId)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var models = (all)
                    ? _databaseHelper.QueryAllTemperatureAsync()
                    : _databaseHelper.QueryUserTemperatureAsync(userId);

                return await models;
            }
        }

        public async Task<IEnumerable<DB.HumidityModel>> GetHumidityModels(bool all, int userId)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var models = (all)
                    ? _databaseHelper.QueryAllHumidityAsync()
                    : _databaseHelper.QueryUserHumidityAsync(userId);

                return await models;
            }
        }

        public async Task<IEnumerable<DB.MotionModel>> GetMotionModels(bool all, int userId)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var models = (all)
                    ? _databaseHelper.QueryAllMotionAsync()
                    : _databaseHelper.QueryUserMotionAsync(userId);

                return await models;
            }
        }
    }
}