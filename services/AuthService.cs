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

        public async Task login(LoginRequestDto loginDto, string deviceName)
        {
            AuthResponseDto tokens = await _apiClient.LoginAsync(loginDto);
            saveToken(tokens.refresh_token);
            saveDeviceName(deviceName);
        }

        public async Task<string> refreshToken(RefreshTokenRequestDto refreshDto) 
        {
            AuthResponseDto tokens = await _apiClient.RefreshToken(refreshDto);
            saveToken(tokens.refresh_token);
            return tokens.access_token;
        }

        private void saveToken(string refreshToken) 
        {
            TokenVault.SaveRefreshToken(refreshToken);
        }

        private void saveDeviceName(string deviceName)
        {
            DeviceVault.SaveDeviceName(deviceName);
        }
    }
}