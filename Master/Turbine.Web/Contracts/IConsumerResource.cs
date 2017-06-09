using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.IO;
using Turbine.Data.Serialize;

namespace Turbine.Web.Contracts
{
    [ServiceContract]
    public interface IConsumerResource
    {
        [WebGet(UriTemplate = "/", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        List<Guid> GetConsumerList();
        
        [WebGet(UriTemplate = "/{consumerID}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Stream GetConsumer(string consumerID);
        
        [WebGet(UriTemplate = "/{consumerID}/log", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Consumer GetConsumerLog(string consumerID);

        [OperationContract]
        [WebInvoke(UriTemplate = "/{consumerID}/stop", Method = Verbs.Post, ResponseFormat = WebMessageFormat.Json)]
        int StopConsumer(string consumerID);
        /*
        [WebGet(UriTemplate = "/config", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Stream GetConsumerConfig();

        [OperationContract]
        [WebInvoke(UriTemplate = "/config/floor", Method = Verbs.Put, ResponseFormat = WebMessageFormat.Json)]
        int RequestFloor(Stream data);

        [OperationContract]
        [WebInvoke(UriTemplate = "/config", Method = Verbs.Put, ResponseFormat = WebMessageFormat.Json)]
        Stream SetConfiguration(Stream data);
         */
    }
}