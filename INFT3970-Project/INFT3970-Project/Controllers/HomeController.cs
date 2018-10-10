using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using INFT3970Project.Helpers;
using Microsoft.AspNetCore.Mvc;
using INFT3970Project.Models;
using Microsoft.Extensions.Configuration;

namespace INFT3970Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration configuration;
        private DatabaseHelper _databaseHelper;

        public HomeController(IConfiguration configuration)
        {
            this.configuration = configuration;
            _databaseHelper = new DatabaseHelper(configuration);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }
        public IActionResult LogIn(string username = null, string password = null)
        {
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(username))
            {
                using (var _databaseHelper = new DatabaseHelper(configuration))
                {
                    var valid = _databaseHelper.Authenticate(username, password);

                    if (valid)
                    {
                        return RedirectToAction("About", "Home");
                    }

                    var loginModel = new LoginModel()
                    {
                        Username = username,
                        Password = password,
                        SuccessfulLogin = false
                    };
                    return View("LogIn", loginModel);

                }
            }

            ViewData["Message"] = "Please enter your login details.";
            return View();

        }

        public IActionResult Error()
        {
            return RedirectToAction("LogIn", "Home");
        }
    }
}
