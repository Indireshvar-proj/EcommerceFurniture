using AngularProject.Models;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DotNetWebAPI.Middlewares
{
    public class AuthMiddleware : IMiddleware
    {
        private readonly UserManager<User> userManager;

        public AuthMiddleware(UserManager<User> _userManager)
        {
            userManager = _userManager;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                string authHeader = context.Request.Headers["Authorization"].ToString();

                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    string tokenStr = authHeader.Substring("Bearer ".Length).Trim();

                    var handler = new JwtSecurityTokenHandler();

                    if (handler.CanReadToken(tokenStr))
                    {
                        var token = handler.ReadJwtToken(tokenStr);

                        var userIdClaim = token.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier);

                        if (userIdClaim != null)
                        {
                            string userId = userIdClaim.Value;

                            User user = await userManager.FindByIdAsync(userId);

                            context.Items["User"] = user;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle errors (optional)
                Console.WriteLine($"AuthMiddleware error: {ex.Message}");
            }

            await next(context);
        }
    }
}