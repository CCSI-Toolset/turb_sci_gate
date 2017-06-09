using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using Turbine.Data.Contract;
using Turbine.Data.Serialize;
using System.Diagnostics;
using System.IO;
using Turbine.Data.Contract.Behaviors;
using Turbine.Producer.Data.Contract.Behaviors;
using Turbine.Producer.Data.Contract;
using Turbine.Lite.Web.Resources.Contracts;


namespace Turbine.Lite.Web.Resources
{
    class SimulationQueryParameters : QueryParameters
    {
        internal static bool GetRecurse(bool defaultValue)
        {
            bool val = defaultValue;
            string param = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["recurse"];
            if (string.IsNullOrEmpty(param))
                return val;

            if (!Boolean.TryParse(param, out val))
            {
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.QUERYPARAM_ERROR,
                    "verbose failed to parse as boolean"
                );
                throw new WebFaultException<ErrorDetail>(detail,
                    System.Net.HttpStatusCode.BadRequest);
            }

            return val;
        }
    }

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, IncludeExceptionDetailInFaults=true)]
    public class SimulationResource : BaseResource,Turbine.Web.Contracts.ISimulationResource
    {
        public Simulations GetSimulations()
        {
            bool verbose = QueryParameters_GetVerbose(false);
            Debug.WriteLine("get all simulations", this.GetType().Name);
            return DataMarshal.GetSimulations(verbose);
        }

        /// <summary>
        /// GET: Returns resource representation of Simulation
        /// </summary>
        /// <param name="name">Simulation name</param>
        /// <returns>Get Version of Simulation</returns>
        public Simulation GetSimulation(string nameOrID)
        {
            Guid simulationid = Guid.Empty;
            bool isGuid = Guid.TryParse(nameOrID, out simulationid);
            Debug.WriteLine("Find simulation: " + nameOrID, this.GetType().Name);
            Simulation sim = null;
            try
            {
                sim = DataMarshal.GetSimulation(nameOrID, isGuid);
                /*if (sim == null)
                {
                    var detail = new ErrorDetail(
                        ErrorDetail.Codes.DATAFORMAT_ERROR,
                        String.Format("simulation \"{0}\" does not exist", nameOrID)
                    );
                    throw new WebFaultException<ErrorDetail>(detail,
                        System.Net.HttpStatusCode.NotFound);
                }
                return sim;*/
            }
            catch (Exception ex)
            {
                Debug.WriteLine("No simulation " + nameOrID, this.GetType().Name);
                Debug.WriteLine(ex.ToString());
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    "No simulation " + nameOrID + ", traceback: " + ex.StackTrace.ToString()
                );
                throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.NotFound);
            }
            return sim;
        }

        /// <summary>
        /// DELETE: Removes the named simulation.  
        /// If specify 'all', all refrenced resources to 'name' are deleted ( jobs, simulation versions, etc ).
        /// </summary>
        /// <param name="name">Simulation name</param>
        /// <returns>success</returns>
        public bool DeleteSimulation(string nameOrID)
        {
            Guid simulationId = Guid.Empty;
            bool isGuid = Guid.TryParse(nameOrID, out simulationId);

            bool all = SimulationQueryParameters.GetRecurse(false);
            ISimulationProducerContract contract = null;
            try
            {
                contract = ProducerSimulationContract.Get(nameOrID);
            }
            catch (Exception ex)
            {
                string msg = String.Format("Failed to Get Simulation {0}: {1} {2}",
                    nameOrID, ex.Message, ex.StackTrace.ToString());
                if (ex.InnerException != null)
                    msg += String.Format("    InnerException:    {0} {1}",
                        ex.InnerException.Message, ex.InnerException.StackTrace.ToString());
                Debug.WriteLine(msg, this.GetType().Name);
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    msg
                );
                throw new WebFaultException<ErrorDetail>(detail,
                    System.Net.HttpStatusCode.InternalServerError);
            }
            return isGuid ? contract.Delete() : contract.DeleteAll();
        }
        
        /// <summary>
        /// PUT: Update Simulation
        /// </summary>
        /// <param name="name">Simulation name</param>
        /// <param name="sim">Simulation Resource Representation</param>
        /// <returns>Updated Simulation</returns>
        public Simulation UpdateSimulation(string nameOrID, Simulation sim)
        {
            Guid simulationId = Guid.Empty;
            bool isGuid = Guid.TryParse(nameOrID, out simulationId);

            ISimulationProducerContract contract = null;
            if (sim == null || sim.Name == null)
            {
                Debug.WriteLine(String.Format("Sim Name: {0}, ID: {1}, app: {2}", sim.Name, sim.Id.ToString(), sim.Application)
                    , this.GetType().Name);
                string msg = String.Format("No Application specified in Simulation {0}", nameOrID);
                Debug.WriteLine(msg, this.GetType().Name);
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    msg
                );
                throw new WebFaultException<ErrorDetail>(detail,
                    System.Net.HttpStatusCode.InternalServerError);
            }

            if (!isGuid && !nameOrID.Equals(sim.Name))
            {
                string msg = String.Format("No URL Resource Name {0} doesn't match Simulation.Name {1}", nameOrID, sim.Name);
                Debug.WriteLine(msg, this.GetType().Name);
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    msg
                );
                throw new WebFaultException<ErrorDetail>(detail,
                    System.Net.HttpStatusCode.InternalServerError);
            }

            string appname = sim.Application;
            
            try
            {
                contract = ProducerSimulationContract.Create(nameOrID, sim.Name, appname);
            }
            catch (Exception ex)
            {
                string msg = String.Format("Failed to Create Simulation {0}: {1} {2}",
                    nameOrID, ex.Message, ex.StackTrace.ToString());
                if (ex.InnerException != null)
                    msg += String.Format("    InnerException:    {0} {1}", 
                        ex.InnerException.Message, ex.InnerException.StackTrace.ToString());
                Debug.WriteLine(msg, this.GetType().Name);
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    msg
                );
                throw new WebFaultException<ErrorDetail>(detail, 
                    System.Net.HttpStatusCode.InternalServerError);
            }

            sim = DataMarshal.GetSimulation(nameOrID, isGuid);

            return sim;

        }
 
        /// <summary>
        /// GET: Get Staged input names
        /// </summary>
        /// <param name="name">Simulation Name</param>
        /// <returns>Array of input names</returns>
        public string[] GetStagedInputs(string nameOrID)
        {
            Guid simulationid = Guid.Empty;
            bool isGuid = Guid.TryParse(nameOrID, out simulationid);

            var stagedInputs = DataMarshal.GetStagedInputs(nameOrID, isGuid);
            if (stagedInputs == null)
            {
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    String.Format("simulation \"{0}\" does not exist", nameOrID)
                );
                throw new WebFaultException<ErrorDetail>(detail,
                    System.Net.HttpStatusCode.NotFound);
            }
            return stagedInputs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Simulation Name</param>
        /// <param name="inputName">input Name</param>
        /// <returns>Contents of input file</returns>
        public Stream GetStagedInputFile(string nameOrID, string inputName)
        {
            Debug.WriteLine(String.Format("GetStagedInputFile: {0}/input/{1}", nameOrID, inputName), this.GetType().Name);
            //OutgoingWebResponseContext ctx = WebOperationContext.Current.OutgoingResponse;
            Guid simulationid = Guid.Empty;
            bool isGuid = Guid.TryParse(nameOrID, out simulationid);

            var e = DataMarshal.GetStagedInputFile(nameOrID, inputName, isGuid);
            if (e == null)
            {
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    String.Format("simulation \"{0}\" input \"{1}\" does not exist", nameOrID, inputName)
                );
                throw new WebFaultException<ErrorDetail>(detail,
                    System.Net.HttpStatusCode.NotFound);
            }
            if (e.Content == null)
            {
                return new MemoryStream();
            }
            this.OutgoingWebResponseContext_ContentType = "text/plain";
            if (e.InputFileType != null)
            {
                this.OutgoingWebResponseContext_ContentType = e.InputFileType;
            }
            this.OutgoingWebResponseContext_StatusCode = System.Net.HttpStatusCode.OK;
            return new MemoryStream(e.Content);
        }

        /// <summary>
        /// PUT: Updates staged input file
        /// </summary>
        /// <param name="name">Simulation name</param>
        /// <param name="inputName">input file name</param>
        /// <param name="data">new contents</param>
        /// <returns>Boolean success</returns>
        public bool UpdateStagedInputFile(string nameOrID, string inputName, Stream data)
        {
            Debug.WriteLine("UpdateStagedInputFile: " + nameOrID, this.GetType().Name);

            if (data == null)
            {
                string msg = String.Format("Simulation Resource {0} Data with input '{1}' is empty", 
                    nameOrID, inputName);
                Debug.WriteLine(msg, this.GetType().Name);
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    msg
                );
                throw new WebFaultException<ErrorDetail>(detail,
                    System.Net.HttpStatusCode.BadRequest);
            }

            //Simulation sim = GetSimulation(name);
            //OutgoingWebResponseContext ctx = WebOperationContext.Current.OutgoingResponse;
            //var content_type = WebOperationContext.Current.IncomingRequest.ContentType;
            var content_type = this.OutgoingWebResponseContext_ContentType;
            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                data.CopyTo(ms);
                bytes = ms.ToArray();
            }

            //StreamReader sr = new StreamReader(data);
            //string s = sr.ReadToEnd();
            //byte[] bytes = System.Text.Encoding.ASCII.GetBytes(s);
            Debug.WriteLine("content_type " + content_type, this.GetType().Name);

            // TODO: Validate Backup File
            var contract = ProducerSimulationContract.Get(nameOrID);
            if (contract == null)
            {
                string msg = String.Format("Simulation Resource {0} Does not Exist", nameOrID);
                Debug.WriteLine(msg, this.GetType().Name);
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    msg
                );
                throw new WebFaultException<ErrorDetail>(detail,
                    System.Net.HttpStatusCode.NotFound);
            }           

            bool success = false;
            try
            {
                success = contract.UpdateInput(inputName, bytes, content_type);
            }
            catch (ArgumentException ex)
            {
                string msg = String.Format("Failed to Update Simulation {0} input {1}: {2}",
                    nameOrID, inputName, ex.Message);
                Debug.WriteLine(msg, this.GetType().Name);
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.AUTHORIZATION_ERROR,
                    msg
                );
                throw new WebFaultException<ErrorDetail>(detail,
                    System.Net.HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                string msg = String.Format("Failed to Update Simulation {0} input {1}: {2}, {3}. ",
                    nameOrID, inputName, ex.Message, ex.StackTrace);
                Debug.WriteLine(msg, this.GetType().Name);
                if (ex.InnerException != null)
                {
                    Debug.WriteLine("InnerException: " + ex.InnerException.Message, this.GetType().Name);
                    Debug.WriteLine(ex.InnerException.StackTrace, this.GetType().Name);
                    msg += "InnerException: " + ex.InnerException.Message;
                    msg += " StackTrace: " + ex.InnerException.StackTrace;                    
                }
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    msg
                );
                throw new WebFaultException<ErrorDetail>(detail,
                    System.Net.HttpStatusCode.InternalServerError);
            }

            Debug.WriteLine("Success: " + success, this.GetType().Name);
            //sim = GetSimulation(name);
            //ctx.ContentType = content_type;
            //ctx.StatusCode = System.Net.HttpStatusCode.OK;
            this.OutgoingWebResponseContext_ContentType = content_type;
            this.OutgoingWebResponseContext_StatusCode = System.Net.HttpStatusCode.OK;

            //return new MemoryStream(bytes);
            return success;
        }

        public bool Validate(string nameOrID)
        {
            Guid simulationId = Guid.Empty;
            bool isGuid = Guid.TryParse(nameOrID, out simulationId);

            bool all = SimulationQueryParameters.GetRecurse(false);
            ISimulationProducerContract contract = null;
            try
            {
                contract = ProducerSimulationContract.Get(nameOrID);
            }
            catch (Exception ex)
            {
                string msg = String.Format("Failed to Get Simulation {0}: {1} {2}",
                    nameOrID, ex.Message, ex.StackTrace.ToString());
                if (ex.InnerException != null)
                    msg += String.Format("    InnerException:    {0} {1}",
                        ex.InnerException.Message, ex.InnerException.StackTrace.ToString());
                Debug.WriteLine(msg, this.GetType().Name);
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    msg
                );
                throw new WebFaultException<ErrorDetail>(detail,
                    System.Net.HttpStatusCode.InternalServerError);
            }
            return isGuid ? contract.Validate() : contract.ValidateAll();
        }
    }
}