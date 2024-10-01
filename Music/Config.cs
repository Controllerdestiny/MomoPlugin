
using System.Text.Json;
using System.Text.Json.Serialization;
using MorMor;

namespace Music;

public class Config
{
    [JsonIgnore]
    public string PATH = Path.Combine(MorMorAPI.SAVE_PATH, "Music.Json");

    [JsonPropertyName("访问Key")]
    public string Key { get; set; } = string.Empty;

    public Config LoadConfig()
    {
        if (File.Exists(PATH))
            return JsonSerializer.Deserialize<Config>(File.ReadAllText(PATH)) ?? new();
        Save();
        return new();
    }

    public void Save()
    {
        File.WriteAllText(PATH, JsonSerializer.Serialize(this, new JsonSerializerOptions()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        }));
    }
}
