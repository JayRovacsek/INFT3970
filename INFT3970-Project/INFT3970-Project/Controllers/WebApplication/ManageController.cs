using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using INFT3970Project.Helpers;
using INFT3970Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;

namespace INFT3970Project.Controllers
{
    [Authorize]
    public class ManageController : BaseController
    {
        public ManageController(IConfiguration configuration, DatabaseHelper databaseHelper, IHttpContextAccessor httpContextAccessor) : base(configuration, databaseHelper, httpContextAccessor)
        {
        }
    
        public IActionResult Manage()
        {
           // var sensors = _databaseHelper.QueryAllSensors();

            //var selectlist = new List<SelectListItem>();
            //foreach (var sensor in sensors)
            //{
            //    selectlist.Add(new SelectListItem { Text = sensor.Name, Value = sensor.SensorId.ToString() });
            //}

            //ViewBag.SelectList = selectlist;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AddSensor()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddSensor(SensorModel model)
        {
            if (model != null)
            {
                if (!string.IsNullOrEmpty(model.Name) && !string.IsNullOrEmpty(model.Description))
                {
                    using (var _databaseHelper = new DatabaseHelper(configuration))
                    {
                        var valid = _databaseHelper.AddSensor(model);

                        if (valid)
                        {
                            ViewData["Message"] = "Sensor Added";
                            return View("Manage");
                        }
                        else { ViewData["Message"] = "Error: Sensor not added";
                            return View("Manage");
                        }

                    }
                }
            }

            ViewData["Message"] = "Please fill in all the details";
            return View();
        }

     
        [HttpGet]
        public async Task<IActionResult> AddRoom()
        {
            return View();
        }
 
        [HttpPost]
        public IActionResult AddRoom(RoomModel model)
        {
            if (true)
            {
                if (!string.IsNullOrEmpty(model.Name) && !string.IsNullOrEmpty(model.Description))
                {
                    using (var _databaseHelper = new DatabaseHelper(configuration))
                    {
                        var valid = _databaseHelper.AddRoom(model);

                        if (valid)
                        {
                            ViewData["Message"] = "Room Added";
                            return View("Manage");
                        }

                    }
                }
            }

            ViewData["Message"] = "Please fill in all the details";
            return View();
        }
    }
}