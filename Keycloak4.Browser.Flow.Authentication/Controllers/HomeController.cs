using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Keycloak4.Browser.Flow.Authentication.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Title = "Index";
            return View();
        }

        [Authorize]
        public IActionResult Detail()
        {
            ViewBag.Title = "Detail";
            return View();
        }
    }
}
