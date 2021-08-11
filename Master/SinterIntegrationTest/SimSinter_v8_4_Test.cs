using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Turbine.Consumer.Contract.Behaviors;
using Turbine.Consumer.Data.Contract.Behaviors;
using Turbine.Data.Contract.Behaviors;
using Turbine.Consumer.SimSinter;
using Turbine.Consumer;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using sinter;
using System.IO;
using Newtonsoft.Json;
using System.Threading;
using System.Text;
using Microsoft.VisualBasic;


namespace SinterIntegrationTest
{
    [TestClass]
    [DeploymentItem(@"models\Hybrid_v0.51_rev1.1_UQ_0809_sinter.json", @"models")]
    [DeploymentItem(@"models\Hybrid_v0.51_rev1.1_UQ_0809.acmf", @"models")]
    [DeploymentItem(@"models\VdV_Reactor\VdV_Reactor_Dynamic_Config.json", @"models")]
    [DeploymentItem(@"models\VdV_Reactor\VdV_Reactor.acmf", @"models")]
    [DeploymentItem(@"models\BFB_Cap\BFB_OUU_COE_opt.json", @"models")]
    [DeploymentItem(@"models\BFB_Cap\BFB_OUU_COE_sim.json", @"models")]
    [DeploymentItem(@"models\BFB_Cap\BFB_OUU_COE.acmf", @"models")]
    [DeploymentItem(@"models\BFBv6_3_new\BFBv6_3_new.json", @"models")]
    [DeploymentItem(@"models\BFBv6_3_new\BFB_v6.3_Process_SS_total_opt_no_trim_mod.acmf", @"models")]
    [DeploymentItem(@"models\BufferTank_FO\BufferTank_FO.gPJ", @"models")]
    [DeploymentItem(@"models\BufferTank_FO\BufferTank_FO.json", @"models")]
    [DeploymentItem(@"models\BufferTank_FO\BufferTank_FO.gENCRYPT", @"models")]
    public class SimSinter_v8_4_Test
    {
        [TestMethod]
        public void TestAspenPlus()
        {
            IDictionary<string, object> output;
            IConsumerRun run = (IConsumerRun)new InMemorySinterConsumerAspenPlus();
            Assert.IsFalse(run.IsEngineRunning);
            Assert.IsFalse(run.IsSimulationInitializing);
            Assert.IsFalse(run.IsSimulationOpened);
            try
            {
                Assert.IsTrue(run.Run());
                Assert.IsTrue(run.IsSimulationOpened);

                IJobConsumerContract job = ((InMemorySinterConsumerAspenPlus)run).job;
                Assert.IsTrue(job.IsSuccess());
                output = job.Process.Output;
                Assert.IsNotNull(output);

                foreach (KeyValuePair<string, object> kv in output)
                {
                    Debug.WriteLine(String.Format("{0} : {1}", kv.Key, kv.Value));
                }
            }
            finally
            {
                Debug.WriteLine("Attempt Cleanup");
                run.CleanUp();
            }

            Object d;
            output.TryGetValue("status", out d);
            JObject statusD = (JObject)d;
            JToken status = statusD.GetValue("value");
            Assert.AreEqual<int>(status.Value<int>(), 0);
        }

        [TestMethod]
        public void TestACM()
        {
            IDictionary<string, object> output;
            IConsumerRun run = (IConsumerRun)new InMemorySinterConsumer();
            Assert.IsFalse(run.IsEngineRunning);
            Assert.IsFalse(run.IsSimulationInitializing);
            Assert.IsFalse(run.IsSimulationOpened);
            // set job before calling Run
            ((InMemorySinterConsumer)run).job = new InMemoryJobACM();
            Newtonsoft.Json.Linq.JObject inputsD = JObject.Parse("{\"UQ_ADS_db\":1.08}");
            Dictionary<String, Object> dd = new Dictionary<string, object>();
            foreach (var v in inputsD)
            {
                Debug.WriteLine("VALUE: " + v);
                dd[v.Key] = v.Value;
            }
            ((InMemorySinterConsumer)run).job.Process.Input = dd;
            try
            {
                Assert.IsTrue(run.Run());
                Assert.IsTrue(run.IsSimulationOpened);

                IJobConsumerContract job = ((InMemorySinterConsumer)run).job;
                Assert.IsTrue(job.IsSuccess());
                output = job.Process.Output;
                Assert.IsNotNull(output);

                foreach (KeyValuePair<string, object> kv in output)
                {
                    Debug.WriteLine(String.Format("{0} : {1}", kv.Key, kv.Value));
                }
            }
            finally
            {
                Debug.WriteLine("Attempt Cleanup");
                run.CleanUp();
            }

            Object d;
            output.TryGetValue("status", out d);
            JObject statusD = (JObject)d;
            JToken status = statusD.GetValue("value");
            Assert.AreEqual<int>(status.Value<int>(), 0);
        }

        [TestMethod]
        public void TestACM_SinterTermination()
        {
            IDictionary<string, object> output;
            IConsumerRun run = (IConsumerRun)new InMemorySinterConsumer();
            Assert.IsFalse(run.IsEngineRunning);
            Assert.IsFalse(run.IsSimulationInitializing);
            Assert.IsFalse(run.IsSimulationOpened);
            // set job before calling Run
            ((InMemorySinterConsumer)run).job = new InMemoryJobACM();
            try
            {
                Assert.IsTrue(run.Run());
                Assert.IsTrue(run.IsSimulationOpened);

                IJobConsumerContract job = ((InMemorySinterConsumer)run).job;
                Assert.IsTrue(job.IsSuccess());
                output = job.Process.Output;
                Assert.IsNotNull(output);

                foreach (KeyValuePair<string, object> kv in output)
                {
                    Debug.WriteLine(String.Format("{0} : {1}", kv.Key, kv.Value));
                }
            }
            finally
            {
                Debug.WriteLine("Attempt Cleanup");
                run.CleanUp();
            }

            Object d;
            output.TryGetValue("status", out d);
            JObject statusD = (JObject)d;
            JToken status = statusD.GetValue("value");
            Assert.AreEqual<int>(status.Value<int>(), 0);
        }


        [TestMethod]
        public void TestACMDynamic()
        {
            IDictionary<string, object> output;
            IConsumerRun run = (IConsumerRun)new InMemorySinterConsumer();
            Assert.IsFalse(run.IsEngineRunning);
            Assert.IsFalse(run.IsSimulationInitializing);
            Assert.IsFalse(run.IsSimulationOpened);
            // set job before calling Run
            ((InMemorySinterConsumer)run).job = new InMemoryJobACM_VdV_Dynamic();
            Newtonsoft.Json.Linq.JObject inputsD = JObject.Parse("{\"Ca_Feed\": [10.0,11.0,12.0,13.0],\"RunMode\": \"Dynamic\", \"TimeSeries\":[0.01,0.02,0.03,0.04]}");
            //Newtonsoft.Json.Linq.JObject inputsD = JObject.Parse("{\"Ca_Feed\": [10.0,10.0,10.0,10.0],\"RunMode\": \"Dynamic\", \"TimeSeries\":[0.01,0.02,0.03,0.04]}");
            //Newtonsoft.Json.Linq.JObject inputsD = JObject.Parse("{\"UQ_ADS_db\":1.08}");
            //Newtonsoft.Json.Linq.JObject inputsD = JObject.Parse("{\"Ca_Feed\":[10.0,10.0,10.0,10.0,10.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,10.5,10.5,10.5,10.5,10.5,10.5,10.5,10.5,10.5,10.5,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,10.5,10.5,10.5,10.5,10.5,10.5,10.5,10.5,10.5,10.5,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0],\"F_Feed\":[3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005],\"Volume\":[100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0],\"RunMode\":\"Dynamic\",\"TimeSeries\":[0.01,0.02,0.03,0.04,0.05,0.06,0.07,0.08,0.09,0.1,0.11,0.12,0.13,0.14,0.15,0.16,0.17,0.18,0.19,0.2,0.21,0.22,0.23,0.24,0.25,0.26,0.27,0.28,0.29,0.3,0.31,0.32,0.33,0.34,0.35000000000000003,0.36,0.37,0.38,0.39,0.4,0.41000000000000003,0.42,0.43,0.44,0.45,0.46,0.47000000000000003,0.48,0.49,0.5,0.51,0.52,0.53,0.54,0.55,0.56,0.57000000000000006,0.58,0.59,0.6,0.61,0.62,0.63,0.64,0.65,0.66,0.67,0.68,0.69000000000000006,0.70000000000000007,0.71,0.72,0.73,0.74,0.75,0.76,0.77,0.78,0.79,0.8,0.81,0.82000000000000006,0.83000000000000007,0.84,0.85,0.86,0.87,0.88,0.89,0.9,0.91,0.92,0.93,0.94000000000000006,0.95000000000000007,0.96,0.97,0.98,0.99,1.0,1.01,1.02,1.03,1.04,1.05,1.06,1.07,1.08,1.09,1.1,1.11,1.12,1.1300000000000001,1.1400000000000001,1.1500000000000001,1.16,1.17,1.18,1.19,1.2,1.21,1.22,1.23,1.24,1.25,1.26,1.27,1.28,1.29,1.3,1.31,1.32,1.33,1.34,1.35,1.36,1.37,1.3800000000000001,1.3900000000000001,1.4000000000000001,1.41,1.42,1.43,1.44,1.45,1.46,1.47,1.48,1.49,1.5,1.51,1.52,1.53,1.54,1.55,1.56,1.57,1.58,1.59,1.6,1.61,1.62,1.6300000000000001,1.6400000000000001,1.6500000000000001,1.6600000000000001,1.67,1.68,1.69,1.7,1.71,1.72,1.73,1.74,1.75,1.76,1.77,1.78,1.79,1.8,1.81,1.82,1.83,1.84,1.85,1.86,1.87,1.8800000000000001,1.8900000000000001,1.9000000000000001,1.9100000000000001,1.92,1.93,1.94,1.95,1.96,1.97,1.98,1.99,2.0,2.0100000000000002,2.02,2.0300000000000002,2.04,2.05,2.06,2.07,2.08,2.09,2.1,2.11,2.12,2.13,2.14,2.15,2.16,2.17,2.18,2.19,2.2,2.21,2.22,2.23,2.24,2.25],\"printlevel\":0,\"homotopy\":0}");
            Dictionary<String, Object> dd = new Dictionary<string, object>();
            foreach (var v in inputsD)
            {
                //Debug.WriteLine("VALUE: " + v);
                dd[v.Key] = v.Value;
            }
            ((InMemorySinterConsumer)run).job.Process.Input = dd;
            try
            {
                Assert.IsTrue(run.Run());
                Assert.IsTrue(run.IsSimulationOpened);

                IJobConsumerContract job = ((InMemorySinterConsumer)run).job;
                //Assert.IsTrue(job.IsSuccess());
                output = job.Process.Output;
                Assert.IsNotNull(output);

                foreach (KeyValuePair<string, object> kv in output)
                {
                    Debug.WriteLine(String.Format("{0} : {1}", kv.Key, kv.Value));
                }
            }
            finally
            {
                Debug.WriteLine("Attempt Cleanup");
                run.CleanUp();
            }

            Object d;
            output.TryGetValue("status", out d);
            JObject statusD = (JObject)d;
            JToken status = statusD.GetValue("value");
            Assert.AreEqual<int>(status.Value<int>(), 0);
        }

        [TestMethod]
        public void SinterDynamicACMTest()
        {
            var path = Properties.Settings.Default.DynamicVdVACMConfig;
            string cwd = Directory.GetCurrentDirectory();
            byte[] buffer = File.ReadAllBytes(path);
            var configuration = Encoding.UTF8.GetString(buffer);
            ISimulation sim = sinter_Factory.createSinter(configuration);
            Assert.IsInstanceOfType(sim, typeof(sinter_SimACM), "Expecing ACM sinter");

            Directory.CreateDirectory("testDynamicACM");
            sim.workingDir = Path.Combine(cwd, "testDynamicACM");
            string configFileName = Path.Combine(sim.workingDir, Properties.Settings.Default.DynamicVdVACMFilename);
            string config = System.Text.Encoding.ASCII.GetString(buffer);

            // Retrieve backup filename from sinter config file
            String filename = "VdV_Reactor.acmf";
            string shortfilename = System.IO.Path.GetFileNameWithoutExtension(filename);
            //string suffix = System.IO.Path.GetExtension(filename);

            Debug.WriteLine(String.Format("Simulation Backup Filename: {0}", filename), this.GetType());

            File.WriteAllBytes(configFileName, buffer);

            byte[] backupData = File.ReadAllBytes(Path.Combine(Properties.Settings.Default.DynamicVdVACMDir, filename));
            var backupFileName = Path.Combine(sim.workingDir, filename);
            File.WriteAllBytes(backupFileName, backupData);
            IDictionary<string, Object> myDict = null;

            try
            {
                sim.openSim();
                JObject inputDict = new JObject();
                //var inputJson = "{\"Ca_Feed\":[10.0,10.0,10.0,10.0,10.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,10.5,10.5,10.5,10.5,10.5,10.5,10.5,10.5,10.5,10.5,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,10.5,10.5,10.5,10.5,10.5,10.5,10.5,10.5,10.5,10.5,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0],\"F_Feed\":[3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005],\"Volume\":[100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0],\"RunMode\":\"Dynamic\",\"TimeSeries\":[0.01,0.02,0.03,0.04,0.05,0.06,0.07,0.08,0.09,0.1,0.11,0.12,0.13,0.14,0.15,0.16,0.17,0.18,0.19,0.2,0.21,0.22,0.23,0.24,0.25,0.26,0.27,0.28,0.29,0.3,0.31,0.32,0.33,0.34,0.35000000000000003,0.36,0.37,0.38,0.39,0.4,0.41000000000000003,0.42,0.43,0.44,0.45,0.46,0.47000000000000003,0.48,0.49,0.5,0.51,0.52,0.53,0.54,0.55,0.56,0.57000000000000006,0.58,0.59,0.6,0.61,0.62,0.63,0.64,0.65,0.66,0.67,0.68,0.69000000000000006,0.70000000000000007,0.71,0.72,0.73,0.74,0.75,0.76,0.77,0.78,0.79,0.8,0.81,0.82000000000000006,0.83000000000000007,0.84,0.85,0.86,0.87,0.88,0.89,0.9,0.91,0.92,0.93,0.94000000000000006,0.95000000000000007,0.96,0.97,0.98,0.99,1.0,1.01,1.02,1.03,1.04,1.05,1.06,1.07,1.08,1.09,1.1,1.11,1.12,1.1300000000000001,1.1400000000000001,1.1500000000000001,1.16,1.17,1.18,1.19,1.2,1.21,1.22,1.23,1.24,1.25,1.26,1.27,1.28,1.29,1.3,1.31,1.32,1.33,1.34,1.35,1.36,1.37,1.3800000000000001,1.3900000000000001,1.4000000000000001,1.41,1.42,1.43,1.44,1.45,1.46,1.47,1.48,1.49,1.5,1.51,1.52,1.53,1.54,1.55,1.56,1.57,1.58,1.59,1.6,1.61,1.62,1.6300000000000001,1.6400000000000001,1.6500000000000001,1.6600000000000001,1.67,1.68,1.69,1.7,1.71,1.72,1.73,1.74,1.75,1.76,1.77,1.78,1.79,1.8,1.81,1.82,1.83,1.84,1.85,1.86,1.87,1.8800000000000001,1.8900000000000001,1.9000000000000001,1.9100000000000001,1.92,1.93,1.94,1.95,1.96,1.97,1.98,1.99,2.0,2.0100000000000002,2.02,2.0300000000000002,2.04,2.05,2.06,2.07,2.08,2.09,2.1,2.11,2.12,2.13,2.14,2.15,2.16,2.17,2.18,2.19,2.2,2.21,2.22,2.23,2.24,2.25],\"printlevel\":0,\"homotopy\":0}";
                var inputJson = "{\"Ca_Feed\":[10.0,10.0,10.0,10.0,10.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,10.5,10.5,10.5,10.5,10.5,10.5,10.5,10.5,10.5,10.5,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,10.5,10.5,10.5,10.5,10.5,10.5,10.5,10.5,10.5,10.5,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.0,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,9.5,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0,11.0],\"F_Feed\":[3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3600.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3780.0000000000005,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3240.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3420.0,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005,3960.0000000000005],\"Volume\":[100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0],\"RunMode\":\"Dynamic\",\"TimeSeries\":[0.01,0.02,0.03,0.04,0.05,0.06,0.07,0.08,0.09,0.1,0.11,0.12,0.13,0.14,0.15,0.16,0.17,0.18,0.19,0.2,0.21,0.22,0.23,0.24,0.25,0.26,0.27,0.28,0.29,0.3,0.31,0.32,0.33,0.34,0.35000000000000003,0.36,0.37,0.38,0.39,0.4,0.41000000000000003,0.42,0.43,0.44,0.45,0.46,0.47000000000000003,0.48,0.49,0.5,0.51,0.52,0.53,0.54,0.55,0.56,0.57000000000000006,0.58,0.59,0.6,0.61,0.62,0.63,0.64,0.65,0.66,0.67,0.68,0.69000000000000006,0.70000000000000007,0.71,0.72,0.73,0.74,0.75,0.76,0.77,0.78,0.79,0.8,0.81,0.82000000000000006,0.83000000000000007,0.84,0.85,0.86,0.87,0.88,0.89,0.9,0.91,0.92,0.93,0.94000000000000006,0.95000000000000007,0.96,0.97,0.98,0.99,1.0,1.01,1.02,1.03,1.04,1.05,1.06,1.07,1.08,1.09,1.1,1.11,1.12,1.1300000000000001,1.1400000000000001,1.1500000000000001,1.16,1.17,1.18,1.19,1.2,1.21,1.22,1.23,1.24,1.25,1.26,1.27,1.28,1.29,1.3,1.31,1.32,1.33,1.34,1.35,1.36,1.37,1.3800000000000001,1.3900000000000001,1.4000000000000001,1.41,1.42,1.43,1.44,1.45,1.46,1.47,1.48,1.49,1.5,1.51,1.52,1.53,1.54,1.55,1.56,1.57,1.58,1.59,1.6,1.61,1.62,1.6300000000000001,1.6400000000000001,1.6500000000000001,1.6600000000000001,1.67,1.68,1.69,1.7,1.71,1.72,1.73,1.74,1.75,1.76,1.77,1.78,1.79,1.8,1.81,1.82,1.83,1.84,1.85,1.86,1.87,1.8800000000000001,1.8900000000000001,1.9000000000000001,1.9100000000000001,1.92,1.93,1.94,1.95,1.96,1.97,1.98,1.99,2.0,2.0100000000000002,2.02,2.0300000000000002,2.04,2.05,2.06,2.07,2.08,2.09,2.1,2.11,2.12,2.13,2.14,2.15,2.16,2.17,2.18,2.19,2.2,2.21,2.22,2.23,2.24,2.25],\"printlevel\":0,\"homotopy\":0}";
                inputDict = JObject.Parse(inputJson);

                Debug.WriteLine("Input: " + JsonConvert.SerializeObject(inputDict), "SinterConsumerTest.SinterDynamicACMTest");

                sim.Vis = false;
                sim.dialogSuppress = true;

                //sim.resetSim();

                sim.sendInputs(inputDict);                
                sim.sendInputsToSim();
                sim.runSim();
                sim.recvOutputsFromSim();
                JObject superDict = sim.getOutputs();
                JObject outputDict = (JObject)superDict["outputs"];
                // HACK: Inefficient Just making it work w/o covariance issues
                string data = outputDict.ToString(Newtonsoft.Json.Formatting.None);
                myDict = JsonConvert.DeserializeObject<IDictionary<string, Object>>(data);
            }
            finally
            {
                sim.closeSim();
            }
            Assert.AreEqual(sinter.sinter_AppError.si_OKAY, sim.runStatus);

            Debug.WriteLine(JsonConvert.SerializeObject(myDict));
        }

        //[TestMethod]
        //public void SinterGPROMSTest()
        //{
        //    IDictionary<string, object> output;
        //    IConsumerRun run = (IConsumerRun)new InMemorySinterConsumerGProms();
        //    Assert.IsFalse(run.IsEngineRunning);
        //    Assert.IsFalse(run.IsSimulationInitializing);
        //    Assert.IsFalse(run.IsSimulationOpened);
        //    // set job before calling Run
        //    ((InMemorySinterConsumerGProms)run).job = new InMemoryJobGProms_BufferTank_FO();
        //    //Newtonsoft.Json.Linq.JObject inputsD = JObject.Parse("{\"inputs\": { \"ProcessName\": \"SimulateTank_sinter\", \"password\": \"BufferTank_FO\", \"AlphaFO\": 0.8,\"SingleReal\": 0.0,\"SingleInt\": 11,\"ArrayInt\": [12,12],\"FlowInFO\": 14.0,\"Mass\": [1.0,2.0,3.0,4.0,5.0],\"HeightFO\": 7.5,\"singleSelector\": \"apple\",\"arraySelector\": [\"red\",\"red\",\"red\"]}}");
        //    Newtonsoft.Json.Linq.JObject inputsD = new JObject();
        //    Dictionary<String, Object> dd = new Dictionary<string, object>();
        //    foreach (var v in inputsD)
        //    {
        //        //Debug.WriteLine("VALUE: " + v);
        //        dd[v.Key] = v.Value;
        //    }
        //    ((InMemorySinterConsumerGProms)run).job.Process.Input = dd;
        //    try
        //    {
        //        Assert.IsTrue(run.Run());
        //        //Assert.IsTrue(run.IsSimulationOpened);

        //        IJobConsumerContract job = ((InMemorySinterConsumerGProms)run).job;
        //        Assert.IsTrue(job.IsSuccess());
        //        output = job.Process.Output;
        //        Assert.IsNotNull(output);

        //        foreach (KeyValuePair<string, object> kv in output)
        //        {
        //            Debug.WriteLine(String.Format("{0} : {1}", kv.Key, kv.Value));
        //        }
        //    }
        //    finally
        //    {
        //        Debug.WriteLine("Attempt Cleanup");
        //        run.CleanUp();
        //    }

        //    Object d;
        //    output.TryGetValue("status", out d);
        //    JObject statusD = (JObject)d;
        //    JToken status = statusD.GetValue("value");
        //    Assert.AreEqual<int>(status.Value<int>(), 0);
        //}

        [TestMethod]
        public void TestACM_BFBv6_3_new_Defaults()
        {
            IDictionary<string, object> output;
            IConsumerRun run = (IConsumerRun)new InMemorySinterConsumer();
            IConsumerMonitor monitor = (IConsumerMonitor)new InMemorySinterConsumerAspenPlusMonitor();

            monitor.setConsumerRun(run);

            Assert.IsFalse(run.IsEngineRunning);
            Assert.IsFalse(run.IsSimulationInitializing);
            Assert.IsFalse(run.IsSimulationOpened);
            // set job before calling Run
            ((InMemorySinterConsumer)run).job = new InMemoryJobACM_BFBv6_3();
            try
            {
                //Assert.IsTrue(run.Run());

                var taskRun = new Task<Boolean>(() => run.Run());
                var taskMonitor = new Task<int>(() => monitor.Monitor(true));
                Debug.WriteLine("Start taskRun", this.GetType().Name);
                taskRun.Start();
                Thread.Sleep(50000);

                Assert.IsTrue(run.IsSimulationOpened);

                Debug.WriteLine("Start taskMonitor", this.GetType().Name);
                taskMonitor.Start();

                Thread.Sleep(12000);

                Assert.IsTrue(run.IsSimulationOpened);

                IJobConsumerContract job = ((InMemorySinterConsumer)run).job;
                //Assert.IsTrue(job.IsSuccess());
                output = job.Process.Output;
                //Assert.IsNotNull(output);

                /*foreach (KeyValuePair<string, object> kv in output)
                {
                    Debug.WriteLine(String.Format("{0} : {1}", kv.Key, kv.Value));
                }*/
            }
            finally
            {
                Debug.WriteLine("Attempt Cleanup");
                run.CleanUp();
            }
        }

        [TestMethod]
        public void TestACM_BFBCap_Inputs()
        {
            IDictionary<string, object> output;
            IConsumerRun run = (IConsumerRun)new InMemorySinterConsumer();
            Assert.IsFalse(run.IsEngineRunning);
            Assert.IsFalse(run.IsSimulationInitializing);
            Assert.IsFalse(run.IsSimulationOpened);
            ((InMemorySinterConsumer)run).job = new InMemoryJobACM_BFBCap();
            ((InMemorySinterConsumer)run).job.Process.Input["UQ_nv"] = 1.0;
            ((InMemorySinterConsumer)run).job.Process.Input["rgndx"] = 0.0225;
            ((InMemorySinterConsumer)run).job.Process.Input["BFBadsT.Lb"] = 4.2; 
            ((InMemorySinterConsumer)run).job.Process.Input["UQ_A3"] = 1.0; 
            ((InMemorySinterConsumer)run).job.Process.Input["UQ_A2"] = 1.0; 
            ((InMemorySinterConsumer)run).job.Process.Input["UQ_A1"] = 1.0; 
            ((InMemorySinterConsumer)run).job.Process.Input["GHXfg.A_exch"] = 5749.66; 
            ((InMemorySinterConsumer)run).job.Process.Input["BFBrgnB.Lb"] = 4.2; 
            ((InMemorySinterConsumer)run).job.Process.Input["UQ_E3"] = 1.0; 
            ((InMemorySinterConsumer)run).job.Process.Input["UQ_E2"] = 1.0; 
            ((InMemorySinterConsumer)run).job.Process.Input["UQ_E1"] = 1.0; 
            ((InMemorySinterConsumer)run).job.Process.Input["UQ_fg_flow"] = 1.0; 
            ((InMemorySinterConsumer)run).job.Process.Input["rgnDt"] = 12.0;
            ((InMemorySinterConsumer)run).job.Process.Input["CW_SHXlean.A_exch"] = 400.0; 
            ((InMemorySinterConsumer)run).job.Process.Input["BFBadsM.Lb"] = 4.2; 
            ((InMemorySinterConsumer)run).job.Process.Input["adsN"] = 15.0; 
            ((InMemorySinterConsumer)run).job.Process.Input["rgnN"] = 15.0; 
            ((InMemorySinterConsumer)run).job.Process.Input["adsdx"] = 0.0275; 
            ((InMemorySinterConsumer)run).job.Process.Input["LR_SHXlean.A_exch"] = 7000.0; 
            ((InMemorySinterConsumer)run).job.Process.Input["BFBrgnT.Lb"] = 4.2; 
            ((InMemorySinterConsumer)run).job.Process.Input["adsDt"] = 15.0; 
            ((InMemorySinterConsumer)run).job.Process.Input["rgnlhx"] = 0.08; 
            ((InMemorySinterConsumer)run).job.Process.Input["adslhx"] = 0.4; 
            ((InMemorySinterConsumer)run).job.Process.Input["UQ_dS1"] = 1.0; 
            ((InMemorySinterConsumer)run).job.Process.Input["UQ_dS2"] = 1.0; 
            ((InMemorySinterConsumer)run).job.Process.Input["UQ_dS3"] = 1.0; 
            ((InMemorySinterConsumer)run).job.Process.Input["BFBadsB.Lb"] = 4.2; 
            ((InMemorySinterConsumer)run).job.Process.Input["LR_SHXrich.A_exch"] = 5000.0; 
            ((InMemorySinterConsumer)run).job.Process.Input["UQ_dH1"] = 1.0; 
            ((InMemorySinterConsumer)run).job.Process.Input["UQ_dH3"] = 1.0;
            ((InMemorySinterConsumer)run).job.Process.Input["UQ_dH2"] = 1.0;

            try
            {
                Assert.IsTrue(run.Run());
                Assert.IsTrue(run.IsSimulationOpened);

                IJobConsumerContract job = ((InMemorySinterConsumer)run).job;
                Assert.IsTrue(job.IsSuccess());
                output = job.Process.Output;
                Assert.IsNotNull(output);

                foreach (KeyValuePair<string, object> kv in output)
                {
                    Debug.WriteLine(String.Format("{0} : {1}", kv.Key, kv.Value));
                }
            }
            finally
            {
                Debug.WriteLine("Attempt Cleanup");
                run.CleanUp();
            }

            Object d;
            output.TryGetValue("status", out d);
            JObject statusD = (JObject)d;
            JToken status = statusD.GetValue("value");
            Assert.AreEqual<int>(status.Value<int>(), 0);
        }


        [TestMethod]
        public void TestACM_BFBCap_Defaults()
        {
            IDictionary<string, object> output;
            IConsumerRun run = (IConsumerRun)new InMemorySinterConsumer();
            Assert.IsFalse(run.IsEngineRunning);
            Assert.IsFalse(run.IsSimulationInitializing);
            Assert.IsFalse(run.IsSimulationOpened);
            ((InMemorySinterConsumer)run).job = new InMemoryJobACM_BFBCap();
            try
            {
                Assert.IsTrue(run.Run());
                Assert.IsTrue(run.IsSimulationOpened);

                IJobConsumerContract job = ((InMemorySinterConsumer)run).job;
                Assert.IsTrue(job.IsSuccess());
                output = job.Process.Output;
                Assert.IsNotNull(output);

                foreach (KeyValuePair<string, object> kv in output)
                {
                    Debug.WriteLine(String.Format("{0} : {1}", kv.Key, kv.Value));
                }
            }
            finally
            {
                Debug.WriteLine("Attempt Cleanup");
                run.CleanUp();
            }

            Object d;
            output.TryGetValue("status", out d);
            JObject statusD = (JObject)d;
            JToken status = statusD.GetValue("value");
            Assert.AreEqual<int>(status.Value<int>(), 0);
        }

        [TestMethod]
        [DeploymentItem(@"models\mea-abs-sinter.json", @"models")]
        [DeploymentItem(@"models\mea-abs.bkp", @"models")]
        public void TestAspenPlusThreading()
        {
            IDictionary<string, object> output;
            IConsumerRun run = (IConsumerRun)new InMemorySinterConsumerAspenPlus();
            IConsumerMonitor monitor = (IConsumerMonitor)new InMemorySinterConsumerAspenPlusMonitor();

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            monitor.setConsumerRun(run);

            Assert.IsFalse(run.IsEngineRunning);
            Assert.IsFalse(run.IsSimulationInitializing);
            Assert.IsFalse(run.IsSimulationOpened);
            try
            {
                var taskRun = new Task<Boolean>(() => run.Run(), tokenSource.Token);
                var taskMonitor = new Task<int>(() => monitor.Monitor(false));
                Debug.WriteLine("Start taskRun", this.GetType().Name);
                taskRun.Start();
                //System.Threading.Thread.Sleep(2000);
                Debug.WriteLine("Start taskMonitor", this.GetType().Name);
                taskMonitor.Start();
                Debug.WriteLine("run.IsSimulationOpened", this.GetType().Name);

                while (run.IsSimulationOpened == false) {
                    Debug.WriteLine("Waiting to Open", this.GetType().Name);
                    if (taskRun.IsFaulted)
                    {
                        Debug.WriteLine("Message: " + taskRun.Exception.Message);
                        foreach (Exception e in taskRun.Exception.InnerExceptions) {
                            Debug.WriteLine("Inner: " + e.Message);
                        }
                        throw new Exception(String.Format("{0}", taskRun.Exception.Message));
                    }
                    System.Threading.Thread.Sleep(500);
                }

                Debug.WriteLine("Simulation Opened!!!!!", this.GetType().Name);

                Assert.IsTrue(run.IsSimulationOpened);

                int sleepInterval = 1000;
                
                //Assert.IsTrue(run.Run());

                bool finish = false;
                //finish = taskRun.Wait(sleepInterval);

                //while (taskMonitor.Wait(sleepInterval) == false) { }

                if (taskMonitor.IsCompleted)
                {
                    Debug.WriteLine("TASK MONITOR IS COMPLETED!!!!!!", this.GetType().Name);
                    try
                    {
                        tokenSource.Cancel();
                        
                        while (taskRun.IsCanceled == false && tokenSource.Token.IsCancellationRequested == false) {
                            Debug.WriteLine("Task is not cancelled yet!!!!!");
                        }
                        Debug.WriteLine(tokenSource.Token.IsCancellationRequested,"Task is cancelled");
                         //*/
                    }
                    catch(Exception e)
                    {
                        Debug.WriteLine("Task is not cancelled!!!!");
                        Debug.WriteLine(e.Message);
                        Debug.WriteLine(e.StackTrace);
                    }
                }

                //Assert.IsTrue(finish);

                if (finish)
                    Debug.WriteLine("TASK IS FINISHED!!!!!");
                else
                    Debug.WriteLine("TASK IS NOT FINISHED!!!!!");

                System.Threading.Thread.Sleep(6000);

                IJobConsumerContract job = ((InMemorySinterConsumerAspenPlus)run).job;

                //Assert.IsFalse(taskRun.IsCompleted);
                //Assert.IsTrue(job.IsSuccess());
                //output = job.Process.Output;
                //Assert.IsNotNull(output);

                //foreach (KeyValuePair<string, object> kv in output)
                //{
                  //  Debug.WriteLine(String.Format("{0} : {1}", kv.Key, kv.Value));
                //}
            }
            finally
            {
                Debug.WriteLine("Attempt Cleanup");
                run.CleanUp();
            }

            //Object d;
            //output.TryGetValue("status", out d);
            //JObject statusD = (JObject)d;
            //JToken status = statusD.GetValue("value");
            //Assert.AreEqual<int>(status.Value<int>(), 0);
        }


        class BadJobSinterConsumer : Turbine.Consumer.AspenTech.AspenSinterConsumer
        {
            public IJobConsumerContract job = null;
            class InMemoryBadJob : InMemoryJob
            {
                protected override void SetupTestParamters()
                {
                    applicationName = "ACM";
                    simulationName = "Hybrid";
                    jobId = 1;
                    process = new InMemoryProcess();
                    throw new Exception("Bad Job");
                }
            }
            override protected IJobConsumerContract GetNextJob()
            {
                job = new InMemoryBadJob();
                return job;
            }
        }

        [TestMethod]
        public void TestBadJob()
        {
            IConsumerRun run = (IConsumerRun)new BadJobSinterConsumer();
            IConsumerMonitor monitor = (IConsumerMonitor)new InMemorySinterConsumerAspenPlusMonitor();
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            monitor.setConsumerRun(run);
            Assert.IsFalse(run.IsEngineRunning);
            Assert.IsFalse(run.IsSimulationInitializing);
            Assert.IsFalse(run.IsSimulationOpened);
            var taskRun = new Task<Boolean>(() => run.Run(), tokenSource.Token);
            var taskMonitor = new Task<int>(() => monitor.Monitor(false));
            taskRun.Start();
            taskMonitor.Start();
            Assert.IsFalse(run.IsSimulationOpened);
            System.Threading.Thread.Sleep(500);
            Assert.IsTrue(taskRun.IsFaulted);
        }
    }
}
