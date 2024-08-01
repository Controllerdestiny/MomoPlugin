using MorMor;
using MorMor.Commands;
using MorMor.Plugin;
using MorMor.Utils;

namespace BindTSplayer;

public class Plugin : MorMorPlugin
{
    public override string Name => "BindTSPlayer";

    public override string Description => "提供除注册以外的绑定账号功能";

    public override string Author => "少司命";

    public override Version Version => new(1, 0, 0, 0);


    private readonly Dictionary<long, List<Tuple<string, string>>> _temp = [];
   
    public override void Initialize()
    {
        CommandManager.Hook.Add(new("绑定", BindPlayer, "onebot.tshock.bind"));
    }

    private async Task BindPlayer(CommandArgs args)
    {
        if (!MorMorAPI.UserLocation.TryGetServer(args.EventArgs.Sender.Id, args.EventArgs.Group.Id, out var server) || server == null)
        {
            await args.EventArgs.Reply("服务器不存在或，未切换至一个服务器！", true);
            return;
        }
        if (MorMorAPI.TerrariaUserManager.GetUserById(args.EventArgs.Sender.Id, server.Name).Count >= server.RegisterMaxCount)
        {
            await args.EventArgs.Reply($"同一个服务器上绑定账户不能超过{server.RegisterMaxCount}个", true);
            return;
        }

        if (args.Parameters.Count == 1)
        {
            var userName = args.Parameters[0];
            var account = await server.QueryAccount();
            if (!account.Status || !account.Accounts.Any(x => x.Name == userName))
            {
                await args.EventArgs.Reply($"没有在服务器中找到{userName}账户，无法绑定!", true);
                return;
            }
            var token = Guid.NewGuid().ToString()[..8];
            AddTempData(args.EventArgs.Group.Id, userName, token);
            MailHelper.SendMail($"{args.EventArgs.Sender.Id}@qq.com", "绑定账号验证码", $"您的验证码为: {token}");
            await args.EventArgs.Reply($"绑定账号 {userName} => {args.EventArgs.Sender.Id} 至{server.Name}服务器!" +
                $"\n请在之后进行使用/绑定 验证 [令牌]" +
                $"\n验证令牌已发送至你的邮箱点击下方链接可查看" +
                $"\nhttps://wap.mail.qq.com/home/index");
        }
        else if (args.Parameters.Count == 2 && args.Parameters[0] == "验证")
        {
            if (GetTempData(args.EventArgs.Group.Id, args.Parameters[1], out var name))
            {
                try
                {
                    MorMorAPI.TerrariaUserManager.Add(args.EventArgs.Sender.Id, args.EventArgs.Group.Id, server.Name, name, "");
                    await args.EventArgs.Reply($"验证完成！\n已绑定账号至{server.Name}服务器!", true);
                }
                catch (Exception ex)
                {
                    await args.EventArgs.Reply(ex.Message, true);
                }
            }
            else
            {
                await args.EventArgs.Reply($"请先输入{args.CommamdPrefix}{args.Name} [名称] 在进行验证", true);
            }
        }
    }

    public void AddTempData(long groupid, string name, string token)
    {
        if (_temp.TryGetValue(groupid, out var list) && list != null)
        {
            list.Add(new Tuple<string, string>(name, token));
        }
        else
        {
            _temp[groupid] = [new Tuple<string, string>(name, token)];
        }
    }

    public bool GetTempData(long groupid, string token, out string? name)
    {
        if (_temp.TryGetValue(groupid, out var list) && list != null)
        {
            var res = list.Find(x => x.Item2 == token);
            name = res?.Item1;
            return res != null;
        }
        name = null;
        return false;
    }

    protected override void Dispose(bool dispose)
    {
       
    }
}
