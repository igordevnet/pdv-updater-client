using System;
using System.IO;

namespace PdvUpdater.Services 
{
    public static class SimpleLogger
    {
        private static readonly string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "updater_logs.txt");

        public static void Log(string message, string level = "INFO")
        {
            try
            {
                string logEntry = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] [{level}] {message}{Environment.NewLine}";
                
                File.AppendAllText(logFilePath, logEntry);

                Console.WriteLine(logEntry.TrimEnd());
            }
            catch
            {}
        }

        public static void Error(string message)
        {
            Log(message, "ERROR");
        }
    }
}