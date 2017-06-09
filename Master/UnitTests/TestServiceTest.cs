using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using Turbine.Web;
using Microsoft.Practices.Unity;
using Turbine.Consumer.Contract.Behaviors;
using Turbine.Lite.Web.Resources;
using System.Configuration;
using Microsoft.Practices.Unity.Configuration;
using Turbine.Producer.Contracts;


namespace Turbine.Test
{
    public class TestWCFContext : IProducerContext
    {
        public string UserName
        {
            get { return "test1"; }
        }
        public string BaseWorkingDirectory
        {
            get { return @"\turine\test\TestWCFContext"; }
        }
    }
    [TestClass]
    public class TestServiceTest
    {
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            var container = Turbine.Producer.Container.GetProducerContainer();
            container.RegisterType<IProducerContext, TestWCFContext>();
        }
        /*
        [TestMethod]
        public void TestGetCollection()
        {
            var service = new TestResource();
            List<SampleItem> data = service.GetCollection();
            Assert.AreEqual(data.Count, 1);
            Assert.IsTrue(data[0].StringValue.StartsWith("HelloX: test1"));
        }
         */
    }

}