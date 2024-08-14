using System.Text;
using MorMor;
using MorMor.Commands;
using MorMor.Configuration;
using MorMor.Event;
using MorMor.Plugin;

namespace OnlineReward;

public class Plugin : MorMorPlugin
{
    public override string Name => "OnlineReward";

    public override string Description => "领取在线时长奖励";

    public override string Author => "少司命";

    public override Version Version => new(1, 0, 0, 0);

    public Config Config = new();
    public override void Initialize()
    {
        Config = Config.LoadConfig();
        CommandManager.Hook.Add(new("领取在线奖励", CReward, MorMor.Permission.OneBotPermissions.OnlineRank));
        OperatHandler.OnCommand += OperatHandler_OnCommand;
        OperatHandler.OnReload += OperatHandler_OnReload;
    }

    private async ValueTask OperatHandler_OnReload(MorMor.EventArgs.ReloadEventArgs args)
    {
        Config = Config.LoadConfig();
        args.Message.Add("\n在线时长奖励配置重读成功!");
        await Task.CompletedTask;
    }

    private async ValueTask OperatHandler_OnCommand(CommandArgs args)
    {
        if (args.Name == "泰拉服务器重置")
        {
            Config.Reward.Clear();
            Config = Config.LoadConfig();
        }   
        await Task.CompletedTask;
    }

    private async ValueTask CReward(CommandArgs args)
    {
        if (MorMorAPI.UserLocation.TryGetServer(args.EventArgs.Sender.Id, args.EventArgs.Group.Id, out var server) && server != null)
        {
            var user = MorMorAPI.TerrariaUserManager.GetUserById(args.EventArgs.Sender.Id, server.Name);
            if (user.Count == 0)
            {
                await args.EventArgs.Reply("未找到注册的账户", true);
                return;
            }
            var online = await server.OnlineRank();
            if (!online.Status)
            {
                await args.EventArgs.Reply(online.Message, true);
                return;
            }
            var sb = new StringBuilder();
            foreach (var u in user)
            {
                if (online.OnlineRank.TryGetValue(u.Name, out var time))
                {
                    Config.Reward.TryGetValue(u.Name, out int ctime);
                    var ntime = time - ctime;
                    if (ntime > 0)
                    {
                        Config.Reward[u.Name] = time;
                        sb.AppendLine($"角色: {u.Name}在线时长{time}秒,本次领取{ntime}秒奖励，共{ntime * Config.TimeRate}个星币!");
                        MorMorAPI.CurrencyManager.Add(args.EventArgs.Group.Id, args.EventArgs.Sender.Id, ntime * Config.TimeRate);
                    }
                    else
                    {
                        sb.AppendLine($"角色: {u.Name}因在线时长不足无法领取");
                    }
                }
                else
                {
                    sb.AppendLine($"角色: {u.Name}因在线时长不足无法领取");
                }
            }
            await args.EventArgs.Reply(sb.ToString().Trim());
            Config.Save();
        }
        else
        {
            await args.EventArgs.Reply("请切换至一个有效的服务器!", true);
            return;
        }
    }

    protected override void Dispose(bool dispose)
    {
        CommandManager.Hook.CommandDelegate.RemoveAll(x => x.CallBack ==  CReward);
        OperatHandler.OnCommand -= OperatHandler_OnCommand;
        OperatHandler.OnReload -= OperatHandler_OnReload;
    }
}
