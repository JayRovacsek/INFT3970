﻿using System;
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
            return RedirectToAction("Temperature", "Dashboard", true);
        }

        public async Task<IActionResult> Temperature(bool all)
        {
            var mode = ApplicationMode.User;

            var userId = Convert.ToInt32(Request.Cookies["UserId"]);

            if (Convert.ToBoolean(GetCookie("IsAdmin")) && all)
            {
                mode = ApplicationMode.Admin;
            }

            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var models = (mode == ApplicationMode.Admin)
                    ? await _databaseHelper.QueryAllTemperature()
                    : (mode == ApplicationMode.User)
                    ? await _databaseHelper.QueryUserTemperature(userId)
                    : await _databaseHelper.QueryUserTemperature(userId);

                var chartData = new ChartDataModel
                {
                    datasets = new List<DataSetModel>()
                };

                foreach (var sensorId in models.Select(x => x.SensorID).Distinct())
                {
                    string colour;

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
                            label = $"Sensor {x.SensorID}",
                            data = models.Select(y => y).Where(y => y.SensorID == sensorId).ToList().ConvertAll(y => new ValueModel
                            {
                                x = y.Date.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds,
                                y = y.Temp
                            })

                        });
                    chartData.datasets.Add(ds.FirstOrDefault());
                }

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