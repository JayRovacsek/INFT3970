using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace INFT3970Project.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Temperature()
        {
            return View();
        }

        public IActionResult Humidity()
        {
            return View();
        }

        public IActionResult Motion()
        {
            return View();
        }
    }
}