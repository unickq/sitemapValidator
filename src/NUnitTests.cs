using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Xml;
using Allure.Commons;
using NUnit.Allure.Core;
using NUnit.Framework;

namespace Unickq.SiteMapValidator
{
    [AllureNUnit]
    internal class NUnitTests
    {
        private static IEnumerable SiteMapUrls
        {
            get
            {
                var list = new List<string>();
                var errorMessage = "ERROR: ";
                XmlNodeList urlNodeList = null;
                try
                {
                    var wc = new WebClient();
                    var xml = wc.DownloadString(Program.SiteMapUrl);
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(xml);
                    urlNodeList = xmlDoc.GetElementsByTagName("url");
                }
                catch (WebException e)
                {
                    if (e.InnerException != null)
                        errorMessage += $"{Program.SiteMapUrl} - {e.InnerException.Message}";
                    else
                        errorMessage += $"{Program.SiteMapUrl} - {e.Message}";
                    list.Add(errorMessage);
                }
                catch (XmlException)
                {
                    list.Add(errorMessage += $"{Program.SiteMapUrl} - Unable to parse XML");
                }

                if (urlNodeList != null)
                {
                    foreach (XmlNode node in urlNodeList)
                    {
                        var url = node["loc"].InnerText;
                        list.Add(url);
                    }

                    if (list.Count == 0)
                        list.Add($"{errorMessage} This type of SiteMap is not supported. {Program.SiteMapUrl}");
                }

                if (Program.MaxCount != 0 && list.Count > 0)
                    list = list.OrderBy(x => new Random().Next()).Take(Program.MaxCount).ToList();

                foreach (var listItem in list) yield return new TestCaseData(listItem);
            }
        }

        [OneTimeSetUp]
        public void Clear()
        {
            var configJson =
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "config.json");
            if (File.Exists(configJson))
            {
                Environment.SetEnvironmentVariable("ALLURE_CONFIG", configJson);
                AllureLifecycle.Instance.CleanupResultDirectory();
            }
            else
            {
                throw new NotSupportedException("Allure is not configured properly");
            }
        }

        [OneTimeTearDown]
        public void AddParameters()
        {
            var sb = new StringBuilder();
            sb.Append($"Url={Program.SiteMapUrl}\n");
            sb.Append($"IP={GetIpAddress()}\n");
            sb.Append($"Host={Dns.GetHostName()}\n");
            File.WriteAllText(Path.Combine(AllureLifecycle.Instance.ResultsDirectory, "environment.properties"),
                sb.ToString());
        }

        [Test]
        [TestCaseSource(typeof(NUnitTests), nameof(SiteMapUrls))]
        [Parallelizable(ParallelScope.Children)]
        public void CheckStatusCode(string url)
        {
            Console.WriteLine(url);
            if (url.StartsWith("ERROR:")) Assert.Fail(url);
            var urlParts = url.Split('/');

            if (urlParts.Length == 5)
            {
                AllureLifecycle.Instance.UpdateTestCase(p => p.labels.Add(Label.ParentSuite(urlParts[3])));
            }
            else if (urlParts.Length == 6)
            {
                AllureLifecycle.Instance.UpdateTestCase(p => p.labels.Add(Label.ParentSuite(urlParts[3])));
                AllureLifecycle.Instance.UpdateTestCase(p => p.labels.Add(Label.Suite(urlParts[4])));
            }
            else if (urlParts.Length > 6)
            {
                AllureLifecycle.Instance.UpdateTestCase(p => p.labels.Add(Label.ParentSuite(urlParts[3])));
                AllureLifecycle.Instance.UpdateTestCase(p => p.labels.Add(Label.Suite(urlParts[4])));
                AllureLifecycle.Instance.UpdateTestCase(p => p.labels.Add(Label.SubSuite(urlParts[5])));
            }

            var timer = new Stopwatch();
            timer.Start();
            var request = WebRequest.Create(url);
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse) request.GetResponse();
                timer.Stop();
                Console.WriteLine($"{response.StatusCode} - {timer.Elapsed.Milliseconds} ms");
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, $"{(int) response.StatusCode}");
            }
            catch (WebException e)
            {
                if (e.Response is HttpWebResponse failedEResponse)
                    Assert.Fail(failedEResponse.StatusCode.ToString());
                else
                    Assert.Fail(e.Message);
            }
            finally
            {
                response?.Close();
            }
        }

        public string GetIpAddress()
        {
            string ipAddress = null;

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530); //Google public DNS and port
                var endPoint = socket.LocalEndPoint as IPEndPoint;
                ipAddress = endPoint.Address.ToString();
            }

            return ipAddress;
        }
    }
}