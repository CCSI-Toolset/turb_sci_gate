using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.ServiceModel;
using System.ServiceModel.Web;
using Turbine.Web.Contracts;
using Turbine.Data.Serialize;
using Turbine.Producer.Data.Contract.Behaviors;
using Turbine.Producer.Data.Contract;
using System.Diagnostics;
using Turbine.Data.Marshal;


namespace Turbine.Web.Resources
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, IncludeExceptionDetailInFaults = true)]
    public class ApplicationResource : IApplicationResource
    {
        public ApplicationList GetApplications()
        {
            System.Diagnostics.Debug.WriteLine("GetApplications", GetType());
            //return new List<String>() { "AspenPlus", "ACM", "Excel" };
            return DataMarshal.GetApplicationList();
        }

        public Data.Serialize.Application Get(string appname)
        {
            return DataMarshal.GetApplication(appname);
        }

        public string CreateSimulation(string appname)
        {
            string name = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["simulation"];
            if (string.IsNullOrEmpty(name))
            {
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.QUERYPARAM_ERROR,
                    "Required parameter \"simulation\":  Provide unique name for factory method."
                    );
                throw new WebFaultException<ErrorDetail>(detail,
                    System.Net.HttpStatusCode.BadRequest);
            }

            ISimulationProducerContract contract = null;
            try
            {
                contract = AspenSimulationContract.Create(name, appname);
            }
            catch (Exception ex)
            {
                string msg = String.Format("Failed to Create Simulation {0}: {1} {2}",
                    name, ex.Message, ex.StackTrace.ToString());
                Debug.WriteLine(msg, this.GetType().Name);
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    msg
                );
                throw new WebFaultException<ErrorDetail>(detail,
                    System.Net.HttpStatusCode.InternalServerError);
            }

            //Simulation sim = DataMarshal.GetSimulation(name);
            var uri = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.RequestUri;
            return new Uri(uri, String.Format("../simulation/{0}", name)).ToString();
        }
    }
}
