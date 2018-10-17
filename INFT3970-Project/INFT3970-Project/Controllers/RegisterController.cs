using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using INFT3970Project.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using INFT3970Project.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

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

        public IActionResult Register(string fName, string lName, string ContactNumber, string Email, string StreetNum, string StreetName, string Postcode, string City, string State, string Country, string Password)
        {
            if (!string.IsNullOrEmpty(fName) && !string.IsNullOrEmpty(lName) && !string.IsNullOrEmpty(ContactNumber) && !string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(StreetNum) && !string.IsNullOrEmpty(StreetName) && !string.IsNullOrEmpty(Postcode) && !string.IsNullOrEmpty(City) && !string.IsNullOrEmpty(State) && !string.IsNullOrEmpty(Country) && !string.IsNullOrEmpty(Password))
            {
                using (var _databaseHelper = new DatabaseHelper(configuration))
                {
                    var valid = _databaseHelper.Authenticate(new RegisterModel() { fName = fName, lName = lName, ContactNumber = ContactNumber, Email = Email, StreetNum= StreetNum, StreetName = StreetName, Postcode = Postcode, City = City, State = State, Country = Country, Password = Password});

                    if (true)
                    {
                        return RedirectToAction("Index", "Login");
                    }

                }
            }

            ViewData["Message"] = "Please enter your Login details.";
            return View("Index");

        }
    }
}
