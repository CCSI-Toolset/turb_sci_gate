using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using Turbine.Producer.Data.Contract;
using Turbine.Consumer.Data.Contract.Behaviors;
using Turbine.Producer.Data.Contract.Behaviors;
using System.IO;

namespace Turbine.Test.Sinter.Excel
{
    class ConsumerContext : Turbine.Consumer.Contract.Behaviors.IConsumerContext
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

    class Utility
    {
        internal static ISimulationProducerContract CreateExcelSimulation(String spreadsheet, String sinter)
        {
            var app = ApplicationProducerContract.Get("Excel");
            Assert.IsNotNull(app);

            ISimulationProducerContract simulation_contract = AspenSimulationContract.Create("test", "Excel");
            Assert.IsNotNull(simulation_contract);

            byte[] data;
            using (var fstream = File.Open(spreadsheet, FileMode.Open))
            {
                using (var ms = new MemoryStream())
                {
                    fstream.CopyTo(ms);
                    data = ms.ToArray();
                }
            }

            System.Threading.Thread.Sleep(10);
            simulation_contract.UpdateInput("spreadsheet", data, "plain/text");
            data = null;
            using (var fstream = File.Open(sinter, FileMode.Open))
            {
                using (var ms = new MemoryStream())
                {
                    fstream.CopyTo(ms);
                    data = ms.ToArray();
                }
            }
            System.Threading.Thread.Sleep(10);
            simulation_contract.UpdateInput("configuration", data, "plain/text");

            return simulation_contract;
        }

        internal static void CheckBaseDirectory()
        {
            string dir = Turbine.Consumer.AppUtility.GetAppContext().BaseWorkingDirectory;
            if (!Directory.Exists(dir))
            {
                var msg = String.Format("EXIT: Base Directory does not exist: {0}", dir);
                Debug.WriteLine(msg);
                throw new FileNotFoundException(msg);
            }
            try
            {
                var acl = Directory.GetAccessControl(dir);
            }
            catch (UnauthorizedAccessException)
            {
                Debug.WriteLine(String.Format("EXIT: Base Directory is not writable: {0}", dir));
                throw;
            }
        }
    }
    [TestClass]
    public class ExcelSinterConsumerTests
    {
        // ..\..\..\..\Documents\models\BFB_v2\branches\BFB_Excel\BFBv5.2.3_new.xlsm
        // ..\..\..\..\Documents\models\BFB_v2\branches\BFB_Excel\BFBv5.2.3_excel.json
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
                container.JobConsumers.ToList().ForEach(l => container.DeleteObject(l));
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
            app.UpdateInputFileType("spreadsheet", true, "plain/text"); System.Threading.Thread.Sleep(10);
            app.UpdateInputFileType("configuration", true, "plain/text");
        }

        [TestMethod]
        [DeploymentItem(@"models/SimpleExcelTest/exceltest.xlsm")]
        [DeploymentItem(@"models/SimpleExcelTest/exceltest-sinter.json")]
        public void TestSimulationProducerContract()
        {
            var app = ApplicationProducerContract.Get("Excel");
            Assert.IsNotNull(app);

            ISimulationProducerContract simulation_contract = AspenSimulationContract.Create("test", "Excel");
            Assert.IsNotNull(simulation_contract);

            byte[] data;
            using (var fstream = File.Open("exceltest.xlsm", FileMode.Open))
            {
                using (var ms = new MemoryStream())
                {
                    fstream.CopyTo(ms);
                    data = ms.ToArray();
                }
            }

            System.Threading.Thread.Sleep(10);
            simulation_contract.UpdateInput("spreadsheet", data, "plain/text");
            data = null;
            using (var fstream = File.Open("exceltest-sinter.json", FileMode.Open))
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
        public void TestRunNoJobs()
        {
            IConsumerRegistrationContract contract = Turbine.Consumer.AppUtility.GetConsumerRegistrationContract();
            Assert.IsNull(contract.Queue);
            contract.Register();

            Turbine.Consumer.Contract.SinterConsumer consumer = new Turbine.Consumer.Excel.ExcelSinterConsumer();
            
            consumer.Run();
        }

        [TestMethod]
        [DeploymentItem(@"models/SimpleExcelTest/exceltest.xlsm")]
        [DeploymentItem(@"models/SimpleExcelTest/exceltest-sinter.json")]
        public void TestDBJobQueue()
        {

            ISimulationProducerContract simulation_contract = Utility.CreateExcelSimulation("exceltest.xlsm", "exceltest-sinter.json");

            var producerJob = simulation_contract.NewJob(Guid.NewGuid(), false, false);
            producerJob.Submit();

            IConsumerRegistrationContract contract = Turbine.Consumer.AppUtility.GetConsumerRegistrationContract();
            //Assert.IsNull(contract.Queue);
            contract.Register();

            IJobQueue queue = new Turbine.Consumer.Data.Contract.DBJobQueue(new string[] {"excel"});
            var consumerJob = queue.GetNext();
            Assert.IsTrue(consumerJob.ApplicationName == "excel", 
                String.Format("Wrong Application {0}",consumerJob.ApplicationName));
            
        }

        [TestMethod]
        [DeploymentItem(@"models/SimpleExcelTest/exceltest.xlsm")]
        [DeploymentItem(@"models/SimpleExcelTest/exceltest-sinter.json")]
        [ExpectedException(typeof(System.InvalidOperationException),
        "Test should fail because providing application with incorrect parameter for input file")]
        public void TestWrongParameter()
        {
            var app = ApplicationProducerContract.Get("Excel");
            Assert.IsNotNull(app);

            ISimulationProducerContract simulation_contract = AspenSimulationContract.Create("test", "Excel");
            Assert.IsNotNull(simulation_contract);

            byte[] data;
            using (var fstream = File.Open("exceltest.xlsm", FileMode.Open))
            {
                using (var ms = new MemoryStream())
                {
                    fstream.CopyTo(ms);
                    data = ms.ToArray();
                }
            }
            Assert.IsTrue(data.Length > 0);
            System.Threading.Thread.Sleep(10);
            simulation_contract.UpdateInput("wrongparameter", data, "plain/text");
        }

        [TestMethod]
        [DeploymentItem(@"models/SimpleExcelTest/exceltest.xlsm")]
        [DeploymentItem(@"models/SimpleExcelTest/exceltest-sinter.json")]
        public void TestExcelSpreadsheet1()
        {
            var app = ApplicationProducerContract.Get("Excel");
            Assert.IsNotNull(app);

            ISimulationProducerContract simulation_contract = AspenSimulationContract.Create("test", "Excel");
            Assert.IsNotNull(simulation_contract);

            byte[] data;
            using (var fstream = File.Open(@"exceltest.xlsm", FileMode.Open))
            {
                using (var ms = new MemoryStream())
                {
                    fstream.CopyTo(ms);
                    data = ms.ToArray();
                }
            }
            Assert.IsTrue(data.Length > 0);
            System.Threading.Thread.Sleep(10);
            simulation_contract.UpdateInput("spreadsheet", data, "plain/text");
            data = null;
            using (var fstream = File.Open("exceltest-sinter.json", FileMode.Open))
            {
                using (var ms = new MemoryStream())
                {
                    fstream.CopyTo(ms);
                    data = ms.ToArray();
                }
            }
            Assert.IsTrue(data.Length > 0);
            System.Threading.Thread.Sleep(10);
            simulation_contract.UpdateInput("configuration", data, "plain/text");
            System.Threading.Thread.Sleep(10);

            var guid = Guid.NewGuid();
            var job_producer_contract = simulation_contract.NewJob(guid, false, false);
            job_producer_contract.Process.Input = new Dictionary<string, Object>() { };
            job_producer_contract.Submit();
            System.Threading.Thread.Sleep(10);

            IConsumerRegistrationContract contract = Turbine.Consumer.AppUtility.GetConsumerRegistrationContract();
            Assert.IsNull(contract.Queue);
            contract.Register();

            Turbine.Consumer.Contract.SinterConsumer consumer = new Turbine.Consumer.Excel.ExcelSinterConsumer();

            //Utility.CheckBaseDirectory();

            consumer.Run();

            int jobID = job_producer_contract.Id;

            Debug.WriteLine("Job GUID" + guid.ToString(), GetType().Name);
            Dictionary<string, Object> json = null;
            using (Turbine.Data.TurbineModelContainer container = new Turbine.Data.TurbineModelContainer())
            {
                Turbine.Data.Job entity = container.Jobs.Single(s => s.Id == job_producer_contract.Id);
                json = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Object>>(entity.Process.Output);
            }

            string output = Newtonsoft.Json.JsonConvert.SerializeObject(json);
            Debug.WriteLine("OUTPUT: " + output, GetType().Name);
        }

        /*
        [TestMethod]
        [DeploymentItem(@"exceltest.xlsm")]
        [DeploymentItem(@"exceltest-sinter.json")]
        public void TestExcelSpreadsheet2()
        {
            var app = ApplicationProducerContract.Get("Excel");
            Assert.IsNotNull(app);

            ISimulationProducerContract simulation_contract = AspenSimulationContract.Create("test", "Excel");
            Assert.IsNotNull(simulation_contract);

            byte[] data;
            using (var fstream = File.Open(ConsumerDataIntegrationTests.Properties.Settings.Default.excelSpreadsheet2, FileMode.Open))
            {
                using (var ms = new MemoryStream())
                {
                    fstream.CopyTo(ms);
                    data = ms.ToArray();
                }
            }
            Assert.IsTrue(data.Length > 0);
            System.Threading.Thread.Sleep(10);
            simulation_contract.UpdateInput("spreadsheet", data, "plain/text");
            data = null;
            using (var fstream = File.Open(ConsumerDataIntegrationTests.Properties.Settings.Default.excelSinter2, FileMode.Open))
            {
                using (var ms = new MemoryStream())
                {
                    fstream.CopyTo(ms);
                    data = ms.ToArray();
                }
            }
            Assert.IsTrue(data.Length > 0);
            System.Threading.Thread.Sleep(10);
            simulation_contract.UpdateInput("configuration", data, "plain/text");
            System.Threading.Thread.Sleep(10);

            var guid = Guid.NewGuid();
            var job_producer_contract = simulation_contract.NewJob(guid, false, false);
            job_producer_contract.Process.Input = new Dictionary<string, Object>() { };
            job_producer_contract.Submit();
            System.Threading.Thread.Sleep(10);

            IConsumerRegistrationContract contract = Turbine.Consumer.AppUtility.GetConsumerRegistrationContract();
            Assert.IsNull(contract.Queue);
            contract.Register();

            Turbine.Consumer.Contract.SinterConsumer consumer = new Turbine.Consumer.Excel.ExcelSinterConsumer();

            //Utility.CheckBaseDirectory();

            consumer.Run();

            int jobID = job_producer_contract.Id;

            Debug.WriteLine("Job GUID" + guid.ToString(), GetType().Name);
            Dictionary<string, Object> json = null;
            using (Turbine.Data.TurbineModelContainer container = new Turbine.Data.TurbineModelContainer())
            {
                Turbine.Data.Job entity = container.Jobs.Single(s => s.Id == job_producer_contract.Id);
                json = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Object>>(entity.Process.Output);
            }

            string output = Newtonsoft.Json.JsonConvert.SerializeObject(json);
            Debug.WriteLine("OUTPUT: " + output, GetType().Name);

            Newtonsoft.Json.Linq.JObject obj = (Newtonsoft.Json.Linq.JObject)json["B49"];
            Assert.AreEqual(obj["value"], 126.08761416126971, "output B49 is incorrect");

            obj = (Newtonsoft.Json.Linq.JObject)json["status"];
            Assert.AreEqual(obj["value"], 0, "output status is incorrect");
        }
         */
    }
}