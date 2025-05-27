using Newtonsoft.Json;

namespace TeamsApplicationServer.Model
{
    public class LambdaUserResponse
    {
        [JsonProperty("SK")]
        public string SK { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("PK")]
        public string PK { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
       
    }
}
