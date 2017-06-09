using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbine.Web.Contracts;
using System.Diagnostics;
using Turbine.Data.Serialize;
using System.ServiceModel.Activation;
using System.ServiceModel;
using System.IO;
using System.ServiceModel.Web;
using Turbine.Producer.Data.Contract.Behaviors;
using Turbine.Lite.Web.Resources.Contracts;
using Turbine.Producer.Data.Contract;

namespace Turbine.Lite.Web.Resources
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, IncludeExceptionDetailInFaults = true)]
    public class ConsumerResource : BaseResource, IConsumerResource
    {
        public List<Guid> GetConsumerList()
        {
            int page = QueryParameters_GetPage(1);
            int rpp = QueryParameters_GetPageSize(1000);
            string status = QueryParameters_GetData("up", "status");
            Debug.WriteLine("GET Consumer List");
            return DataMarshal.GetConsumers(status, page, rpp);
        }

        public Stream GetConsumer(string consumerID)
        {
            int page = QueryParameters_GetPage(1);
            int rpp = QueryParameters_GetPageSize(1000);
            bool verbose = QueryParameters_GetVerbose(false);
            Guid consumer = new Guid(consumerID);

            string json = "";

            string simulation = null;
            Debug.WriteLine(String.Format("get session {0} job resources {1},{2}", consumerID, page, rpp), this.GetType().Name);
            var jobs = DataMarshal.GetJobs(Guid.Empty, simulation, consumer, null, null, page, rpp, verbose);
            json = Newtonsoft.Json.JsonConvert.SerializeObject(jobs);
            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        }

        public Consumer GetConsumerLog(string consumerID)
        {
            Consumer consumer = null;
            Guid guid = new Guid(consumerID);
            Debug.WriteLine(String.Format("GET consumer log {0}", consumerID), this.GetType().Name);
            try
            {            
                consumer = DataMarshal.GetConsumer(guid);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("No consumer with Id " + consumerID, this.GetType().Name);
                Debug.WriteLine(ex.ToString());
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    "No consumer with Id " + consumerID + ", traceback: " + ex.StackTrace.ToString()
                );
                throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.NotFound);
            }

            return consumer;
        }

        public int StopConsumer(string consumerID)
        {
            int stopped = 0;
            Guid guid = new Guid(consumerID);
            Debug.WriteLine(String.Format("Trying to stop consumer {0}", consumerID), this.GetType().Name);
            try
            {
                stopped = DataMarshal.StopConsumer(guid);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to stop consumer with Id " + consumerID, this.GetType().Name);
                Debug.WriteLine(ex.ToString());
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    "Failed consumer with Id " + consumerID + ", traceback: " + ex.StackTrace.ToString()
                );
                throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.NotFound);
            }

            switch (stopped)
            {
                case -1:
                    Debug.WriteLine("Consumer is already down", this.GetType().Name);
                    break;
                case 0:
                    Debug.WriteLine(String.Format("Failed to stop consumer {0}", consumerID), this.GetType().Name);
                    break;
                default:
                    Debug.WriteLine(String.Format("Succeeded to stop consumer {0}", consumerID), this.GetType().Name);
                    break;
            }

            return stopped;
        }
    }
}
