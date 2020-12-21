using System;
using System.Diagnostics;
using System.IO;
using TechTalk.SpecFlow;

namespace SpecFlowSelenium.Hooks
{
    [Binding]
    public static class SettingSteps
    {
        private const string ApplicationName = nameof(NebulaWarsMonitor);
        private static Process _process;

        [AfterTestRun]
        public static void AfterEnd()
        {
            if(!_process.HasExited) _process.Kill();
        }

        // From https://logcorner.com/setup-selenium-web-browser-automation-and-asp-net-core/
        [BeforeTestRun]
        public static void StartServer()
        {
            var applicationPath = GetApplicationPath(ApplicationName);
            _process = new Process
            {
                StartInfo =
                {
                    FileName = "dotnet",
                    Arguments = $@"run --project {applicationPath}\{ApplicationName}.csproj pwd={Globals.PasswordForTesting}"
                }
            };
            _process.Start();
        }

        private static string GetApplicationPath(string applicationName)
        {
            var currentDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var solutionFolder = currentDirectory.Parent.Parent.Parent.Parent.ToString();
            return Path.Combine(solutionFolder, applicationName);
        }
    }
}
