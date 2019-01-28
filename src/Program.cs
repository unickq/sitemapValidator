using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Allure.Commons;
using McMaster.Extensions.CommandLineUtils;
using NUnitLite;

namespace Unickq.SiteMapValidator
{
    internal class Program
    {
        public static string SiteMapUrl;
        public static int MaxCount;

        private static readonly CommandLineApplication App = new CommandLineApplication
        {
            Description = "Parses url, generates NUnit tests with Allure reports",
            ThrowOnUnexpectedArgument = false
        };


        public static int Main(string[] args)
        {
            var nUnitParams = new List<string> {"--noh"};
            var nUnitResult = 0;

            var optionUrl = App.Option<string>("-u|--url <URL>", "Sitemap url to test", CommandOptionType.SingleValue)
                .IsRequired();
            var optionNumber = App.Option<int>("-n|--count <N>", "Max urls to test",
                CommandOptionType.SingleValue);
            var optionNUnitWorkers =
                App.Option<int>("-w|--workers", "NUnit workers count", CommandOptionType.SingleValue);
            var optionIsTeamCity =
                App.Option<bool>("--teamcity", "TeamCity NUnit listener", CommandOptionType.NoValue);
            var optionIsAllure =
                App.Option<bool>("--allure", "Execute local Allure instance", CommandOptionType.NoValue);
            App.VersionOption("-v|--version",
                FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion, "AAAAAAA");
            App.HelpOption("-h|--help");


            App.Execute(args);
            if (optionIsTeamCity.ParsedValue) nUnitParams.Add("--teamcity");
            if (optionNUnitWorkers.ParsedValue > 0) nUnitParams.Add($"--workers={optionNUnitWorkers.ParsedValue}");

            if (optionUrl.HasValue())
            {
                SiteMapUrl = optionUrl.ParsedValue;
                MaxCount = optionNumber.ParsedValue;
                nUnitResult = new AutoRun().Execute(nUnitParams.ToArray());
            }

            try
            {
                if (optionIsAllure.HasValue())
                    Process.Start("allure", $"serve {AllureLifecycle.Instance.ResultsDirectory}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to launch Allure - {e.Message}. Try:");
                Console.WriteLine($"allure serve {AllureLifecycle.Instance.ResultsDirectory}");
            }

            return nUnitResult;
        }
    }
}