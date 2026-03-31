using System;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace PdvUpdater.Services {
    public class BackupManager
    {
        public void CreateBackupAndCleanOld()
        {
            string directory = AppContext.BaseDirectory;
            string pdvPath = Path.Combine(directory, "PdvFX.exe");

            if (!File.Exists(pdvPath)) return;

            var versionInfo = FileVersionInfo.GetVersionInfo(pdvPath);
            string fullVersion = versionInfo.FileVersion ?? "0.0.0.0";
            string lastDigits = fullVersion.Split('.').Last();

            string backupFileName = $"PdvFX{lastDigits}.exe";
            string backupFilePath = Path.Combine(directory, backupFileName);

            if (File.Exists(backupFilePath))
            {
                File.Delete(backupFilePath);
            }
            
            File.Move(pdvPath, backupFilePath);
            SimpleLogger.Log($"Backup criado: {backupFileName}");

            CleanOldBackups(directory, 5);
        }

        private void CleanOldBackups(string directory, int maxBackups)
        {
            var pdvFiles = new DirectoryInfo(directory).GetFiles("PdvFX*.exe");

            var backups = pdvFiles
                .Where(f => f.Name.ToLower() != "pdvfx.exe")
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