using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using INFT3970Project.Helpers;
using INFT3970Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            return View();
        }

        [HttpPost]
        public IActionResult UpdatePassword(UserAndPasswordModel model)
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

            ViewData["Message"] = "Didnt work";
            return View();
        }
    }
}