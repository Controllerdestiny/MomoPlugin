
using MomoAPI.Entities;
using MomoAPI.Entities.Segment;
using MomoAPI.Entities.Segment.DataModel;
using MomoAPI.Extensions;
using MorMor.Commands;
using MorMor.Plugin;
using PuppeteerSharp;
using System.Security.Policy;

namespace Disorder;

public class Plugin : MorMorPlugin
{
    private const string FellowUrl = "https://oiapi.net/API/CPUni/";

    private readonly static HttpClient client = new();

    private static Config Config = new();

    public override void Initialize()
    {
        Config = Config.Read();
        CommandManager.Hook.AddGroupCommand(new("今日道侣", Fellow, ""));
    }

    private async ValueTask Fellow(CommandArgs args)
    {
        var w = Config.GetFollow(args.EventArgs.SenderInfo.UserId);
        long targerid = 0;
        var targetName = string.Empty;
        if (w != null && w.Time.ToString("yyyy MM dd") == DateTime.Now.ToString("yyyy MM dd"))
        {
            targerid = w.Follow;
            targetName = w.WeaponName;

        }
        else
        { 
            var(status, members) = await args.EventArgs.OneBotAPI.GetGroupMemberList(args.EventArgs.Group.Id);
            var targer = members.OrderBy(x => Guid.NewGuid()).First();
            targerid = targer.UserId;
            targetName = targer.Nick.Length > 6 ? targer.Nick[..6] : targer.Nick;
            Config.SaveFollow(args.EventArgs.Sender.Id, targerid, targetName);
            Config.Save();
        }
       
        var stream = await client.GetByteArrayAsync(FellowUrl + $"?first={(args.EventArgs.SenderInfo.Name.Length > 6 ? args.EventArgs.SenderInfo.Name[..6] : args.EventArgs.SenderInfo.Name)}&second={targetName}");
        List<CustomNode> nodes =[
                new CustomNode(targetName, args.EventArgs.SelfId, new MessageBody().Text($"今日道侣")),
                new CustomNode(targetName, args.EventArgs.SelfId, new MessageBody().Image($"http://q.qlogo.cn/headimg_dl?dst_uin={targerid}&spec=640&img_type=png").Text($"账号: {targerid}\n昵称: {targetName}")),
                new CustomNode(targetName, args.EventArgs.SelfId, new MessageBody().Image(stream))
            ];
        await args.EventArgs.OneBotAPI.SendGroupForwardMsg(args.EventArgs.Group.Id, nodes);
    }

    protected override void Dispose(bool dispose)
    {
        CommandManager.Hook.GroupCommandDelegate.RemoveAll(x => x.CallBack == Fellow);
    }
}
