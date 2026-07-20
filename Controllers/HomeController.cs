using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SharedCircle.Models;
using System.Diagnostics;

namespace SharedCircle.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return Redirect("/Identity/Account/Login");
            }

            return View();
        }
    }
}