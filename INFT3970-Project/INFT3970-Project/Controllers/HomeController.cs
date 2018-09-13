using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using INFT3970Project.Models;
using System.Data;

namespace INFT3970Project.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }
        public IActionResult LogIn()
        {
            ViewData["Message"] = "Please enter your login details.";

            return View();
        }

        public IActionResult Register()
        {
            ViewData["Message"] = "Please enter your details.";

            return View();
        }

        public void Save(String fName, String lName, String email, String Password, Int32 phone, Int32 StreetNum, String StreetName, Int32 PostCode, String City, String State, String Country)
        {
  
        SqlConnection con = new SqlConnection("Server=tcp:inft3970.database.windows.net,1433;Initial Catalog=INFT3970;Persist Security Info=False;User ID=Dean;Password=***REMOVED***;MultipleActiveResultSets=True;Encrypt=False;TrustServerCertificate=True;Connection Timeout=100;");
            SqlCommand cmd = con.CreateCommand();
            cmd.CommandText = "Execute AddUser @fName @lName @ContactNumber @Email @StreetNum @StreetName @Postcode @City @State @Country @HashedPassword";

            cmd.Parameters.Add("@fName", SqlDbType.VarChar, 40).Value = fName;
            cmd.Parameters.Add("@lName", SqlDbType.VarChar, 40).Value = lName;
            cmd.Parameters.Add("@ContactNumber", SqlDbType.VarChar, 10).Value = phone;
            cmd.Parameters.Add("@Email", SqlDbType.VarChar, 50).Value = email;
            cmd.Parameters.Add("@StreetNum", SqlDbType.VarChar, 10).Value = StreetNum;
            cmd.Parameters.Add("@StreetName", SqlDbType.VarChar, 50).Value = StreetName;
            cmd.Parameters.Add("@Postcode", SqlDbType.VarChar, 5).Value = PostCode;
            cmd.Parameters.Add("@City", SqlDbType.VarChar, 30).Value = City;
            cmd.Parameters.Add("@State", SqlDbType.VarChar, 3).Value = State;
            cmd.Parameters.Add("@Country", SqlDbType.VarChar, 50).Value = Country;
            cmd.Parameters.Add("@HashedPassword", SqlDbType.VarChar, 50).Value = Password;
            
            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();
           


        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
