using System;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace PdvUpdater.Services {
    public class BackupManager
    {
        public void CreateBackupAndCleanOld(string exeType)
        {
            string directory = AppContext.BaseDirectory;
            string exePath;

            switch (exeType) {
                case "PdvFX":
                    exePath = Path.Combine(directory, "PdvFX.exe");
                    break;
                
                case "DotMart":
                    exePath = Path.Combine(directory, "DotMart.exe");
                    break;

                default:
                    throw new Exception($"Tipo de execultável inválido: {exeType}");
            }

            if (!File.Exists(exePath)) return;

            var versionInfo = FileVersionInfo.GetVersionInfo(exePath);
            string fullVersion = versionInfo.FileVersion ?? "0.0.0.0";
            string lastDigits = fullVersion.Split('.').Last();

            string backupFileName = $"{exeType}{lastDigits}.exe";
            string backupFilePath = Path.Combine(directory, backupFileName);

            if (File.Exists(backupFilePath))
            {
                File.Delete(backupFilePath);
            }
            
            File.Move(exePath, backupFilePath);
            SimpleLogger.Log($"Backup criado: {backupFileName}");

            CleanOldBackups(directory, 5, exeType);
        }

        private void CleanOldBackups(string directory, int maxBackups, string exeType)
        {
            FileInfo[] exeFiles;
            string exeName;

            switch (exeType) {
                case "PdvFX":
                    exeFiles = new DirectoryInfo(directory).GetFiles("Pdv*.exe");
                    exeName = "PdvFX.exe";
                    break;

                case "DotMart":
                    exeFiles = new DirectoryInfo(directory).GetFiles("DotMart*.exe");
                    exeName = "DotMart.exe";
                    break;

                default:
                    throw new Exception($"Tipo de execultável inválido: {exeType}");
            }

            var backups = exeFiles
                .Where(f => f.Name.ToLower() != exeName)
                .OrderByDescending(f => f.LastWriteTime)
                .ToList();

            if (backups.Count > maxBackups)
            {
                var filesToDelete = backups.Skip(maxBackups);

                foreach (var file in filesToDelete)
                {
                    try
                    {
                        file.Delete();
                        SimpleLogger.Log($"Lixo limpo. Backup antigo apagado: {file.Name}");
                    }
                    catch (Exception ex)
                    {
                        SimpleLogger.Error($"Erro ao apagar arquivo antigo {file.Name}: {ex.Message}");
                    }
                }
            }
        }
    }
}