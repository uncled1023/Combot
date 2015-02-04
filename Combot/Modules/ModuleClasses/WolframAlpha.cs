using System.Net;
using System.Text;
using System.Xml;

namespace Combot.Modules.ModuleClasses
{
    public class WolframAlpha : Module
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
            string URL = "http://api.wolframalpha.com/v2/query?input=" + System.Web.HttpUtility.UrlEncode(command.Arguments["Query"]) + "&appid=" + GetOptionValue("API") + "&format=plaintext";
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
                switch (command.MessageType)
                {
                    case MessageType.Channel:
                        Bot.IRC.SendPrivateMessage(command.Location, errorMessage);
                        break;
                    case MessageType.Query:
                        Bot.IRC.SendPrivateMessage(command.Nick.Nickname, errorMessage);
                        break;
                    case MessageType.Notice:
                        Bot.IRC.SendNotice(command.Nick.Nickname, errorMessage);
                        break;
                }
            }
            if (xnList.Count > 1)
            {
                string queryMessage = string.Format("Result for: {0}", xnList[0]["subpod"]["plaintext"].InnerText);
                string resultMessage = xnList[1]["subpod"]["plaintext"].InnerText;
                switch (command.MessageType)
                {
                    case MessageType.Channel:
                        Bot.IRC.SendPrivateMessage(command.Location, queryMessage);
                        Bot.IRC.SendPrivateMessage(command.Location, resultMessage);
                        break;
                    case MessageType.Query:
                        Bot.IRC.SendPrivateMessage(command.Nick.Nickname, queryMessage);
                        Bot.IRC.SendPrivateMessage(command.Nick.Nickname, resultMessage);
                        break;
                    case MessageType.Notice:
                        Bot.IRC.SendPrivateMessage(command.Nick.Nickname, queryMessage);
                        Bot.IRC.SendPrivateMessage(command.Nick.Nickname, resultMessage);
                        break;
                }
            }
            else
            {
                string errorMessage = string.Format("No results found for \u0002{0}\u000F.", command.Arguments["Query"]);
                switch (command.MessageType)
                {
                    case MessageType.Channel:
                        Bot.IRC.SendPrivateMessage(command.Location, errorMessage);
                        break;
                    case MessageType.Query:
                        Bot.IRC.SendPrivateMessage(command.Nick.Nickname, errorMessage);
                        break;
                    case MessageType.Notice:
                        Bot.IRC.SendNotice(command.Nick.Nickname, errorMessage);
                        break;
                }
            }
        }
    }
}