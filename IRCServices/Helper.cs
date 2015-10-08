using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Combot.IRCServices
{
    public static class Helper
    {
        public static string ModeToString(this UserModeInfo mode)
        {
            return string.Format("{0}{1}", (mode.Set) ? "+" : "-", mode.Mode.ToString());
        }

        public static string ModesToString(this List<UserModeInfo> modes)
        {
            string modeStr = string.Empty;
            bool curSet = true;
            foreach (UserModeInfo mode in modes)
            {
                bool addSet = false;
                if (curSet != mode.Set)
                    addSet = true;
                modeStr += ((addSet) ? ((curSet) ? "+" : "-") : string.Empty) + mode.Mode.ToString();
                curSet = mode.Set;
            }
            return modeStr;
        }

        public static string ModeToString(this ChannelModeInfo mode)
        {
            string param = string.Empty;
            if (!string.IsNullOrEmpty(mode.Parameter))
                param = " " + mode.Parameter;
            return string.Format("{0}{1}{2}", (mode.Set) ? "+" : "-", mode.Mode.ToString(), param);
        }

        public static string ModesToString(this List<ChannelModeInfo> modes)
        {
            string modeStr = string.Empty;
            bool curSet = true;
            bool addSet = true;
            foreach (ChannelModeInfo mode in modes)
            {
                modeStr += ((addSet) ? ((mode.Set) ? "+" : "-") : string.Empty) + mode.Mode.ToString();
                addSet = (curSet == mode.Set);
                curSet = mode.Set;
            }
            string param = string.Empty;
            foreach (ChannelModeInfo mode in modes)
            {
                if (!string.IsNullOrEmpty(mode.Parameter))
                    param += " " + mode.Parameter;
            }

            modeStr += param;

            return modeStr;
        }
    }
}
