using PdvUpdater.Api;
using System;
using System.Diagnostics;
using System.IO;
using System.Net; 
using System.Threading.Tasks;

namespace PdvUpdater.Services 
{
    public class FileService
    {
        private readonly ApiClient _apiClient;
        private readonly string _pdvPath;
        private readonly string _exeFolder;

        public FileService()
        {
            this._apiClient = new ApiClient();
            this._exeFolder = AppDomain.CurrentDomain.BaseDirectory;
            this._pdvPath = Path.Combine(_exeFolder, "PdvFX.exe");
        }

        public async Task<Boolean> CompareVersion(string accessToken)
        {
            var serverVersion = await _apiClient.GetVersionAsync(accessToken);
            var localVersion = FileVersionInfo.GetVersionInfo(_pdvPath);

            if (new Version(serverVersion.version) > new Version(localVersion.FileVersion)) {
                return true;
            } else {
                return false;
            }
        }
    }
}