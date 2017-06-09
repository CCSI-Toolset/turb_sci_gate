using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;


namespace Turbine.Producer.AWS
{
    public static class AppUtility
    {
        public static IUnityContainer container = new UnityContainer();
        static AppUtility()
        {
            container.LoadConfiguration("producer.aws.properties");
        }
        public static IAWSProducerContext GetContext()
        {
            return container.Resolve<IAWSProducerContext>();
        }
    }
}
