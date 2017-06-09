using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using Turbine.Web.Contracts;
using System.IO;
using System.ServiceModel.Web;
using WebOperationContext = System.ServiceModel.Web.MockedWebOperationContext;
using Moq;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using Turbine.Consumer.Data.Contract.Behaviors;
using System.Data.Entity.Core;
using Turbine.Lite.Web.Resources.Contracts;
using Turbine.Data.Contract.Behaviors;
using Turbine.Consumer.Contract.Behaviors;
using Microsoft.Practices.Unity;


namespace TurbineLiteConsumerIntegrationTest
{
    /// <summary>
    /// These tests test the Producer and Consumer contracts working together.
    /// </summary>
    /// 
    class SystemContext : Turbine.Consumer.Contract.Behaviors.IContext
    {
        public string BaseWorkingDirectory
        {
            get { return Directory.GetCurrentDirectory(); }
        }
    }

    internal class MockSimulationResource : Turbine.Lite.Web.Resources.SimulationResource
    {
        override protected string OutgoingWebResponseContext_ContentType
        {
            get 
            {
                return WebOperationContext.Current.OutgoingResponse.ContentType; 
            }
            set
            {
                WebOperationContext.Current.OutgoingResponse.ContentType = value;
            }
        }
        override protected System.Net.HttpStatusCode OutgoingWebResponseContext_StatusCode
        {
            get
            {
                return WebOperationContext.Current.OutgoingResponse.StatusCode;
            }
            set
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = value;
            }
        }
    }

    internal class MockSessionResource : Turbine.Lite.Web.Resources.SessionResource
    {
        override protected string OutgoingWebResponseContext_ContentType
        {
            get
            {
                return WebOperationContext.Current.OutgoingResponse.ContentType;
            }
            set
            {
                WebOperationContext.Current.OutgoingResponse.ContentType = value;
            }
        }
        override protected System.Net.HttpStatusCode OutgoingWebResponseContext_StatusCode
        {
            get
            {
                return WebOperationContext.Current.OutgoingResponse.StatusCode;
            }
            set
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = value;
            }
        }

        /// <summary>
        /// return default value
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        override protected int QueryParameters_GetPage(int p)
        {
            return p;
        }

        override protected int QueryParameters_GetPageSize(int p)
        {
            return p;
        }

        override protected bool QueryParameters_GetVerbose(bool p)
        {
            return p;
        }
        override protected bool QueryParameters_GetMetaData(bool p)
        {
            return p;
        }
    } 


    [TestClass]
    //[DeploymentItem("EntityFramework.SqlServer.dll")]
    //[DeploymentItem("EntityFramework.SqlServerCompact")]
    [DeploymentItem(@"models\Hybrid_v0.51_rev1.1_UQ_0809_sinter.json", @"models")]
    [DeploymentItem(@"models\Hybrid_v0.51_rev1.1_UQ_0809.acmf", @"models")]
    public class ConsumerContractTest
    {
        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            Console.WriteLine("assembly init");
            /* MSTest issue:
             * EF6 Problem is caused because the compiler doesn't output the 
             * EntityFramework.SqlServer.dll as it does not detect if it's 
             * used somewhere (it's only used through dependency injection)
             */
            var _a = typeof(System.Data.Entity.SqlServer.SqlProviderServices);
            var _b = typeof(System.Data.Entity.SqlServerCompact.SqlCeProviderServices);
            var _c = typeof(Microsoft.Practices.ServiceLocation.ServiceLocator);
            var _d = typeof(Turbine.Lite.Consumer.Data.Contract.ConsumerRegistrationContract);
            CleanUpDatabase();

            // DIJ
            TestProducerContext.name = "Administrator";
            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                db.Users.Add(new Turbine.Data.Entities.User { Name = TestProducerContext.name });
                db.SaveChanges();
            }
            // Create the ServiceHost.
            try
            {

                using (var db = new Turbine.DataEF6.ProducerContext())
                {
                    Debug.WriteLine("Application ProducerContext", "TurbineLite");
                    db.Applications.Add(new Turbine.Data.Entities.Application { Name = "ACM", Version = "7.3" });
                    db.Applications.Add(new Turbine.Data.Entities.Application { Name = "AspenPlus" });
                    db.Applications.Add(new Turbine.Data.Entities.Application { Name = "Excel" });
                    db.SaveChanges();
                }

                using (var db = new Turbine.DataEF6.ProducerContext())
                {
                    var item = db.Applications.Single(s => s.Name == "ACM");
                    item.InputFileTypes.Add(new Turbine.Data.Entities.InputFileType
                    {
                        Id = Guid.NewGuid(),
                        Name = "configuration",
                        Required = true,
                        Type = "plain/text"
                    });
                    item.InputFileTypes.Add(new Turbine.Data.Entities.InputFileType
                    {
                        Id = Guid.NewGuid(),
                        Name = "aspenfile",
                        Required = true,
                        Type = "plain/text"
                    });
                    db.SaveChanges();
                }

                using (var db = new Turbine.DataEF6.ProducerContext())
                {
                    var item = db.Applications.Single(s => s.Name == "Excel");
                    item.InputFileTypes.Add(new Turbine.Data.Entities.InputFileType
                    {
                        Id = Guid.NewGuid(),
                        Name = "configuration",
                        Required = true,
                        Type = "plain/text"
                    });
                    item.InputFileTypes.Add(new Turbine.Data.Entities.InputFileType
                    {
                        Id = Guid.NewGuid(),
                        Name = "spreadsheet",
                        Required = true,
                        Type = "plain/text"
                    });
                    db.SaveChanges();
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                Debug.WriteLine("Ignore Application entries already made");
            }
        }
        public static void CleanUpDatabase()
        {
            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                db.Database.ExecuteSqlCommand("delete FROM Processes");
                db.Database.ExecuteSqlCommand("delete FROM Messages");
                db.Database.ExecuteSqlCommand("delete FROM Jobs");
                db.Database.ExecuteSqlCommand("delete FROM Sessions");
                db.Database.ExecuteSqlCommand("delete FROM InputFileTypes");
                db.Database.ExecuteSqlCommand("delete FROM SimulationStagedInputs");
                db.Database.ExecuteSqlCommand("delete FROM Applications");
                db.Database.ExecuteSqlCommand("delete FROM Simulations");
                db.Database.ExecuteSqlCommand("delete FROM Users");
            }
        }
        [AssemblyCleanup()]
        public static void AssemblyCleanup()
        {
            Console.WriteLine("assembly cleanup");
        }

        [TestInitialize()]
        public void ConsumerContractTestInitialize() 
        {
            IUnityContainer container = Turbine.Consumer.AppUtility.container;
            if (container.IsRegistered<IConsumerContext>())
            {
                IConsumerContext consumerCtx = Turbine.Consumer.AppUtility.GetConsumerContext();
                consumerCtx.BindSimulationName = null;
            }
        }

        [TestMethod]
        public void SimulationResourceTest()
        {
            _SimulationResourceTest();
        }

        private string _SimulationResourceTest()
        {
            Mock<IWebOperationContext> mockContext = new Mock<IWebOperationContext> { DefaultValue = DefaultValue.Mock };
            var simulationName = Guid.NewGuid().ToString();
            Debug.WriteLine("Test");
            //ISessionResource iSession = new Turbine.Lite.Web.Resources.SessionResource();
            ISimulationResource iSimulation = new MockSimulationResource();
            iSimulation.UpdateSimulation(simulationName, new Turbine.Data.Serialize.Simulation { Application = "ACM", Name = simulationName });
            using (new MockedWebOperationContext(mockContext.Object))
            {
                using (var fstream = File.Open(@"models\Hybrid_v0.51_rev1.1_UQ_0809_sinter.json", FileMode.Open))
                {
                    iSimulation.UpdateStagedInputFile(simulationName, "configuration", fstream);
                }
            }
            //mockContext.VerifySet(c => c.OutgoingResponse.ContentType, "application/atom+xml");
            using (new MockedWebOperationContext(mockContext.Object))
            {
                using (var fstream = File.Open(@"models\Hybrid_v0.51_rev1.1_UQ_0809.acmf", FileMode.Open))
                {
                    iSimulation.UpdateStagedInputFile(simulationName, "aspenfile", fstream);
                }
            }
            //mockContext.VerifySet(c => c.OutgoingResponse.ContentType, "application/atom+xml");

            return simulationName;
        }

        [TestMethod]
        public void SimulationResourceInputDirectoryTest()
        {
            Mock<IWebOperationContext> mockContext = new Mock<IWebOperationContext> { DefaultValue = DefaultValue.Mock };
            var simulationName = Guid.NewGuid().ToString();
            Debug.WriteLine("Test");
            //ISessionResource iSession = new Turbine.Lite.Web.Resources.SessionResource();
            ISimulationResource iSimulation = new MockSimulationResource();
            iSimulation.UpdateSimulation(simulationName, new Turbine.Data.Serialize.Simulation { Application = "ACM", Name = simulationName });
            using (new MockedWebOperationContext(mockContext.Object))
            {
                using (var fstream = File.Open(@"models\Hybrid_v0.51_rev1.1_UQ_0809_sinter.json", FileMode.Open))
                {
                    iSimulation.UpdateStagedInputFile(simulationName, "configuration", fstream);
                }
            }
            //mockContext.VerifySet(c => c.OutgoingResponse.ContentType, "application/atom+xml");
            using (new MockedWebOperationContext(mockContext.Object))
            {
                using (var fstream = File.Open(@"models\Hybrid_v0.51_rev1.1_UQ_0809.acmf", FileMode.Open))
                {
                    iSimulation.UpdateStagedInputFile(simulationName, "aspenfile", fstream);
                }

                using (var fstream = File.Open(@"models\Hybrid_v0.51_rev1.1_UQ_0809.acmf", FileMode.Open))
                {
                    iSimulation.UpdateStagedInputFile(simulationName, "var/aspenfile", fstream);
                }
            }
            //mockContext.VerifySet(c => c.OutgoingResponse.ContentType, "application/atom+xml");

            Debug.WriteLine("SimulationName: " + simulationName);

            //return simulationName;
        }

        /// <summary>
        /// Create a dummy Excel simulation
        /// </summary>
        /// <returns></returns>
        private string _CreateExcelSimulationTest()
        {
            Mock<IWebOperationContext> mockContext = new Mock<IWebOperationContext> { DefaultValue = DefaultValue.Mock };
            var simulationName = Guid.NewGuid().ToString();
            Debug.WriteLine("Test");
            //ISessionResource iSession = new Turbine.Lite.Web.Resources.SessionResource();
            ISimulationResource iSimulation = new MockSimulationResource();
            iSimulation.UpdateSimulation(simulationName, new Turbine.Data.Serialize.Simulation { Application = "Excel", Name = simulationName });
            using (new MockedWebOperationContext(mockContext.Object))
            {
                using (var fstream = File.Open(@"models\Hybrid_v0.51_rev1.1_UQ_0809_sinter.json", FileMode.Open))
                {
                    iSimulation.UpdateStagedInputFile(simulationName, "configuration", fstream);
                }
            }
            //mockContext.VerifySet(c => c.OutgoingResponse.ContentType, "application/atom+xml");
            using (new MockedWebOperationContext(mockContext.Object))
            {
                using (var fstream = File.Open(@"models\Hybrid_v0.51_rev1.1_UQ_0809.acmf", FileMode.Open))
                {
                    iSimulation.UpdateStagedInputFile(simulationName, "spreadsheet", fstream);
                }
            }
            //mockContext.VerifySet(c => c.OutgoingResponse.ContentType, "application/atom+xml");

            return simulationName;
        }

        /// <summary>
        /// CreateExcelJobs
        /// </summary>
        /// <returns>session guid with 1 excel job</returns>
        private Guid CreateExcelJobs()
        {
            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                db.Database.ExecuteSqlCommand("delete FROM Processes");
                db.Database.ExecuteSqlCommand("delete FROM Messages");
                db.Database.ExecuteSqlCommand("delete FROM Jobs");
            }
            DateTime startTime = DateTime.UtcNow;
            Mock<IWebOperationContext> mockContext = new Mock<IWebOperationContext> { DefaultValue = DefaultValue.Mock };
            var simulationName = _CreateExcelSimulationTest();
            Debug.WriteLine("Test");
            ISessionResource session = new MockSessionResource();

            var sessionList = session.GetSessionList();
            var count = sessionList.Count;

            Assert.IsTrue(count >= 0);

            Guid sessionId = session.CreateSession();
            sessionList = session.GetSessionList();
            count += 1;
            Assert.AreEqual(count, sessionList.Count);

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

            var contents = JsonConvert.SerializeObject(jobList);
            Debug.WriteLine("JOBS: " + contents);
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(contents));
            var l = session.AppendJobs(sessionId.ToString(), stream);

            Assert.AreEqual(jobList.Count, l.Count);
            Debug.WriteLine("JOBS: " + String.Join(",", l));
            var jobId = l.Single();

            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                var job = db.Jobs.Single(j => j.Count == jobId);
                Assert.AreEqual("create", job.State);
                Assert.IsTrue(job.Create > startTime);
                Assert.IsNull(job.Submit);
                Assert.IsNull(job.Setup);
                Assert.IsNull(job.Running);
                Assert.IsNull(job.Finished);
            }

            byte[] dataArray = null;
            using (Stream s = session.StatusSession(sessionId.ToString()))
            {
                using (var ms = new MemoryStream())
                {
                    s.CopyTo(ms);
                    dataArray = ms.ToArray();
                }
            }
            contents = Encoding.UTF8.GetString(dataArray);
            Debug.WriteLine("Session Status: " + contents);
            SessionStatusDict statusDict =
                JsonConvert.DeserializeObject<SessionStatusDict>(contents);

            Assert.AreEqual(1, statusDict.Create);
            Assert.AreEqual(0, statusDict.Submit);
            Assert.AreEqual(0, statusDict.Setup);
            Assert.AreEqual(0, statusDict.Running);
            Assert.AreEqual(0, statusDict.Success);
            Assert.AreEqual(0, statusDict.Error);
            Assert.AreEqual(0, statusDict.Cancel);
            Assert.AreEqual(0, statusDict.Terminate);
            Assert.AreEqual(0, statusDict.Pause);
            Assert.AreEqual(0, statusDict.Locked);

            return sessionId;
        }

        /// <summary>
        /// Remove all Jobs, then create new session with a job and run through
        /// consumer job contrace
        /// </summary>
        [TestMethod]
        public void SessionResourceCreateTest()
        {
            _SessionResourceCreateTest();
        }

        private Guid _SessionResourceCreateTest()
        {
            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                db.Database.ExecuteSqlCommand("delete FROM Processes");
                db.Database.ExecuteSqlCommand("delete FROM Messages");
                db.Database.ExecuteSqlCommand("delete FROM Jobs");
            }
            DateTime startTime = DateTime.UtcNow;
            Mock<IWebOperationContext> mockContext = new Mock<IWebOperationContext> { DefaultValue = DefaultValue.Mock };
            var simulationName = _SimulationResourceTest();
            Debug.WriteLine("Test");
            ISessionResource session = new MockSessionResource();

            var sessionList = session.GetSessionList();
            var count = sessionList.Count;

            Assert.IsTrue(count >= 0);
            
            Guid sessionId = session.CreateSession();
            sessionList = session.GetSessionList();
            count += 1;
            Assert.AreEqual(count, sessionList.Count);

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

            var contents = JsonConvert.SerializeObject(jobList);
            Debug.WriteLine("JOBS: " + contents);
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(contents));
            var l  = session.AppendJobs(sessionId.ToString(), stream);

            Assert.AreEqual(jobList.Count, l.Count);
            Debug.WriteLine("JOBS: " + String.Join(",", l));
            var jobId = l.Single();

            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                var job = db.Jobs.Single(j => j.Count == jobId);
                Assert.AreEqual("create", job.State);
                Assert.IsTrue(job.Create > startTime);
                Assert.IsNull(job.Submit);
                Assert.IsNull(job.Setup);
                Assert.IsNull(job.Running);
                Assert.IsNull(job.Finished);
            }

            byte[] dataArray = null;
            using (Stream s = session.StatusSession(sessionId.ToString()))
            {
                using (var ms = new MemoryStream())
                {
                    s.CopyTo(ms);
                    dataArray = ms.ToArray();
                }
            }
            contents = Encoding.UTF8.GetString(dataArray);
            Debug.WriteLine("Session Status: " + contents);
            SessionStatusDict statusDict = 
                JsonConvert.DeserializeObject<SessionStatusDict>(contents);

            Assert.AreEqual(1, statusDict.Create);
            Assert.AreEqual(0, statusDict.Submit);
            Assert.AreEqual(0, statusDict.Setup);
            Assert.AreEqual(0, statusDict.Running);
            Assert.AreEqual(0, statusDict.Success);
            Assert.AreEqual(0, statusDict.Error);
            Assert.AreEqual(0, statusDict.Cancel);
            Assert.AreEqual(0, statusDict.Terminate);
            Assert.AreEqual(0, statusDict.Pause);
            Assert.AreEqual(0, statusDict.Locked);


            return sessionId;
        }


        [TestMethod]
        public void SessionResourceCreateTestWithConsumer()
        {
            _SessionResourceCreateTestWithConsumer();
        }

        private Guid _SessionResourceCreateTestWithConsumer()
        {
            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                db.Database.ExecuteSqlCommand("delete FROM Processes");
                db.Database.ExecuteSqlCommand("delete FROM Messages");
                db.Database.ExecuteSqlCommand("delete FROM Jobs");
            }
            DateTime startTime = DateTime.UtcNow;
            Mock<IWebOperationContext> mockContext = new Mock<IWebOperationContext> { DefaultValue = DefaultValue.Mock };
            var simulationName = _SimulationResourceTest();
            Debug.WriteLine("Test");
            ISessionResource session = new MockSessionResource();

            var sessionList = session.GetSessionList();
            var count = sessionList.Count;

            Assert.IsTrue(count >= 0);

            Guid sessionId = session.CreateSession();
            sessionList = session.GetSessionList();
            count += 1;
            Assert.AreEqual(count, sessionList.Count);

            IConsumerRegistrationContract cc = Turbine.Consumer.AppUtility.GetConsumerRegistrationContract();
            IConsumerRun run = Turbine.Consumer.AppUtility.GetConsumerRunContract();
            IConsumerRun anotherRun = Turbine.Consumer.AppUtility.GetConsumerRunContract();
            Debug.WriteLine(String.Format("Supported Applications:  {0}", String.Join(",", run.SupportedApplications)), GetType().Name);
            Assert.IsNull(cc.Queue);
            cc.Register(run);
            cc.Register(anotherRun);
            Assert.IsNotNull(cc.Queue);
            Debug.WriteLine(String.Format("Supported Applications:  {0}", String.Join(",", run.SupportedApplications)), GetType().Name);

            var jobList = new List<JobRequestWithConsumer>();
            var inputD = new JObject();
            inputD.Add("test", Guid.NewGuid());
            jobList.Add(new JobRequestWithConsumer
            {
                Initialize = false,
                Reset = false,
                Simulation = simulationName,
                ConsumerId = anotherRun.ConsumerId,
                Input = new JObject()
            });

            var contents = JsonConvert.SerializeObject(jobList);
            Debug.WriteLine("JOBS: " + contents);
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(contents));
            var l = session.AppendJobs(sessionId.ToString(), stream);

            Assert.AreEqual(jobList.Count, l.Count);
            Debug.WriteLine("JOBS: " + String.Join(",", l));
            var jobId = l.Single();

            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                var job = db.Jobs.Single(j => j.Count == jobId);
                Assert.AreEqual("create", job.State);
                Assert.IsTrue(job.Create > startTime);
                Assert.IsNull(job.Submit);
                Assert.IsNull(job.Setup);
                Assert.IsNull(job.Running);
                Assert.IsNull(job.Finished);
            }
            
            int startSessionInt = session.StartSession(sessionId.ToString());

            //Debug.WriteLine("Count: " + count.ToString(), "_SessionResourceCreateTestWithConsumer");
            //Debug.WriteLine("startSessionInt: " + startSessionInt.ToString(), "_SessionResourceCreateTestWithConsumer");
            //Assert.AreEqual(count, startSessionInt);

            IJobConsumerContract contract = cc.Queue.GetNext(run);
            Assert.IsNull(contract);

            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                var job = db.Jobs.Single(j => j.Count == jobId);
                Assert.AreEqual("submit", job.State);
                Assert.IsTrue(job.Submit > job.Create);
                Assert.IsNull(job.Setup);
                Assert.IsNull(job.Running);
                Assert.IsNull(job.Finished);
            }

            return sessionId;
        }

        /// <summary>
        /// Remove all Jobs, then create new session with a job and run through
        /// consumer job contrace
        /// </summary>
        [TestMethod]
        public void SessionResourceStateSuccessTest()
        {
            _SessionResourceStateSuccessTest();
        }
        private Guid _SessionResourceStateSuccessTest()
        {
            int count = 1;
            ISessionResource session = new MockSessionResource();
            var sessionId = _SessionResourceCreateTest();

            Assert.AreEqual(count, session.StartSession(sessionId.ToString()));

            byte[] dataArray = null;
            using (Stream s = session.StatusSession(sessionId.ToString()))
            {
                using (var ms = new MemoryStream())
                {
                    s.CopyTo(ms);
                    dataArray = ms.ToArray();
                }
            }

            string contents = Encoding.UTF8.GetString(dataArray);
            Debug.WriteLine("Session Status: " + contents);
            SessionStatusDict statusDict = JsonConvert.DeserializeObject<SessionStatusDict>(contents);

            Assert.AreEqual(0, statusDict.Create);
            Assert.AreEqual(1, statusDict.Submit);
            Assert.AreEqual(0, statusDict.Setup);
            Assert.AreEqual(0, statusDict.Running);
            Assert.AreEqual(0, statusDict.Success);
            Assert.AreEqual(0, statusDict.Error);
            Assert.AreEqual(0, statusDict.Cancel);
            Assert.AreEqual(0, statusDict.Terminate);
            Assert.AreEqual(0, statusDict.Pause);
            Assert.AreEqual(0, statusDict.Locked);

            int jobId;
            // Start Consumer
            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                var job = db.Jobs.Single(j => j.State == "submit");
                Assert.AreEqual("submit", job.State);
                Assert.IsTrue(job.Submit > job.Create);
                Assert.IsNull(job.Setup);
                Assert.IsNull(job.Running);
                Assert.IsNull(job.Finished);
                jobId = job.Count;
            }

            IConsumerRegistrationContract cc = Turbine.Consumer.AppUtility.GetConsumerRegistrationContract();
            IConsumerRun run = Turbine.Consumer.AppUtility.GetConsumerRunContract();
            Debug.WriteLine(String.Format("Supported Applications:  {0}", String.Join(",", run.SupportedApplications)), GetType().Name);
            Assert.IsNull(cc.Queue);
            cc.Register(run);
            Assert.IsNotNull(cc.Queue);
            Debug.WriteLine(String.Format("Supported Applications:  {0}", String.Join(",", run.SupportedApplications)), GetType().Name);

            IJobConsumerContract contract = cc.Queue.GetNext(run);
            Assert.IsNotNull(contract);
            Assert.AreEqual(contract.Id, jobId);

            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                var job = db.Jobs.Single(j => j.Count == jobId);
                Assert.AreEqual("locked", job.State);
                Assert.IsTrue(job.Submit > job.Create);
                Assert.IsNull(job.Setup);
                Assert.IsNull(job.Running);
                Assert.IsNull(job.Finished);
            }

            contract.Setup();
            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                var job = db.Jobs.Single(j => j.Count == jobId);
                Assert.AreEqual("setup", job.State);
                Assert.IsTrue(job.Submit > job.Create);
                Assert.IsTrue(job.Setup > job.Submit);
                Assert.IsNull(job.Running);
                Assert.IsNull(job.Finished);
            }
            contract.Running();
            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                var job = db.Jobs.Single(j => j.Count == jobId);
                Assert.AreEqual("running", job.State);
                Assert.IsTrue(job.Submit > job.Create);
                Assert.IsTrue(job.Setup > job.Submit);
                Assert.IsTrue(job.Running > job.Setup);
                Assert.IsNull(job.Finished);
            }
            IProcess process = contract.Process;
            var inputDict = process.Input;
            var outputDict = new Dictionary<string, Object>();

            foreach (var key in inputDict.Keys.ToArray<string>())
            {
                if (inputDict[key] == null)
                    continue;
                outputDict[key] = String.Format("{0} OUTPUT", inputDict[key]);
                process.AddStdout(String.Format("Add Output {0}\n", key));
            }

            process.AddStdout("Fake Job Completed");
            process.Output = outputDict;
            contract.Success();

            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                var job = db.Jobs.Single(j => j.Count == jobId);
                Assert.AreEqual("success", job.State);
                Assert.IsTrue(job.Submit > job.Create);
                Assert.IsTrue(job.Setup > job.Submit);
                Assert.IsTrue(job.Running > job.Setup);
                Assert.IsTrue(job.Finished > job.Running);
            }

            return sessionId;
        }

        /// <summary>
        /// Remove all Jobs, then create new session with a job and run through
        /// consumer job contract
        /// </summary>
        [TestMethod]
        public void SessionResourceStopTest()
        {
            int count = 1;
            ISessionResource session = new MockSessionResource();
            var sessionId = _SessionResourceCreateTest();

            Assert.AreEqual(count, session.StartSession(sessionId.ToString()));

            byte[] dataArray = null;
            using (Stream s = session.StatusSession(sessionId.ToString()))
            {
                using (var ms = new MemoryStream())
                {
                    s.CopyTo(ms);
                    dataArray = ms.ToArray();
                }
            }

            string contents = Encoding.UTF8.GetString(dataArray);
            Debug.WriteLine("Session Status: " + contents);
            SessionStatusDict statusDict = JsonConvert.DeserializeObject<SessionStatusDict>(contents);

            Assert.AreEqual(0, statusDict.Create);
            Assert.AreEqual(1, statusDict.Submit);
            Assert.AreEqual(0, statusDict.Setup);
            Assert.AreEqual(0, statusDict.Running);
            Assert.AreEqual(0, statusDict.Success);
            Assert.AreEqual(0, statusDict.Error);
            Assert.AreEqual(0, statusDict.Cancel);
            Assert.AreEqual(0, statusDict.Terminate);
            Assert.AreEqual(0, statusDict.Pause);
            Assert.AreEqual(0, statusDict.Locked);

            int jobId;
            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                var job = db.Jobs.Single(j => j.State == "submit");
                Assert.AreEqual("submit", job.State);
                Assert.IsTrue(job.Submit > job.Create);
                Assert.IsNull(job.Setup);
                Assert.IsNull(job.Running);
                Assert.IsNull(job.Finished);
                jobId = job.Count;
            }

            // Stop Session

            Assert.AreEqual(count, session.StopSession(sessionId.ToString()));

            dataArray = null;
            using (Stream s = session.StatusSession(sessionId.ToString()))
            {
                using (var ms = new MemoryStream())
                {
                    s.CopyTo(ms);
                    dataArray = ms.ToArray();
                }
            }

            contents = Encoding.UTF8.GetString(dataArray);
            Debug.WriteLine("Session Status: " + contents);
            statusDict = JsonConvert.DeserializeObject<SessionStatusDict>(contents);

            Assert.AreEqual(0, statusDict.Create);
            Assert.AreEqual(0, statusDict.Submit);
            Assert.AreEqual(0, statusDict.Setup);
            Assert.AreEqual(0, statusDict.Running);
            Assert.AreEqual(0, statusDict.Success);
            Assert.AreEqual(0, statusDict.Error);
            Assert.AreEqual(0, statusDict.Cancel);
            Assert.AreEqual(0, statusDict.Terminate);
            Assert.AreEqual(1, statusDict.Pause);
            Assert.AreEqual(0, statusDict.Locked);


            // Start Consumer
            IConsumerRegistrationContract cc = Turbine.Consumer.AppUtility.GetConsumerRegistrationContract();
            IConsumerRun run = Turbine.Consumer.AppUtility.GetConsumerRunContract();
            Assert.IsNull(cc.Queue);
            cc.Register(run);
            Assert.IsNotNull(cc.Queue);

            IJobConsumerContract contract = cc.Queue.GetNext(run);
            Assert.IsNull(contract);
        }


        /// <summary>
        /// Test if Excel Consumer picks up Aspen Job.
        /// </summary>
        [TestMethod]
        public void ExcelConsumerNoJobTest()
        {
            int count = 1;
            ISessionResource session = new MockSessionResource();
            var sessionId = _SessionResourceCreateTest();

            Assert.AreEqual(count, session.StartSession(sessionId.ToString()));

            byte[] dataArray = null;
            using (Stream s = session.StatusSession(sessionId.ToString()))
            {
                using (var ms = new MemoryStream())
                {
                    s.CopyTo(ms);
                    dataArray = ms.ToArray();
                }
            }

            string contents = Encoding.UTF8.GetString(dataArray);
            Debug.WriteLine("Session Status: " + contents);
            SessionStatusDict statusDict = JsonConvert.DeserializeObject<SessionStatusDict>(contents);

            Assert.AreEqual(0, statusDict.Create);
            Assert.AreEqual(1, statusDict.Submit);
            Assert.AreEqual(0, statusDict.Setup);
            Assert.AreEqual(0, statusDict.Running);
            Assert.AreEqual(0, statusDict.Success);
            Assert.AreEqual(0, statusDict.Error);
            Assert.AreEqual(0, statusDict.Cancel);
            Assert.AreEqual(0, statusDict.Terminate);
            Assert.AreEqual(0, statusDict.Pause);
            Assert.AreEqual(0, statusDict.Locked);

            int jobId;
            // Start Consumer
            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                var job = db.Jobs.Single(j => j.State == "submit");
                Assert.AreEqual("submit", job.State);
                Assert.IsTrue(job.Submit > job.Create);
                Assert.IsNull(job.Setup);
                Assert.IsNull(job.Running);
                Assert.IsNull(job.Finished);
                jobId = job.Count;
            }

            IConsumerRegistrationContract cc = Turbine.Consumer.AppUtility.GetConsumerRegistrationContract();
            //IConsumerRun run = Turbine.Consumer.AppUtility.GetConsumerRunContract();
            IConsumerRun run = new TurbineLiteConsumerIntegrationTest.DummyExcelSinterConsumer();

            Debug.WriteLine(String.Format("Supported Applications:  {0}", String.Join(",", run.SupportedApplications)), GetType().Name);
            Assert.IsNull(cc.Queue);
            cc.Register(run);
            Assert.IsNotNull(cc.Queue);
            Debug.WriteLine(String.Format("Supported Applications:  {0}", String.Join(",", run.SupportedApplications)), GetType().Name);

            // Contract should return Null because there are no relevant jobs available
            IJobConsumerContract contract = cc.Queue.GetNext(run);
            Assert.IsNull(contract);
        }
        /// <summary>
        /// Test if Aspen Consumer picks up Excel Job.
        /// </summary>
        [TestMethod]
        public void AspenConsumerNoJobTest()
        {
            int count = 1;
            ISessionResource session = new MockSessionResource();
            Guid sessionId = CreateExcelJobs();

            Assert.AreEqual(count, session.StartSession(sessionId.ToString()));

            byte[] dataArray = null;
            using (Stream s = session.StatusSession(sessionId.ToString()))
            {
                using (var ms = new MemoryStream())
                {
                    s.CopyTo(ms);
                    dataArray = ms.ToArray();
                }
            }

            string contents = Encoding.UTF8.GetString(dataArray);
            Debug.WriteLine("Session Status: " + contents);
            SessionStatusDict statusDict = JsonConvert.DeserializeObject<SessionStatusDict>(contents);

            Assert.AreEqual(0, statusDict.Create);
            Assert.AreEqual(1, statusDict.Submit);
            Assert.AreEqual(0, statusDict.Setup);
            Assert.AreEqual(0, statusDict.Running);
            Assert.AreEqual(0, statusDict.Success);
            Assert.AreEqual(0, statusDict.Error);
            Assert.AreEqual(0, statusDict.Cancel);
            Assert.AreEqual(0, statusDict.Terminate);
            Assert.AreEqual(0, statusDict.Pause);
            Assert.AreEqual(0, statusDict.Locked);

            int jobId;
            // Start Consumer
            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                var job = db.Jobs.Single(j => j.State == "submit");
                Assert.AreEqual("submit", job.State);
                Assert.IsTrue(job.Submit > job.Create);
                Assert.IsNull(job.Setup);
                Assert.IsNull(job.Running);
                Assert.IsNull(job.Finished);
                jobId = job.Count;
            }

            IConsumerRegistrationContract cc = Turbine.Consumer.AppUtility.GetConsumerRegistrationContract();
            //IConsumerRun run = Turbine.Consumer.AppUtility.GetConsumerRunContract();
            IConsumerRun run = new TurbineLiteConsumerIntegrationTest.DummyAspenSinterConsumer();

            Debug.WriteLine(String.Format("Supported Applications:  {0}", String.Join(",", run.SupportedApplications)), GetType().Name);
            Assert.IsNull(cc.Queue);
            cc.Register(run);
            Assert.IsNotNull(cc.Queue);
            Debug.WriteLine(String.Format("Supported Applications:  {0}", String.Join(",", run.SupportedApplications)), GetType().Name);

            IJobConsumerContract contract = cc.Queue.GetNext(run);
            Assert.IsNull(contract);
        }

        /// <summary>
        /// Bind AspenConsumer to a simulation using the IConsumerContext, test if it picks up only
        /// jobs with that simulation.
        /// </summary>
        [TestMethod]
        public void AspenConsumerBindSimulationTest()
        {
            IUnityContainer container = Turbine.Consumer.AppUtility.container;
            IConsumerContext consumerCtx = Turbine.Consumer.AppUtility.GetConsumerContext();
            container.RegisterInstance<IConsumerContext>(consumerCtx);
            // BIND TO BOGUS Simulation Name
            consumerCtx.BindSimulationName = Guid.NewGuid().ToString();

            int count = 1;
            ISessionResource session = new MockSessionResource();
            var sessionId = _SessionResourceCreateTest();

            Assert.AreEqual(count, session.StartSession(sessionId.ToString()));

            byte[] dataArray = null;
            using (Stream s = session.StatusSession(sessionId.ToString()))
            {
                using (var ms = new MemoryStream())
                {
                    s.CopyTo(ms);
                    dataArray = ms.ToArray();
                }
            }

            string contents = Encoding.UTF8.GetString(dataArray);
            Debug.WriteLine("Session Status: " + contents);
            SessionStatusDict statusDict = JsonConvert.DeserializeObject<SessionStatusDict>(contents);

            Assert.AreEqual(0, statusDict.Create);
            Assert.AreEqual(1, statusDict.Submit);
            Assert.AreEqual(0, statusDict.Setup);
            Assert.AreEqual(0, statusDict.Running);
            Assert.AreEqual(0, statusDict.Success);
            Assert.AreEqual(0, statusDict.Error);
            Assert.AreEqual(0, statusDict.Cancel);
            Assert.AreEqual(0, statusDict.Terminate);
            Assert.AreEqual(0, statusDict.Pause);
            Assert.AreEqual(0, statusDict.Locked);

            int jobId;
            // Start Consumer
            string simulationName = null;
            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                var job = db.Jobs.Single(j => j.State == "submit");
                Assert.AreEqual("submit", job.State);
                Assert.IsTrue(job.Submit > job.Create);
                Assert.IsNull(job.Setup);
                Assert.IsNull(job.Running);
                Assert.IsNull(job.Finished);
                jobId = job.Count;
                simulationName = job.Simulation.Name;
            }

            IConsumerRegistrationContract cc = Turbine.Consumer.AppUtility.GetConsumerRegistrationContract();
            IConsumerRun run = Turbine.Consumer.AppUtility.GetConsumerRunContract();
            Debug.WriteLine(String.Format("Supported Applications:  {0}", String.Join(",", run.SupportedApplications)), GetType().Name);
            Assert.IsNull(cc.Queue);
            cc.Register(run);
            Assert.IsNotNull(cc.Queue);
            Debug.WriteLine(String.Format("Supported Applications:  {0}", String.Join(",", run.SupportedApplications)), GetType().Name);

            IJobConsumerContract contract = cc.Queue.GetNext(run);
            Assert.IsNull(contract);

            consumerCtx.BindSimulationName = simulationName;
            run = Turbine.Consumer.AppUtility.GetConsumerRunContract();
            cc.Register(run);
            contract = cc.Queue.GetNext(run);
            Assert.IsNotNull(contract);

            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                var job = db.Jobs.Single(j => j.Count == jobId);
                Assert.AreEqual("locked", job.State);
                Assert.IsTrue(job.Submit > job.Create);
                Assert.IsNull(job.Setup);
                Assert.IsNull(job.Running);
                Assert.IsNull(job.Finished);
            }
            /*
            contract.Setup();
            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                var job = db.Jobs.Single(j => j.Count == jobId);
                Assert.AreEqual("setup", job.State);
                Assert.IsTrue(job.Submit > job.Create);
                Assert.IsTrue(job.Setup > job.Submit);
                Assert.IsNull(job.Running);
                Assert.IsNull(job.Finished);
            }
            contract.Running();
            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                var job = db.Jobs.Single(j => j.Count == jobId);
                Assert.AreEqual("running", job.State);
                Assert.IsTrue(job.Submit > job.Create);
                Assert.IsTrue(job.Setup > job.Submit);
                Assert.IsTrue(job.Running > job.Setup);
                Assert.IsNull(job.Finished);
            }
            IProcess process = contract.Process;
            var inputDict = process.Input;
            var outputDict = new Dictionary<string, Object>();

            foreach (var key in inputDict.Keys.ToArray<string>())
            {
                if (inputDict[key] == null)
                    continue;
                outputDict[key] = String.Format("{0} OUTPUT", inputDict[key]);
                process.AddStdout(String.Format("Add Output {0}\n", key));
            }

            process.AddStdout("Fake Job Completed");
            process.Output = outputDict;
            contract.Success();

            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                var job = db.Jobs.Single(j => j.Count == jobId);
                Assert.AreEqual("success", job.State);
                Assert.IsTrue(job.Submit > job.Create);
                Assert.IsTrue(job.Setup > job.Submit);
                Assert.IsTrue(job.Running > job.Setup);
                Assert.IsTrue(job.Finished > job.Running);
            }

            return sessionId;
             */
        }

        /// <summary>
        /// Remove all Jobs, then create new session with a job and run through
        /// consumer job contract.  Start, Stop, Start.
        /// </summary>
        [TestMethod]
        public void SessionResourceReStartTest()
        {
            int count = 1;
            ISessionResource session = new MockSessionResource();
            var sessionId = _SessionResourceCreateTest();

            Assert.AreEqual(count, session.StartSession(sessionId.ToString()));

            byte[] dataArray = null;
            using (Stream s = session.StatusSession(sessionId.ToString()))
            {
                using (var ms = new MemoryStream())
                {
                    s.CopyTo(ms);
                    dataArray = ms.ToArray();
                }
            }

            string contents = Encoding.UTF8.GetString(dataArray);
            Debug.WriteLine("Session Status: " + contents);
            SessionStatusDict statusDict = JsonConvert.DeserializeObject<SessionStatusDict>(contents);

            Assert.AreEqual(0, statusDict.Create);
            Assert.AreEqual(1, statusDict.Submit);
            Assert.AreEqual(0, statusDict.Setup);
            Assert.AreEqual(0, statusDict.Running);
            Assert.AreEqual(0, statusDict.Success);
            Assert.AreEqual(0, statusDict.Error);
            Assert.AreEqual(0, statusDict.Cancel);
            Assert.AreEqual(0, statusDict.Terminate);
            Assert.AreEqual(0, statusDict.Pause);
            Assert.AreEqual(0, statusDict.Locked);

            int jobId;
            DateTime? submitDateTime = null;
            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                var job = db.Jobs.Single(j => j.State == "submit");
                Assert.AreEqual("submit", job.State);
                Assert.IsTrue(job.Submit > job.Create);
                Assert.IsNull(job.Setup);
                Assert.IsNull(job.Running);
                Assert.IsNull(job.Finished);
                jobId = job.Count;
                submitDateTime = job.Submit;
            }

            // Stop Session

            Assert.AreEqual(count, session.StopSession(sessionId.ToString()));

            dataArray = null;
            using (Stream s = session.StatusSession(sessionId.ToString()))
            {
                using (var ms = new MemoryStream())
                {
                    s.CopyTo(ms);
                    dataArray = ms.ToArray();
                }
            }

            contents = Encoding.UTF8.GetString(dataArray);
            Debug.WriteLine("Session Status: " + contents);
            statusDict = JsonConvert.DeserializeObject<SessionStatusDict>(contents);

            Assert.AreEqual(0, statusDict.Create);
            Assert.AreEqual(0, statusDict.Submit);
            Assert.AreEqual(0, statusDict.Setup);
            Assert.AreEqual(0, statusDict.Running);
            Assert.AreEqual(0, statusDict.Success);
            Assert.AreEqual(0, statusDict.Error);
            Assert.AreEqual(0, statusDict.Cancel);
            Assert.AreEqual(0, statusDict.Terminate);
            Assert.AreEqual(1, statusDict.Pause);
            Assert.AreEqual(0, statusDict.Locked);


            // Start Consumer
            IConsumerRegistrationContract cc = Turbine.Consumer.AppUtility.GetConsumerRegistrationContract();
            IConsumerRun run = Turbine.Consumer.AppUtility.GetConsumerRunContract();
            Assert.IsNull(cc.Queue);
            cc.Register(run);
            Assert.IsNotNull(cc.Queue);

            IJobConsumerContract contract = cc.Queue.GetNext(run);
            Assert.IsNull(contract);

            // Start Session

            Assert.AreEqual(count, session.StartSession(sessionId.ToString()));

            dataArray = null;
            using (Stream s = session.StatusSession(sessionId.ToString()))
            {
                using (var ms = new MemoryStream())
                {
                    s.CopyTo(ms);
                    dataArray = ms.ToArray();
                }
            }

            contents = Encoding.UTF8.GetString(dataArray);
            Debug.WriteLine("Session Status: " + contents);
            statusDict = JsonConvert.DeserializeObject<SessionStatusDict>(contents);

            Assert.AreEqual(0, statusDict.Create);
            Assert.AreEqual(1, statusDict.Submit);
            Assert.AreEqual(0, statusDict.Setup);
            Assert.AreEqual(0, statusDict.Running);
            Assert.AreEqual(0, statusDict.Success);
            Assert.AreEqual(0, statusDict.Error);
            Assert.AreEqual(0, statusDict.Cancel);
            Assert.AreEqual(0, statusDict.Terminate);
            Assert.AreEqual(0, statusDict.Pause);
            Assert.AreEqual(0, statusDict.Locked);



            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                var job = db.Jobs.Single(j => j.State == "submit");
                Assert.AreEqual("submit", job.State);
                Assert.IsTrue(job.Submit > job.Create);
                Assert.IsTrue(job.Submit > submitDateTime);
                Assert.IsTrue(submitDateTime > job.Create);
                Assert.IsNull(job.Setup);
                Assert.IsNull(job.Running);
                Assert.IsNull(job.Finished);
                jobId = job.Count;
            }
            contract = cc.Queue.GetNext(run);
            Assert.IsNotNull(contract);
        }

        /// <summary>
        /// Remove all Jobs, then create new session with a job and run through
        /// consumer job contract
        /// </summary>
        [TestMethod]
        public void SessionResourceDeleteTest()
        {
            var sessionId = _SessionResourceStateSuccessTest();
            var jobList = new List<Guid>();
            ISessionResource resource = new MockSessionResource();

            //resource.StartSession(sessionId.ToString());

            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                var session = db.Sessions.FirstOrDefault(i => i.Id == sessionId);
                Assert.IsNotNull(session);
                Assert.IsNotNull(db.Jobs.FirstOrDefault(i => i.SessionId == sessionId));
                foreach (var job in session.Jobs)
                {
                    Guid id = job.Id;
                    jobList.Add(id);
                    Assert.IsNotNull(db.Jobs.FirstOrDefault(i => i.Id == id));
                    Assert.IsNotNull(db.Processes.FirstOrDefault(m => m.Id == id));
                    Assert.IsNotNull(db.Messages.FirstOrDefault(m => m.JobId == id));
                    //Assert.IsNotNull(db.StagedInputFiles.FirstOrDefault(m => m.jobId == id));
                    //Assert.IsNotNull(db.StagedOutputFiles.FirstOrDefault(m => m.jobId == id));
                }
            }

            resource.DeleteSession(sessionId.ToString());

            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                Assert.IsNull(db.Sessions.FirstOrDefault(i => i.Id == sessionId));
                Assert.IsNull(db.Jobs.FirstOrDefault(i => i.SessionId == sessionId));
                foreach (var id in jobList)
                {
                    Assert.IsNull(db.Jobs.FirstOrDefault(i => i.Id == id));
                    Assert.IsNull(db.Processes.FirstOrDefault(m => m.Id == id));
                    Assert.IsNull(db.Messages.FirstOrDefault(m => m.JobId == id));
                    Assert.IsNull(db.StagedInputFiles.FirstOrDefault(m => m.jobId == id));
                    Assert.IsNull(db.StagedOutputFiles.FirstOrDefault(m => m.jobId == id));
                }
            }
        }
    }
}
