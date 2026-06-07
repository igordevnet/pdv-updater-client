using System;
using System.Diagnostics;
using PdvUpdater.Api;
using PdvUpdater.DTOs;
using System.Threading.Tasks;
using System.IO;

namespace PdvUpdater.Services
{
    public class UpdaterFlow
    {

        private readonly ApiClient _apiClient;
        private readonly BackupManager _backupM;
        private readonly string _deviceName;

        public UpdaterFlow()
        {
            this._apiClient = new ApiClient();
            this._backupM = new BackupManager();
            var vaultData = DataVault.GetData();

            if (vaultData == null)
            {
                throw new Exception("Data.dat não encontrado");
            }

            this._deviceName = vaultData.DeviceName;
        }

        public async Task DownloadNewVersion(string accessToken)
        {

            var data = DataVault.GetData();
            string exeType = data.ExeType;
            string exeFolder = AppDomain.CurrentDomain.BaseDirectory;

            string currentPath;
            string tempPath;

            switch (exeType)
            {
                case "PdvFX":
                    currentPath = Path.Combine(exeFolder, "PdvFX.exe");
                    tempPath = Path.Combine(exeFolder, "PdvFX.temp");
                    break;

                case "DotMart":
                    currentPath = Path.Combine(exeFolder, "DotMart.exe");
                    tempPath = Path.Combine(exeFolder, "DotMart.temp");
                    break;

                default:
                    throw new Exception($"Invalid exeType: {exeType}");
            }

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

                _backupM.CreateBackupAndCleanOld(exeType);

                try
                {
                    File.Copy(tempPath, currentPath, true);
                    File.Delete(tempPath);
                }
                catch (Exception ex)
                {
                    SimpleLogger.Error($"Erro ao atualizar: {ex.Message}");
                }

                string installedVersion =
                FileVersionInfo
                    .GetVersionInfo(currentPath)
                    .FileVersion;

                await NotifyDownloadCompleteAsync(accessToken, installedVersion);

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

        public async Task NotifyDownloadCompleteAsync(string accessToken, string version)
        {
            var saveDto = new NotifyDownloadCompleteDto
            {
                accessToken = accessToken,
                deviceName = _deviceName,
                version = version
            };

            await _apiClient.NotifyDownloadCompleteAsync(saveDto);
        }
    }
}