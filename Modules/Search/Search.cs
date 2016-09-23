using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Combot.Modules.Plugins
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
                case "Searx":
                    SearxSearch(command);
                    break;
            }
        }

        private void GoogleSearch(CommandMessage command)
        {
            string urlTemplate = "http://ajax.googleapis.com/ajax/services/search/web?v=1.0&safe=off&q={0}";
            Uri searchUrl = new Uri(string.Format(urlTemplate, command.Arguments["Query"]));
            WebClient web = new WebClient();
            web.Encoding = Encoding.UTF8;
            try
            {
                string page = web.DownloadString(searchUrl);

                JObject parsed = (JObject) JsonConvert.DeserializeObject(page);
                int responseCode = parsed.Value<int>("responseStatus");
                if (responseCode < 300 && responseCode >= 200)
                {
                    if (parsed["responseData"]["results"].Any())
                    {
                        var result = parsed["responseData"]["results"][0];
                        string url = result.Value<string>("unescapedUrl");
                        string title = HttpUtility.HtmlDecode(HttpUtility.UrlDecode(StripTagsCharArray(result.Value<string>("titleNoFormatting"))));
                        string content = HttpUtility.HtmlDecode(HttpUtility.UrlDecode(StripTagsCharArray(result.Value<string>("content"))));
                        string resultMessage = string.Format("[{0}] \u0002{1}\u000F: {2}.", url, title, content);
                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, resultMessage);
                    }
                    else
                    {
                        string noResults = string.Format("No results found for \u0002{0}\u000F.", command.Arguments["Query"]);
                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, noResults);
                    }
                }
                else
                {
                    string errorCode = string.Format("Unable to search for \u0002{0}\u000F.  Google returned status code \u0002{1}\u000F.", command.Arguments["Query"], responseCode);
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, errorCode);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    int code = (int)((HttpWebResponse)ex.Response).StatusCode;
                    string errorCode = string.Format("Unable to search for \u0002{0}\u000F.  Google returned status code \u0002{1}\u000F.", command.Arguments["Query"], code);
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, errorCode);
                }
                else
                {
                    string errorCode = string.Format("Unable to search for \u0002{0}\u000F.", command.Arguments["Query"]);
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, errorCode);
                }
            }
        }

        private void SearxSearch(CommandMessage command)
        {
            JArray hosts = (JArray)GetOptionValue("Hosts");
            JArray engines = (JArray)GetOptionValue("Engines");

            WebException curException = new WebException();
            bool hasError = false;
            bool hasNoResults = false;

            // Step through each host until we get a valid response
            foreach (string host in hosts)
            {
                string urlTemplate = "{0}?q={1}&engines={2}&format=json";
                Uri searchUrl = new Uri(string.Format(urlTemplate, host, HttpUtility.UrlEncode(command.Arguments["Query"]), string.Join(",", engines)));

                WebClient web = new WebClient();
                web.Encoding = Encoding.UTF8;
                web.Credentials = CredentialCache.DefaultCredentials;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                try
                {
                    string page = web.DownloadString(searchUrl);

                    JObject parsed = (JObject)JsonConvert.DeserializeObject(page);
                    if (parsed["results"].Any())
                    {
                        var result = parsed["results"][0];
                        string url = result.Value<string>("url");
                        string title = HttpUtility.HtmlDecode(HttpUtility.UrlDecode(StripTagsCharArray(result.Value<string>("title"))));
                        string content = HttpUtility.HtmlDecode(HttpUtility.UrlDecode(StripTagsCharArray(result.Value<string>("content"))));
                        string resultMessage = string.Format("[{0}] \u0002{1}\u000F: {2}.", url, title, content);
                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, resultMessage);
                        return;
                    }
                    else
                    {
                        hasNoResults = true;
                    }
                }
                catch (WebException ex)
                {
                    hasError = true;
                    curException = ex;
                }
            }
            if (hasError && !hasNoResults)
            {
                if (curException.Response != null)
                {
                    int code = (int)((HttpWebResponse)curException.Response).StatusCode;
                    string errorCode = string.Format("Unable to search for \u0002{0}\u000F.  Returned status code \u0002{1}\u000F.", command.Arguments["Query"], code);
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, errorCode);
                }
                else
                {
                    string errorCode = string.Format("Unable to search for \u0002{0}\u000F.", command.Arguments["Query"]);
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, errorCode);
                }
            }
            else
            {
                string noResults = string.Format("No results found for \u0002{0}\u000F.", command.Arguments["Query"]);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, noResults);
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
