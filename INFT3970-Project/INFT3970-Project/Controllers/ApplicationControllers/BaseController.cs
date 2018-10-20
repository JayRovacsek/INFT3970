﻿using System;
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
        public enum ApplicationMode
        {
            User = 0,
            Admin = 1,
            Demo = 2
        }

        public BaseController(IConfiguration configuration, DatabaseHelper databaseHelper, IHttpContextAccessor httpContextAccessor)
        {
            this.configuration = configuration;
            _databaseHelper = new DatabaseHelper(configuration);
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCookie(string key)
        {
            return Request.Cookies[key];
        }

        public void SetCookie(string key, string value, int? expireTime)
        {
            CookieOptions option = new CookieOptions();
            if (expireTime.HasValue)
                option.Expires = DateTime.Now.AddMinutes(expireTime.Value);
            else
                option.Expires = DateTime.Now.AddMilliseconds(10);
            Response.Cookies.Append(key, value, option);
        }

        public void RemoveCookie(string key)
        {
            Response.Cookies.Delete(key);
        }

        public string GetRandomColour()
        {
            var random = new Random();
            var r = random.Next(0, 255);
            return $"rgba({random.Next(0, 255)}, {random.Next(0, 255)}, {random.Next(0, 255)}, 1)";
        }
    }
}