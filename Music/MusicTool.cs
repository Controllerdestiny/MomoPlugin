
using System.Text;

namespace Music;


public class MusicTool
{
    private static readonly Dictionary<long, string> MusicLocal = new();
    private static readonly Dictionary<long, string> MusicName = new();

    public static async Task<List<QQ.MusicItem>> GetMusicQQList(string musicName)
    {
        return await QQ.MusicQQ.GetMusicList(musicName, Plugin.Config.Key);
    }

    public static async Task<string> QQMusic(string musicName)
    {
        var list = await GetMusicQQList(musicName);
        string ret = "";
        int i = 1;
        list.ForEach(x =>
        {
            ret += $"[{i}].{x.Song} -- {x.Singer}\n";
            i++;
        });
        ret += "资源来自于QQ音乐";
        return ret;
    }

    public static async Task<string> GetMusicQQMarkdown(string musicName)
    {
        var list = await GetMusicQQList(musicName);
        var sb = new StringBuilder($$"""<div align="center">""");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("# QQ音乐");
        for (int i = 0; i < list.Count; i++)
        {
            sb.AppendLine($"## `{i + 1}`- {list[i].Song} -- {string.Join(",", list[i].Singer)}");
        }
        sb.AppendLine();
        sb.AppendLine($$"""</div>""");
        return sb.ToString();
    }

    public static async Task<string> GetMusic163Markdown(string musicName)
    {
        var list = await GetMusic163List(musicName);
        var sb = new StringBuilder($$"""<div align="center">""");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("# 网易云音乐");
        for (int i = 0; i < list.Count; i++)
        {
            sb.AppendLine($"## `{i + 1}`- {list[i].Name} -- {string.Join(",", list[i].Singers)}");
        }
        sb.AppendLine();
        sb.AppendLine($$"""</div>""");
        return sb.ToString();
    }

    public static async Task<string> WangYiMusic(string musicName)
    {
        var list = await GetMusic163List(musicName);
        string ret = "";
        int i = 1;
        list.ForEach(x =>
        {
            ret += $"[{i}].{x.Name} -- {string.Join(",", x.Singers)}\n";
            i++;
        });
        ret += "资源来自于网易音乐";
        return ret;

    }

    public static async Task<List<_163.MusicData>> GetMusic163List(string musicName)
    {
        return await new _163.Music_163().GetMusicListByName(musicName);
    }

    public static async Task<QQ.MusicData?> GetMusicQQ(string musicName, int index)
    {
        return await QQ.MusicQQ.GetMusic(musicName, index, Plugin.Config.Key);
    }

    public static async Task<_163.MusicData?> GetMusic163(string musicName, int index)
    {
        return await new _163.Music_163().GetMusic(musicName, index);
    }

    public static void ChangeLocal(string local, long uin)
    {
        MusicLocal[uin] = local;
    }
    public static void ChangeName(string name, long uin)
    {
        MusicName[uin] = name;
    }

    public static string GetLocal(long uin)
    {
        if (MusicLocal.TryGetValue(uin, out var music))
        {
            return music;
        }
        return "QQ";
    }

    public static string? GetName(long uin)
    {
        if (MusicName.TryGetValue(uin, out var music))
        {
            return music;
        }
        return null;
    }
}
