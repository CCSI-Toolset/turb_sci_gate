using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Turbine.Data.Contract.Behaviors;


namespace Turbine.Orchestrator.AWS
{
    public static class AppUtility
    {
        public static IUnityContainer container = new UnityContainer();
        static AppUtility()
        {
            container.LoadConfiguration("orchestrator.aws.properties");
        }
        public static IAWSOrchestratorContext GetContext()
        {
            return container.Resolve<IAWSOrchestratorContext>();
        }
    }
}
