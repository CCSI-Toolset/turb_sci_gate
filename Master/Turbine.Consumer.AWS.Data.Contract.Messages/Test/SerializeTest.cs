using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Turbine.AWS.Messages;

namespace Turbine.Consumer.AWS.Data.Contract.Messages.Test
{
    [TestClass]
    public class SerializeTest
    {
        /// <summary>
        /// RPC
        /// {
        ///     Operation :   JobStateChange
        ///     Message : 
        ///     {
        ///         Id : GUID
        ///         Timestamp : DateTime
        ///         State : Setup
        ///     }
        ///  }
        /// </summary>
        [TestMethod]
        public void TestConsumerStateChangeNotification()
        {

            // Consumer JobStateChange
            var env = new RPCEnvelope();
            var con = new JobStateChange();
            env.Message = con;
            env.Operation = con.GetType().Name;
            con.Id = Guid.NewGuid();
            con.Timestamp = DateTime.UtcNow;
            con.State = "Setup";

            var iso = new Newtonsoft.Json.Converters.IsoDateTimeConverter();
            iso.DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffK";

            string json = Newtonsoft.Json.JsonConvert
                .SerializeObject(env, iso);

            // Orchestrator JobStateChange
            JToken token = JObject.Parse(json);
            String op = (string)token.SelectToken("Operation");
            JToken msg = token.SelectToken("Mesage");
            Assert.IsTrue(op == "JobStateChange");

            var serializer = new Newtonsoft.Json.JsonSerializer();
            Newtonsoft.Json.JsonReader reader = token["Message"].CreateReader();

            var job = serializer.Deserialize<JobStateChange>(reader);

            Assert.AreEqual(((JobStateChange)env.Message).State, job.State);
        }
        /*
        [TestMethod]
        public void TestJobMessage()
        {
            var job = Guid.NewGuid();
            var consumer = Guid.NewGuid();
            JobMessage msg = new JobSetupMessage(job, consumer);
            msg.Id = Guid.NewGuid();
            msg.Timestamp = DateTime.UtcNow;

            //var formatter = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            var iso = new Newtonsoft.Json.Converters.IsoDateTimeConverter();
            iso.DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffK";

            string json = Newtonsoft.Json.JsonConvert
                .SerializeObject(msg, iso);
            Console.Out.WriteLine(json);

            System.Threading.Thread.Sleep(10000);
            var msg2 = Newtonsoft.Json.JsonConvert.DeserializeObject<JobSetupMessage>(json, iso);

            Assert.AreNotEqual(msg.Id, msg2.Id);
            Assert.AreNotEqual(msg.Id, Guid.Empty);
            Assert.AreNotEqual(msg2.Id, Guid.Empty);
            Assert.AreEqual(msg.JobId, msg2.JobId);
            Assert.AreEqual(msg.Timestamp, msg2.Timestamp);
            Console.Out.WriteLine(msg.Timestamp.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffK"));
            Console.Out.WriteLine(msg2.Timestamp.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffK"));
            Console.Out.WriteLine(Math.Abs((msg.Timestamp - msg2.Timestamp).TotalSeconds));
            Assert.IsTrue(Math.Abs((msg.Timestamp - msg2.Timestamp).TotalSeconds) < 1);
        }
         * */

        /*
        [TestMethod]
        public void TestJobStateChange()
        {
            var con = new JobStateChange();
            con.Id = Guid.NewGuid();
            con.Timestamp = DateTime.UtcNow;

            var iso = new Newtonsoft.Json.Converters.IsoDateTimeConverter();
            iso.DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffK";

            string json = Newtonsoft.Json.JsonConvert
                .SerializeObject(con, iso);
            Console.Out.WriteLine(json);

            var msg2 = Newtonsoft.Json.JsonConvert.DeserializeObject<JobStateChange>(json, iso);
            Assert.AreEqual(con.State, msg2.State);
            Assert.AreEqual(con.Id, msg2.Id);
            Console.Out.WriteLine(con.Timestamp.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffK"));
            Console.Out.WriteLine(msg2.Timestamp.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffK"));
            
            Console.Out.WriteLine(Math.Abs((con.Timestamp - msg2.Timestamp).TotalSeconds));
            // Timestamps gave microseconds, only keeping milliseconds
            Assert.IsTrue(Math.Abs((con.Timestamp - msg2.Timestamp).TotalSeconds) < 0.001);
            //Assert.AreEqual(con.Timestamp, msg2.Timestamp);
        }
         */
        /*
        [TestMethod]
        public void TestEnvelope()
        {
            var env = new RPCEnvelope();
            var con = new JobStateChange();
            env.Message = con;
            env.Operation = con.GetType().Name;
            con.Id = Guid.NewGuid();
            con.Timestamp = DateTime.UtcNow;
            con.State = "Setup";

            var iso = new Newtonsoft.Json.Converters.IsoDateTimeConverter();
            iso.DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffK";

            string json = Newtonsoft.Json.JsonConvert
                .SerializeObject(env, iso);

            Console.Out.WriteLine(json);

            var env2 = Newtonsoft.Json.JsonConvert.DeserializeObject<RPCEnvelope>(json, iso);
            Assert.AreEqual(env.Operation, env2.Operation);
            Assert.AreEqual(((JobStateChange)env.Message).State, ((JobStateChange)env2.Message).State);
        }
         */
    }
}
