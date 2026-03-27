using System;
using System.Diagnostics;
using System.Threading.Tasks;
using PdvUpdater.Api;
using PdvUpdater.DTOs;
using PdvUpdater.Services;

namespace PdvUpdater
{
    class Program
    {
        private static AuthService authService = new AuthService();

        static async Task Main(string[] args)
        {
            string refreshToken = TokenVault.GetRefreshToken();

            if (string.IsNullOrEmpty(refreshToken))
            {
                await RunSetupMode(authService);
                return;
            }

            await RunSilentUpdateMode(authService, refreshToken);
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
                deviceId = "12345",
            };

            authService.login(loginDto, deviceName);

            Console.WriteLine("\n[SUCESSO] Caixa registrado e vinculado a esta máquina!");
            Console.WriteLine("O atualizador já pode rodar silenciosamente.");
            Console.WriteLine("Pressione qualquer tecla para sair...");
            Console.ReadKey();
        }

        static async Task RunSilentUpdateMode(AuthService authService, string refreshToken)
        {
            var refreshDto = new RefreshTokenRequestDto
            {
                refreshToken = refreshToken,
                deviceId = "12345",
            };

            string accessToken = await authService.refreshToken(refreshDto);

            var fileService = new FileService();

            Boolean shouldUpdate = await fileService.compareVersion(accessToken);

            if (shouldUpdate) 
            {
                var updaterFlow = new UpdaterFlow();
                await updaterFlow.downloadNewVersion(accessToken);
            }

            //Process.Start(@"PdvFX.exe");
            Environment.Exit(0);
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