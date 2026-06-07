using System;
using Newtonsoft.Json;
using PdvUpdater.model;

namespace PdvUpdater.DTOs{
    public class UpdateCheckDto {
        [JsonProperty("version")]
        public string version { get; set;}
        [JsonProperty("exeType")]
        public ForceUpdate exeType { get; set;}
        [JsonProperty("force")]
        public bool force { get; set;}
        [JsonProperty("cnpj")]
        public string cnpj { get; set;}
    }
}