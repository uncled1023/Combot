using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Combot.IRCServices.Messaging;

namespace Combot.Modules.Plugins
{
    public class Url_Parsing : Module
    {
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
                    MatchCollection urlMatches = urlRegex.Matches(message.Message);
                    for (int i = 0; i < urlMatches.Count; i++)
                    {
                        Match urlMatch = urlMatches[i];
                        Uri url = new Uri(urlMatch.Value);
                        HttpWebRequest webRequest = (HttpWebRequest) WebRequest.Create(url);
                        webRequest.Method = "HEAD";
                        webRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                        try
                        {
                            using (HttpWebResponse webResponse = (HttpWebResponse) webRequest.GetResponse())
                            {
                                int code = (int) webResponse.StatusCode;
                                if (code == 200)
                                {
                                    string contentType = webResponse.ContentType.Split('/')[0];
                                    long contentLength = webResponse.ContentLength;
                                    switch (contentType)
                                    {
                                        case "text":
                                            Regex ytRegex = new Regex("(((youtube.*(v=|/v/))|(youtu\\.be/))(?<ID>[-_a-zA-Z0-9]+))");
                                            if (!ytRegex.IsMatch(message.Message) || !Bot.Modules.Exists(mod => mod.Name == "YouTube"))
                                            {
                                                WebClient x = new WebClient();
                                                x.Encoding = Encoding.UTF8;
                                                string source = x.DownloadString(urlMatch.ToString());
                                                string title = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
                                                int maxTitle = Convert.ToInt32(GetOptionValue("Max Title"));
                                                if (title.Length > (int)maxTitle)
                                                {
                                                    title = string.Format("{0}...", title.Substring(0, (int)maxTitle));
                                                }
                                                Bot.IRC.Command.SendPrivateMessage(message.Channel, string.Format("[URL] {0} ({1})", HttpUtility.HtmlDecode(HttpUtility.UrlDecode(StripTagsCharArray(title))), url.Host));
                                            }
                                            break;
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
                                int code = (int) ((HttpWebResponse) ex.Response).StatusCode;
                                Bot.IRC.Command.SendPrivateMessage(message.Channel, string.Format("[URL] Response Code: \u0002{0}\u0002 ({1})", code, url.Host));
                            }
                        }
                    }
                }
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
    }
}
