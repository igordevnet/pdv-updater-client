using Newtonsoft.Json;
namespace PdvUpdater.DTOs{
    public class AuthResponseDto {
        [JsonProperty("access_token")]
        public string access_token { get; set; }
        [JsonProperty("refresh_token")]
        public string refresh_token { get; set; }

    }
}