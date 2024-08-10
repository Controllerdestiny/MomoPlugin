

using System.Text.Json;
using System.Text.Json.Serialization;
using MorMor;

namespace OnlineReward;

public class Config
{
    [JsonIgnore]
    public readonly string PATH = Path.Combine(MorMorAPI.SAVE_PATH, "OnlineReward.json");

    [JsonPropertyName("领取比例")]
    public int TimeRate { get; set; } = 100;

    [JsonPropertyName("领取记录")]
    public Dictionary<string, int> Reward { get; set; } = [];

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
