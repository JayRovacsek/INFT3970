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
    public class AdminController : BaseController
    {
        public AdminController(IConfiguration configuration, DatabaseHelper databaseHelper, IHttpContextAccessor httpContextAccessor) : base(configuration, databaseHelper, httpContextAccessor)
        {
        }
    
        /// <summary>
        /// Index Page
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
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

        /// <summary>
        /// Synchronous method to add a sensor
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult AddSensor()
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

        /// <summary>
        /// Method to handle POST route of Synchronous AddSensor method
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Synchronous method to delete sensor
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteSensor(SensorModel model)
        {
            if (model != null)
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

            ViewData["Message"] = "Please fill in all the details";
            return View("Manage, AddRoom");
        }

        /// <summary>
        /// Synchronous method to return view when adding a room
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Synchronous method to handle POST of room model to be added
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Synchronous method to delete room
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Synchronous method to view logs
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Logs()
        {
            var model = _databaseHelper.QueryAllLogs(100);
            return View(model.ToList());
        }
    }
}