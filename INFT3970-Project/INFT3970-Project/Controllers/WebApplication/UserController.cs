using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using INFT3970Project.Helpers;
using INFT3970Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;

namespace INFT3970Project.Controllers
{
    [Authorize]
    public class UserController : BaseController
    {
        public UserController(IConfiguration configuration, DatabaseHelper databaseHelper, IHttpContextAccessor httpContextAccessor) : base(configuration, databaseHelper, httpContextAccessor)
        {
        }

        public IActionResult Index()
        {
            //var sensors = _databaseHelper.QueryAllSensors();

            //var selectlist = new List<SelectListItem>();
            //foreach (var sensor in sensors)
            //{
            //    selectlist.Add(new SelectListItem { Text = sensor.Name, Value = sensor.SensorId.ToString() });
            //}

            //ViewBag.SelectList = selectlist;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> UpdatePassword()
        {
            var userId = Convert.ToInt32(Request.Cookies["UserId"]);
            var username = await _databaseHelper.QueryUserEmail(userId);
            var userAndPasswordModel = new UserAndPasswordModel
            {
                User = new UserModel
                {
                    Email = username
                }
            };

            return View(userAndPasswordModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserDetails()
        {
            var userId = Convert.ToInt32(Request.Cookies["UserId"]);
            var UserDetails = await _databaseHelper.QueryUserDetails(userId);
            var updateUserDetailsModel = new UpdateUserDetailsModel
            {
                UserID = new UpdateUserDetailsModel()
                {
                    UserDetails.Value1 = "fName";

                    LastName = UserDetails.lName,
                    ContactNumber = UserDetails.ContractNumber,
                    Email = UserDetails.Email,
                    Status = UserDetails.Status,
                    StreetNumber = UserDetails.StreetNum,
                    StreetName = UserDetails.StreetName,
                    City = UserDetails.City,
                    State = UserDetails.State,
                    Postcode = UserDetails.Postcode,
                    Country = UserDetails.Country,
                }
            };

            return View(UserDetails.FirstOrDefault());
        }

        [HttpPost]
        public IActionResult UpdatePassword(UserAndPasswordModel model)
        {
            if(model != null)
            {
                if (!string.IsNullOrEmpty(model.User.Email) && !string.IsNullOrEmpty(model.User.Email))
                {
                    using (var _databaseHelper = new DatabaseHelper(configuration))
                    {
                        var valid = _databaseHelper.UpdatePassword(model);

                        if (valid)
                        {
                            ViewData["Message"] = "Password Changed";
                            return View("Manage");
                        }
                    }
                }
            }

            ViewData["Message"] = "Didnt work";
            return View();
        }
    }
}