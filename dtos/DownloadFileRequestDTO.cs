using Newtonsoft.Json;

namespace PdvUpdater.DTOs{
    public class DownloadFileRequestDto {
         [JsonProperty("accessToken")]
        public string accessToken { get; set; }
        [JsonProperty("deviceName")]
        public string deviceName { get; set; }
    }
}