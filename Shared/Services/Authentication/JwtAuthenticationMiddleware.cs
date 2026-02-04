using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Shared.Services.Authentication
{
    public class JwtAuthenticationMiddleware(IHttpClientFactory _httpClientFactory, ManualJwtValidator _manualJwtValidator) : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var authorization = context.Request.Headers[HeaderNames.Authorization].ToString();
            if (!string.IsNullOrEmpty(authorization))
            {
                var token = authorization.Substring("Bearer ".Length);
                var principal = await _manualJwtValidator.ValidateAsync(token);

                context.User = principal;
            }

            await next(context);
        }
    }
}
