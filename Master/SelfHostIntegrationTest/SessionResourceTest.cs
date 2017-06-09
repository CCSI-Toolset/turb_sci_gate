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
    public class SessionResourceTest
    {
        [TestMethod]
        public void PostSessionCreateTest()
        {
            using (WebClient wc = new WebClient())
            {
                var url = "http://localhost:8000/TurbineLite/session";
                //wc.Headers.Add("Content-Type", "application/json");
                var s = wc.UploadString(url, "");
                Console.WriteLine("OUTPUT: " + s);
                Assert.IsTrue(s.StartsWith("\""));
                Assert.IsTrue(s.EndsWith("\""));
                var t = s.Substring(1, s.Length - 2);
                Guid result = Guid.Parse(t);
            }
        }

        [TestMethod]
        public void GetSessionListTest()
        {
            using (WebClient wc = new WebClient())
            {
                var url = "http://localhost:8000/TurbineLite/session";
                //wc.Headers.Add("Content-Type", "application/json");
                var s = wc.DownloadString(url);
                Debug.WriteLine(s);
                JArray array = JArray.Parse(s);
                Assert.IsTrue(array.Count() >= 0);
                Console.WriteLine("OUTPUT: " + s);
            }
        }

        [TestMethod]
        public void GetSessionByIdTest()
        {
            Guid result = Guid.Empty;
            using (WebClient wc = new WebClient())
            {
                var url = "http://localhost:8000/TurbineLite/session";
                //wc.Headers.Add("Content-Type", "application/json");
                var s = wc.UploadString(url, "");
                Console.WriteLine("OUTPUT: " + s);
                Assert.IsTrue(s.StartsWith("\""));
                Assert.IsTrue(s.EndsWith("\""));
                var t = s.Substring(1, s.Length - 2);
                result = Guid.Parse(t);
            }
            Assert.AreNotEqual(result, Guid.Empty);

            using (WebClient wc = new WebClient())
            {
                var url = String.Format("http://localhost:8000/TurbineLite/session/{0}", result);
                //wc.Headers.Add("Content-Type", "application/json");
                var s = wc.DownloadString(url);
                JArray array = JArray.Parse(s);
                Assert.AreEqual(array.Count(), 0);
            }
        }

        /// <summary>
        /// This test appends jobs to a session GUID, creates a new simulation.
        /// </summary>
        /// <returns>simulation name ( GUID ) </returns>
        [TestMethod]
        public string ProducerSessionStateTest()
        {
            Guid result = Guid.Empty;
            var url = "http://localhost:8000/TurbineLite/session";
            using (WebClient wc = new WebClient())
            {
                //wc.Headers.Add("Content-Type", "application/json");
                var s = wc.UploadString(url, "");
                Console.WriteLine("OUTPUT: " + s);
                Assert.IsTrue(s.StartsWith("\""));
                Assert.IsTrue(s.EndsWith("\""));
                var t = s.Substring(1, s.Length - 2);
                result = Guid.Parse(t);
            }
            Assert.AreNotEqual(result, Guid.Empty);
            url = String.Format("http://localhost:8000/TurbineLite/session/{0}", result);
            using (WebClient wc = new WebClient())
            {   
                //wc.Headers.Add("Content-Type", "application/json");
                var s = wc.DownloadString(url);
                JArray array = JArray.Parse(s);
                Assert.AreEqual(array.Count(), 0);
            }

            string simulationName = new SimulationResourceTest().PutSimulationStagedInputTest();
            var jobList = new JobRequestList();
            var inputD = new JObject();
            inputD.Add("test", Guid.NewGuid());
            jobList.Add(new JobRequest
            {
                Initialize = false,
                Reset = false,
                Simulation = simulationName,
                Input = new JObject()
            });

            var json = JsonConvert.SerializeObject(jobList);
            Debug.WriteLine("Job Descriptions: {0}", json);
            
            using (WebClient wc = new WebClient())
            {
                //wc.Headers.Add("Content-Type", "application/json");
                var s = wc.UploadString(url, json);
                Debug.WriteLine("Append Jobs (POST) {0}: {1}", url, s);

                Debug.WriteLine("GET job: " + s);
                var l = JsonConvert.DeserializeObject<List<int>>(s);
                Assert.AreEqual(l.Count(), 1);
                var jobUrl = String.Format("http://localhost:8000/TurbineLite/job/{0}", l[0]);
                Debug.WriteLine("GET job: " + jobUrl);
                s = wc.DownloadString(jobUrl);
                Debug.WriteLine("GET job: " + s);
                var job = JsonConvert.DeserializeObject<JobDescription>(s);
                Assert.IsNotNull(job);
            }

            JobDescriptionList jobDescList = null;

            using (WebClient wc = new WebClient())
            {
                var s = wc.DownloadString(url);
                Debug.WriteLine("session jobs: " + s);
                jobDescList = JsonConvert.DeserializeObject<JobDescriptionList>(s);
                Assert.AreEqual(jobDescList.Count(), 1);
                Debug.WriteLine(String.Format("session jobs: {0}", jobDescList));
            }

            Assert.IsNotNull(jobDescList);
            int count = 0;
            foreach (var job in jobDescList)
            {
                Debug.WriteLine(String.Format("Job: {0}", job.Id));
                if (count == 0)
                    Assert.IsTrue(job.Id > count);
                else
                    Assert.AreEqual(job.Id, count);
                count = job.Id + 1;

                Assert.AreEqual(job.State, "create");
                Assert.IsTrue(job.Create > DateTime.MinValue);
                Assert.IsTrue(job.Create < DateTime.UtcNow);
                Assert.IsNull(job.Submit);
                Assert.IsNull(job.Setup);
                Assert.IsNull(job.Running);
                Assert.IsNull(job.Finished);
            }
            // start -- send to submit
            url = String.Format("http://localhost:8000/TurbineLite/session/{0}/start", result);
            using (WebClient wc = new WebClient())
            {
                //wc.Headers.Add("Content-Type", "application/json");
                var s = wc.UploadString(url, "POST", "");
                //JArray array = JArray.Parse(s);
                //Assert.AreEqual(array.Count(), 1);
                count = int.Parse(s);
                Assert.AreEqual(count, 1);
            }

            jobDescList = null;
            url = String.Format("http://localhost:8000/TurbineLite/session/{0}", result);
            using (WebClient wc = new WebClient())
            {
                var s = wc.DownloadString(url);
                Debug.WriteLine("session jobs: " + s);
                jobDescList = JsonConvert.DeserializeObject<JobDescriptionList>(s);
                Assert.AreEqual(jobDescList.Count(), 1);
                Debug.WriteLine(String.Format("session jobs: {0}", jobDescList));
            }
            count = 0;
            foreach (var job in jobDescList)
            {
                Debug.WriteLine(String.Format("Job: {0}", job.Id));
                if (count == 0)
                    Assert.IsTrue(job.Id > count);
                else
                    Assert.AreEqual(job.Id, count);
                count = job.Id + 1;

                Assert.AreEqual(job.State, "submit");
                Assert.IsTrue(job.Create > DateTime.MinValue);
                Assert.IsTrue(job.Create < DateTime.UtcNow);
                Assert.IsTrue(job.Submit > job.Create);
                Assert.IsNull(job.Setup);
                Assert.IsNull(job.Running);
                Assert.IsNull(job.Finished);
            }

            // start -- send to submit
            url = String.Format("http://localhost:8000/TurbineLite/session/{0}/stop", result);
            using (WebClient wc = new WebClient())
            {
                //wc.Headers.Add("Content-Type", "application/json");
                var s = wc.UploadString(url, "POST", "");
                //JArray array = JArray.Parse(s);
                //Assert.AreEqual(array.Count(), 1);
                count = int.Parse(s);
                Assert.AreEqual(count, 1);
            }
            jobDescList = null;
            url = String.Format("http://localhost:8000/TurbineLite/session/{0}", result);
            using (WebClient wc = new WebClient())
            {
                var s = wc.DownloadString(url);
                Debug.WriteLine("session jobs: " + s);
                jobDescList = JsonConvert.DeserializeObject<JobDescriptionList>(s);
                Assert.AreEqual(jobDescList.Count(), 1);
                Debug.WriteLine(String.Format("session jobs: {0}", jobDescList));
            }
            count = 0;
            foreach (var job in jobDescList)
            {
                Debug.WriteLine(String.Format("Job: {0}", job.Id));
                if (count == 0)
                    Assert.IsTrue(job.Id > count);
                else
                    Assert.AreEqual(job.Id, count);
                count = job.Id + 1;

                Assert.AreEqual("pause", job.State);
                Assert.IsTrue(job.Create > DateTime.MinValue);
                Assert.IsTrue(job.Create < DateTime.UtcNow);
                Assert.IsTrue(job.Submit > job.Create);
                Assert.IsNull(job.Setup);
                Assert.IsNull(job.Running);
                Assert.IsNull(job.Finished);
            }
            return simulationName;
        }
    }
}
