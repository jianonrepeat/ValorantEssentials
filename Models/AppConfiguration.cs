using System.Text.Json;
using System.Text.Json.Serialization;

namespace ValorantEssentials.Models
{
    public class AppConfiguration
    {
        [JsonPropertyName("ValorantPaksPath")]
        public string ValorantPaksPath { get; set; } = string.Empty;

        [JsonPropertyName("QResUrl")]
        public string QResUrl { get; set; } = "https://github.com/jianonrepeat/ScreenResolutionChanger/raw/refs/heads/master/QRes.exe";

        [JsonPropertyName("PaksRepoUrl")]
        public string PaksRepoUrl { get; set; } = "https://raw.githubusercontent.com/jianonrepeat/ValorantEssentials/main/paks";

        public static AppConfiguration LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                var defaultConfig = new AppConfiguration();
                defaultConfig.SaveToFile(filePath);
                return defaultConfig;
            }

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<AppConfiguration>(json) ?? new AppConfiguration();
        }

        public void SaveToFile(string filePath)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(filePath, json);
        }
    }
}