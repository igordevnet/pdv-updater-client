using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace PdvUpdater.Api
{
    public static class ApiEndpoints
    {
        private static readonly string _baseUrl;

        static ApiEndpoints()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfiguration config = builder.Build();

            _baseUrl = config["ApiConfig:BaseUrl"];
        }

        public static string Login => $"{_baseUrl}/auth/local/signin";
        public static string Refresh => $"{_baseUrl}/auth/refresh";

        public static string CheckVersion => $"{_baseUrl}/updates/check";

        public static string Download => $"{_baseUrl}/updates/download";

        public static string Save => $"{_baseUrl}/updates/save";
    }
}