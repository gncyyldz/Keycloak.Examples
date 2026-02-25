using System.ComponentModel.DataAnnotations;

namespace Keycloak6.AspNETCore_İle_Refresh_Credentials_Flow_Örneği.Models
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Username or Email")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }
    }
}
