using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using PdvUpdater.DTOs;
using PdvUpdater.Services;

namespace PdvUpdater
{
    class Program
    {
        private static AuthService AuthService = new AuthService();

        static async Task Main(string[] args)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            var vaultData = DataVault.GetData();
            string refreshToken = vaultData?.RefreshToken;

            if (string.IsNullOrEmpty(refreshToken))
            {
                await RunSetupMode(AuthService);
                return;
            }

            await RunSilentUpdateMode(AuthService, refreshToken);
        }

        static async Task RunSetupMode(AuthService authService)
        {
            Console.Title = "Setup - Atualizador PDV";
            Console.WriteLine("=========================================");
            Console.WriteLine("   REGISTRO DO CAIXA - SISTEMA PDV       ");
            Console.WriteLine("=========================================\n");

            Console.Write("Digite o nome da empresa: ");
            string name = Console.ReadLine();

            Console.Write("Digite a Senha: ");
            string password = ReadPasswordHidden();

            Console.Write("Nome deste Dispositivo (ex: Caixa 01): ");
            string deviceName = Console.ReadLine();

            Console.WriteLine("\nAutenticando com o servidor...");

            var loginDto = new LoginRequestDto
           {
                name = name,
                password = password,
                deviceName = deviceName,
                deviceId = Guid.NewGuid().ToString(),
            };

            try
            {
                await authService.Login(loginDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[ERRO] Falha ao registrar este caixa: {ex.Message}");
                Console.WriteLine("Pressione qualquer tecla para sair...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("\n[SUCESSO] Caixa registrado e vinculado a esta máquina!");
            Console.WriteLine("O atualizador já pode rodar silenciosamente.");
            Console.WriteLine("Pressione qualquer tecla para sair...");
            Console.ReadKey();
        }

        static async Task RunSilentUpdateMode(AuthService authService, string refreshToken)
        {
            var data = DataVault.GetData();
            var refreshDto = new RefreshTokenRequestDto
            {
                refreshToken = refreshToken,
                deviceId =  data.DeviceId,
            };

            string exeFolder = AppDomain.CurrentDomain.BaseDirectory;
            string pdvPath = Path.Combine(exeFolder, "PdvFX.exe");

            try
            {
                string accessToken = await authService.RefreshToken(refreshDto);

                var fileService = new FileService();

                Boolean shouldUpdate = await fileService.CompareVersion(accessToken);

                if (shouldUpdate)
                {
                    var updaterFlow = new UpdaterFlow();
                 
                    await updaterFlow.DownloadNewVersion(accessToken);
                   
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.WriteLine("Iniciando o PDV...");

                if (File.Exists(pdvPath))
                {
                    Process.Start(pdvPath);
                }

                Environment.Exit(0);
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
    }
}