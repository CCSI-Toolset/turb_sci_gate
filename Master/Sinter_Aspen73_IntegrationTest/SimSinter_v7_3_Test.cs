using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Turbine.Consumer.Contract.Behaviors;
using Turbine.Consumer.Data.Contract.Behaviors;
using Turbine.Data.Contract.Behaviors;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json.Linq;


namespace Sinter_Aspen73_IntegrationTest
{
    [TestClass]
    public class SimSinter_v7_3_Test
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
    }
}
