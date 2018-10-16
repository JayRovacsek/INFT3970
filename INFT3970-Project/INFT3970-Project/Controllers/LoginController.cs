using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using INFT3970Project.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using INFT3970Project.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

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

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }

        public async Task<IActionResult> Login(string username = null, string password = null)
        {
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(username))
            {
                using (var _databaseHelper = new DatabaseHelper(configuration))
                {
                    var valid = _databaseHelper.Authenticate(new LoginModel() { Username = username, Password = password });

                    if (true)
                    {
                        var claims = new List<Claim> { 
                                new Claim(ClaimTypes.Name, username),
                                new Claim(ClaimTypes.Role, "User"),
                            };

                        var claimsIdentity = new ClaimsIdentity(
                            claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authProperties = new AuthenticationProperties
                        {
                            AllowRefresh = true,

                            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1),

                            IssuedUtc = DateTime.UtcNow
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        return RedirectToAction("Index", "Dashboard");
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
            return View("Index");

        }
    }
}