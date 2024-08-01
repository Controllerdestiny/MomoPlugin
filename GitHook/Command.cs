using MorMor.Attributes;
using MorMor.Commands;
using MorMor.Configuration;

namespace GitHook;

[CommandSeries]
public class Command
{
    [CommandMatch("git", "onebot.githook.admin")]
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
                    Plugin.Config.Remove(res, args.EventArgs.Group.Id);
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
        ConfigHelpr.Write(Plugin.SavePath, Plugin.Config);
    }
}
