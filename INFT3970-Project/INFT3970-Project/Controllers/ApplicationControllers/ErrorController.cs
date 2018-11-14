using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using INFT3970Project.Models;
using Microsoft.AspNetCore.Mvc;

namespace INFT3970Project.Controllers
{
    public class ErrorController : Controller
    {
        /// <summary>
        /// Base error controller
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="httpCode"></param>
        /// <returns></returns>
        public IActionResult Index(string errorMessage, int httpCode)
        {
            return View(new ErrorModel
            {
                HttpCode = httpCode,
                ErrorMessage = errorMessage
            });
        }
    }
}