using PdvUpdater.Api;
using System;
using System.Linq;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace PdvUpdater.Services
{
    public class FileService
    {
        private ApiClient _apiClient;
        private RestartService _restartService;
        private UpdaterFlow _updaterFlow;
        private string _cnpj;
        private string _exePath;
        private string _exeFolder;
        private string _exeName;
        private string _exeType;

        public FileService(string exeType)
        {
            serviceSetup(exeType);
            setCnpj();
        }

        public async Task<bool> CompareVersion(string accessToken, bool isBoot)
        {
            var serverPayload = await _apiClient.GetVersionAsync(accessToken);
            var localVersion = getFileVersion();

            if (!serverPayload.force)
            {
                if (new Version(serverPayload.version) > new Version(localVersion))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (serverPayload.cnpj != _cnpj)
                {
                    return false;
                }
                if (new Version(serverPayload.version) < new Version(localVersion))
                {
                    await rollBackToTheTargetVersion(serverPayload.version, accessToken);

                    if (!isBoot && _exeType == "PdvFX")
                    {
                        await _restartService.CheckAndRestartAsync();
                    }

                    return false;
                }
                if (new Version(serverPayload.version) > new Version(localVersion))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void EnsureExeIsReady()
        {
            if (File.Exists(_exePath))
            {
                return;
            }

            SimpleLogger.Log("ALERTA: Arquivo executável não encontrado. Tentando recuperação via rollback...", "WARN");

            var backupFiles = new DirectoryInfo(_exeFolder)
                .GetFiles($"{_exeName}*.exe")
                .Where(f => f.Name.ToLower() != $"{_exeName.ToLower()}.exe")
                .OrderByDescending(f => f.LastWriteTime)
                .ToList();

            if (backupFiles.Any())
            {
                var latestBackup = backupFiles.First();

                try
                {
                    File.Copy(latestBackup.FullName, _exePath, overwrite: true);
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

        public string getFileVersion()
        {
            return FileVersionInfo.GetVersionInfo(_exePath).FileVersion;
        }


        private void setCnpj()
        {
            try
            {
                var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                IConfiguration config = builder.Build();

                _cnpj = config["CNPJ"];
            }
            catch (Exception ex)
            {
                SimpleLogger.Error($"\n[ERRO] Falha ao ler o cnpj: {ex.Message}");
            }
        }

        private void serviceSetup(string exeType)
        {
            this._apiClient = new ApiClient();
            this._restartService = new RestartService();
            this._exeFolder = AppContext.BaseDirectory;
            this._exeType = exeType;
            this._updaterFlow = new UpdaterFlow();

            switch (exeType)
            {
                case "PdvFX":
                    this._exePath = Path.Combine(_exeFolder, "PdvFX.exe");
                    this._exeName = "PdvFX";
                    break;

                case "DotMart":
                    this._exePath = Path.Combine(_exeFolder, "DotMart.exe");
                    this._exeName = "DotMart";
                    break;

                default:
                    throw new Exception($"\n[ERRO] Tipo de executável inválido: {exeType}");
            }

        }

        private async Task rollBackToTheTargetVersion(string targetVersion, string accessToken)
        {
            var backupFiles = new DirectoryInfo(_exeFolder)
                .GetFiles($"{_exeName}*.exe")
                .Where(f => f.Name.ToLower() != $"{_exeName.ToLower()}.exe")
                .OrderByDescending(f => f.LastWriteTime)
                .ToList();

            if (backupFiles.Any())
            {
                var lastDigits = targetVersion.Split('.').Last();

                var targetFile = backupFiles.FirstOrDefault(f =>
                    f.Name.Contains(lastDigits));

                if (targetFile == null)
                {
                    SimpleLogger.Error("Versão alvo não encontrada.");
                    return;
                }

                try
                {
                    string rollbackTempPath =
                        Path.Combine(_exeFolder, "rollback.temp");

                    File.Copy(targetFile.FullName, rollbackTempPath, true);

                    File.Replace(rollbackTempPath, _exePath, null);

                    File.Delete(rollbackTempPath);

                    SimpleLogger.Log(
                        $"Recuperação concluída! Versão restaurada: {targetFile.Name}",
                        "INFO"
                    );

                    await _updaterFlow.NotifyDownloadCompleteAsync(accessToken, targetVersion);
                }
                catch (Exception ex)
                {
                    SimpleLogger.Error(
                        $"Falha fatal ao tentar restaurar backup: {ex.Message}"
                    );
                }
            }
            else
            {
                SimpleLogger.Error("ERRO CRÍTICO! Backups não encontrados!");
            }
        }
    }
}