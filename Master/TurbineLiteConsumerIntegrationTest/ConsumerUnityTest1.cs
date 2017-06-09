using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Practices.Unity;
using Turbine.Consumer.Contract.Behaviors;

namespace TurbineLiteConsumerIntegrationTest
{
    [TestClass]
    public class ConsumerUnityTest1
    {
        private IConsumerContext consumerCtx;
        //private IContext appCtx;
        [TestMethod]
        public void TestMethod1()
        {
            consumerCtx = Turbine.Consumer.AppUtility.GetConsumerContext();
            //appCtx = Turbine.Consumer.AppUtility.GetAppContext();


            //Assert.AreNotSame(consumerCtx, Turbine.Consumer.AppUtility.GetConsumerContext());
            IUnityContainer container = Turbine.Consumer.AppUtility.container;
            //container.RegisterInstance<IContext>(consumerCtx);
            container.RegisterInstance<IConsumerContext>(consumerCtx);

            Assert.AreSame(consumerCtx, Turbine.Consumer.AppUtility.GetConsumerContext());


        }
    }
}
