using Newtonsoft.Json;

namespace iRacing_setups.Model
{
    public class ConfigDTO
    {
        [JsonProperty("setupsPath")]
        public string SetupsPath { get; set; }

        [JsonProperty("backupPath")]
        public string BackupPath { get; set; }

        [JsonProperty("drivePath")]
        public string DrivePath { get; set; }

        [JsonProperty("includeFolders")]
        public string IncludeFolders { get; set; }
    }
}
