
using MomoAPI.Extensions;
using System.Text.Json.Serialization;

namespace Disorder;

public class Config
{
    public class FollowWeapon
    {
        [JsonPropertyName("时间")]
        public DateTime Time  { get; set;}

        [JsonPropertyName("道侣")]
        public long Follow { get; set; }

        [JsonPropertyName("名称")]
        public string WeaponName { get; set;} = string.Empty;
    }

    private static string PATH = Path.Combine(MorMor.MorMorAPI.SAVE_PATH, "Follow.json");

    [JsonPropertyName("道侣数据")]
    public Dictionary<long, FollowWeapon> TempSave { get; set; } = [];

    public FollowWeapon? GetFollow(long userid)
    {
        if (TempSave.TryGetValue(userid, out var temp))
        {
            return temp;
        }
        return null;
    }

    public void SaveFollow(long userid, long targetid, string targetName)
    {
        TempSave[userid] = new FollowWeapon()
        {
            Follow = targetid,
            WeaponName = targetName,
            Time = DateTime.Now
        };
    }

    public void Save()
    { 
        File.WriteAllText(PATH,this.ToJson());
    }

    public static Config Read()
    {
        if (File.Exists(PATH))
        {
            return File.ReadAllText(PATH).ToObject<Config>() ?? new();
        }
        else
        { 
            var config = new Config();
            config.Save();
            return config;
        }
    }
}
