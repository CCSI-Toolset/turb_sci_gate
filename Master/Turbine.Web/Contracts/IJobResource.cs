using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Web;
using Turbine.Data.Serialize;
using System.IO;

namespace Turbine.Web.Contracts
{
    [ServiceContract]
    public interface IJobResource
    {
        [OperationContract]
        [WebGet(UriTemplate = "/", ResponseFormat = WebMessageFormat.Json)]
        Stream GetJobResources();

        [OperationContract]
        [WebInvoke(UriTemplate = "/", Method = Verbs.Post, ResponseFormat = WebMessageFormat.Json)]
        Stream CreateJob(Stream data);

        [OperationContract]
        [WebGet(UriTemplate = "/{id}", ResponseFormat = WebMessageFormat.Json)]
        Stream GetJob(string id);

        [OperationContract]
        [WebInvoke(UriTemplate = "/{id}", Method = Verbs.Post, ResponseFormat = WebMessageFormat.Json)]
        Stream SubmitJob(string id);

        [OperationContract]
        [WebInvoke(UriTemplate = "/{id}/cancel", Method = Verbs.Post, ResponseFormat = WebMessageFormat.Json)]
        Stream CancelJob(string id);

        [OperationContract]
        [WebInvoke(UriTemplate = "/{id}/terminate", Method = Verbs.Post, ResponseFormat = WebMessageFormat.Json)]
        Stream TerminateJob(string id);

        [OperationContract]
        [WebInvoke(UriTemplate = "/{id}/kill", Method = Verbs.Post, ResponseFormat = WebMessageFormat.Json)]
        Stream KillJob(string id);
    }
}