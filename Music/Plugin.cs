global using MomoAPI.IO;
using MomoAPI.Entities;
using MomoAPI.Entities.Segment;
using MorMor.Commands;
using MorMor.EventArgs;
using MorMor.Extensions;
using MorMor.Permission;
using MorMor.Plugin;

namespace Music;

public class Plugin : MorMorPlugin
{
    public override string Name => "Music";

    public override string Description => "提供点歌功能";

    public override string Author => "少司命";

    public override Version Version => new(1, 0, 0, 0);

    public static Config Config { get; set; } = new();

    public override void Initialize()
    {
        Config = Config.LoadConfig();   
        MorMor.Event.OperatHandler.OnReload += ReloadCondig;
        CommandManager.Hook.AddGroupCommand(new("点歌", Music, OneBotPermissions.Music));
        CommandManager.Hook.AddGroupCommand(new("选", ChageMusic, OneBotPermissions.Music));
    }

    protected override void Dispose(bool dispose)
    {
        MorMor.Event.OperatHandler.OnReload -= ReloadCondig;
        CommandManager.Hook.GroupCommandDelegate.RemoveAll(x => x.CallBack == Music);
        CommandManager.Hook.GroupCommandDelegate.RemoveAll(x => x.CallBack == ChageMusic);
    }

    public static async ValueTask ReloadCondig(ReloadEventArgs? args = null)
    {
        Config = Config.LoadConfig();
        await Task.CompletedTask;
    }

    #region 点歌
    private async ValueTask Music(CommandArgs args)
    {
        if (args.Parameters.Count > 0)
        {
            var musicName = string.Join(" ", args.Parameters);
            if (args.Parameters[0] == "网易")
            {
                if (args.Parameters.Count > 1)
                {
                    try
                    {
                        await args.EventArgs.Reply(new MessageBody().MarkdownImage(await MusicTool.GetMusic163Markdown(musicName[2..])));
                    }
                    catch (Exception ex)
                    {
                        Log.ConsoleError($"点歌错误:{ex.Message}");
                    }
                    MusicTool.ChangeName(musicName[2..], args.EventArgs.Sender.Id);
                    MusicTool.ChangeLocal("网易", args.EventArgs.Sender.Id);
                }
                else
                {
                    await args.EventArgs.Reply("请输入一个歌名!");
                }
            }
            else if (args.Parameters[0] == "QQ")
            {
                if (args.Parameters.Count > 1)
                {
                    await args.EventArgs.Reply(new MessageBody().MarkdownImage(await MusicTool.GetMusicQQMarkdown(musicName[2..])));
                    MusicTool.ChangeName(musicName[2..], args.EventArgs.Sender.Id);
                    MusicTool.ChangeLocal("QQ", args.EventArgs.Sender.Id);
                }
                else
                {
                    await args.EventArgs.Reply("请输入一个歌名!");
                }
            }
            else
            {
                var type = MusicTool.GetLocal(args.EventArgs.Sender.Id);
                if (type == "网易")
                {
                    try
                    {
                        await args.EventArgs.Reply(new MessageBody().MarkdownImage(await MusicTool.GetMusic163Markdown(musicName)));
                    }
                    catch (Exception ex)
                    {
                        await args.EventArgs.Reply(ex.Message);
                    }
                    MusicTool.ChangeName(musicName, args.EventArgs.Sender.Id);
                }
                else
                {

                    await args.EventArgs.Reply(new MessageBody().MarkdownImage(await MusicTool.GetMusicQQMarkdown(musicName)));
                    MusicTool.ChangeName(musicName, args.EventArgs.Sender.Id);
                }

            }
        }
        else
        {
            await args.EventArgs.Reply("请输入一个歌名!");
        }
    }
    #endregion

    #region 选歌
    private async ValueTask ChageMusic(CommandArgs args)
    {
        if (args.Parameters.Count > 0)
        {
            var musicName = MusicTool.GetName(args.EventArgs.Sender.Id);
            if (musicName != null)
            {
                if (int.TryParse(args.Parameters[0], out int id))
                {
                    if (MusicTool.GetLocal(args.EventArgs.Sender.Id) == "QQ")
                    {
                        try
                        {
                            var music = await MusicTool.GetMusicQQ(musicName, id);
                            await args.EventArgs.Reply(
                            [

                                MomoSegment.Music_QQ(music.Url,music.Music,music.Picture,music.Song,music.Singer)
                            ]);
                        }
                        catch (Exception ex)
                        {

                            await args.EventArgs.Reply(ex.Message);

                        }
                    }
                    else
                    {
                        try
                        {
                            var music = await MusicTool.GetMusic163(musicName, id);
                            await args.EventArgs.Reply(
                            [
                                MomoSegment.Music_163(
                                music.JumpUrl,
                                music.MusicUrl,
                                music.Picture,
                                music.Name,
                                string.Join(",",music.Singers))
                            ]);
                        }
                        catch (Exception ex)
                        {
                            await args.EventArgs.Reply(ex.Message);
                        }
                    }

                }
                else
                {
                    await args.EventArgs.Reply("请输入一个正确的序号!");
                }

            }
            else
            {
                await args.EventArgs.Reply("请先点歌!");
            }

        }
        else
        {
            await args.EventArgs.Reply("请输入一个正确的序号!");
        }
    }
    #endregion  
}
