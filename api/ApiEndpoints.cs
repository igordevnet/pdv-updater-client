namespace PdvUpdater.Api {
    public static class ApiEndpoints {
        private const string _baseUrl = "http://localhost:3000"; 

        public static string login => $"{_baseUrl}/auth/local/signin";
        public static string refresh => $"{_baseUrl}/auth/refresh";

        public static string checkVersion => $"{_baseUrl}/updates/check";

        public static string download => $"{_baseUrl}/updates/download";

        public static string save => $"{_baseUrl}/updates/save";
    } 
}