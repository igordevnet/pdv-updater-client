using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PdvUpdater.DTOs;
using PdvUpdater.Services;
using PdvUpdater.model;

namespace PdvUpdater
{
    class Program
    {
        private static AuthService AuthService = new AuthService();
        private static Mutex _appMutex;
        private static bool _isUpdating = false;
        private static FileService _fileService;
        private static RestartService _restartService;
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern bool AllocConsole();

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern bool FreeConsole();

        static async Task Main(string[] args)
        {
            bool createdNew;

            _appMutex = new Mutex(true, "Global\\PdvUpdater", out createdNew);

            if (!createdNew)
            {
                return;
            }

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            var vaultData = DataVault.GetData();
            string refreshToken = vaultData?.RefreshToken;

            if (string.IsNullOrEmpty(refreshToken))
            {   
                AllocConsole();

                Console.Title = "Seletor de Sistema";
                string[] appOptions = { "PdvFX", "DotMart" };
                string selectedApp = ShowInteractiveMenu("Selecione o Sistema", appOptions);

                await RunSetupMode(AuthService, selectedApp);

                FreeConsole();
                return;
            }

            _fileService = new FileService(vaultData.ExeType);
            _restartService = new RestartService();

            await RunSilentUpdateMode(AuthService, vaultData);
        }

        static async Task RunSetupMode(AuthService authService, string selectedApp)
        {
            Console.Title = "Setup - Atualizador PDV";
            Console.WriteLine("=========================================");
            Console.WriteLine($"   REGISTRO DO CAIXA - SISTEMA {selectedApp.ToUpper()}       ");
            Console.WriteLine("=========================================\n");

            Console.Write("Digite o nome da empresa: ");
            string name = Console.ReadLine();

            Console.Write("Digite a Senha: ");
            string password = ReadPasswordHidden();

            Console.Write($"Nome deste Dispositivo (ex: {selectedApp} 01): ");
            string deviceName = Console.ReadLine();

            Console.WriteLine("\nAutenticando com o servidor...");

            var loginDto = new LoginRequestDto
           {
                name = name,
                password = password,
                deviceName = deviceName,
                deviceId = Guid.NewGuid().ToString(),
                exeType = selectedApp
            };

            try
            {
                await authService.Login(loginDto);
            }
            catch (Exception ex)
            {
                SimpleLogger.Error($"\n[ERRO] Falha ao registrar este dispositivo: {ex.Message}");
                SimpleLogger.Error("Pressione qualquer tecla para sair...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("\n[SUCESSO] Caixa registrado e vinculado a esta máquina!");
            Console.WriteLine("O atualizador já pode rodar silenciosamente.");
            Console.WriteLine("Pressione qualquer tecla para sair...");
            Console.ReadKey();
        }

        static async Task RunSilentUpdateMode(AuthService authService, VaultData data)
        {
            var exeType = data.ExeType;
            var refreshDto = new RefreshTokenRequestDto
            {
                refreshToken = data.RefreshToken,
                deviceId =  data.DeviceId,
                exeType = exeType
            };

            string exeFolder = AppDomain.CurrentDomain.BaseDirectory;
            string exePath = null;

            switch (exeType) {
                case "PdvFX":
                    exePath = Path.Combine(exeFolder, "PdvFX.exe");
                    break;
                
                case "DotMart":
                    exePath = Path.Combine(exeFolder, "DotMart.exe");
                    break;

                default:
                    throw new Exception($"Invalid exeType: {exeType}");
            }

            try
            {
                await updateExe(authService, refreshDto, exeType, true);
            }
            catch (Exception ex)
            {
                SimpleLogger.Error(ex.ToString());
            }
            finally
            {
                SimpleLogger.Log($"Iniciando o {exeType.ToUpper()}...");

                _fileService.EnsureExeIsReady();

                var processName = Path.GetFileNameWithoutExtension(exePath);

                var alreadyRunning = Process
                    .GetProcessesByName(processName)
                    .Any();

                if (!alreadyRunning)
                {
                    if (File.Exists(exePath))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = exePath,
                            UseShellExecute = true,
                            Verb = "runas"
                        });
                    }
                }

                await RunBackgroundUpdater(authService);
            }
        }

        static string ReadPasswordHidden()
        {
            string pass = "";
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    pass = pass.Substring(0, (pass.Length - 1));
                    Console.Write("\b \b");
                }
            } while (key.Key != ConsoleKey.Enter);
            
            Console.WriteLine();
            return pass;
        }
    

        private static string ShowInteractiveMenu(string title, string[] options)
        {
            int selectedIndex = 0;
            ConsoleKey key;

            Console.CursorVisible = false;

            do
            {
                Console.Clear();
                Console.WriteLine($"=========================================");
                Console.WriteLine($" {title.ToUpper()} ");
                Console.WriteLine($"=========================================\n");
                Console.WriteLine("Use as setas para CIMA/BAIXO e pressione ENTER para selecionar:\n");

                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.WriteLine($" > {options[i]} ");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"   {options[i]} ");
                    }
                }

                key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow)
                {
                    selectedIndex--;
                    if (selectedIndex < 0) selectedIndex = options.Length - 1;
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    selectedIndex++;
                    if (selectedIndex >= options.Length) selectedIndex = 0;
                }

            } while (key != ConsoleKey.Enter);

            Console.CursorVisible = true;
            Console.Clear();

            return options[selectedIndex];
        }
    
        private static async Task updateExe(
            AuthService authService, 
            RefreshTokenRequestDto refreshDto, 
            string exeType,
            bool isBoot
        ) {
            if (_isUpdating) {
                return;
            }

            SimpleLogger.Log("Iniciando atualização checkup...");

            _isUpdating = true;

            try
            {
                var updaterFlow = new UpdaterFlow();

                string accessToken = await authService.RefreshToken(refreshDto);

                Boolean shouldUpdate = await _fileService.CompareVersion(accessToken, isBoot);

                if (shouldUpdate)
                {                 
                    await updaterFlow.DownloadNewVersion(accessToken);

                    if (exeType == "PdvFX" && !isBoot) {
                        await _restartService.CheckAndRestartAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.Error(ex.ToString());
            }
            finally {
                _isUpdating = false;
            }
        }

        private static async Task RunBackgroundUpdater(
            AuthService authService
        )
        {
            
            await Task.Delay(TimeSpan.FromMinutes(10));

            
            SimpleLogger.Log("Iniciando Polling...");

            while (true)
            {
                try
                {
                    var data = DataVault.GetData();

                    if (data == null)
                    {
                        SimpleLogger.Error("Data.dat não encontrado.");
                        return;
                    }

                    var exeType = data.ExeType;
                    var refreshDto = new RefreshTokenRequestDto
                    {
                        refreshToken = data.RefreshToken,
                        deviceId =  data.DeviceId,
                        exeType = exeType
                    };

                    await updateExe(authService, refreshDto, exeType, false);
                }
                catch (Exception ex)
                {
                    SimpleLogger.Error(ex.ToString());
                }

                await Task.Delay(TimeSpan.FromMinutes(10));
            }
        }
    }
}