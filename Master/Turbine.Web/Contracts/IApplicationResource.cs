using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using Turbine.Data.Serialize;
using System.IO;


namespace Turbine.Web.Contracts
{
    [ServiceContract]
    public interface IApplicationResource
    {
        // Empty string is equivalent to "/".  
        // In order to make a queryable resource
        // Need to create separate contract "IRootResources"
        [WebGet(UriTemplate = "/", ResponseFormat=WebMessageFormat.Json)]
        [OperationContract]
        ApplicationList GetApplications();

        [WebGet(UriTemplate = "/{appname}", ResponseFormat=WebMessageFormat.Json)]
        [OperationContract]
        Application Get(string appname);

        [OperationContract]
        [WebInvoke(UriTemplate = "/{appname}", Method = Verbs.Post, ResponseFormat = WebMessageFormat.Json)]
        string CreateSimulation(string appname);

        /*
        [OperationContract]
        [WebInvoke(UriTemplate = "/{appname}", Method = Verbs.Put, ResponseFormat=WebMessageFormat.Json)]
        Application Update(string appname);


        [WebGet(UriTemplate = "/{appname}/input", BodyStyle = WebMessageBodyStyle.Bare)]
        [OperationContract]
        Stream GetFileInputTypes(string appname);

        [WebInvoke(UriTemplate = "/{appname}/input/{name}", Method = Verbs.Put, BodyStyle=WebMessageBodyStyle.Bare)]
        [OperationContract]
        Stream UpdateInputFileType(string appname, string naeStream data);

        [WebGet(UriTemplate = "/{appname}/input/{name}", BodyStyle=WebMessageBodyStyle.Bare)]
        [OperationContract]
        Stream GetInputFileType(string appname, string name);
        */
    }
}
