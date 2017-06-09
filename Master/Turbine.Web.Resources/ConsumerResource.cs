using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbine.Web.Contracts;
using System.ServiceModel.Activation;
using System.ServiceModel;
using Turbine.Data.Serialize;
using System.ServiceModel.Web;
using System.IO;
using System.Diagnostics;
using Turbine.Producer;
using Turbine.Producer.Contracts;
using Turbine.Data.Marshal;

namespace Turbine.Web.Resources
{
    class ConsumerQueryParameters : QueryParameters
    {
        internal static String GetStatus()
        {
            string val = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["status"];
            if (string.IsNullOrEmpty(val))
                return null;
            return val;
        }
    }

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, IncludeExceptionDetailInFaults = true)]
    public class ConsumerResource : IConsumerResource
    {
        private IConsumerResourceAccessor GetConsumerResourceAccessor() 
        {
            IConsumerResourceAccessor accessor;
            try
            {
                accessor = Container.GetConsumerResourceAccessor();
            }
            catch (Exception)
            {
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.SERVER_ERROR,
                    "Failed to get IConsumerResourceAccessor"
                );
                throw new WebFaultException<ErrorDetail>(detail,
                    System.Net.HttpStatusCode.InternalServerError);
            }
            return accessor;
        }

        public Data.Serialize.Consumers GetConsumerList()
        {
            return DataMarshal.GetConsumers(ConsumerQueryParameters.GetStatus());
        }

        public Data.Serialize.Consumer GetConsumer(string consumerID)
        {
            Guid guid = new Guid(consumerID);
            Debug.WriteLine(String.Format("GET consumer {0}", consumerID), this.GetType().Name);
            return DataMarshal.GetConsumer(guid);
        }

        /*
        public int RequestFloor(System.IO.Stream data)
        {
            int num = 0;
            bool success = false;
            string content = "";
            using (StreamReader sr = new StreamReader(data))
            {
                content = sr.ReadToEnd();
                //Dictionary<string, int> dict =
                //    Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
                //dict.TryGetValue("floor", out num);
                success = Int32.TryParse(content, out num);
            }
            if (!success)
            {
                Debug.WriteLine("Failed to parse floor request: " + content, GetType());
                return 0;
            }

            var accessor = GetConsumerResourceAccessor();
            accessor.SetConsumerConfigFloor(num);

            return num;
        }

        public Stream GetConsumerConfig()
        {
            var accessor = GetConsumerResourceAccessor();
            Dictionary<string,Object> d = accessor.GetConsumerConfig();
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(d);
            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        }
        */

        public Stream GetConsumerLog(string consumerID)
        {
            Guid guid = new Guid(consumerID);
            Debug.WriteLine(String.Format("GET consumer log {0}", consumerID), this.GetType().Name);
            var consumer = DataMarshal.GetConsumer(guid);
            var instanceID = consumer.instanceID;
            var accessor = GetConsumerResourceAccessor();
            var builder = new System.Text.StringBuilder();
            builder.AppendLine(String.Format("instanceID {0} {1} ", consumer.instanceID, consumer.status));
            accessor.GetConsumerLog(instanceID, builder);
            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(builder.ToString()));
        }


        /*
        public Stream SetConfiguration(Stream data)
        {
            Debug.WriteLine("set configuration", GetType());
            string content = "";
            Dictionary<string, Object> inputDict;
            using (StreamReader sr = new StreamReader(data))
            {
                content = sr.ReadToEnd();
                inputDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Object>>(content);
            }
            var accessor = GetConsumerResourceAccessor();
            var outputDict = accessor.SetConsumerConfigurationOptions(inputDict);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(outputDict);

            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        }
         */
    }
}