using Keycloak7.AspNETCore_İle_First_Broker_Login_Flow_Örneği.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Keycloak7.AspNETCore_İle_First_Broker_Login_Flow_Örneği.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public IActionResult SecurePage()
        {
            ViewBag.Name = User.Identity.Name;

            ViewBag.Email = User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;

            return View();
        }
    }
}
