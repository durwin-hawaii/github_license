using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitApp1.Models;
using Microsoft.AspNetCore.Mvc;

namespace GitApp1.Controllers
{
    class Global
    {
        public static int remaining = 0;
        public static DateTime timeout = DateTime.UtcNow.AddHours(1);
    }
    public class RepositoryController : Controller
    {
        public IActionResult Index(Microsoft.AspNetCore.Http.IFormCollection form)
        {
            GitHub gh = new GitHub();
            string organization = form["searchFor"];
            if (organization != null && !organization.Equals(""))
            {
                gh.GetRepositories(form["searchFor"]);
            }
            ViewBag.Remaining = Global.remaining;
            return View(gh);
        }

        public string License(string repo)
        {
            GitHub gh = new GitHub();
            if (gh.Open_A_Pull_Request(repo))
                return "OK";
            else
                return "ERROR";
        }

        public int TimeOut()
        {
            // this is percent of one hour
            return 100 - (int)(((Global.timeout - DateTime.UtcNow).TotalSeconds)  / 36);
        }
    }
}