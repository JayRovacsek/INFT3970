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
using Microsoft.AspNetCore.Http;

namespace INFT3970Project.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(IConfiguration configuration, DatabaseHelper databaseHelper, IHttpContextAccessor httpContextAccessor) : base(configuration, databaseHelper, httpContextAccessor)
        {
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }
    }
}
