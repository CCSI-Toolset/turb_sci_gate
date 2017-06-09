using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Collections.Generic;
using System.ServiceModel.Web;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Net;
using Newtonsoft.Json;
using System.Linq;
using Turbine.Web.Contracts;
using System.ServiceModel.Description;
using System.ServiceModel;
using System.IO;


namespace SelfHostIntegrationTest
{
    [TestClass]
    public class ApplicationResourceTest
    {
        [TestMethod]
        public void GetApplicationListTest()
        {
            using (WebClient wc = new WebClient())
            {
                var json = wc.DownloadString("http://localhost:8000/TurbineLite/application");
                Console.WriteLine("RESPONSE: {0}", json);
                var appList = JsonConvert.DeserializeObject<ApplicationList>(json);
                Assert.IsTrue(appList.Count == 3);
                foreach (var app in appList)
                {
                    Console.WriteLine("    {0}", app.Name);
                }
            }
        }
        [TestMethod]
        public void GetApplicationACMTest()
        {
            using (WebClient wc = new WebClient())
            {
                var json = wc.DownloadString("http://localhost:8000/TurbineLite/application/acm");
                Console.WriteLine("RESPONSE: {0}", json);
                var app = JsonConvert.DeserializeObject<Application>(json);
                Assert.AreEqual<string>(app.Name, "ACM");
            }
        }
        [TestMethod]
        public void GetApplicationAspenPlusTest()
        {
            using (WebClient wc = new WebClient())
            {
                var json = wc.DownloadString("http://localhost:8000/TurbineLite/application/aspenplus");
                Console.WriteLine("RESPONSE: {0}", json);
                var app = JsonConvert.DeserializeObject<Application>(json);
                Assert.AreEqual<string>(app.Name, "AspenPlus");
            }
        }
        [TestMethod]
        public void GetApplicationExcelTest()
        {
            using (WebClient wc = new WebClient())
            {
                var json = wc.DownloadString("http://localhost:8000/TurbineLite/application/excel");
                Console.WriteLine("RESPONSE: {0}", json);
                var app = JsonConvert.DeserializeObject<Application>(json);
                Assert.AreEqual<string>(app.Name, "Excel");
            }
        }
    }
}
