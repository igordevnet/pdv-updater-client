using System;
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

            _backupM.CreateBackupAndCleanOld();

            SimpleLogger.Log("Baixando a nova versão...");

            await _apiClient.DownloadFileAsync(accessToken, tempPath);
            File.Move(tempPath, currentPath);

            var saveDto = new NotifyDownloadCompleteDto
            {
                accessToken = accessToken,
                deviceName = deviceName,
            };

            await _apiClient.NotifyDownloadCompleteAsync(saveDto);

            SimpleLogger.Log("Download concluído!");
        }
    }
}