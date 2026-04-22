using System;
using System.Diagnostics;
using PdvUpdater.Api;
using PdvUpdater.DTOs;
using System.Threading.Tasks;
using System.IO;

namespace PdvUpdater.Services {
    public class UpdaterFlow { 

        private readonly ApiClient _apiClient;
        private readonly BackupManager _backupM;

        public UpdaterFlow() {
            this._apiClient = new ApiClient();
            this._backupM = new BackupManager();
        }

        public async Task DownloadNewVersion(string accessToken)
        {

            var data = DataVault.GetData();
                
            string deviceName = data.DeviceName;
            string exeFolder = AppDomain.CurrentDomain.BaseDirectory;

            string currentPath = Path.Combine(exeFolder, "PdvFX.exe");

            string tempPath = Path.Combine(exeFolder, "PdvFX.temp");

            try
            {
                SimpleLogger.Log("Baixando a nova versão...");
                await _apiClient.DownloadFileAsync(accessToken, tempPath);

                if (!File.Exists(tempPath) || new FileInfo(tempPath).Length == 0)
                {
                    throw new Exception("O arquivo instalado não é válido");
                }

                var versionInfo = FileVersionInfo.GetVersionInfo(tempPath);
                if (string.IsNullOrEmpty(versionInfo.FileVersion))
                {
                    throw new Exception("O arquivo não tem informações de versão");
                }

                _backupM.CreateBackupAndCleanOld();

                File.Move(tempPath, currentPath);

                var saveDto = new NotifyDownloadCompleteDto
                {
                    accessToken = accessToken,
                    deviceName = deviceName,
                };

                await _apiClient.NotifyDownloadCompleteAsync(saveDto);

                SimpleLogger.Log("Download concluído!");
            }
            catch (Exception ex)
            {
                SimpleLogger.Error($"Erro ao atualizar: {ex.Message}");

                if (File.Exists(tempPath)) 
                {
                    File.Delete(tempPath);            
                }
            }

        }
    }
}