using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GitApp1.Models;

namespace GitApp1.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        { 
            return RedirectToRoute(new { controller = "Repository", action = ""});
        }
    }


}
