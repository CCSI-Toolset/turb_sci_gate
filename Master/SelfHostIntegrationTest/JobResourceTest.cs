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
    public class JobResourceTest
    {
        [TestMethod]
        public void GetJobsTest()
        {
            using (WebClient wc = new WebClient())
            {
                var url = "http://localhost:8000/TurbineLite/job";
                //wc.Headers.Add("Content-Type", "application/json");
                var json = wc.DownloadString(url);
                Console.WriteLine("OUTPUT: " + json);
                JArray a = JArray.Parse(json);
                Assert.AreEqual(0, a.Count);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.Net.WebException), "Expecting HTTP 404 Resource Not Found:  Job Resource Does not exist")]
        public void GetJobByIdTest_HTTP404()
        {
            using (WebClient wc = new WebClient())
            {
                var url = "http://localhost:8000/TurbineLite/job/100000000000000000000";
                //wc.Headers.Add("Content-Type", "application/json");
                var json = wc.DownloadString(url);
            }
        }
    }
}
