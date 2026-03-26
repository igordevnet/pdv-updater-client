using System;
using System.Net; 
using System.Threading.Tasks;

namespace PdvUpdater.Services 
{
    public class FileService
    {
        public FileService()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public async Task<bool> DownloadFileAsync(string downloadUrl, string destinationPath)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    await client.DownloadFileTaskAsync(new Uri(downloadUrl), destinationPath);
                }
                
                return true; 
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao baixar o arquivo: " + ex.Message);
                return false; 
            }
        }
    }
}