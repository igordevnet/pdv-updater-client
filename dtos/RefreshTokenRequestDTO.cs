using Newtonsoft.Json;

namespace PdvUpdater.DTOs{
    public class RefreshTokenRequestDto {
        [JsonProperty("refreshToken")]
        public string refreshToken { get; set; }
        [JsonProperty("deviceId")]
        public string deviceId { get; set; }
    }
}