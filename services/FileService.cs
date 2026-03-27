using System;
using System.Net; 
using System.Threading.Tasks;
using PdvUpdater.Api;
using System.Diagnostics;

namespace PdvUpdater.Services 
{
    public class FileService
    {
        private readonly ApiClient _apiClient;

        public FileService()
        {
            this._apiClient = new ApiClient();
        }

        public async Task<Boolean>compareVersion(string accessToken)
        {
            var serverVersion = await _apiClient.GetVersionAsync(accessToken);
            var localVersion = FileVersionInfo.GetVersionInfo(@"PdvFX.exe");

            if (new Version(serverVersion.version) > new Version(localVersion.FileVersion)) {
                return true;
            } else {
                return false;
            }
        }
    }
}