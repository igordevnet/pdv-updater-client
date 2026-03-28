using Newtonsoft.Json;

namespace PdvUpdater.DTOs{
    public class UpdateCheckDto {
        [JsonProperty("version")]
        public string version { get; set;}
    }
}