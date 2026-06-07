using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace PdvUpdater.Services
{
    public class RestartService
    {
        private readonly string _iniPath;
        private readonly string _exePath;

        public RestartService()
        {
            string baseDir = AppContext.BaseDirectory;

            _iniPath = Directory
            .EnumerateFiles(AppContext.BaseDirectory, "NFCe*.ini")
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();

            if (_iniPath != null)
            {
                SimpleLogger.Log($"Último .ini: {_iniPath}");
            }
            else
            {
                SimpleLogger.Error("Nenhum ini encontrado.");
            }

            _exePath = Path.Combine(AppContext.BaseDirectory, "PdvFX.exe");
        }

        public async Task CheckAndRestartAsync()
        {
            try
            {
                var status = GetPdvStatus(_iniPath);

                SimpleLogger.Log($"Status inicial do PDV = {status}");

                if (status != 2)
                {
                    SimpleLogger.Log("PDV não está livre → Reinicialização adiada");
                    return;
                }

                await Task.Delay(1500);

                var statusAfter = GetPdvStatus(_iniPath);

                SimpleLogger.Log($"Status depois do delay = {statusAfter}");

                if (statusAfter == 2)
                {
                    SimpleLogger.Log("Reiniciando Pdv...");
                    RestartPdv(_exePath);
                }
                else
                {
                    SimpleLogger.Log("PDV está em venda novamente → Reinicialização abortada");
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.Error($"RestartService erro: {ex.Message}");
            }
        }

        private void RestartPdv(string exePath)
        {
            try
            {
                var processName = Path.GetFileNameWithoutExtension(exePath);

                var processes = Process.GetProcessesByName(processName);

                foreach (var p in processes)
                {
                    try
                    {
                        p.Kill();
                        p.WaitForExit(3000);
                    }
                    catch (Exception killEx)
                    {
                        SimpleLogger.Error($"Falha ao finalizar processo: {killEx.Message}");
                    }
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true
                });

                SimpleLogger.Log("PDV reiniciado com sucesso!");
            }
            catch (Exception ex)
            {
                SimpleLogger.Error($"Reinicialização falhou: {ex}");
            }
        }

        private int GetPdvStatus(string iniPath)
        {
            try
            {
                if (!File.Exists(iniPath))
                    return -1;

                var lines = File.ReadAllLines(iniPath);

                var statusLine = lines.FirstOrDefault(l => l.StartsWith("Estado="));
                
                if (statusLine == null)
                    return -1;

                return int.Parse(statusLine.Split('=')[1]);
            }
            catch (Exception ex)
            {
                SimpleLogger.Error($"Erro ao ler o ini: {ex.Message}");
                return -1;
            }
        }
    }
}