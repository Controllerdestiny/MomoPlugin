using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MomoAPI.Entities;
using MomoAPI.Entities.Segment;
using MorMor;
using MorMor.Plugin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bilibili
{
    public partial class BilibiliPlugin : MorMorPlugin
    {
        public override string Name => "Bilibili插件";

        public override string Description => "提供Bilibili相关功能";

        public override string Author => "少司命";

        public override Version Version => new(1, 0, 0, 2);

        private HttpClient _httpClient;
        public BilibiliPlugin()
        {
            _httpClient = new();
        }

        private async Task<MessageBody> ParseVideo(string parseUrl, string id)
        {
            var message = new MessageBody();
            try
            {
                

                //var url = $"https://api.bilibili.com/x/web-interface/view?aid={aid}";
                var response = await _httpClient.GetAsync(parseUrl);
                response.EnsureSuccessStatusCode();
                var json = JObject.Parse(await response.Content.ReadAsStringAsync());
                var code = (int?)json["code"] ?? throw new Exception("code is null.");
                if (code != 0)
                {
                    throw new Exception($"code is {code}.");
                }
                var data = (JObject?)json["data"] ?? throw new Exception("data is null.");
                var title = (string?)data["title"] ?? throw new Exception("data.title is null.");
                var pic = (string?)data["pic"] ?? throw new Exception("data.pic is null.");
                //var picStream = await _httpClient.GetStreamAsync(pic);
                var owner = (JObject?)data["owner"] ?? throw new Exception("data.owner is null.");
                var ownerName = (string?)owner["name"] ?? throw new Exception("data.owner.name is null.");
                var ctime = (long?)data["ctime"] ?? throw new Exception("data.ctime is null.");
                var stat = (JObject?)data["stat"] ?? throw new Exception("data.stat is null.");
                var view = (string?)stat["view"] ?? throw new Exception("data.stat.view is null.");
                var like = (string?)stat["like"] ?? throw new Exception("data.stat.like is null.");
                var coin = (string?)stat["coin"] ?? throw new Exception("data.stat.coin is null.");
                var favorite = (string?)stat["favorite"] ?? throw new Exception("data.stat.favorite is null.");
                var share = (string?)stat["share"] ?? throw new Exception("data.stat.share is null.");
                var danmaku = (string?)stat["danmaku"] ?? throw new Exception("data.stat.danmaku is null.");
                var reply = (string?)stat["reply"] ?? throw new Exception("data.stat.reply is null.");
                var sb = new StringBuilder();
                sb.AppendLine($"https://www.bilibili.com/video/{id}");
                sb.AppendLine($"UP主：{ownerName}");
                sb.AppendLine($"发布时间：{ctime.ToDateTime().ToString("yyyy-MM-dd HH:mm:ss")}");
                sb.AppendLine($"播放量：{view}");
                sb.AppendLine($"点赞：{like}");
                sb.AppendLine($"投币：{coin}");
                sb.AppendLine($"收藏：{favorite}");
                sb.AppendLine($"转发：{share}");
                sb.AppendLine($"弹幕：{danmaku}");
                sb.AppendLine($"评论：{reply}");
                message.Image(pic);
                message.Add(title);
                message.Add(sb.ToString().Trim());

                try
                {
                    var aid2 = (string?)data["aid"] ?? throw new Exception("data.aid is null.");
                    var url2 = $"https://api.bilibili.com/x/v2/reply?type=1&sort=1&ps=1&oid={aid2}";
                    var response2 = await _httpClient.GetAsync(url2);
                    response2.EnsureSuccessStatusCode();
                    var json2 = JObject.Parse(await response2.Content.ReadAsStringAsync());
                    var code2 = (int?)json2["code"] ?? throw new Exception("code is null.");
                    if (code2 != 0)
                    {
                        throw new Exception("code is not 0.");
                    }
                    var data2 = (JObject?)json2["data"] ?? throw new Exception("data is null.");
                    var replies2_ = data2["replies"] ?? throw new Exception("data.replies is null.");
                    if (replies2_.Type != JTokenType.Null)
                    {
                        var replies2 = (JArray?)replies2_ ?? throw new Exception("data.replies is null.");
                        if (replies2.Count > 0)
                        {
                            var reply2 = (JObject?)replies2[0] ?? throw new Exception("data.reply[0] is null.");
                            var content2 = (JObject?)reply2["content"] ?? throw new Exception("data.reply[0].content is null.");
                            var message2 = (string?)content2["message"] ?? throw new Exception("data.reply[0].content.message is null.");
                            var pictures = (JArray?)content2["pictures"] ?? throw new Exception("data.reply[0].content.pictures is null.");
                            var sb2 = new StringBuilder();
                            sb2.AppendLine("热评：");
                            sb2.AppendLine(message2);
                            message.Add(sb2.ToString().Trim());

                            for (int i = 0; i < pictures.Count; i++)
                            {
                                var picture = (JObject?)pictures[i] ?? throw new Exception($"data.reply[0].content.pictures[{i}] is null.");
                                var imgsrc = (string?)picture["img_src"] ?? throw new Exception($"data.reply[0].content.pictures[{i}].img_src is null.");
                                message.Image(imgsrc);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"热评获取失败：{ex.Message}");
                }

                return message;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                message.Text(ex.Message);
            }
            return message;
        }

        public override void Initialize()
        {
            _httpClient = new HttpClient();
            MorMorAPI.Service.Event.OnGroupMessage += Event_OnGroupMessage;
        }

        private async Task Event_OnGroupMessage(MomoAPI.EventArgs.GroupMessageEventArgs args)
        {
            var text = args.MessageContext.RawText;
            if (args.MessageContext.Messages.FirstOrDefault(x => x.Type == MomoAPI.Enumeration.SegmentType.Json)?.MessageData is MomoAPI.Entities.Segment.DataModel.Json json)
            {
                var data = JsonConvert.DeserializeObject<JObject>(json.Connect);
                var appid = data?["meta"]?["detail_1"]?["appid"];
                if (appid?.ToString() == "1109937557")
                    text = (data?["meta"]?["detail_1"]?["qqdocurl"])?.ToString();
            }
            var b23 = B23_Regex().Match(text);
            var bv = Bv_Regex().Match(text);
            var av = Av_Regex().Match(text);
            if (b23.Success)
            {
                try
                {
                    var _b23 = b23.Groups["B23"].Value;
                    var url = $"https://{_b23}";
                    var response = await _httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    var path = response.RequestMessage?.RequestUri?.OriginalString ?? throw new Exception("request uri is null.");
                    var bvid = BVIDRegex().Match(path).Groups["BVID"].Value;
                    url = $"https://api.bilibili.com/x/web-interface/view?bvid={bvid}";
                    var message = await ParseVideo(url, bvid);
                    if (message is null)
                    {
                        return;
                    }
                    await args.Reply(message);
                }
                catch (Exception ex)
                {
                    await args.Reply(ex.Message);
                }
            }
            else if (bv.Success)
            {
                var bvid = bv.Groups["BVID"].Value;
                var url = $"https://api.bilibili.com/x/web-interface/view?bvid={bvid}";

                var message = await ParseVideo(url, bvid);
                if (message is null)
                {
                    return;
                }
                await args.Reply(message);
            }
            else if (av.Success)
            {
                var sAid = av.Groups["AID"].Value;
                if (!int.TryParse(sAid, out var aid))
                {
                    return;
                }
                var url = $"https://api.bilibili.com/x/web-interface/view?aid={aid}";

                var message = await ParseVideo(url, $"av{aid}");
                if (message is null)
                {
                    return;
                }
                await args.Reply(message);
            }

        }

        protected override void Dispose(bool dispose)
        {
            _httpClient.Dispose();
            base.Dispose();
        }

        [GeneratedRegex(".*(?<BVID>BV[0-9A-Za-z]+).*")]
        private static partial Regex BVIDRegex();


        [GeneratedRegex(@".*(?<B23>b23\.tv/[a-zA-Z0-9]+).*")]
        private static partial Regex B23_Regex();


        [GeneratedRegex(@"[^0-9A-Za-z]*(?<BVID>BV[0-9A-Za-z]+).*")]
        private static partial Regex Bv_Regex();

        [GeneratedRegex(@"[^0-9A-Za-z]*AV(?<AID>[0-9]+).*")]
        private static partial Regex Av_Regex();
    }
}