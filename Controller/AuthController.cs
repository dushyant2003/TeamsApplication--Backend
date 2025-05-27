using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using TeamsApplicationServer.Helpers;
using TeamsApplicationServer.Model;
using TeamsApplicationServer.Repository;

namespace TeamsApplicationServer.Controller
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IUserRepository _userRepository;
        private readonly HttpClient _httpClient = new HttpClient();
     
        public AuthController(IConfiguration config, IHttpClientFactory httpClientFactory, IUserRepository userRepository)
        {
            _config = config;
            _httpClient = httpClientFactory.CreateClient();
            _userRepository = userRepository;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                // 1. Call Lambda - Active Directory
                var lambdaResponse = await CallLambdaFunction(request.Username);
                lambdaResponse.ToString();
                if (lambdaResponse.Password != request.Password)
                    return Unauthorized();

                string userId = lambdaResponse.Username.ToLower().Replace(" ", "_");

                await _userRepository.GetOrCreateUserAsync(userId, lambdaResponse.Username, lambdaResponse.Email);

                // . Generate JWT if valid
                var token = GenerateJwtToken(request.Username);
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error calling Lambda: {ex.Message}");
            }
        }

        private async Task<LambdaUserResponse> CallLambdaFunction(string username)
        {
            var lambdaUrl = _config["AWS:UserDirectoryLambdaUrl"];

            var lambdaRequest = new
            {
                action = "get",
                username = username
            };

            
            var json = JsonConvert.SerializeObject(lambdaRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(lambdaUrl, content);
            //if (!response.IsSuccessStatusCode)
            //{
            //    var errorContent = await response.Content.ReadAsStringAsync();
            //    throw new Exception($"Lambda call failed: {response.StatusCode}, {errorContent}");
            //}

            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<LambdaUserResponse>(responseContent);
        }
        private string GenerateJwtToken(string username)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _config["Jwt:Key"]!)); // Store key in appsettings.json

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}
