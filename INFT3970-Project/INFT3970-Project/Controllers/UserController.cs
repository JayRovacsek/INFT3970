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
            var userSensorPasswordViewModel = _databaseHelper.GetUserSensorAndPasswordViewModel(1);
            //throw new ApplicationException("Need to fix this to get userId");
            return View(userSensorPasswordViewModel);
        }


        [HttpPost]
        public IActionResult UpdatePassword(LoginModel model)
        {
            // UPDATE PASSWORD HERE

            var success = true;

            if (success)
            {
                return RedirectToAction("Index", "User", "Successfully updated password");
            }
            return RedirectToAction("Index", "User", "Unable to update password");
        }
    }
}