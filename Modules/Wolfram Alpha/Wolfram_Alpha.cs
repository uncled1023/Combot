using System;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;

namespace Combot.Modules.Plugins
{
    public class Wolfram_Alpha : Module
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
                case "Wolfram Alpha Search":
                    GetResults(command);
                    break;
            }
        }

        private void GetResults(CommandMessage command)
        {
            string URL = "http://api.wolframalpha.com/v2/query?input=" + HttpUtility.UrlEncode(command.Arguments["Query"]) + "&appid=" + GetOptionValue("API") + "&format=plaintext";
            XmlNodeList xnList = null;
            try
            {
                WebClient web = new WebClient();
                web.Encoding = Encoding.UTF8;
                string results = web.DownloadString(URL);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(results);
                xnList = xmlDoc.SelectNodes("/queryresult/pod");
            }
            catch
            {
                string errorMessage = string.Format("Unable to fetch results for \u0002{0}\u000F.", command.Arguments["Query"]);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, errorMessage);
            }
            if (xnList != null && xnList.Count > 1)
            {
                string queryMessage = string.Format("Result for: {0}", xnList[0]["subpod"]["plaintext"].InnerText);
                string resultMessage = xnList[1]["subpod"]["plaintext"].InnerText;
                int maxResults = Convert.ToInt32(GetOptionValue("Max Response Length"));
                if (resultMessage.Length > (int)maxResults)
                {
                    resultMessage = string.Format("{0}...", resultMessage.Substring(0, (int)maxResults));
                }
                char[] tails = {';', ' '};
                resultMessage = resultMessage
                    .Replace(Environment.NewLine, "; ")
                    .Replace("  |  ", ", ")
                    .Replace(" | ", "; ")
                    .TrimEnd(tails);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, queryMessage);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, resultMessage);
            }
            else
            {
                string errorMessage = string.Format("No results found for \u0002{0}\u000F.", command.Arguments["Query"]);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, errorMessage);
            }
        }
    }
}
