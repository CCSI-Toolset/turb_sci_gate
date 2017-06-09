using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbine.Producer.Contracts;
using System.ServiceModel.Web;
using Turbine.Data.Serialize;

namespace StandAloneWebApplicationXP
{
    public class ConsumerResourceAccessor : IConsumerResourceAccessor
    {
        public Dictionary<string, object> GetConsumerConfig()
        {
            throw new WebFaultException(System.Net.HttpStatusCode.NotImplemented);
        }

        public void SetConsumerConfigFloor(int num)
        {
            throw new WebFaultException(System.Net.HttpStatusCode.NotImplemented);
        }

        public void GetConsumerLog(string instanceID, System.Text.StringBuilder builder)
        {
            throw new WebFaultException(System.Net.HttpStatusCode.NotImplemented);
        }

        public Dictionary<string, object> SetConsumerConfigurationOptions(Dictionary<string, object> inputDict)
        {
            throw new WebFaultException(System.Net.HttpStatusCode.NotImplemented);
        }
    }
}