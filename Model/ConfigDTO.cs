using Newtonsoft.Json;

namespace iRacing_setups.Model
{
    public class ConfigDTO
    {
        [JsonProperty("setupsPath")]
        public string SetupsPath { get; set; }

        [JsonProperty("backupPath")]
        public string BackupPath { get; set; }

        [JsonProperty("mail")]
        public string Mail { get; set; }

        [JsonProperty("clientToken")]
        public string ClientToken { get; set; }

        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        [JsonProperty("clientSecret")]
        public string ClientSecret { get; set; }

        [JsonProperty("includeFolders")]
        public string IncludeFolders { get; set; }
    }
}
