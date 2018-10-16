using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using INFT3970Project.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace INFT3970Project.Controllers
{
    public class RegisterController : Controller
    {
        private readonly IConfiguration configuration;
        private DatabaseHelper _databaseHelper;

        public RegisterController(IConfiguration configuration, DatabaseHelper databaseHelper)
        {
            this.configuration = configuration;
            _databaseHelper = databaseHelper;
        }

        public IActionResult Index()
        {
            ViewData["Message"] = "Please enter your details.";

            return View();
        }

        public IActionResult RegisterAccount(string username, string password)
        {
            return View();
        }
    }
}