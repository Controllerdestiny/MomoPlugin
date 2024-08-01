﻿using Microsoft.Extensions.Primitives;
using System.Net;
using System.Text;
using MorMor.Plugin;
using Octokit.Webhooks;
using MorMor;
using MorMor.Configuration;
using System.Reflection;
using MorMor.Commands;
using MorMor.Permission;

namespace GitHook;

public class Plugin : MorMorPlugin
{
    public override string Name => "Githubr";

    public override string Description => "监听github Webhook";

    public override string Author => "少司命";

    public override Version Version => new(1, 0, 0, 0);

    public static readonly string SavePath = Path.Combine(MorMorAPI.SAVE_PATH, "GitWebHook.json");

    private readonly WebhookEventProcessor WebhookEventProcessor;

    private readonly HttpListener HttpListener;

    internal static Config Config = new();
    public Plugin()
    {
        Config = ConfigHelpr.LoadConfig<Config>(SavePath);
        HttpListener = new HttpListener();
        WebhookEventProcessor = new WebHook();
    }

    private Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
    {
        string resourceName = $"{Assembly.GetExecutingAssembly().GetName().Name}.{new AssemblyName(args.Name).Name}.dll";
        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
        {
            if (stream == null)
                throw new NullReferenceException("无法加载程序集:" + args.Name);
            byte[] assemblyData = new byte[stream.Length];
            stream.Read(assemblyData, 0, assemblyData.Length);
            return Assembly.Load(assemblyData);
        }
    }
    public override void Initialize()
    {
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        CommandManager.Hook.Add(new("git", GitHubActionManager, "onebot.git.hook"));
        MorMor.Event.OperatHandler.OnReload += OperatHandler_OnReload;
        HttpListener.Prefixes.Add($"http://*:{Config.Port}{Config.Path}");
        HttpListener.Start();
        HttpListener.BeginGetContext(OnContext, null);
    }

    private Task OperatHandler_OnReload(MorMor.EventArgs.ReloadEventArgs args)
    {
        Config = ConfigHelpr.LoadConfig<Config>(SavePath);
        args.Message.Add("github webhook 重读成功!");
        return Task.CompletedTask;
    }

    private async void OnContext(IAsyncResult ar)
    {
        HttpListener.BeginGetContext(OnContext, null);
        var data = HttpListener.EndGetContext(ar);
        if (!Config.Enable)
            return;
        if (data.Request.HttpMethod == "POST")
        {
            var hearder = new Dictionary<string, StringValues>();
            foreach (var key in data.Request.Headers.AllKeys)
            {
                if (!string.IsNullOrEmpty(key))
                    hearder[key] = data.Request.Headers[key];
            }
            using StreamReader stream = new(data.Request.InputStream);
            var body = stream.ReadToEnd();
            await WebhookEventProcessor.ProcessWebhookAsync(hearder, body);
        }
        var result = Encoding.UTF8.GetBytes("response on github");
        data.Response.StatusCode = 200;
        data.Response.OutputStream.Write(result, 0, result.Length);
        data.Response.Close();
    }

    public async Task GitHubActionManager(CommandArgs args)
    {
        string? MatchAction()
        {
            var subcmd = args.Parameters[0];
            var type = args.Parameters[1];
            var msg = $"{subcmd} {type} Success!";
            switch (type)
            {
                case "release":
                    return Octokit.Webhooks.WebhookEventType.Release;
                case "pr":
                    return Octokit.Webhooks.WebhookEventType.PullRequest;
                case "push":
                    return Octokit.Webhooks.WebhookEventType.Push;
                case "star":
                    return Octokit.Webhooks.WebhookEventType.Star;
                default:
                    return null;
            }
        }
        if (args.Parameters.Count == 2)
        {
            var subcmd = args.Parameters[0];
            var type = args.Parameters[1];
            var res = MatchAction();
            if (args.Parameters[0].ToLower() == "listen")
            {
                if (res != null)
                {
                    Plugin.Config.Add(res, args.EventArgs.Group.Id);
                    await args.EventArgs.Reply($"{subcmd} {type} Success!");
                }
                else
                {
                    await args.EventArgs.Reply($"未知的事件类型:{type}!");
                }
            }
            else if (args.Parameters[0].ToLower() == "remove")
            {
                if (res != null)
                {
                    Config.Remove(res, args.EventArgs.Group.Id);
                    await args.EventArgs.Reply($"{subcmd} {type} Success!");
                }
                else
                {
                    await args.EventArgs.Reply($"未知的事件类型:{type}!");
                }
            }
            else
            {
                await args.EventArgs.Reply($"子命令错误!");
            }
        }
        else
        {
            await args.EventArgs.Reply($"语法错误，正确语法:{args.CommamdPrefix}{args.Name} [listen|remove] [release|pr|star|push]");
        }
        ConfigHelpr.Write(SavePath, Config);
    }


    protected override void Dispose(bool dispose)
    {
        if (dispose)
            HttpListener.Close();
    }
}