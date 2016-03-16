using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Combot
{
    public static class Utility
    {
        public static string GetAssemblyDirectory()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        public static string GetExceptionMessage(this Exception ex, bool recursive = false, bool stackTrace = false)
        {
            string message = ex.Message;
            if (ex.InnerException != null && recursive)
            {
                message += "Inner Exception: " + GetExceptionMessage(ex.InnerException, recursive);
            }
            else if(ex.StackTrace != null && stackTrace)
            {
                message += "Stack Trace: " + ex.StackTrace;
            }
            return message;
        }
    }
}
