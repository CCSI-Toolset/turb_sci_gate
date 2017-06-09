using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Turbine.Consumer.Contract.Behaviors;
using Turbine.Consumer.Data.Contract.Behaviors;
using Turbine.Producer.Data.Contract.Behaviors;
using Turbine.Producer.Data.Contract;
using Turbine.Data.Contract;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace ConsumerDataIntegrationTests
{
    /// <summary>
    /// To get MSTest to copy assemblies must all be used in test code,
    /// will not copy assemblies that are just referenced which is a problem for DIJ.
    /// (See: http://www.dotnetthoughts.net/mstest-exe-does-not-deploy-all-items/).
    /// </summary>
    class MSTestHack
    {
        Turbine.Consumer.Data.Contract.AspenJobConsumerContract ajcc = null;
        Turbine.Consumer.Data.Contract.ConsumerRegistrationContract crc = null;
        Turbine.Consumer.Data.Contract.DBJobQueue dbjq = null;
    }

    /// <summary>
    /// SinterHelperFunctions just for pulling information from a configuration file
    /// </summary>
    class SinterHelperFunctions
    {
        public static String getBackupFilename(String configData)
        {
            try
            {
                Dictionary<String, Object> jsonConfig = JsonConvert.DeserializeObject<Dictionary<String, Object>>(configData);
                return (String)jsonConfig["aspenfile"];
            }
            catch (Newtonsoft.Json.JsonReaderException ex)
            {
                //Do Nothing
            }

            //If trying to parse JSON threw an exception, it must be the old file type               
            String[] lines = configData.Split(Environment.NewLine.ToCharArray());
            foreach (String line in lines)
            {
                String[] tokens = line.Split('|');
                if (tokens.Length != 2)
                {
                    continue;
                }
                if (tokens[0].Trim().CompareTo("file") == 0)
                {
                    return tokens[1].Trim();
                }

            }
            throw new System.IO.IOException(string.Format("Sinter Config Filetype unknown, No File| block found in sinter configuration file."));
        }
    }

    class TestProducerContext : Turbine.Producer.Contracts.IProducerContext
    {
        private string x = "josh";
        public string UserName
        {
            get
            {
                //var name = "fakeusername";
                var name = x;
                x += "1";
                using (var container = new Turbine.Data.TurbineModelContainer())
                {
                    var u = container.Users.SingleOrDefault<Turbine.Data.User>(s => s.Name == name);
                    if (u == null)
                    {
                        container.Users.AddObject(new Turbine.Data.User()
                        {
                            Name = name,
                            Token = ""
                        });
                        container.SaveChanges();
                    }
                }
                return name;
            }
        }
    }

    class ConsoleContext : IContext
    {
        public string BaseWorkingDirectory
        {
            get { return ""; }
        }
    }

    class ConsumerContext : IConsumerContext
    {
        private static string hostname = System.Net.Dns.GetHostName();
        internal static Guid id = Guid.NewGuid();
        public Guid Id
        {
            get
            {
                Debug.WriteLine(String.Format("ConsumerContext: GUID {0}", id), this.GetType().Name);
                return id;
            }
        }
        public string Hostname
        {
            get
            {
                return hostname;
            }
        }
    }

    [TestClass]
    public class ConsumerDataContractTest
    {
        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            Debug.WriteLine("Assembly Init", "ConsumerDataContractTest");

        }

        [TestInitialize()]
        public void MyTestInitialize()
        {
            Debug.WriteLine("Initialize", this.GetType().Name);
            ConsumerContext.id = Guid.NewGuid();
            using (var container = new Turbine.Data.TurbineModelContainer())
            {
                container.Messages.ToList().ForEach(a => container.DeleteObject(a));
                container.Jobs.ToList().ForEach(m => container.DeleteObject(m));
                container.SinterProcesses.ToList().ForEach(s => container.DeleteObject(s));
                container.Simulations.ToList().ForEach(n => container.DeleteObject(n));
                container.Applications.ToList().ForEach(i => container.DeleteObject(i));
                container.InputFileTypes.ToList().ForEach(j => container.DeleteObject(j));
                container.StagedInputFiles.ToList().ForEach(k => container.DeleteObject(k));
                container.SimulationStagedInputs.ToList().ForEach(l => container.DeleteObject(l));

                container.SaveChanges();

            }
            // Must Sleep so Timestamps are different
            var app = ApplicationProducerContract.Create("AspenPlus", "0.1"); System.Threading.Thread.Sleep(10);
            app.UpdateInputFileType("aspenfile", true, "plain/text"); System.Threading.Thread.Sleep(10);
            app.UpdateInputFileType("configuration", true, "plain/text"); System.Threading.Thread.Sleep(10);

            app = ApplicationProducerContract.Create("ACM", "0.1"); System.Threading.Thread.Sleep(10);
            app.UpdateInputFileType("aclm", true, "plain/text"); System.Threading.Thread.Sleep(10);
            app.UpdateInputFileType("configuration", true, "plain/text"); System.Threading.Thread.Sleep(10);

            app = ApplicationProducerContract.Create("gProms", "0.1"); System.Threading.Thread.Sleep(10);
            app.UpdateInputFileType("gproms", true, "plain/text"); System.Threading.Thread.Sleep(10);
            app.UpdateInputFileType("configuration", true, "plain/text"); System.Threading.Thread.Sleep(10);

            app = ApplicationProducerContract.Create("excel", "0.1"); System.Threading.Thread.Sleep(10);
            app.UpdateInputFileType("excel", true, "plain/text"); System.Threading.Thread.Sleep(10);
            app.UpdateInputFileType("configuration", true, "plain/text"); 
        }

        [TestMethod]
        public void TestConsumerRegistrationContract()
        {
            // NHibernate
            IConsumerRegistrationContract contract = Turbine.Consumer.AppUtility.GetConsumerRegistrationContract();
            contract.Register();

            Guid guid = Turbine.Consumer.AppUtility.GetConsumerContext().Id;

            // EF 
            Turbine.Data.Serialize.Consumer obj =
                Turbine.Data.Marshal.DataMarshal.GetConsumer(guid);

            Assert.AreEqual<string>(guid.ToString(), obj.Id);
            Assert.AreEqual<string>("up", obj.status);

            contract.UnRegister();
            obj = Turbine.Data.Marshal.DataMarshal.GetConsumer(guid);
            Assert.AreEqual<string>("down", obj.status);

        }

        [TestMethod]
        [DeploymentItem(@"models\mea\mea-sinter.txt")]
        [DeploymentItem(@"models\mea\mea.bkp")]
        public void TestSimulationProducerContract()
        {
            var app = ApplicationProducerContract.Get("AspenPlus");
            Assert.IsNotNull(app);

            ISimulationProducerContract simulation_contract = AspenSimulationContract.Create("test", "AspenPlus");
            Assert.IsNotNull(simulation_contract);

            byte[] data;
            using (var fstream = File.Open("mea.bkp", FileMode.Open))
            {
                using (var ms = new MemoryStream())
                {
                    fstream.CopyTo(ms);
                    data = ms.ToArray();
                }
            }

            System.Threading.Thread.Sleep(10);
            simulation_contract.UpdateInput("aspenfile", data, "plain/text");
            data = null;
            using (var fstream = File.Open("mea-sinter.txt", FileMode.Open))
            {
                using (var ms = new MemoryStream())
                {
                    fstream.CopyTo(ms);
                    data = ms.ToArray();
                }
            }

            System.Threading.Thread.Sleep(10);
            simulation_contract.UpdateInput("configuration", data, "plain/text");

        }

        [TestMethod]
        [ExpectedException(typeof(IllegalAccessException),
            "Test should fail because Consumer was UnRegistered")]
        public void TestJobProducerConsumerContractsSetupFailUnRegisterConsumer()
        {
            // Create test Simulation
            TestSimulationProducerContract();

            ISimulationProducerContract simulation_contract = AspenSimulationContract.Get("test");
            ISessionProducerContract producer = new AspenSessionProducerContract();
            Guid session_id = producer.Create();
            IJobProducerContract job_producer_contract = simulation_contract.NewJob(session_id, false, true);

            // consumers
            /*
            IJobQueue queue = AppUtility.GetJobQueue();
            IJobConsumerContract job = queue.GetNext();
            Assert.IsNull(job);
            job_producer_contract.Submit();
            job = queue.GetNext();
            Assert.IsNotNull(job);
            */
            // NHibernate
            IConsumerRegistrationContract contract = Turbine.Consumer.AppUtility.GetConsumerRegistrationContract();
            Assert.IsNull(contract.Queue);
            contract.Register();
            Assert.IsNotNull(contract.Queue);
            Assert.IsNull(contract.Queue.GetNext());  // nothing on queue

            job_producer_contract.Submit();
            IJobConsumerContract job = contract.Queue.GetNext();
            Assert.IsNotNull(job);
            contract.UnRegister();

            job.Setup();

            SimpleFile backup = null;
            SimpleFile config = null;
            foreach (var f in job.GetSimulationInputFiles())
            {
                if (f.name == "configuration")
                    config = f;
                else
                    backup = f;
            }

            Assert.AreEqual(config.name, "configuration");

            String filename = SinterHelperFunctions
                .getBackupFilename(System.Text.Encoding.ASCII.GetString(config.content));
            Assert.AreEqual(backup.name, "");

            //Assert(, 
            //IJobConsumerContract consumer = AppUtility.GetJobConsumerContract(id);
        }

        [TestMethod]
        [DeploymentItem(@"models/mea/mea-sinter.txt")]
        [DeploymentItem(@"models/mea/mea.bkp")]
        public void TestJobProducerConsumerContracts()
        {
            // producers
            ISessionProducerContract producer = new AspenSessionProducerContract();
            Guid session_id = producer.Create();
            Assert.IsNotNull(session_id);

            ISimulationProducerContract simulation_contract;
            simulation_contract = AspenSimulationContract.Create("test", "AspenPlus");

            Assert.IsTrue(File.Exists("mea-sinter.txt"));
            Assert.IsTrue(File.Exists("mea.bkp"));

            var configContent = File.ReadAllBytes("mea-sinter.txt");
            var aspenfileContent = File.ReadAllBytes("mea.bkp");

            Assert.IsTrue(configContent.Length > 0);
            Assert.IsTrue(aspenfileContent.Length > 0);

            System.Threading.Thread.Sleep(100);
            simulation_contract.UpdateInput("configuration",
                configContent,
                "plain/text");
            System.Threading.Thread.Sleep(1000);
            simulation_contract.UpdateInput("aspenfile",
                aspenfileContent,
                "plain/text");

            IJobProducerContract job_producer_contract = simulation_contract.NewJob(session_id, false, true);

            // consumers
            //IJobQueue queue = AppUtility.GetJobQueue();
            // NHibernate
            IConsumerRegistrationContract contract = Turbine.Consumer.AppUtility.GetConsumerRegistrationContract();
            contract.Register();
            IJobConsumerContract job = contract.Queue.GetNext();
            Assert.IsNull(job);
            job_producer_contract.Submit();
            job = contract.Queue.GetNext();
            Assert.IsNotNull(job);

            job.Setup();

            IEnumerable<SimpleFile> files = job.GetSimulationInputFiles();
            Assert.AreEqual<int>(files.Count<SimpleFile>(), 2);
            foreach (var f in files)
            {
                Assert.IsNotNull(f);
                Assert.IsNotNull(f.name);
                Debug.WriteLine("File: " + f.name, this.GetType().Name);
                Assert.IsNotNull(f.content);
                
                Assert.IsTrue(f.content.Length > 0);

                if (f.name == "configuration")
                {
                    Assert.AreEqual<int>(f.content.Length, configContent.Length);
                }
                else if (f.name == "aspenfile")
                {
                    Assert.AreEqual<int>(f.content.Length, aspenfileContent.Length);
                }
                else
                {
                    Assert.Fail(String.Format("Unknown File {0}", f.name));
                }
            }
            //byte[] backup = job.GetSimulationBackup();
            //byte[] config = job.GetSimulationConfiguration();

            //Assert(, 
            //IJobConsumerContract consumer = AppUtility.GetJobConsumerContract(id);
        }

        [TestMethod]
        public void TestSimulationProducerConsumerContracts()
        {
            var simulation_name = "TestSimulationProducerConsumerContracts";
            var input_name = "configuration"; 
            ISimulationProducerContract contract = AspenSimulationContract.Create(simulation_name, "ACM");
            using (var container = new Turbine.Data.TurbineModelContainer())
            {
                var obj = container.Simulations.Single<Turbine.Data.Simulation>(s => s.Name == simulation_name);
                var input = obj.SimulationStagedInputs.Single<Turbine.Data.SimulationStagedInput>(i => i.Name == input_name);
                Assert.IsNull(input.Hash);
                Assert.IsNull(input.Content);
                Debug.WriteLine("SimulationStagedInput: " + input.Name, this.GetType().Name);

                input_name = "aclm";
                input = obj.SimulationStagedInputs.Single<Turbine.Data.SimulationStagedInput>(i => i.Name == input_name);
                Assert.IsNull(input.Hash);
                Assert.IsNull(input.Content);
                Debug.WriteLine("SimulationStagedInput: " + input.Name, this.GetType().Name);
            }

            input_name = "configuration";
            contract = AspenSimulationContract.Get(simulation_name);
            byte[] data = Encoding.UTF8.GetBytes("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            var provider = System.Security.Cryptography.MD5CryptoServiceProvider.Create();
            byte[] hash = provider.ComputeHash(data);
            var sb = new StringBuilder();
            foreach (byte b in hash)
                sb.Append(b.ToString("X2"));
            string hval = sb.ToString();

            var content_type = "plain/text";
            bool success = contract.UpdateInput(input_name, data, content_type);

            using (var container = new Turbine.Data.TurbineModelContainer())
            {
                var obj = container.Simulations.OrderByDescending(q => q.Create).First<Turbine.Data.Simulation>(s => s.Name == simulation_name);
                var input = obj.SimulationStagedInputs.Single<Turbine.Data.SimulationStagedInput>(i => i.Name == input_name);
                Debug.WriteLine(String.Format("SimulationStagedInput: {0}, {1}, {2}", input.Name, input.Hash, Encoding.UTF8.GetString(input.Content)), 
                    this.GetType().Name);
                Assert.AreEqual(input.Hash, hval);
                Assert.AreEqual(Encoding.UTF8.GetString(input.Content), "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
                
            }
        }

        [TestMethod]
        [DeploymentItem(@"models/mea/mea-sinter.txt")]
        [DeploymentItem(@"models/mea/mea.bkp")]
        public void TestSessionProducerConsumerContracts()
        {
            // producers
            ISessionProducerContract producer = new AspenSessionProducerContract();
            Guid session_id = producer.Create();
            Assert.IsNotNull(session_id);

            ISimulationProducerContract simulation_contract = AspenSimulationContract.Create("testX", "AspenPlus");

            var bytes = File.ReadAllBytes("mea-sinter.txt");
            Assert.IsNotNull(bytes);
            Assert.IsTrue(bytes.Length > 0);
            System.Threading.Thread.Sleep(100);
            simulation_contract.UpdateInput("configuration", bytes, "plain/text");

            bytes = File.ReadAllBytes("mea.bkp");
            Assert.IsNotNull(bytes);
            Assert.IsTrue(bytes.Length > 0);
            System.Threading.Thread.Sleep(100);
            simulation_contract.UpdateInput("aspenfile", bytes, "plain/text");

            IJobProducerContract job_producer_contract = simulation_contract.NewJob(session_id, false, true);
            job_producer_contract = simulation_contract.NewJob(session_id, false, true);

            IConsumerRegistrationContract contract = Turbine.Consumer.AppUtility.GetConsumerRegistrationContract();
            contract.Register();

            IJobConsumerContract job = contract.Queue.GetNext();
            Assert.IsNull(job);
            job_producer_contract.Submit();
            job = contract.Queue.GetNext();
            Assert.IsNotNull(job);
            Assert.AreEqual(job.Id, job_producer_contract.Id);


            job.Setup();

            SimpleFile backup = null;
            SimpleFile config = null;
            foreach (var f in job.GetSimulationInputFiles())
            {
                if (f.name == "configuration")
                    config = f;
                else
                    backup = f;
            }

            Assert.AreEqual(config.name, "configuration");

            String filename = SinterHelperFunctions
                .getBackupFilename(System.Text.Encoding.ASCII.GetString(config.content));
            Assert.AreEqual(backup.name, "aspenfile");


            // NEED TO SET INPUT Before Setting to Run
            //j
            try
            {
                job.Running();
                Assert.Fail("Job.Process.Input Must be set before setting job to run");
            }
            catch (InvalidStateChangeException) {}
            job.Process.Input = new Dictionary<string, Object>();
            job.Running();
            job.Success();

            job = contract.Queue.GetNext();
            Assert.IsNull(job);

        }

        /*
        [TestMethod]
        public void TestJobQueue()
        {
            IJobQueue queue = AppUtility.GetJobQueue();
            IJobConsumerContract job = queue.GetNext();
            Assert.IsNull(job);
        }

        [TestMethod]
        [ExpectedException(typeof(IllegalAccessException),
            "Test should fail because Consumer was not registered")]
        public void TestJobProducerConsumerContractsSetupFailNoConsumer()
        {
            // Create test Simulation
            TestSimulationProducerContract();

            // producers
            ISessionProducerContract producer = new AspenSessionProducerContract();
            Guid session_id = producer.Create();
            Assert.IsNotNull(session_id);

            ISimulationProducerContract simulation_contract = AspenSimulationContract.Get("test");
            IJobProducerContract job_producer_contract = simulation_contract.NewJob(session_id, false, true);

            // consumers
            IJobQueue queue = AppUtility.GetJobQueue();
            IJobConsumerContract job = queue.GetNext();
            Assert.IsNull(job);
            job_producer_contract.Submit();
            job = queue.GetNext();
            Assert.IsNotNull(job);
            try
            {
                job.Setup();
            }
            catch (IllegalAccessException)
            {
                throw;
            }
        }
        */
    }
}
