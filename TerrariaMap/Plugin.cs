using System.Diagnostics;
using System.Runtime.InteropServices;
using MomoAPI.Entities;
using MomoAPI.EventArgs;
using MorMor;
using MorMor.Commands;
using MorMor.Configuration;
using MorMor.Event;
using MorMor.EventArgs;
using MorMor.Permission;
using MorMor.Plugin;
using MorMor.TShock.Server;

namespace TerrariaMap;

public class TerrariaMap : MorMorPlugin
{
    public override string Name => "TerrariaMap";

    public override string Description => "生成TerrariaMap的插件";

    public override string Author => "少司命";

    public override Version Version => new(1, 0, 0, 0);

    public Config Config { get; set; } = new();

    public string SavePath = Path.Combine(MorMorAPI.SAVE_PATH, "TerrariaMap.json");

    public override void Initialize()
    {
        Config = Config.LoadConfig();
        CommandManager.Hook.AddGroupCommand(new("获取地图", LoadWorld, OneBotPermissions.GenerateMap));
        OperatHandler.OnReload += ReloadCondig;
        MorMorAPI.Service.Event.OnGroupUpLoadFile += OnUpFile;
    }

    private async ValueTask OnUpFile(GroupUpLoadFileEventArgs args)
    {
        try
        {
            if (args.UpLoad.Size > 1024 * 1024 * 30)
                return;
            var (status, fileinfo) = await args.OneBotAPI.GetFile(args.UpLoad.ID);
                if (string.IsNullOrEmpty(fileinfo.Base64))
                    return;
                var buffer = Convert.FromBase64String(fileinfo.Base64);

                if (TerrariaServer.IsReWorld(buffer))
                {
                    await args.OneBotAPI.SendGroupMessage(args.GroupId, "检测到Terraria地图，正在生成.map文件....");
                    var uuid = Guid.NewGuid().ToString();
                    Spawn(uuid);
                    var (name, data) = IPCO.Start(uuid, buffer);
                    await args.OneBotAPI.SendGroupMessage(args.GroupId,MessageBody.Builder().File("base64://" + Convert.ToBase64String(data), name));
                }
        }
        catch (Exception e)
        {
            await args.OneBotAPI.SendGroupMessage(args.GroupId, "[GetFile] Error" + e.Message);
        }
    }

    private async ValueTask LoadWorld(CommandArgs args)
    {
        if (MorMorAPI.UserLocation.TryGetServer(args.EventArgs.Sender.Id, args.EventArgs.Group.Id, out var server) && server != null)
        {
            var file = await server.GetWorldFile();
            if (file.Status)
            {
                var base64 = Convert.ToBase64String(file.WorldBuffer);
                await args.EventArgs.Reply(new MessageBody().File("base64://" + base64, file.WorldName + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".wld"));
            }
            else
            {
                await args.EventArgs.Reply("无法连接到服务器!");
            }
        }
        else
        {

            await args.EventArgs.Reply("请切换至一个有效的服务器!");
        }
    }

    public async ValueTask ReloadCondig(ReloadEventArgs args)
    { 
        Config = ConfigHelpr.LoadConfig(SavePath, Config);
        await Task.CompletedTask;
    } 


    private void Spawn(string uuid)
    {
        Process process = new();
        process.StartInfo.WorkingDirectory = Config.AppPath;
        process.StartInfo.FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "TerrariaMap.exe" : "TerrariaMap";
        process.StartInfo.Arguments = "-mapname " + uuid;
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.RedirectStandardInput = false;
        process.StartInfo.RedirectStandardOutput = false;
        process.StartInfo.RedirectStandardError = false;
        process.StartInfo.CreateNoWindow = true;
        if (process.Start())
        {
            process.Close();
        }
    }

    protected override void Dispose(bool dispose)
    {
        OperatHandler.OnReload -= ReloadCondig;
        MorMorAPI.Service.Event.OnGroupUpLoadFile -= OnUpFile;
        CommandManager.Hook.GroupCommandDelegate.RemoveAll(x=>x.CallBack == LoadWorld);
    }
}
