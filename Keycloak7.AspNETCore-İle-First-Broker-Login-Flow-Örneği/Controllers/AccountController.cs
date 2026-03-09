using Keycloak7.AspNETCore_İle_First_Broker_Login_Flow_Örneği.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Keycloak7.AspNETCore_İle_First_Broker_Login_Flow_Örneği.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet("/login")]
        public IActionResult Login(string returnUrl = "/")
        {
            return View(new LoginViewModel() { ReturnUrl = returnUrl });
        }

        [HttpPost("/login")]
        public IActionResult Login(LoginViewModel model)
        {
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = model.ReturnUrl
            }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpPost()]
        public IActionResult LoginWithGoogle(LoginViewModel model)
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/", Items = { ["kc_idp_hint"] = "google" } }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [Authorize]
        [HttpGet("/logout")]
        public IActionResult Logout()
        {
            return SignOut(new AuthenticationProperties { RedirectUri = "/" }, CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}
