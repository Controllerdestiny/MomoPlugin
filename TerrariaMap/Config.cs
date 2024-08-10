

using System.Text.Json;
using System.Text.Json.Serialization;
using MorMor;

namespace TerrariaMap;

public class Config
{
    [JsonIgnore]
    public string PATH = Path.Combine(MorMorAPI.SAVE_PATH, "TerrariaMap.Json");

    [JsonPropertyName("程序路径")]
    public string AppPath { get; set; } = string.Empty;

    public Config LoadConfig()
    {
        if (File.Exists(PATH))
            return JsonSerializer.Deserialize<Config>(File.ReadAllText(PATH)) ?? new();
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
