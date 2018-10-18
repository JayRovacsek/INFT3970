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
using Microsoft.AspNetCore.Http;

namespace INFT3970Project.Controllers
{
    public class LoginController : BaseController
    {
        public LoginController(IConfiguration configuration, DatabaseHelper databaseHelper, IHttpContextAccessor httpContextAccessor) : base(configuration, databaseHelper, httpContextAccessor)
        {
        }

        public IActionResult Index()
        {
            if (User.IsInRole("User"))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [Authorize]
        public int GetUserId(string username)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var result = _databaseHelper.GetUserId(username);
                return result;
            }
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            RemoveAllCookies();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }

        [Authorize]
        public bool IsAdministrator(int userId)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var result = _databaseHelper.IsUserAdministrator(userId);
                return result;
            }
        }

        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!string.IsNullOrEmpty(model.Username) && !string.IsNullOrEmpty(model.Username))
            {
                using (var _databaseHelper = new DatabaseHelper(configuration))
                {
                    var valid = _databaseHelper.Authenticate(new LoginModel() { Username = model.Username, Password = model.Password });

                    if (valid)
                    {
                        RemoveAllCookies();

                        var claims = new List<Claim> { 
                                new Claim(ClaimTypes.Name, model.Username),
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

                        SetCookie("UserId", GetUserId(model.Username).ToString(), 60);

                        var userId = GetUserId(model.Username);

                        if (IsAdministrator(userId))
                        {
                            SetCookie("IsAdmin", "true", 60);
                        }

                        return RedirectToAction("Index", "Home");
                    }

                    var LoginModel = new LoginModel()
                    {
                        Username = model.Username,
                        Password = model.Password,
                        SuccessfulLogin = false
                    };
                    return View("Index", LoginModel);

                }
            }

            ViewData["Message"] = "Please enter your Login details.";
            return View("Index");
        }

        public void RemoveAllCookies()
        {
            foreach (var cookie in Request.Cookies)
            {
                RemoveCookie(cookie.Key);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var valid = await _databaseHelper.Register(model);

                if (valid)
                {
                    return RedirectToAction("Index", "Login");
                }

            }

            return View();
        }
    }
}