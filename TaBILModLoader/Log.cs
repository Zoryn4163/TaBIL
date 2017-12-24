using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaBILModLoader
{
    public static class Log
    {
        public static string LogPath { get; private set; }
        public static StreamWriter LogStream { get; private set; }

        public static void Initialize(string logFilePath)
        {
            LogPath = logFilePath;
            LogStream = new StreamWriter(LogPath, false, Encoding.UTF8);
            LogStream.AutoFlush = true;
        }

        public static void Out(object o)
        {
            LogStream.WriteLine($"[{DateTime.Now}] {o.ToString()}");
        }
    }
}
