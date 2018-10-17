using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using INFT3970Project.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using INFT3970Project.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace INFT3970Project.Controllers
{
    public class UpdatePasswordController
    {
        private readonly IConfiguration configuration;
        private DatabaseHelper _databaseHelper;

        public UpdatePasswordController(IConfiguration configuration, DatabaseHelper databaseHelper)
        {
            this.configuration = configuration;
            _databaseHelper = databaseHelper;
        }

        public IActionResult _UpdatePassword()
        {
            ViewData["Message"] = "Please enter your details.";

            return View();
        }

        public IActionResult UpdatePassword(string Username, string Password)
        {
            if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
            {
                using (var _databaseHelper = new DatabaseHelper(configuration))
                {
                    var valid = _databaseHelper.Authenticate(new UpdatingPasswordModel() { Username = Username, Password = Password });

                    if (true)
                    {
                        return RedirectToAction("Index", "Login");
                    }

                }
            }

            ViewData["Message"] = "Please enter your Login details.";
            return View("Index");


        }
    }
}
}
