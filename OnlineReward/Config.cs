using Newtonsoft.Json;

namespace OnlineReward;

public class Config
{
    [JsonProperty("领取比例")]
    public int TimeRate { get; set; } = 100;

    [JsonProperty("领取记录")]
    public Dictionary<string, int> Reward { get; set; } = [];
}
