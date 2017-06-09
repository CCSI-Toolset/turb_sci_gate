using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Turbine.Data.Contract.Behaviors;
using Turbine.Consumer.Data.Contract.Behaviors;
using Turbine.Consumer.Contract.Behaviors;

namespace Turbine.Consumer
{
    // Unity Dependency Injection
    //
    public static class AppUtility
    {
        public static IUnityContainer container = new UnityContainer();
        static AppUtility()
        {
            container.LoadConfiguration("consumer");
        }
        public static IContext GetAppContext()
        {
            IContext ctx = container.Resolve<IContext>();
            return ctx;
        }
        public static IConsumerContext GetConsumerContext()
        {
            IConsumerContext ctx = container.Resolve<IConsumerContext>();
            return ctx;
        }

        public static IConsumerRegistrationContract GetConsumerRegistrationContract()
        {
            return container.Resolve<IConsumerRegistrationContract>();
        }

        public static IConsumerRun GetConsumerRunContract()
        {
            return container.Resolve<IConsumerRun>();
        }

        public static IConsumerMonitor GetConsumerMonitorContract()
        {
            return container.Resolve<IConsumerMonitor>();
        }

        /*
        public static IJobConsumerContract GetJobConsumerContract(int id)
        {
            var contract = container.Resolve<IJobConsumerContract>();
            contract.Id = id;
            return contract;
        }

        public static IJobQueue GetJobQueue()
        {
            return container.Resolve<IJobQueue>();
        }
         * */

        /*
        public static IJobQueue GetJobQueue(string[] apps)
        {
            var contract = container.Resolve<IJobQueue>();
            contract.SetSupportedApplications(apps);
            return contract;
        }
        */

        public static IJobQueue GetJobQueue(IConsumerRun run)
        {
            var contract = container.Resolve<IJobQueue>();
            contract.SetSupportedApplications(run);
            return contract;
        }
    }
}
