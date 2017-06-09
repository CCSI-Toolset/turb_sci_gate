using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using Turbine.Web.Contracts;
using Turbine.Web.Resources;
using Turbine.Producer.Data.Contract.Behaviors;
using Turbine.Producer.Data.Contract;

namespace ProducerDataIntegrationTests
{
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

    class ConsoleContext : Turbine.Consumer.Contract.Behaviors.IContext
    {
        public string BaseWorkingDirectory
        {
            get { return "NOTHING"; }
        }
    }
    [TestClass]
    public class SessionIntegrationTest
    {
        /* // NOTE: CAN NOT TEST QUERY PARAMS WITHOUT Mocking the HTTP Request
        [TestMethod]
        public void TestSessionResource()
        {
        
            //ISessionProducerContract producer = new AspenSessionProducerContract();
            Stopwatch sw = new Stopwatch();
            sw = new Stopwatch();

            sw.Start();
            var resource = new SessionResource();
            var session_list = resource.GetSessionList();
            sw.Stop();

            Console.WriteLine("Session Get({0}) Elapsed={1}", session_list.Count, sw.Elapsed);
        }
         */

        [TestMethod]
        public void TestISessionProducerContract()
        {
            // producers
            ISessionProducerContract producer = new AspenSessionProducerContract();
            Guid session_id = producer.Create();
            Assert.IsNotNull(session_id);
        }
    }

}
