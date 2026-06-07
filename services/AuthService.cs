using Newtonsoft.Json;
using PdvUpdater.model;
using PdvUpdater.Api;
using PdvUpdater.DTOs;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PdvUpdater.Services {
    public class AuthService {
        private ApiClient _apiClient;

        public AuthService() {
            this._apiClient = new ApiClient();
        }

        public async Task Login(LoginRequestDto loginDto)
        {
            AuthResponseDto tokens = await _apiClient.LoginAsync(loginDto);
             
            var data = new VaultData {
                RefreshToken = tokens.refresh_token,
                DeviceId = loginDto.deviceId,
                DeviceName = loginDto.deviceName,
                ExeType = loginDto.exeType
            };

            DataVault.SaveData(data);
        }

        public async Task<string> RefreshToken(RefreshTokenRequestDto refreshDto) 
        {
            AuthResponseDto tokens = await _apiClient.RefreshToken(refreshDto);
            DataVault.UpdateRefreshToken(tokens.refresh_token);
            return tokens.access_token;
        }

    }
}