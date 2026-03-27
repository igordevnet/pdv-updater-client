using System;
using System.Threading.Tasks;
using PdvUpdater.DTOs;
using PdvUpdater.Api;

namespace PdvUpdater.Services {
    public class AuthService {
        private ApiClient _apiClient;

        public AuthService() {
            this._apiClient = new ApiClient();
        }

        public async Task Login(LoginRequestDto loginDto, string deviceName)
        {
            AuthResponseDto tokens = await _apiClient.LoginAsync(loginDto);
            SaveToken(tokens.refresh_token);
            SaveDeviceName(deviceName);
        }

        public async Task<string> RefreshToken(RefreshTokenRequestDto refreshDto) 
        {
            AuthResponseDto tokens = await _apiClient.RefreshToken(refreshDto);
            SaveToken(tokens.refresh_token);
            return tokens.access_token;
        }

        private void SaveToken(string refreshToken) 
        {
            TokenVault.SaveRefreshToken(refreshToken);
        }

        private void SaveDeviceName(string deviceName)
        {
            DeviceVault.SaveDeviceName(deviceName);
        }
    }
}