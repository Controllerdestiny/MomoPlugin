using System.Diagnostics;
using System.Runtime.InteropServices;
using MomoAPI.Entities;
using MorMor;
using MorMor.Configuration;
using MorMor.Event;
using MorMor.EventArgs;
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
        Config = ConfigHelpr.LoadConfig(SavePath, Config);
        OperatHandler.OnReload += ReloadCondig;
        MorMorAPI.Service.Event.OnGroupMessage += Event_OnGroupMessage;
    }

    public async Task ReloadCondig(ReloadEventArgs args)
    { 
        Config = ConfigHelpr.LoadConfig(SavePath, Config);
        await Task.CompletedTask;
    } 

    private async Task Event_OnGroupMessage(MomoAPI.EventArgs.GroupMessageEventArgs args)
    {
        if (args.MessageContext.Messages.Any(x => x.Type == MomoAPI.Enumeration.SegmentType.File))
        {
            try
            {
                var fileid = args.MessageContext.GetFileId();
                if (fileid != null)
                {
                    var (status, fileinfo) = await args.OneBotAPI.GetFile(fileid);
                    if (string.IsNullOrEmpty(fileinfo.Base64) || fileinfo.FileSize > 1024 * 1024 * 30)
                        return;
                    var buffer = Convert.FromBase64String(fileinfo.Base64);

                    if (TerrariaServer.IsReWorld(buffer))
                    {
                        await args.Reply("检测到Terraria地图，正在生成.map文件....");
                        var uuid = Guid.NewGuid().ToString();
                        Spawn(uuid);
                        var (name, data) = IPCO.Start(uuid, buffer);
                        await args.Reply(new MessageBody().File("base64://" + Convert.ToBase64String(data), name));
                    }
                }
            }
            catch (Exception e)
            {
                await args.Reply("[GetFile] Error" + e.Message);
            }

        }
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
        MorMorAPI.Service.Event.OnGroupMessage -= Event_OnGroupMessage;
    }
}
