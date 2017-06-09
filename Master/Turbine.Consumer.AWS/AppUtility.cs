using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;


namespace Turbine.Consumer.AWS
{
    public static class AppUtility
    {
        public static IUnityContainer container = new UnityContainer();
        static AppUtility()
        {
            container.LoadConfiguration("consumer.aws.properties");
        }
        public static IAWSContext GetContext()
        {
            return container.Resolve<IAWSContext>();
        }
    }
}
