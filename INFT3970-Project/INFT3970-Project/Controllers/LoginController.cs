using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using INFT3970Project.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using INFT3970Project.Models;

namespace INFT3970Project.Controllers
{
    public class LoginController : Controller
    {
        private readonly IConfiguration configuration;
        private DatabaseHelper _databaseHelper;

        public LoginController(IConfiguration configuration, DatabaseHelper databaseHelper)
        {
            this.configuration = configuration;
            _databaseHelper = new DatabaseHelper(configuration);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login(string username = null, string password = null)
        {
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(username))
            {
                using (var _databaseHelper = new DatabaseHelper(configuration))
                {
                    var valid = _databaseHelper.Authenticate(new LoginModel() { Username = username, Password = password });

                    if (valid)
                    {
                        return RedirectToAction("About", "Home");
                    }

                    var LoginModel = new LoginModel()
                    {
                        Username = username,
                        Password = password,
                        SuccessfulLogin = false
                    };
                    return View("Index", LoginModel);

                }
            }

            ViewData["Message"] = "Please enter your Login details.";
            return View();

        }
    }
}