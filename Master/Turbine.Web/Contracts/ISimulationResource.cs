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
    public interface ISimulationResource
    {
        // Empty string is equivalent to "/".  
        // In order to make a queryable resource
        // Need to create separate contract "IRootResources"
        [WebGet(UriTemplate = "/", ResponseFormat=WebMessageFormat.Json)]
        [OperationContract]
        Simulations GetSimulations();

        [WebGet(UriTemplate = "/{nameOrID}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Simulation GetSimulation(string nameOrID);

        [WebInvoke(UriTemplate = "/{nameOrID}", Method = Verbs.Delete, ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        bool DeleteSimulation(string nameOrID);
        
        [OperationContract]
        [WebInvoke(UriTemplate = "/{nameOrID}", Method = Verbs.Put, 
            ResponseFormat=WebMessageFormat.Json, RequestFormat=WebMessageFormat.Json)]
        Simulation UpdateSimulation(string nameOrID, Simulation sim);

        [WebGet(UriTemplate = "/{nameOrID}/input", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        string[] GetStagedInputs(string nameOrID);

        [WebGet(UriTemplate = "/{nameOrID}/input/{inputName}", BodyStyle = WebMessageBodyStyle.Bare)]
        [OperationContract]
        Stream GetStagedInputFile(string nameOrID, string inputName);

        [WebInvoke(UriTemplate = "/{nameOrID}/input/{*inputName}", Method = Verbs.Put, ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        bool UpdateStagedInputFile(string nameOrID, string inputName, Stream data);

        [WebInvoke(UriTemplate = "/{nameOrID}/validate", Method = Verbs.Get, BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        bool Validate(string nameOrID);
    }
}
