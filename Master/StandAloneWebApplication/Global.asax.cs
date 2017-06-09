using System;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Routing;
using System.Diagnostics;
using System.Security.Principal;
using System.Text;
using System.Security.Cryptography;
using Turbine.Web.Security;
using Microsoft.Practices.Unity;
//using Turbine.Web;


namespace StandAloneWebApplication
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            Debug.WriteLine("Application_Start", GetType());
            RouteTable.Routes.Add(new ServiceRoute("application", new WebServiceHostFactory(), typeof(Turbine.Web.Resources.ApplicationResource)));
            RouteTable.Routes.Add(new ServiceRoute("simulation", new WebServiceHostFactory(), typeof(Turbine.Web.Resources.SimulationResource)));
            RouteTable.Routes.Add(new ServiceRoute("job", new WebServiceHostFactory(), typeof(Turbine.Web.Resources.JobResource)));
            RouteTable.Routes.Add(new ServiceRoute("session", new WebServiceHostFactory(), typeof(Turbine.Web.Resources.SessionResource)));
            RouteTable.Routes.Add(new ServiceRoute("consumer", new WebServiceHostFactory(), typeof(Turbine.Web.Resources.ConsumerResource)));
        }
    }
}