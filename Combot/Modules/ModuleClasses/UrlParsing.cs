using System;
using System.Net;
using System.Web;
using System.Text.RegularExpressions;
using Combot.IRCServices.Messaging;

namespace Combot.Modules.ModuleClasses
{
    public class UrlParsing : Module
    {
        public override void Initialize()
        {
            Bot.IRC.Message.ChannelMessageReceivedEvent += HandleChannelMessage;
        }

        public void HandleChannelMessage(object sender, ChannelMessage message)
        {
            Regex urlRegex = new Regex("(https?)://([\\w-]+\\.)+[\\w-]+(/[\\w-./?%&=]*)?");

            if (urlRegex.IsMatch(message.Message))
            {
                MatchCollection urlMatches = urlRegex.Matches(message.Message);
                for (int i = 0; i < urlMatches.Count; i++)
                {
                    Match urlMatch = urlMatches[i];
                    Uri url = new Uri(urlMatch.Value);
                    WebRequest webRequest = HttpWebRequest.Create(url);
                    webRequest.Method = "HEAD";
                    using (WebResponse webResponse = webRequest.GetResponse())
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
                                    string source = x.DownloadString(urlMatch.ToString());
                                    string title = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
                                    Bot.IRC.SendPrivateMessage(message.Channel, string.Format("[URL] {0} ({1})", HttpUtility.UrlDecode(StripTagsCharArray(title)), url.Host));
                                }
                                break;
                            case "image":
                                Bot.IRC.SendPrivateMessage(message.Channel, string.Format("[{0}] Size: {1}", webResponse.ContentType, ToFileSize(contentLength)));
                                break;
                            case "video":
                                Bot.IRC.SendPrivateMessage(message.Channel, string.Format("[Video] Type: {0} | Size: {1}", webResponse.ContentType.Split('/')[1], ToFileSize(contentLength)));
                                break;
                            case "application":
                                Bot.IRC.SendPrivateMessage(message.Channel, string.Format("[Application] Type: {0} | Size: {1}", webResponse.ContentType.Split('/')[1], ToFileSize(contentLength)));
                                break;
                            case "audio":
                                Bot.IRC.SendPrivateMessage(message.Channel, string.Format("[Audio] Type: {0} | Size: {1}", webResponse.ContentType.Split('/')[1], ToFileSize(contentLength)));
                                break;
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