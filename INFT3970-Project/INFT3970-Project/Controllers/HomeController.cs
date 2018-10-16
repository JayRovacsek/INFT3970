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
using Microsoft.AspNetCore.Authorization;

namespace INFT3970Project.Controllers
{
    [Authorize]
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

        public IActionResult Error()
        {
            return RedirectToAction("Login", "Home");
        }
    }
}
