using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Combot.IRCServices.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Combot.Modules.Plugins
{
    public class Url_Parsing : Module
    {
        private const string YOUTUBE_URL = "(((youtube.*(v=|/v/))|(youtu\\.be/))(?<ID>[-_a-zA-Z0-9]+))";
        public override void Initialize()
        {
            Bot.IRC.Message.ChannelMessageReceivedEvent += HandleChannelMessage;
        }

        public void HandleChannelMessage(object sender, ChannelMessage message)
        {
            Regex urlRegex = new Regex("(https?)://([\\w-]+\\.)+[\\w-]+(/[\\w-./?%&=]*)?");

            if (Enabled
                && !Bot.ServerConfig.ChannelBlacklist.Contains(message.Channel)
                && !Bot.ServerConfig.NickBlacklist.Contains(message.Sender.Nickname)
                && !ChannelBlacklist.Contains(message.Channel)
                && !NickBlacklist.Contains(message.Sender.Nickname)
                && !Bot.IsCommand(message.Message))
            {
                if (urlRegex.IsMatch(message.Message))
                {
                    // Check to see if it's being spammed
                    if (Bot.SpamCheck(Bot.IRC.Channels.Find(chan => chan.Name == message.Channel), message.Sender, this, new Command() { Name = string.Format( "{0} Commands", Name) }))
                    {
                        MatchCollection urlMatches = urlRegex.Matches(message.Message);
                        for (int i = 0; i < urlMatches.Count; i++)
                        {
                            Match urlMatch = urlMatches[i];
                            Uri url = new Uri(urlMatch.Value);
                            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                            webRequest.Method = "HEAD";
                            webRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                            try
                            {
                                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                                {
                                    int code = (int)webResponse.StatusCode;
                                    if (code == 200)
                                    {
                                        string contentType = webResponse.ContentType.Split('/')[0];
                                        long contentLength = webResponse.ContentLength;
                                        switch (contentType)
                                        {
                                            case "image":
                                                Bot.IRC.Command.SendPrivateMessage(message.Channel, string.Format("[{0}] Size: {1}", webResponse.ContentType, ToFileSize(contentLength)));
                                                break;
                                            case "video":
                                                Bot.IRC.Command.SendPrivateMessage(message.Channel, string.Format("[Video] Type: {0} | Size: {1}", webResponse.ContentType.Split('/')[1], ToFileSize(contentLength)));
                                                break;
                                            case "application":
                                                Bot.IRC.Command.SendPrivateMessage(message.Channel, string.Format("[Application] Type: {0} | Size: {1}", webResponse.ContentType.Split('/')[1], ToFileSize(contentLength)));
                                                break;
                                            case "audio":
                                                Bot.IRC.Command.SendPrivateMessage(message.Channel, string.Format("[Audio] Type: {0} | Size: {1}", webResponse.ContentType.Split('/')[1], ToFileSize(contentLength)));
                                                break;
                                            default:
                                                Regex ytRegex = new Regex(YOUTUBE_URL);
                                                if (ytRegex.IsMatch(urlMatch.ToString()))
                                                {
                                                    Match ytMatch = ytRegex.Match(urlMatch.ToString());
                                                    string youtubeMessage = GetYoutubeDescription(ytMatch.Groups["ID"].Value);

                                                    Regex ytTitle = new Regex(YOUTUBE_URL);
                                                    if (ytTitle.IsMatch(youtubeMessage))
                                                    {
                                                        youtubeMessage = ytTitle.Replace(youtubeMessage, string.Empty);
                                                    }
                                                    Bot.IRC.Command.SendPrivateMessage(message.Channel, youtubeMessage);
                                                }
                                                else
                                                {
                                                    ParseTitle(message, urlMatch.ToString());
                                                }
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        Bot.IRC.Command.SendPrivateMessage(message.Channel, string.Format("[URL] Returned Status Code \u0002{0}\u0002 ({1})", code, url.Host));
                                    }
                                }
                            }
                            catch (WebException ex)
                            {
                                if (ex.Response != null)
                                {
                                    int code = (int)((HttpWebResponse)ex.Response).StatusCode;
                                    Bot.IRC.Command.SendPrivateMessage(message.Channel, string.Format("[URL] Response Code: \u0002{0}\u0002 ({1})", code, url.Host));
                                }
                            }
                            catch (OutOfMemoryException ex)
                            {
                                Bot.IRC.Command.SendPrivateMessage(message.Channel, string.Format("[URL] \u0002Site content was too large\u0002 ({0})", url.Host));
                            }
                        }
                    }
                }
            }
        }

        public void ParseTitle(ChannelMessage message, string urlString)
        {
            string title = string.Empty;
            bool startTagFound = false;
            Uri url = new Uri(urlString);

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(urlString);
            StreamReader streamReader = new StreamReader(req.GetResponse().GetResponseStream());

            Char[] buf = new Char[256];
            int count = streamReader.Read(buf, 0, 256);

            var stopwatch = Stopwatch.StartNew();
            TimeSpan timeout = new TimeSpan(0, 0, 15);
            while (count > 0 && stopwatch.Elapsed < timeout)
            {
                String outputData = new String(buf, 0, count);

                if (!startTagFound)
                {
                    // check for a full match
                    Match fullMatch = Regex.Match(outputData, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase);
                    if (fullMatch.Success)
                    {
                        title = fullMatch.Groups["Title"].Value;
                        break;
                    }
                }

                string pattern = string.Empty;
                if (startTagFound)
                {
                    pattern = @"^(?<Title>[\s\S]*?)\</title\>";
                    Match match = Regex.Match(outputData, pattern, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        title += match.Groups["Title"].Value;
                        break;
                    }
                    title += outputData;
                }
                else
                {
                    pattern = @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)$";
                    Match match = Regex.Match(outputData, pattern, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        title = match.Groups["Title"].Value;
                        startTagFound = true;
                    }
                }
                count = streamReader.Read(buf, 0, 256);
            }
            streamReader.Close();

            if (!string.IsNullOrEmpty(title))
            {
                int maxTitle = Convert.ToInt32(GetOptionValue("Max Title"));
                if (title.Length > (int)maxTitle)
                {
                    title = string.Format("{0}...", title.Substring(0, (int)maxTitle));
                }
                Bot.IRC.Command.SendPrivateMessage(message.Channel, string.Format("[URL] {0} ({1})", HttpUtility.HtmlDecode(HttpUtility.UrlDecode(StripTagsCharArray(title))), url.Host));
            }
        }

        /// <summary>
        /// Remove HTML tags from string using char array.
        /// </summary>
        public static string StripTagsCharArray(string source)
        {
            char[] array = new char[source.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < source.Length; i++)
            {
                char let = source[i];
                if (let == '<')
                {
                    inside = true;
                    continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }

        public static string ToFileSize(long source)
        {
            const int byteConversion = 1024;
            double bytes = Convert.ToDouble(source);

            if (bytes >= Math.Pow(byteConversion, 3)) //GB Range
            {
                return string.Concat(Math.Round(bytes / Math.Pow(byteConversion, 3), 2), " GB");
            }
            else if (bytes >= Math.Pow(byteConversion, 2)) //MB Range
            {
                return string.Concat(Math.Round(bytes / Math.Pow(byteConversion, 2), 2), " MB");
            }
            else if (bytes >= byteConversion) //KB Range
            {
                return string.Concat(Math.Round(bytes / byteConversion, 2), " KB");
            }
            else //Bytes
            {
                return string.Concat(bytes, " Bytes");
            }
        }

        private string GetYoutubeDescription(string ID)
        {
            string description = string.Empty;
            string urlTemplate = "https://www.googleapis.com/youtube/v3/videos?part=snippet,statistics,contentDetails&id={0}&key={1}";
            Uri searchUrl = new Uri(string.Format(urlTemplate, ID, GetOptionValue("API Key")));
            WebClient web = new WebClient();
            web.Encoding = Encoding.UTF8;
            try
            {
                string page = web.DownloadString(searchUrl);

                JObject parsed = (JObject)JsonConvert.DeserializeObject(page);
                var data = parsed["items"].First();

                description = string.Format("\u0002{0}\u000F", data["snippet"]["title"]);

                if (data["contentDetails"]["duration"] == null)
                {
                    return description;
                }

                string length = data["contentDetails"]["duration"].Value<string>();

                Regex lengthRegex = new Regex(@"PT((?<Days>[0-9]+)D)?((?<Hours>[0-9]+)H)?((?<Minutes>[0-9]+)M)?((?<Seconds>[0-9]+)S)?");
                Match lengthMatch = lengthRegex.Match(length);
                double totalTime = 0;

                if (lengthMatch.Groups["Days"].Success)
                    totalTime += 86400.0 * Convert.ToDouble(lengthMatch.Groups["Days"].Value);

                if (lengthMatch.Groups["Hours"].Success)
                    totalTime += 3600.0 * Convert.ToDouble(lengthMatch.Groups["Hours"].Value);

                if (lengthMatch.Groups["Minutes"].Success)
                    totalTime += 60.0 * Convert.ToDouble(lengthMatch.Groups["Minutes"].Value);

                if (lengthMatch.Groups["Seconds"].Success)
                    totalTime += Convert.ToDouble(lengthMatch.Groups["Seconds"].Value);

                TimeSpan duration = TimeSpan.FromSeconds(totalTime);
                description += string.Format(" | Length: \u0002{0}\u000F", duration.ToString("g"));

                if (data["statistics"] != null)
                {
                    JToken stats = data["statistics"];
                    int likes = (stats["likeCount"] != null) ? stats["likeCount"].Value<int>() : 0;
                    string pluralLikes = (likes > 1) ? "s" : string.Empty;
                    int dislikes = (stats["dislikeCount"] != null) ? stats["dislikeCount"].Value<int>() : 0;
                    string pluralDislikes = (dislikes > 1) ? "s" : string.Empty;
                    double percent = 100.0 * ((double)likes / (likes + dislikes));
                    description += string.Format(" | Rating: {0} Like{1}, {2} Dislike{3} (\u0002{4}\u000F%)", likes, pluralLikes, dislikes, pluralDislikes, Math.Round(percent, 1));

                    description += string.Format(" | Views: \u0002{0}\u000F", data["statistics"]["viewCount"].Value<int>());
                }

                DateTime uploadDate = Convert.ToDateTime(data["snippet"]["publishedAt"].Value<string>());

                description += string.Format(" | Uploaded By: \u0002{0}\u000F on \u0002{1}\u000F", data["snippet"]["channelTitle"].Value<string>(), uploadDate.ToString("R"));

                if (data["contentDetails"]["contentRating"] != null)
                {
                    description += " | \u0002NSFW\u000F";
                }
            }
            catch (WebException ex)
            {
                description = string.Empty;
            }

            return description;
        }
    }
}
