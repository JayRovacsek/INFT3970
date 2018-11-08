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

        public async Task<IActionResult> Temperature()
        {
            var userId = Convert.ToInt32(Request.Cookies["UserId"]);

            var models = await GetTemperatureModels(Convert.ToBoolean(GetCookie("IsAdmin")), false, userId, null);

            var predictiveModels = await GetPredictiveTemperatureModels(userId);

            var chartData = ConvertTemperatureToChart(models);
            var predictiveChartData = ConvertTemperatureToChart(predictiveModels);

            var viewModels = new Tuple<ChartDataModel, ChartDataModel>(chartData, predictiveChartData);

            return View(viewModels);
        }

        private async Task<IEnumerable<AverageTemperatureModelWithId>> GetPredictiveTemperatureModels(int userId)
        {
            var results = new List<AverageTemperatureModelWithId>();

            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                results.AddRange(await _databaseHelper.QueryPredicitiveTemperatureAsync(userId));
            }

            return results;
        }

        public async Task<IActionResult> Humidity()
        {
            var userId = Convert.ToInt32(Request.Cookies["UserId"]);

            var models = await GetHumidityModels(Convert.ToBoolean(GetCookie("IsAdmin")), false, userId, null);

            var chartData = ConvertHumidityToChart(models);

            return View(chartData);
            // NEEED TO FIX THE ABOVE FOR DEMO MODE.
        }

        public async Task<IActionResult> Motion()
        {
            var userId = Convert.ToInt32(Request.Cookies["UserId"]);

            var models = await GetMotionModels(Convert.ToBoolean(GetCookie("IsAdmin")), userId, null);

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

                var temperature = await GetTemperatureModels(Convert.ToBoolean(GetCookie("IsAdmin")), false, userId, null);
                var temperatureChartData = ConvertTemperatureToChart(temperature);

                var humidity = await GetHumidityModels(Convert.ToBoolean(GetCookie("IsAdmin")),false, userId, null);
                var humidityChartData = ConvertHumidityToChart(humidity);

                var motion = await GetMotionModels(Convert.ToBoolean(GetCookie("IsAdmin")), userId, null);
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

        public ChartDataModel ConvertTemperatureToChart(IEnumerable<AverageTemperatureModelWithId> models)
        {
            var chartData = new ChartDataModel
            {
                datasets = new List<DataSetModel>()
            };

            foreach (var sensorId in models.Select(x => x.SensorId).Distinct())
            {
                var colour = GetRandomColour();
                var location = _databaseHelper.QuerySensorLocation(sensorId).FirstOrDefault().Description;

                var ds = models.Select(x => x)
                    .Where(x => x.SensorId == sensorId).ToList()
                    .ConvertAll(x => new DataSetModel
                    {
                        type = "line",
                        borderColor = colour,
                        backgroundColour = colour,
                        fill = false,
                        borderWidth = 1,
                        label = $"Sensor {x.SensorId}: {location}",
                        data = models.Select(y => y).Where(y => y.SensorId == sensorId).ToList().ConvertAll(y => new ValueModel
                        {
                            x = y.EndTime.Equals(default(DateTime)) ? y.StartTime.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds : y.EndTime.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds,
                            y = y.Temperature
                        })

                    });
                chartData.datasets.Add(ds.FirstOrDefault());
            }
            return chartData;
        }

        public ChartDataModel ConvertHumidityToChart(IEnumerable<AverageHumidityModelWithId> models)
        {
            var chartData = new ChartDataModel
            {
                datasets = new List<DataSetModel>()
            };

            foreach (var sensorId in models.Select(x => x.SensorId).Distinct())
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
                    .Where(x => x.SensorId == sensorId).ToList()
                    .ConvertAll(x => new DataSetModel
                    {
                        type = "line",
                        borderColor = colour,
                        backgroundColour = colour,
                        fill = false,
                        borderWidth = 1,
                        label = $"Sensor {x.SensorId}: {location}",
                        data = models.Select(y => y).Where(y => y.SensorId == sensorId).ToList().ConvertAll(y => new ValueModel
                        {
                            x = y.EndTime.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds,
                            y = y.Humidity
                        })

                    });
                chartData.datasets.Add(ds.FirstOrDefault());
            }
            return chartData;
        }

        public ChartDataModel ConvertMotionToChart(IEnumerable<MotionCountModelWithId> models)
        {
            var chartData = new ChartDataModel
            {
                datasets = new List<DataSetModel>()
            };

            foreach (var sensorId in models.Select(x => x.SensorId).Distinct())
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

                var maxMotion = models.Select(x => x)
                    .Where(x => x.SensorId == sensorId)
                    .Select(x => x.MotionCount).ToList()
                    .OrderByDescending(x => x).FirstOrDefault();

                var ds = models.Select(x => x)
                    .Where(x => x.SensorId == sensorId).ToList()
                    .ConvertAll(x => new DataSetModel
                    {
                        type = "bar",
                        borderColor = colour,
                        backgroundColour = colour,
                        fill = false,
                        borderWidth = 1,
                        label = $"Sensor {x.SensorId}: {location}",
                        data = models.Select(y => y).Where(y => y.SensorId == sensorId).ToList().ConvertAll(y => new ValueModel
                        {
                            x = y.EndTime.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds,
                            y = (y.MotionCount / maxMotion) * 100
                        })

                    });
                chartData.datasets.Add(ds.FirstOrDefault());
            }
            return chartData;
        }

        public async Task<IEnumerable<AverageTemperatureModelWithId>> GetTemperatureModels(bool all, bool demo, int userId, DateTime? startTime)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var models = (all)
                    ? _databaseHelper.QueryAllTemperatureAsync(startTime, null)
                    : _databaseHelper.QueryUserTemperatureAsync(userId, demo, startTime, null);

                return await models;
            }
        }

        public async Task<IEnumerable<AverageHumidityModelWithId>> GetHumidityModels(bool all, bool demo, int userId, DateTime? startTime)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var models = (all)
                    ? _databaseHelper.QueryAllHumidityAsync(startTime, null)
                    : _databaseHelper.QueryUserHumidityAsync(userId, demo, startTime, null);

                return await models;
            }
        }

        public async Task<IEnumerable<MotionCountModelWithId>> GetMotionModels(bool all, int userId, DateTime? startTime)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var models = (all)
                    ? _databaseHelper.QueryAllMotionAsync(startTime, null)
                    : _databaseHelper.QueryUserMotionAsync(userId, startTime, null);

                return await models;
            }
        }
    }
}