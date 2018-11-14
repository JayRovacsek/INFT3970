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
    public class BaseController : Controller
    {
        public IConfiguration configuration;
        public IHttpContextAccessor _httpContextAccessor;
        public DatabaseHelper _databaseHelper;

        /// <summary>
        /// Base configuration for all controllers
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="databaseHelper"></param>
        /// <param name="httpContextAccessor"></param>
        public BaseController(IConfiguration configuration, DatabaseHelper databaseHelper, IHttpContextAccessor httpContextAccessor)
        {
            this.configuration = configuration;
            _databaseHelper = new DatabaseHelper(configuration);
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Method to get cookie value for all controllers
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetCookie(string key)
        {
            return Request.Cookies[key];
        }

        /// <summary>
        /// Method to set cookie values for all controllers
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expireTime"></param>
        public void SetCookie(string key, string value, int? expireTime)
        {
            CookieOptions option = new CookieOptions();
            if (expireTime.HasValue)
                option.Expires = DateTime.Now.AddMinutes(expireTime.Value);
            else
                option.Expires = DateTime.Now.AddMilliseconds(10);
            Response.Cookies.Append(key, value, option);
        }

        /// <summary>
        /// Method to remove cookie for all controllers
        /// </summary>
        /// <param name="key"></param>
        public void RemoveCookie(string key)
        {
            Response.Cookies.Delete(key);
        }

        /// <summary>
        /// Method to generate randomised RGB value
        /// </summary>
        /// <returns></returns>
        public string GetRandomColour()
        {
            var random = new Random();
            var r = random.Next(0, 255);
            return $"rgba({random.Next(0, 255)}, {random.Next(0, 255)}, {random.Next(0, 255)}, 1)";
        }
    }
}