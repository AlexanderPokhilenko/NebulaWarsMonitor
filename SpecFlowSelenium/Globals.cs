using System;

namespace SpecFlowSelenium
{
    public static class Globals
    {
        public const string PasswordForTesting = "test_password";
        public const int Port = 5000;
        public static string GetUrl(string path) =>
            new UriBuilder("http", "localhost", Port, path).ToString();
    }
}
