using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace VendingMachines.API.Extensions
{
    public static class JwtAuthenticationExtension
    {
        public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var tokenFromQuery = context.Request.Query["jwt_token"];
                            if (!string.IsNullOrEmpty(tokenFromQuery))
                            {
                                context.Token = tokenFromQuery;
                                return Task.CompletedTask;
                            }

                            if (context.Request.Cookies.TryGetValue("jwt_token", out var token) &&
                                !string.IsNullOrEmpty(token) && IsPossibleJwtToken(token))
                            {
                                context.Token = token;
                            }

                            return Task.CompletedTask;
                        },

                        OnAuthenticationFailed = context =>
                        {
                            context.NoResult();
                            return Task.CompletedTask;
                        }
                    };
                });
        }

        private static bool IsPossibleJwtToken(string? token)
        {
            if (string.IsNullOrWhiteSpace(token)) 
                return false;

            var parts = token.Split('.');
            return parts.Length == 3 && !string.IsNullOrEmpty(parts[0]) &&
                   !string.IsNullOrEmpty(parts[1]) && !string.IsNullOrEmpty(parts[2]);
        }
    }
}
