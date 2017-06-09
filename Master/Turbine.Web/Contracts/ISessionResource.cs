using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Web;
using System.ServiceModel;
using Turbine.Data.Serialize;
using System.IO;

namespace Turbine.Web.Contracts
{
    [ServiceContract]
    public interface ISessionResource
    {
        [WebGet(UriTemplate = "/", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        List<Guid> GetSessionList();

        [OperationContract]
        [WebInvoke(UriTemplate = "/", Method = Verbs.Post, ResponseFormat = WebMessageFormat.Json)]
        Guid CreateSession();

        [OperationContract]
        [WebInvoke(UriTemplate = "/{sessionid}", Method = Verbs.Post, ResponseFormat = WebMessageFormat.Json)]
        List<int> AppendJobs(string sessionid, Stream data);

        [WebGet(UriTemplate = "/{sessionid}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Stream GetSession(string sessionid);

        [OperationContract]
        [WebInvoke(UriTemplate = "/{sessionid}", Method = Verbs.Delete, ResponseFormat = WebMessageFormat.Json)]
        int DeleteSession(string sessionid);

        [OperationContract]
        [WebInvoke(UriTemplate = "/{sessionid}/copy", Method = Verbs.Post, ResponseFormat = WebMessageFormat.Json)]
        Stream CopySession(string sessionid);

        [OperationContract]
        [WebInvoke(UriTemplate = "/{sessionid}/start", Method = Verbs.Post, ResponseFormat = WebMessageFormat.Json)]
        int StartSession(string sessionid);

        [OperationContract]
        [WebInvoke(UriTemplate = "/{sessionid}/stop", Method = Verbs.Post, ResponseFormat = WebMessageFormat.Json)]
        int StopSession(string sessionid);

        [OperationContract]
        [WebInvoke(UriTemplate = "/{sessionid}/kill", Method = Verbs.Post, ResponseFormat = WebMessageFormat.Json)]
        int KillSession(string sessionid);

        [OperationContract]
        [WebGet(UriTemplate = "/{sessionid}/status", ResponseFormat = WebMessageFormat.Json)]
        Stream StatusSession(string sessionid);

        [WebInvoke(UriTemplate = "/{sessionid}/description", Method = Verbs.Put, 
            ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        bool UpdateDescription(string sessionid, Stream data);

        [WebGet(UriTemplate = "/{sessionid}/description", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Stream GetDescription(string sessionid);

        [OperationContract]
        [WebInvoke(UriTemplate = "/{sessionid}/result", Method = Verbs.Post, ResponseFormat = WebMessageFormat.Json)]
        Guid CreateSessionResults(string sessionid);

        [OperationContract]
        [WebInvoke(UriTemplate = "/{sessionid}/result/{generatorid}", Method = Verbs.Post, ResponseFormat = WebMessageFormat.Json)]
        int CreateSessionResultsPage(string sessionid, string generatorid);

        [OperationContract]
        [WebInvoke(UriTemplate = "/{sessionid}/result/{generatorid}/{pagenum}", Method = Verbs.Get, ResponseFormat = WebMessageFormat.Json)]
        Stream StartSessionGeneratorResults(string sessionid, string generatorid, string pagenum);

        [OperationContract]
        [WebInvoke(UriTemplate = "/{sessionid}/result/{generatorid}", Method = Verbs.Delete, ResponseFormat = WebMessageFormat.Json)]
        string DeleteSessionGeneratorResults(string sessionid, string generatorid);
    }
}
