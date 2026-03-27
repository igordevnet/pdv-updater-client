using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json; 
using PdvUpdater.DTOs;

namespace PdvUpdater.Api
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;

        public ApiClient() {
            this._httpClient = new HttpClient();
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto loginDto)
        {
            var url = ApiEndpoints.login;

            string jsonBody = JsonConvert.SerializeObject(loginDto);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            
            return JsonConvert.DeserializeObject<AuthResponseDto>(responseBody);
        }

        public async Task<AuthResponseDto> RefreshToken(RefreshTokenRequestDto refreshDto) 
        {
            var url = ApiEndpoints.refresh;
            
            string jsonBody = JsonConvert.SerializeObject(refreshDto);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<AuthResponseDto>(responseBody);
        }

        public async Task<UpdateCheckDto> GetVersionAsync(string accessToken)
        {
            var url = ApiEndpoints.checkVersion;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<UpdateCheckDto>(responseBody);
        }

        public async Task DownloadFileAsync(DownloadFileRequestDto downloadDto, string path)
        {
            var url = ApiEndpoints.download(downloadDto.deviceName);

            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", downloadDto.accessToken);

                using (HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {

                    response.EnsureSuccessStatusCode();

                    using (var fileStream = await response.Content.ReadAsStreamAsync())
                    {
                        
                        using (var wStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await fileStream.CopyToAsync(wStream);
                        }
                    }
                }
            }
        }
    }
}