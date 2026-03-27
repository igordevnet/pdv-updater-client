namespace PdvUpdater.Api {
    public static class ApiEndpoints {
        private const string _baseUrl = "http://localhost:3000"; 

        public static string login => $"{_baseUrl}/auth/local/signin";
        public static string refresh => $"{_baseUrl}/auth/refresh";

        public static string checkVersion => $"{_baseUrl}/updates/check";

        public static string download(string deviceName) {
            return $"{_baseUrl}/updates/download?deviceName={deviceName}";
        }
    } 
}