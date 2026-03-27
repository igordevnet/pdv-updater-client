namespace PdvUpdater.Api {
    public static class ApiEndpoints {
        private const string _baseUrl = "http://localhost:3000"; 

        public static string Login => $"{_baseUrl}/auth/local/signin";
        public static string Refresh => $"{_baseUrl}/auth/refresh";

        public static string CheckVersion => $"{_baseUrl}/updates/check";

        public static string Download => $"{_baseUrl}/updates/download";

        public static string Save => $"{_baseUrl}/updates/save";
    } 
}