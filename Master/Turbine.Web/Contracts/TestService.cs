using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Diagnostics;
using Microsoft.Practices.Unity;
using Turbine.Common;


namespace Turbine.Web
{
    // Start the service and browse to http://<machine_name>:<port>/Service1/help to view the service's generated help page
    // NOTE: By default, a new instance of the service is created for each call; change the InstanceContextMode to Single if you want
    // a single instance of the service to process all calls.	
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    // NOTE: If the service is renamed, remember to update the global.asax.cs file
    public class TestService
    {
        private static TraceSource tsource = new TraceSource("turbineLogListener");
        // TODO: Implement the collection resource that will contain the SampleItem instances

        [WebGet(UriTemplate = "", ResponseFormat=WebMessageFormat.Json)]
        public List<SampleItem> GetCollection()
        {
            //Debug.Listeners.Add(new TextWriterTraceListener(new System.IO.FileStream(@"C:\logs\hi.txt", System.IO.FileMode.Append)));
            //Debug.WriteLine("GetCollection: " + OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.GetHashCode());
            tsource.TraceEvent(TraceEventType.Start, 0, "GetCollection Trace Event");
            tsource.TraceEvent(TraceEventType.Stop, 0, "GetCollection Trace Event");

            //String username = OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name;
            //String username = AppUtility.context.UserName;
            String username = "hi";
            try
            {
                username = AppUtility.GetAppContext().UserName;
            }
            catch (Exception ex)
            {
                return new List<SampleItem>() { new SampleItem() { Id=1, 
                    StringValue = ex.ToString() }};
                
            }
            return new List<SampleItem>() { new SampleItem() { Id = 1, StringValue = "HelloX: " + username } };
        }

        [WebInvoke(UriTemplate = "", Method = "POST")]
        public SampleItem Create(SampleItem instance)
        {
            // TODO: Add the new instance of SampleItem to the collection
            throw new NotImplementedException();
        }

        [WebGet(UriTemplate = "{id}", ResponseFormat=WebMessageFormat.Json)]
        public SampleItem Get(string id)
        {
            // TODO: Return the instance of SampleItem with the given id
            throw new NotImplementedException();
        }

        [WebInvoke(UriTemplate = "{id}", Method = "PUT")]
        public SampleItem Update(string id, SampleItem instance)
        {
            // TODO: Update the given instance of SampleItem in the collection
            throw new NotImplementedException();
        }

        [WebInvoke(UriTemplate = "{id}", Method = "DELETE")]
        public void Delete(string id)
        {
            // TODO: Remove the instance of SampleItem with the given id from the collection
            throw new NotImplementedException();
        }

    }
}
