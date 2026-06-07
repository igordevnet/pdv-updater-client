using System;
using System.IO;

namespace PdvUpdater.Services
{
    public static class SimpleLogger
    {
        private static readonly string logFilePath =
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "updater_logs.txt"
            );

        private static readonly string backupLogPath =
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "updater_logs_old.txt"
            );

        private const long MaxLogSizeBytes = 5 * 1024 * 1024;

        public static void Log(string message, string level = "INFO")
        {
            try
            {
                RotateLogIfNeeded();

                string logEntry =
                    $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] " +
                    $"[{level}] {message}{Environment.NewLine}";

                File.AppendAllText(logFilePath, logEntry);

                Console.WriteLine(logEntry.TrimEnd());
            }
            catch
            {
            }
        }

        public static void Error(string message)
        {
            Log(message, "ERROR");
        }

        private static void RotateLogIfNeeded()
        {
            try
            {
                if (!File.Exists(logFilePath))
                    return;

                FileInfo fileInfo = new FileInfo(logFilePath);

                if (fileInfo.Length < MaxLogSizeBytes)
                    return;

                if (File.Exists(backupLogPath))
                {
                    File.Delete(backupLogPath);
                }

                File.Move(logFilePath, backupLogPath);
            }
            catch
            {
            }
        }
    }
}