using System;
using PdvUpdater.Api;
using PdvUpdater.DTOs;
using System.Threading.Tasks;
using System.IO;

namespace PdvUpdater.Services {
    public class UpdaterFlow { 

        private readonly ApiClient _apiClient;

        public UpdaterFlow() {
            this._apiClient = new ApiClient();
        }

        public async Task DownloadNewVersion(string accessToken)
        {
            string deviceName = DeviceVault.GetDeviceName();
            string exeFolder = AppDomain.CurrentDomain.BaseDirectory;

            string currentPath = Path.Combine(exeFolder, "PdvFX.exe");
            string oldPath = Path.Combine(exeFolder, "PdvFX.exe.old");

            if (File.Exists(oldPath))
            {
                File.Delete(oldPath);
            }

            if (File.Exists(currentPath))
            {
                File.Move(currentPath, oldPath);
                Console.WriteLine("Arquivo atual renomeado para .old com sucesso.");
            }


            Console.WriteLine("Baixando a nova versão...");

            await _apiClient.DownloadFileAsync(accessToken, currentPath);

            var saveDto = new NotifyDownloadCompleteDto
            {
                accessToken = accessToken,
                deviceName = deviceName,
            };

            await _apiClient.NotifyDownloadCompleteAsync(saveDto);

            Console.WriteLine("Download concluído!");
        }
    }
}