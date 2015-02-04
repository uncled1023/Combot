using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Combot.Modules.ModuleClasses
{
    public class Search : Module
    {
        public override void Initialize()
        {
            Bot.CommandReceivedEvent += HandleCommandEvent;
        }

        public override void ParseCommand(CommandMessage command)
        {
            Command foundCommand = Commands.Find(c => c.Triggers.Contains(command.Command));

            switch (foundCommand.Name)
            {
                case "Google":
                    GoogleSearch(command);
                    break;
                case "Bing":
                    break;
            }
        }

        private void GoogleSearch(CommandMessage command)
        {
            string urlTemplate = "http://ajax.googleapis.com/ajax/services/search/web?v=1.0&safe=off&q={0}";
            Uri searchUrl = new Uri(string.Format(urlTemplate, command.Arguments["Query"]));
            WebClient web = new WebClient();
            web.Encoding = Encoding.UTF8;
            string page = web.DownloadString(searchUrl);

            JObject parsed = (JObject) JsonConvert.DeserializeObject(page);
            int responseCode = parsed.Value<int>("responseStatus");
            if (responseCode < 300 && responseCode >= 200)
            {
                if (parsed["responseData"]["results"].Any())
                {
                    var result = parsed["responseData"]["results"][0];
                    string url = result.Value<string>("unescapedUrl");
                    string title = HttpUtility.UrlDecode(StripTagsCharArray(result.Value<string>("titleNoFormatting")));
                    string content = HttpUtility.UrlDecode(StripTagsCharArray(result.Value<string>("content")));
                    string resultMessage = string.Format("[{0}] \u0002{1}\u000F: {2}.", url, title, content);
                    switch (command.MessageType)
                    {
                        case MessageType.Channel:
                            Bot.IRC.SendPrivateMessage(command.Location, resultMessage);
                            break;
                        case MessageType.Query:
                            Bot.IRC.SendPrivateMessage(command.Nick.Nickname, resultMessage);
                            break;
                        case MessageType.Notice:
                            Bot.IRC.SendNotice(command.Nick.Nickname, resultMessage);
                            break;
                    }
                }
                else
                {
                    string noResults = string.Format("No results found for \u0002{0}\u000F.", command.Arguments["Query"]);
                    switch (command.MessageType)
                    {
                        case MessageType.Channel:
                            Bot.IRC.SendPrivateMessage(command.Location, noResults);
                            break;
                        case MessageType.Query:
                            Bot.IRC.SendPrivateMessage(command.Nick.Nickname, noResults);
                            break;
                        case MessageType.Notice:
                            Bot.IRC.SendNotice(command.Nick.Nickname, noResults);
                            break;
                    }
                }
            }
            else
            {
                string errorCode = string.Format("Unable to search for \u0002{0}\u000F.  Google returned status code \u0002{1}\u000F.", command.Arguments["Query"], responseCode);
                switch (command.MessageType)
                {
                    case MessageType.Channel:
                        Bot.IRC.SendPrivateMessage(command.Location, errorCode);
                        break;
                    case MessageType.Query:
                        Bot.IRC.SendPrivateMessage(command.Nick.Nickname, errorCode);
                        break;
                    case MessageType.Notice:
                        Bot.IRC.SendNotice(command.Nick.Nickname, errorCode);
                        break;
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
    }
}