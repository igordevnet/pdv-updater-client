using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PdvUpdater
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string refreshToken = TokenVault.GetRefreshToken();

            if (string.IsNullOrEmpty(refreshToken))
            {
                RunSetupMode();
                return; // Encerra após o setup. O técnico abre de novo ou o PC é reiniciado.
            }

            // 3. MODO SILENCIOSO (Dia a dia da operadora de caixa)
            await RunSilentUpdateMode(refreshToken);
        }

        static void RunSetupMode()
        {
            Console.Title = "Setup - Atualizador PDV";
            Console.WriteLine("=========================================");
            Console.WriteLine("   REGISTRO DO CAIXA - SISTEMA PDV       ");
            Console.WriteLine("=========================================\n");

            Console.Write("Digite o nome da empresa: ");
            string name = Console.ReadLine();

            Console.Write("Digite a Senha: ");
            string password = ReadPasswordHidden(); // Esconde a senha

            Console.Write("Nome deste Dispositivo (ex: Caixa 01): ");
            string deviceName = Console.ReadLine();

            Console.WriteLine("\nAutenticando com o servidor...");

            // AQUI VOCÊ VAI CHAMAR O SEU NESTJS (Rota de Login)
            // Exemplo fictício:
            // var token = await ApiClient.LoginAsync(email, password, deviceName);
            
            // Simulando o sucesso:
            string tokenRecebidoDaApi = "eyJh...token_gigante_aqui"; 
            TokenVault.SaveRefreshToken(tokenRecebidoDaApi);

            Console.WriteLine("\n[SUCESSO] Caixa registrado e vinculado a esta máquina!");
            Console.WriteLine("O atualizador já pode rodar silenciosamente.");
            Console.WriteLine("Pressione qualquer tecla para sair...");
            Console.ReadKey();
        }

        static async Task RunSilentUpdateMode(string refreshToken)
        {
            // O ideal é não dar nenhum Console.WriteLine aqui para o CMD fechar na velocidade da luz.
            
            // 1. Bate no NestJS com o Refresh Token para pegar o Access Token
            // 2. Chama a rota GET /updates/check
            // 3. Se precisar, baixa o arquivo e faz a troca (File.Move)
            
            // 4. Inicia o PDV e mata o atualizador
            Process.Start("pdv.exe");
            Environment.Exit(0);
        }

        // Função utilitária clássica do C# para esconder a senha no terminal
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