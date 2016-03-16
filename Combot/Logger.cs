using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Combot
{
    public static class Logger
    {
        public static void LogToFile(string directory, string fileName, Exception ex, int maxSize)
        {
            LogToFile(directory, fileName, ex.GetExceptionMessage(true), maxSize);
        }

        public static void LogToFile(string directory, string fileName, string message, int maxSize)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string logFile = Path.Combine(directory, fileName + Constants.LOGFILEEXT);
            // Check to see if we need to create a new log
            if (File.Exists(logFile))
            {
                TrimLogFile(directory, fileName, maxSize);
            }
            // Write the log to the main log file
            StreamWriter logWriter = File.AppendText(logFile);
            logWriter.WriteLine(string.Format("[{0}] {1}", DateTime.Now.ToString("G"), message));
            logWriter.Close();
        }

        private static void TrimLogFile(string logDir, string fileName, int maxSize)
        {
            string logFile = Path.Combine(logDir, fileName);
            FileInfo file = new FileInfo(logFile);
            long fileSize = file.Length;
            if (fileSize > maxSize)
            {
                // The file is too large, we need to increment the file names of the log files
                string[] files = Directory.GetFiles(logDir);
                for (int i = files.GetUpperBound(0) - 1; i >= 0; i--)
                {
                    string newFileName = fileName + "_" + (i + 1) + Constants.LOGFILEEXT;
                    string newFile = Path.Combine(logDir, newFileName);
                    File.Move(files[i], newFile);
                }
            }
        }
    }
}
