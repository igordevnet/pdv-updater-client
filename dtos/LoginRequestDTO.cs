using Newtonsoft.Json;

namespace PdvUpdater.DTOs{
    public class LoginRequestDto
    {
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("password")]
        public string password { get; set; }
        [JsonProperty("deviceName")]
        public string deviceName { get; set; }
        [JsonProperty("deviceId")]
        public string deviceId { get; set; }
    }
}