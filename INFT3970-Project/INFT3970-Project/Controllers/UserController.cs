using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using INFT3970Project.Helpers;
using INFT3970Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;

namespace INFT3970Project.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IConfiguration configuration;
        private DatabaseHelper _databaseHelper;

        public UserController(IConfiguration configuration, DatabaseHelper databaseHelper)
        {
            this.configuration = configuration;
            _databaseHelper = databaseHelper;
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

        [HttpPost]
        public IActionResult UpdatePassword(UserAndPasswordModel model)
        {
            if(model != null)
            {
                if (!string.IsNullOrEmpty(model.User.Email) && !string.IsNullOrEmpty(model.User.Email))
                {
                    using (var _databaseHelper = new DatabaseHelper(configuration))
                    {
                        var valid = _databaseHelper.UpdatePassword(model);

                        if (valid)
                        {
                            ViewData["Message"] = "Password Changed";
                            return View("Manage");
                        }

                    }
                }
            }

            ViewData["Message"] = "Didnt work";
            return View();
        }
    }
}