using System;
using System.Threading.Tasks;

namespace PdvUpdater.Services
{
    public class UpdaterFlow
    {
        private readonly FileService _fileService;
        private readonly string _newFileName = "PDV_Atualizado.exe";
        public UpdaterFlow()
        {
            _fileService = new FileService();
        }
        public async Task StartUpdateAsync(string apiUrl)
        {
            Console.WriteLine("Iniciando a busca por atualizações...");

            bool isSuccess = await _fileService.DownloadFileAsync(apiUrl, _newFileName);

            if (isSuccess)
            {
                Console.WriteLine("Download concluído, PDV atualizado.");
                
                Environment.Exit(0); 
            }
            else
            {
                Console.WriteLine("Falha ao atualizar. O PDV continuará na versão atual.");
            }
        }
    }
}