using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using TeamsApplicationServer.Helpers;
using TeamsApplicationServer.Repository;
using TeamsApplicationServer.WebSockets;

namespace TeamsApplicationServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // configure JWT authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                };
            });

            builder.Services.AddControllers();

            builder.Services.AddSingleton<IUserIdProvider, NameUserIdProvider>();
            builder.Services.AddSingleton<IUserRepository, UserRepository>();
            builder.Services.AddSingleton<IChatRepository, ChatRepository>();
            builder.Services.AddSingleton<IGroupRepository, GroupRepository>();
            builder.Services.AddSignalR();

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyHeader()
                          .AllowAnyMethod()
                          .SetIsOriginAllowed(_ => true)
                          .AllowCredentials();
                });
            });

            builder.Services.AddHttpClient();

            builder.WebHost.UseUrls("http://localhost:5197");

            var app = builder.Build();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCors();
            app.MapHub<ChatHub>("/chatHub");
            app.MapControllers();

            
            app.Run();

        }

        
    }
}
