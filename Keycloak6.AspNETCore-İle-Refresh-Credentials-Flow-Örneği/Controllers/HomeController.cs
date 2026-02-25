using Keycloak6.AspNETCore_İle_Refresh_Credentials_Flow_Örneği.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Keycloak6.AspNETCore_İle_Refresh_Credentials_Flow_Örneği.Controllers
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
    }
}
