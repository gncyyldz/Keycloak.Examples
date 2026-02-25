using Keycloak6.AspNETCore_İle_Refresh_Credentials_Flow_Örneği.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Net.Mime;
using System.Text;

namespace Keycloak6.AspNETCore_İle_Refresh_Credentials_Flow_Örneği.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login(string returnUrl = "/")
        {
            return View(new LoginViewModel() { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = model.ReturnUrl
            }, "oidc");
        }

        public IActionResult Logout()
        {
            return SignOut("Cookies", "oidc");
        }

        public async Task<IActionResult> ResetMailAsync(string userId = "66d5598d-7295-4ec4-ac72-d30628b268ef", string realm = "master")
        {
            var token = await HttpContext.GetTokenAsync("access_token");
            HttpClient httpClient = new HttpClient() { BaseAddress = new Uri("http://127.0.0.1:8080/") };
            httpClient.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {token}");

            var content = new StringContent("""["UPDATE_PASSWORD"]""", Encoding.UTF8, MediaTypeNames.Application.Json);
            var response = await httpClient.PutAsync($"/admin/realms/{realm}/users/{userId}/execute-actions-email", content);
            var result = await response.Content.ReadAsStringAsync();


            return RedirectToAction(nameof(AccountController.Login));
        }
    }
}
