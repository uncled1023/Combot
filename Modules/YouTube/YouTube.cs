using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Combot.IRCServices.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Combot.Modules.Plugins
{
    public class YouTube : Module
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
                case "YouTube Search":
                    YoutubeSearch(command);
                    break;
            }
        }

        private void YoutubeSearch(CommandMessage command)
        {
            string urlTemplate = "https://www.googleapis.com/youtube/v3/search?part=snippet&q={0}&key={1}";
            Uri searchUrl = new Uri(string.Format(urlTemplate, command.Arguments["Query"], GetOptionValue("API Key")));
            WebClient web = new WebClient();
            web.Encoding = Encoding.UTF8;
            try
            {
                string page = web.DownloadString(searchUrl);

                JObject parsed = (JObject) JsonConvert.DeserializeObject(page);
                if (parsed["items"].Any())
                {
                    string videoID = parsed["items"].First()["id"].Value<string>("videoId");
                    string vidDescription = GetYoutubeDescription(videoID);
                    string youtubeMessage = string.Format("{0} - {1}", vidDescription, string.Format("http://youtu.be/{0}", videoID));
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, youtubeMessage);
                }
                else
                {
                    string noResults = string.Format("No results found for \u0002{0}\u000F.", command.Arguments["Query"]);
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, noResults);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    int code = (int)((HttpWebResponse)ex.Response).StatusCode;
                    string errorCode = string.Format("Unable to search for \u0002{0}\u000F.  Youtube returned status code \u0002{1}\u000F.", command.Arguments["Query"], code);
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, errorCode);
                }
                else
                {
                    string errorCode = string.Format("Unable to search for \u0002{0}\u000F.", command.Arguments["Query"]);
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, errorCode);
                }
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
