using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using PdvUpdater.Services;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json; 
using PdvUpdater.DTOs;

namespace PdvUpdater.Api
{
    public class ApiClient
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        private async Task HandleResponseErrorsAsync(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                string errorBody = await response.Content.ReadAsStringAsync();
                int statusCode = (int)response.StatusCode;

                throw new Exception($"Erro na API ({statusCode}): {errorBody}");
            } else {
                SimpleLogger.Log($"Api Response: Status={response.StatusCode}, Request={response.RequestMessage?.RequestUri}");
            }
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto loginDto)
        {
            var url = ApiEndpoints.Login;

            string jsonBody = JsonConvert.SerializeObject(loginDto);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            await HandleResponseErrorsAsync(response);

            string responseBody = await response.Content.ReadAsStringAsync();
            
            return JsonConvert.DeserializeObject<AuthResponseDto>(responseBody);
        }

        public async Task<AuthResponseDto> RefreshToken(RefreshTokenRequestDto refreshDto) 
        {
            var url = ApiEndpoints.Refresh;
            
            string jsonBody = JsonConvert.SerializeObject(refreshDto);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            await HandleResponseErrorsAsync(response);

            string responseBody = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<AuthResponseDto>(responseBody);
        }

        public async Task<UpdateCheckDto> GetVersionAsync(string accessToken)
        {
            var url = ApiEndpoints.CheckVersion;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            await HandleResponseErrorsAsync(response);

            string responseBody = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<UpdateCheckDto>(responseBody);
        }

        public async Task DownloadFileAsync(string accessToken, string path)
        {
            var url = ApiEndpoints.Download;

            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                using (HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {

                    await HandleResponseErrorsAsync(response);

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

        public async Task NotifyDownloadCompleteAsync(NotifyDownloadCompleteDto saveDto)
        {
            var url = ApiEndpoints.Save;
            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", saveDto.accessToken);

            var payload = new { deviceName = saveDto.deviceName, version = saveDto.version };

            string jsonBody = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            await HandleResponseErrorsAsync(response);
        }
    }
}