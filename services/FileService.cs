using PdvUpdater.Api;
using System;
using System.Linq;
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

            if (new Version(serverVersion.version) > new Version("3.0.0.30")) {
                return true;
            } else {
                return false;
            }
        }

        public void EnsurePdvIsReady()
        {
            string directory = AppContext.BaseDirectory;
            string pdvPath = Path.Combine(directory, "PdvFX.exe");

            if (File.Exists(pdvPath)) 
            {
                return;
            }

            SimpleLogger.Log("ALERTA: PdvFX.exe não encontrado. Tentando recuperação via rollback...", "WARN");

            var backupFiles = new DirectoryInfo(directory)
                .GetFiles("PdvFX*.exe")
                .Where(f => f.Name.ToLower() != "pdvfx.exe")
                .OrderByDescending(f => f.LastWriteTime)
                .ToList();

            if (backupFiles.Any())
            {
                var latestBackup = backupFiles.First();
                
                try 
                {
                    File.Copy(latestBackup.FullName, pdvPath, overwrite: true);
                    SimpleLogger.Log($"Recuperação concluída! Versão restaurada: {latestBackup.Name}", "INFO");
                }
                catch (Exception ex)
                {
                    SimpleLogger.Error($"Falha fatal ao tentar restaurar backup: {ex.Message}");
                }
            }
            else
            {
                SimpleLogger.Error("ERRO CRÍTICO: Nenhum executável ou backup encontrado na pasta!");
            }
        }
    }
}