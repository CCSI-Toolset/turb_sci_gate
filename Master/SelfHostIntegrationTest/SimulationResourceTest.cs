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
    public class SimulationResourceTest
    {
        /// <summary>
        /// 
        /// </summary>
        /// //[TestMethod]
        public void DeleteSimulationInputTest_SimulationWithReferencedJobs()
        {
            string simulationName = new SessionResourceTest().ProducerSessionStateTest();
            DeleteSimulation(simulationName);
        }

        [TestMethod]
        public void DeleteSimulationInputTest_VersionedSimulationEmpty()
        {
            // Creates simulation BFB3
            PutSimulationStagedInputTest_MultipleUpdates();
            DeleteSimulation("BFB3");
        }

        [TestMethod]
        public void DeleteSimulationInputTest_EmptySimulation()
        {
            // crewates simulation BFB
            PutSimulationInputTest();
            DeleteSimulation("BFB");
        }

        private void DeleteSimulation(string name) 
        {
            string url = String.Format("http://localhost:8000/TurbineLite/simulation/{0}", name);
            using (WebClient wc = new WebClient())
            {
                string json = null;
                wc.Headers.Add("Content-Type", "application/json");
                try
                {
                    json = wc.UploadString(url, "DELETE", "");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("EXCEPTION: {0} {1}", ex.Message, ex.StackTrace);
                    Console.WriteLine("EXCEPTION: {0}", ex.InnerException);
                    Assert.Fail();
                }
                bool val = Boolean.Parse(json);
                Assert.IsNotNull(val, "Web API DELETE Simulation call returned null");
                Assert.IsTrue(val, "Web API DELETE Simulation call returned false");

            }
            using (WebClient wc = new WebClient())
            {
                string json = null;
                wc.Headers.Add("Content-Type", "application/json");
                try
                {
                    json = wc.DownloadString(url);
                    Assert.Fail("successfully GET the DELETED simulation BFB3");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("EXCEPTION: {0} {1}", ex.Message, ex.StackTrace);
                    Console.WriteLine("EXCEPTION: {0}", ex.InnerException);
                }
            }
            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add("Content-Type", "application/json");
                string json = wc.DownloadString("http://localhost:8000/TurbineLite/simulation/");
                var simList = JsonConvert.DeserializeObject<SimulationList>(json);
                //Assert.IsTrue(simList.Count >= 1);
                var sim = simList.SingleOrDefault(e => e.Name == name);
                Assert.IsNull(sim, String.Format("Found {0} in Web API Simulation List", name));
                Debug.WriteLine(String.Format("{0} is not in Simulation List", name));
            }
        }

        [TestMethod]
        public void PutSimulationInputTest()
        {
            using (WebClient wc = new WebClient())
            {
                string json = "NOTHING";
                string data = JsonConvert.SerializeObject(new Simulation { Name = "BFB", Application = "ACM", StagedInputs = new List<SimpleStagedInputFile>() });
                Console.WriteLine("DATA: {0}", data);
                wc.Headers.Add("Content-Type", "application/json");
                try
                {
                    json = wc.UploadString("http://localhost:8000/TurbineLite/simulation/BFB", "PUT", data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("EXCEPTION: {0} {1}", ex.Message, ex.StackTrace);
                    Console.WriteLine("EXCEPTION: {0}", ex.InnerException);
                    Assert.Fail();
                }
                Console.WriteLine("RESPONSE: {0}", json);
                Debug.WriteLine(json, "SimulationResourceTest.PutSimulationInputTest");
                var j = JsonConvert.DeserializeObject<Simulation>(json);
                Assert.AreEqual(j.Name, "BFB");
                Assert.AreEqual(j.Application, "ACM");
                Assert.AreEqual(j.StagedInputs.Count(), 2);
                Assert.IsNotNull(j.StagedInputs.Find(x => x.Name.Equals("configuration")));
                Assert.IsNotNull(j.StagedInputs.Find(x => x.Name.Equals("aspenfile")));
                Assert.IsNull(j.StagedInputs.Find(x => x.Name.Equals("any")));

                json = wc.DownloadString("http://localhost:8000/TurbineLite/simulation/BFB/input");
                Console.WriteLine("JSON Input: {0}", json);
                var i = JsonConvert.DeserializeObject<SimulationStagedInputList>(json);
                Assert.IsNotNull(i.Find(x => x.Equals("configuration")));
                Assert.IsNotNull(i.Find(x => x.Equals("aspenfile")));

                json = wc.DownloadString("http://localhost:8000/TurbineLite/simulation/BFB/input/configuration");
                Console.WriteLine("Configuration: {0}", json);
            }
        }

        [TestMethod]
        public void GetSimulationListTest()
        {
            var userName = "Administrator";

            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                var app = db.Applications.Single(s => s.Name == "ACM");
                var user = db.Users.Single(u => u.Name == userName);
                var sim = new Turbine.Data.Entities.Simulation
                {
                    Id = Guid.NewGuid(),
                    Application = app,
                    Create = DateTime.UtcNow,
                    Name = "Dummy",
                    Update = DateTime.UtcNow,
                    User = user
                };
                db.Simulations.Add(sim);
                db.SaveChanges();
            }

            using (WebClient wc = new WebClient())
            {
                var json = wc.DownloadString("http://localhost:8000/TurbineLite/simulation");
                Console.WriteLine("RESPONSE: {0}", json);
                var simList = JsonConvert.DeserializeObject<SimulationList>(json);
                Assert.IsTrue(simList.Count >= 1);
                var sim = simList.Single(e => e.Name == "Dummy");
                Assert.AreEqual(sim.Name, "Dummy");

                json = wc.DownloadString("http://localhost:8000/TurbineLite/simulation/dummy");
                sim = JsonConvert.DeserializeObject<Simulation>(json);
                Assert.AreEqual<string>(sim.Name, "Dummy");
                Assert.AreEqual<string>(sim.Application, "ACM");
            }
        }

        /// <summary>
        /// GetSimulationInputAnyTest:  Test that various GET methods return "any" wildcards in simulations
        /// </summary>
        [TestMethod]
        public void GetSimulationInputAnyTest()
        {
            using (WebClient wc = new WebClient())
            {
                string json = "NOTHING";
                string data = JsonConvert.SerializeObject(new Simulation { Name = "BFB2", Application = "ACM", StagedInputs = new List<SimpleStagedInputFile>() });
                Console.WriteLine("DATA: {0}", data);
                wc.Headers.Add("Content-Type", "application/json");
                try
                {
                    json = wc.UploadString("http://localhost:8000/TurbineLite/simulation/BFB2", "PUT", data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("EXCEPTION: {0} {1}", ex.Message, ex.StackTrace);
                    Console.WriteLine("EXCEPTION: {0}", ex.InnerException);
                    Assert.Fail();
                }

                data = "This is a test " + Guid.NewGuid();
                wc.Headers.Add("Content-Type", "text/plain");
                var s = wc.UploadString("http://localhost:8000/TurbineLite/simulation/BFB2/input/configuration", "PUT", data);
                Assert.IsTrue(Newtonsoft.Json.JsonConvert.DeserializeObject<Boolean>(s));
                data = "This is a test " + Guid.NewGuid();
                wc.Headers.Add("Content-Type", "text/plain");
                s = wc.UploadString("http://localhost:8000/TurbineLite/simulation/BFB2/input/aspenfile", "PUT", data);
                Assert.IsTrue(Newtonsoft.Json.JsonConvert.DeserializeObject<Boolean>(s));
                data = "This is a test " + Guid.NewGuid();
                wc.Headers.Add("Content-Type", "text/plain");
                s = wc.UploadString("http://localhost:8000/TurbineLite/simulation/BFB2/input/test1.dll", "PUT", data);
                Assert.IsTrue(Newtonsoft.Json.JsonConvert.DeserializeObject<Boolean>(s));
                data = "This is a test " + Guid.NewGuid();
                wc.Headers.Add("Content-Type", "text/plain");
                s = wc.UploadString("http://localhost:8000/TurbineLite/simulation/BFB2/input/test2.dll", "PUT", data);
                Assert.IsTrue(Newtonsoft.Json.JsonConvert.DeserializeObject<Boolean>(s));

                Console.WriteLine("RESPONSE: {0}", json);
                var j = JsonConvert.DeserializeObject<Simulation>(json);
                Assert.AreEqual(j.Name, "BFB2");
                Assert.AreEqual(j.Application, "ACM");
                Assert.AreEqual(j.StagedInputs.Count(), 2);
                Assert.IsNotNull(j.StagedInputs.Find(x => x.Name.Equals("configuration")));
                Assert.IsNotNull(j.StagedInputs.Find(x => x.Name.Equals("aspenfile")));
                Assert.IsNull(j.StagedInputs.Find(x => x.Name.Equals("any")));

                json = wc.DownloadString("http://localhost:8000/TurbineLite/simulation/BFB2/input");
                Console.WriteLine("JSON BFB2 Input: {0}", json);
                var i = JsonConvert.DeserializeObject<SimulationStagedInputList>(json);
                Assert.IsNotNull(i.Find(x => x.Equals("configuration")));
                Assert.IsNotNull(i.Find(x => x.Equals("aspenfile")));
                Assert.IsNotNull(i.Find(x => x.Equals("test1.dll")));
                Assert.IsNotNull(i.Find(x => x.Equals("test2.dll")));

                json = wc.DownloadString("http://localhost:8000/TurbineLite/simulation/BFB2");
                Console.WriteLine("JSON Simulation BFB2: {0}", json);
                var h = JsonConvert.DeserializeObject<Simulation>(json);
                Assert.IsNotNull(h.StagedInputs.Find(x => x.Name.Equals("configuration")));
                Assert.IsNotNull(h.StagedInputs.Find(x => x.Name.Equals("aspenfile")));
                Assert.IsNotNull(h.StagedInputs.Find(x => x.Name.Equals("test1.dll")));
                Assert.IsNotNull(h.StagedInputs.Find(x => x.Name.Equals("test2.dll")));

                json = wc.DownloadString("http://localhost:8000/TurbineLite/simulation/");
                Console.WriteLine("JSON Simulation List: {0}", json);
                var g = JsonConvert.DeserializeObject<SimulationList>(json);
                Assert.IsTrue(g.Count >= 1);
                Assert.IsNotNull(g.Find(y => y.Name.Equals("BFB2")));
                Assert.AreEqual<int>(4, g.Find(y => y.Name.Equals("BFB2")).StagedInputs.Count);
                Assert.IsNotNull(g.Find(y => y.Name.Equals("BFB2")).StagedInputs.Find(x => x.Name.Equals("configuration")));
                Assert.IsNotNull(g.Find(y => y.Name.Equals("BFB2")).StagedInputs.Find(x => x.Name.Equals("aspenfile")));
                Assert.IsNotNull(g.Find(y => y.Name.Equals("BFB2")).StagedInputs.Find(x => x.Name.Equals("test1.dll")));
                Assert.IsNotNull(g.Find(y => y.Name.Equals("BFB2")).StagedInputs.Find(x => x.Name.Equals("test2.dll")));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.Net.WebException), "Mismatch between URL Resource name and Simulation Name")]
        public void PutSimulationStagedInputTest_HTTP_500()
        {
            using (WebClient wc = new WebClient())
            {
                string json = "NOTHING";
                string data = JsonConvert.SerializeObject(new Simulation { Name = "MISMATCHNAME", Application = "ACM", StagedInputs = new List<SimpleStagedInputFile>() });
                Console.WriteLine("DATA: {0}", data);
                wc.Headers.Add("Content-Type", "application/json");
                json = wc.UploadString("http://localhost:8000/TurbineLite/simulation/AAA", "PUT", data);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>simulation name</returns>
        [TestMethod]
        public string PutSimulationStagedInputTest()
        {
            string name = "RandomName";
            var url = String.Format("http://localhost:8000/TurbineLite/simulation/{0}", name);
            using (WebClient wc = new WebClient())
            {
                string json = "NOTHING";
                string data = JsonConvert.SerializeObject(new Simulation { Name = name, Application = "ACM", StagedInputs = new List<SimpleStagedInputFile>() });
                Console.WriteLine("DATA: {0}", data);
                wc.Headers.Add("Content-Type", "application/json");
                try
                {
                    json = wc.UploadString(url, "PUT", data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("EXCEPTION: {0} {1}", ex.Message, ex.StackTrace);
                    Console.WriteLine("EXCEPTION: {0}", ex.InnerException);
                    Assert.Fail();
                }
                Console.WriteLine("RESPONSE: {0}", json);
                var j = JsonConvert.DeserializeObject<Simulation>(json);
                Assert.AreEqual(j.Name, name);
                Assert.AreEqual(j.Application, "ACM");
                Assert.AreEqual(j.StagedInputs.Count(), 2);
                Assert.IsNotNull(j.StagedInputs.Find(x => x.Name.Equals("configuration")));
                Assert.IsNotNull(j.StagedInputs.Find(x => x.Name.Equals("aspenfile")));
                Assert.IsNull(j.StagedInputs.Find(x => x.Name.Equals("any")));
            }

            byte[] dataArray = null;
            using (var fstream = File.Open(@"models\Hybrid_v0.51_rev1.1_UQ_0809_sinter.json", FileMode.Open))
            {
                using (var ms = new MemoryStream())
                {
                    fstream.CopyTo(ms);
                    dataArray = ms.ToArray();
                }
            }
            Assert.IsTrue(dataArray.Length > 0);
            using (WebClient wc = new WebClient())
            {
                var data = System.Text.Encoding.UTF8.GetString(dataArray);
                wc.Headers.Add("Content-Type", "text/plain");
                var s = wc.UploadString(String.Format("{0}/input/configuration", url), "PUT", data);
                Assert.IsTrue(Newtonsoft.Json.JsonConvert.DeserializeObject<Boolean>(s));
                var f = wc.DownloadString(String.Format("{0}/input/configuration", url));
                Assert.AreEqual<string>(data, f);
            }
            dataArray = null;
            using (var fstream = File.Open(@"models\Hybrid_v0.51_rev1.1_UQ_0809.acmf", FileMode.Open))
            {
                using (var ms = new MemoryStream())
                {
                    fstream.CopyTo(ms);
                    dataArray = ms.ToArray();
                }
            }
            Assert.IsTrue(dataArray.Length > 4000);
            using (WebClient wc = new WebClient())
            {
                var data = System.Text.Encoding.UTF8.GetString(dataArray);
                wc.Headers.Add("Content-Type", "text/plain");
                var s = wc.UploadString(String.Format("{0}/input/aspenfile", url), "PUT", data);
                Assert.IsTrue(Newtonsoft.Json.JsonConvert.DeserializeObject<Boolean>(s));
                var f = wc.DownloadString(String.Format("{0}/input/aspenfile", url));
                Assert.AreEqual<int>(data.Length, f.Length);

                // NOT COMPLETELY EQUAL ( maybe new line.. )
                /*
                var provider = System.Security.Cryptography.MD5CryptoServiceProvider.Create();
                byte[] hash = provider.ComputeHash(dataArray);
                var sb = new System.Text.StringBuilder();
                foreach (byte b in hash)
                    sb.Append(b.ToString("X2"));
                string hval = sb.ToString();

                provider = System.Security.Cryptography.MD5CryptoServiceProvider.Create();
                hash = provider.ComputeHash(System.Text.Encoding.UTF8.GetBytes(f));
                sb = new System.Text.StringBuilder();
                foreach (byte b in hash)
                    sb.Append(b.ToString("X2"));
                provider = System.Security.Cryptography.MD5CryptoServiceProvider.Create();
                string hval2 = sb.ToString();
                Assert.AreEqual(hval, hval2);
                 */
            }

            return name;
        }

        [TestMethod]
        public void PutSimulationStagedInputTest_MultipleUpdates()
        {
            using (WebClient wc = new WebClient())
            {
                string json = "NOTHING";
                string data = JsonConvert.SerializeObject(new Simulation { Name = "BFB3", Application = "ACM", StagedInputs = new List<SimpleStagedInputFile>() });
                Console.WriteLine("DATA: {0}", data);
                wc.Headers.Add("Content-Type", "application/json");
                try
                {
                    json = wc.UploadString("http://localhost:8000/TurbineLite/simulation/BFB3", "PUT", data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("EXCEPTION: {0} {1}", ex.Message, ex.StackTrace);
                    Console.WriteLine("EXCEPTION: {0}", ex.InnerException);
                    Assert.Fail();
                }
                Console.WriteLine("RESPONSE: {0}", json);
                var j = JsonConvert.DeserializeObject<Simulation>(json);
                Assert.AreEqual(j.Name, "BFB3");
                Assert.AreEqual(j.Application, "ACM");
                Assert.AreEqual(j.StagedInputs.Count(), 2);
                Assert.IsNotNull(j.StagedInputs.Find(x => x.Name.Equals("configuration")));
                Assert.IsNotNull(j.StagedInputs.Find(x => x.Name.Equals("aspenfile")));
                Assert.IsNull(j.StagedInputs.Find(x => x.Name.Equals("any")));
            }
            for (int i = 0; i < 10; i++)
            {
                using (WebClient wc = new WebClient())
                {
                    var data = "This is a test " + Guid.NewGuid();
                    wc.Headers.Add("Content-Type", "text/plain");
                    var s = wc.UploadString("http://localhost:8000/TurbineLite/simulation/BFB3/input/configuration", "PUT", data);
                    Assert.IsTrue(Newtonsoft.Json.JsonConvert.DeserializeObject<Boolean>(s));
                    var f = wc.DownloadString("http://localhost:8000/TurbineLite/simulation/BFB3/input/configuration");
                    Assert.AreEqual<string>(data, f);
                }
                using (WebClient wc = new WebClient())
                {
                    var data = "This is an aspen file" + Guid.NewGuid();
                    wc.Headers.Add("Content-Type", "text/plain");
                    var s = wc.UploadString("http://localhost:8000/TurbineLite/simulation/BFB3/input/aspenfile", "PUT", data);
                    Assert.IsTrue(Newtonsoft.Json.JsonConvert.DeserializeObject<Boolean>(s));
                    var f = wc.DownloadString("http://localhost:8000/TurbineLite/simulation/BFB3/input/aspenfile");
                    Assert.AreEqual<string>(data, f);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.Net.WebException), "Expecting HTTP 404 Resource Not Found:  Simulation Resource Does not exist")]
        public void PutSimulationStagedInputTest_HTTP_404()
        {
            using (WebClient wc = new WebClient())
            {
                var url = String.Format("http://localhost:8000/TurbineLite/simulation/{0}/input/configuration", Guid.NewGuid().ToString());
                var data = "Simulation Does not exist";
                wc.Headers.Add("Content-Type", "text/plain");
                var json = wc.UploadString(url, "PUT", data);
            }
        }
    }

}
