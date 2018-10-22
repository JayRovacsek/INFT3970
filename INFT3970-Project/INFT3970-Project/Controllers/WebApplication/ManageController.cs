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
            var sensors = _databaseHelper.QueryAllSensors();

            var selectlist = new List<SelectListItem>();
            foreach (var sensor in sensors)
            {
                selectlist.Add(new SelectListItem { Text = sensor.Name, Value = sensor.SensorId.ToString() });
            }

            ViewBag.SelectList = selectlist;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AddSensor()
        {
            var user = _databaseHelper.QueryAllUsers();

            IEnumerable<SelectListItem> items = user.Select(c => new SelectListItem
            {
                Value = c.UserId.ToString(),
                Text = c.fName
            });

            ViewBag.Users = items;

            var rooms = _databaseHelper.QueryAllRooms();

            IEnumerable<SelectListItem> room = rooms.Select(c => new SelectListItem
            {
                Value = c.RoomID.ToString(),
                Text = c.Name
            });

            ViewBag.AllRooms = room;

            var Sensors = _databaseHelper.QueryAllSensors();

            IEnumerable<SelectListItem> Sensor = Sensors.Select(c => new SelectListItem
            {
                Value = c.SensorId.ToString(),
                Text = c.Name
            });

            ViewBag.AllSensors = Sensor;


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


        [HttpPost]
        public ActionResult DeleteSensor(SensorModel model)
        {
            int a = 1;
            if (true)
            {
                if (model.SensorId != null)
                {
                    using (var _databaseHelper = new DatabaseHelper(configuration))
                    {
                        var valid = _databaseHelper.DeleteSensor(model);

                        if (true)
                        {
                            ViewData["Message"] = "Sensor Deleted";
                            return View("Manage");
                        }

                    }
                }
            }

            ViewData["Message"] = "Please fill in all the details";
            return View("Manage, AddRoom");
        }




        [HttpGet]
        public IActionResult AddRoom()
        {

            var rooms = _databaseHelper.QueryAllRooms();

            IEnumerable<SelectListItem> room = rooms.Select(c => new SelectListItem
            {
                Value = c.RoomID.ToString(),
                Text = c.Name
            });

            ViewBag.AllRooms = room;


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



        [HttpPost]
        public ActionResult DeleteRoom(RoomModel model)
        {
            int a = 1;
            if (true)
            {
                if (model.RoomID != null)
                {
                    using (var _databaseHelper = new DatabaseHelper(configuration))
                    {
                        var valid = _databaseHelper.DeleteRoom(model);

                        if (true)
                        {
                            ViewData["Message"] = "Room Deleted";
                            return View("Manage");
                        }

                    }
                }
            }

            ViewData["Message"] = "Please fill in all the details";
            return View("Manage, AddRoom");
        }
    }
}