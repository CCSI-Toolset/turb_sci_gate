using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Activation;
using System.ServiceModel;
using System.ServiceModel.Web;
using Turbine.Web.Contracts;
using Turbine.Data.Serialize;
using Turbine.DataEF6;
using Turbine.Producer.Data.Contract.Behaviors;
using Turbine.Lite.Web.Resources.Contracts;


namespace Turbine.Lite.Web.Resources
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, IncludeExceptionDetailInFaults = true)]
    public class ApplicationResource : IApplicationResource
    {
        /// <summary>
        /// GET: Array Application Resources
        /// </summary>
        /// <returns>ApplicationList</returns>
        public ApplicationList GetApplications()
        {
            System.Diagnostics.Debug.WriteLine("GetApplications", GetType());
            return DataMarshal.GetApplicationList();
        }

        /// <summary>
        /// GET: Application Resource Representation
        /// </summary>
        /// <param name="appname">Application name</param>
        /// <returns>Application</returns>
        public Data.Serialize.Application Get(string appname)
        {
            return DataMarshal.GetApplication(appname);
        }

        /// <summary>
        /// POST: Creates a new simulation
        /// </summary>
        /// <param name="appname">Application name</param>
        /// <returns>URI of New simulation</returns>
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
                contract = ProducerSimulationContract.Create(name, name, appname);
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

            var uri = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.RequestUri;
            return new Uri(uri, String.Format("../simulation/{0}", name)).ToString();
        }

    }
}
