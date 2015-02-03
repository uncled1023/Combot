using System;
using System.Xml;

namespace Combot.Modules.ModuleClasses
{
    public class Weather : Module
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
                case "Weather":
                    getWeather(command);
                    break;
                case "Forecast":
                    getForecast(command);
                    break;
            }
        }

        private void getForecast(CommandMessage command)
        {
            int days = 6;
            if (command.Arguments.ContainsKey("Days"))
            {
                if (!int.TryParse(command.Arguments["Days"], out days))
                {
                    days = 6;
                }
            }
            XmlDocument doc2 = new XmlDocument();

            // Load data  
            doc2.Load("http://api.wunderground.com/auto/wui/geo/WXCurrentObXML/index.xml?query=" + command.Arguments["Location"]);

            // Get forecast with XPath  
            XmlNodeList nodes2 = doc2.SelectNodes("/current_observation");

            string location = "";
            if (nodes2.Count > 0)
            {
                foreach (XmlNode node2 in nodes2)
                {
                    XmlNodeList sub_node2 = doc2.SelectNodes("/current_observation/display_location");
                    foreach (XmlNode xn2 in sub_node2)
                    {
                        location = xn2["full"].InnerText;
                    }
                }
            }

            XmlDocument doc = new XmlDocument();
            // Load data  
            doc.Load("http://api.wunderground.com/auto/wui/geo/ForecastXML/index.xml?query=" + command.Arguments["Location"]);

            // Get forecast with XPath  
            XmlNodeList nodes = doc.SelectNodes("/forecast/simpleforecast");

            string weekday = string.Empty;
            string highf = string.Empty;
            string lowf = string.Empty;
            string highc = string.Empty;
            string lowc = string.Empty;
            string conditions = string.Empty;
            if (location != ", " && !String.IsNullOrEmpty(location))
            {
                if (nodes != null && nodes.Count > 0)
                {
                    string startMsg = string.Format("{0} day forecast for {1}", days, command.Arguments["Location"]);
                    if (command.MessageType == MessageType.Channel || command.MessageType == MessageType.Notice)
                    {
                        Bot.IRC.SendNotice(command.Nick.Nickname, startMsg);
                    }
                    else
                    {
                        Bot.IRC.SendPrivateMessage(command.Nick.Nickname, startMsg);
                    }
                    int index = 0;
                    foreach (XmlNode node in nodes)
                    {
                        foreach (XmlNode sub_node in node)
                        {
                            if (index < days)
                            {
                                weekday = sub_node["date"].SelectSingleNode("weekday").InnerText;
                                highf = sub_node["high"].SelectSingleNode("fahrenheit").InnerText;
                                highc = sub_node["high"].SelectSingleNode("celsius").InnerText;
                                lowf = sub_node["low"].SelectSingleNode("fahrenheit").InnerText;
                                lowc = sub_node["low"].SelectSingleNode("celsius").InnerText;
                                conditions = sub_node["conditions"].InnerText;
                                string forecastMsg = string.Format("{0}: {1} with a high of {2} F ({3} C) and a low of {4} F ({5} C).", weekday, conditions, highf, highc, lowf, lowc);
                                if (command.MessageType == MessageType.Channel || command.MessageType == MessageType.Notice)
                                {
                                    Bot.IRC.SendNotice(command.Nick.Nickname, forecastMsg);
                                }
                                else
                                {
                                    Bot.IRC.SendPrivateMessage(command.Nick.Nickname, forecastMsg);
                                }
                            }
                            index++;
                        }
                    }
                }
                else
                {
                    string noWeather = string.Format("No weather information available for \u0002{0}\u000F", command.Arguments["Location"]);
                    if (command.MessageType == MessageType.Channel || command.MessageType == MessageType.Notice)
                    {
                        Bot.IRC.SendNotice(command.Nick.Nickname, noWeather);
                    }
                    else
                    {
                        Bot.IRC.SendPrivateMessage(command.Nick.Nickname, noWeather);
                    }
                }
            }
            else
            {
                string noWeather = string.Format("No weather information available for \u0002{0}\u000F", command.Arguments["Location"]);
                if (command.MessageType == MessageType.Channel || command.MessageType == MessageType.Notice)
                {
                    Bot.IRC.SendNotice(command.Nick.Nickname, noWeather);
                }
                else
                {
                    Bot.IRC.SendPrivateMessage(command.Nick.Nickname, noWeather);
                }
            }
        }

        private void getWeather(CommandMessage command)
        {
            XmlDocument doc = new XmlDocument();

            // Load data  
            doc.Load("http://api.wunderground.com/auto/wui/geo/WXCurrentObXML/index.xml?query=" + command.Arguments["Location"]);

            // Get forecast with XPath  
            XmlNodeList nodes = doc.SelectNodes("/current_observation");
            string weatherMsg = string.Empty;
            string location = string.Empty;
            string temp = string.Empty;
            string weather = string.Empty;
            string humidity = string.Empty;
            string wind_dir = string.Empty;
            string wind_mph = string.Empty;
            if (nodes != null && nodes.Count > 0)
            {
                foreach (XmlNode node in nodes)
                {
                    XmlNodeList sub_node = doc.SelectNodes("/current_observation/display_location");
                    foreach (XmlNode xn in sub_node)
                    {
                        location = xn["full"].InnerText;
                    }
                    temp = node["temperature_string"].InnerText;
                    weather = node["weather"].InnerText;
                    humidity = node["relative_humidity"].InnerText;
                    wind_dir = node["wind_dir"].InnerText;
                    wind_mph = node["wind_mph"].InnerText;
                }
                if (location != ", ")
                {
                    weatherMsg = string.Format("{0} is currently {1} with a temperature of {2}.  The humidity is {3} with winds blowing {4} at {5} mph", location, weather, temp, humidity, wind_dir, wind_mph);
                }
                else
                {
                    weatherMsg = string.Format("No weather information available for \u0002{0}\u000F", command.Arguments["Location"]);
                }
            }
            else
            {
                weatherMsg = string.Format("No weather information available for \u0002{0}\u000F", command.Arguments["Location"]);
            }
            if (command.MessageType == MessageType.Channel)
            {
                Bot.IRC.SendPrivateMessage(command.Location, weatherMsg);
            }
            else if (command.MessageType == MessageType.Notice)
            {
                Bot.IRC.SendNotice(command.Nick.Nickname, weatherMsg);
            }
            else if (command.MessageType == MessageType.Query)
            {
                Bot.IRC.SendPrivateMessage(command.Nick.Nickname, weatherMsg);
            }
        }
    }
}