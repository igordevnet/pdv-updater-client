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

        public async Task downloadNewVersion(string accessToken) {
            string deviceName = DeviceVault.GetDeviceName();

            var downloadDto = new DownloadFileRequestDto 
            {
                accessToken = accessToken,
                deviceName = deviceName,
            };

            await _apiClient.DownloadFileAsync(downloadDto, @"PdvFX.exe");
        }
    }
}