using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Diagnostics;
using Turbine.Web.Contracts;
using Turbine.Data.Serialize;
using Turbine.Data.Contract;
using System.IO;
using Turbine.Data.Contract.Behaviors;
using Turbine.Producer.Data.Contract.Behaviors;
using Turbine.Producer.Data.Contract;
//using Turbine.Data.Marshal;
using Turbine.Lite.Web.Resources.Contracts;


namespace Turbine.Lite.Web.Resources
{
    class JobQueryParameters : QueryParameters
    {
        internal static Guid GetSessionId()
        {
            Guid val;
            string param = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["session"];
            if (string.IsNullOrEmpty(param))
                return Guid.Empty;

            if (!Guid.TryParse(param, out val))
            {
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.QUERYPARAM_ERROR,
                    "session failed to parse as Guid"
                );
                throw new WebFaultException<ErrorDetail>(detail,
                    System.Net.HttpStatusCode.BadRequest);
            }
            return val;
        }

        internal static string GetSimulationName()
        {
            return WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["simulation"];
        }
        internal static string GetState()
        {
            return WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["state"];
        }
        internal static string GetOrder()
        {
            return WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["order"];
        }
        internal static Guid GetConsumerId()
        {
            Guid val;
            string param = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["consumer"];
            if (string.IsNullOrEmpty(param))
                return Guid.Empty;

            if (!Guid.TryParse(param, out val))
            {
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.QUERYPARAM_ERROR,
                    "session failed to parse as Guid"
                );
                throw new WebFaultException<ErrorDetail>(detail,
                    System.Net.HttpStatusCode.BadRequest);
            }
            return val;
        }
    }

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, IncludeExceptionDetailInFaults=true)]
    public class JobResource : IJobResource
    {
        public Stream GetJobResources()
        {
            int page = JobQueryParameters.GetPage(1);
            int rpp = JobQueryParameters.GetPageSize(1000);
            bool verbose = JobQueryParameters.GetVerbose(false);
            Guid sessionID = JobQueryParameters.GetSessionId();
            string simulation = JobQueryParameters.GetSimulationName();
            string state = JobQueryParameters.GetState();
            string order = JobQueryParameters.GetOrder();
            Guid consumerID = JobQueryParameters.GetConsumerId();

            Debug.WriteLine(String.Format("get job resources {0},{1}",page,rpp), this.GetType().Name);
            List<Dictionary<string, Object>> jobs = null;
            try
            {
                jobs = DataMarshal.GetJobs(sessionID, simulation, consumerID, state, order, page, rpp, verbose);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(String.Format("EXCEPTION: {0}   {1}", ex, ex.Message), this.GetType().Name);
                Debug.WriteLine("Stack Trace: " + ex.StackTrace, this.GetType().Name);
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    "Get Jobs Query Failed\n\nMessage: " + ex.Message + "\n\n\nStack Trace: " + ex.StackTrace
                    );
                throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
            }
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(jobs);
            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        }

  
        public Stream GetJob(string id)
        {
            bool verbose = JobQueryParameters.GetVerbose(false);
            Debug.WriteLine("GET job: " + id, this.GetType().Name);

            int myid = Convert.ToInt32(id);
            Dictionary<string, Object> d;
            try
            {
                d = DataMarshal.GetJobDict(myid, verbose);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(String.Format("EXCEPTION: {0}   {1}", ex, ex.Message), this.GetType().Name);
                Debug.WriteLine("Stack Trace: " + ex.StackTrace, this.GetType().Name);
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    "No Job.Id=" + id
                    );
                throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.NotFound);
            }
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(d);
            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        }


        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data">JSON Object { Session, Simulation, initialize, reset, Inputs }</param>
        /// <returns></returns>
        public Stream CreateJob(Stream data)
        {
            Debug.WriteLine("create new job", this.GetType().Name);
            StreamReader sr = new StreamReader(data);
            string json = sr.ReadToEnd();
            Debug.WriteLine("JSON: " + json, this.GetType().Name);
            Dictionary<string, Object> inputData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Object>>(json);
            Object name = "";
            Guid sessionID;

            if (!inputData.TryGetValue("Session", out name))
            {
                Debug.WriteLine("Missing Session", this.GetType().Name);
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    "Key Session is required"
                );
                throw new WebFaultException<ErrorDetail>(detail, 
                    System.Net.HttpStatusCode.BadRequest);
            }
            try
            {
                sessionID = Guid.Parse((string)name);
            }
            catch (Exception)
            {
                Debug.WriteLine("Bad Session", this.GetType().Name);
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    "Bad Session ID, must be GUID"
                );
                throw new WebFaultException<ErrorDetail>(detail, 
                    System.Net.HttpStatusCode.BadRequest);
            
            }

            if (!inputData.TryGetValue("Simulation", out name))
            {
                Debug.WriteLine("Missing simulation name", this.GetType().Name);
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    "Key Simulation is required"
                );
                throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
            }


            Object tmp = false;
            if (!inputData.TryGetValue("initialize", out tmp))
            {
                tmp = false;
            }
            bool initialize;
            try
            {
                initialize = (bool)tmp;
            }
            catch (InvalidCastException)
            {
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    "Key initialize is required to be a boolean"
                );
                throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
            }

            if (!inputData.TryGetValue("reset", out tmp))
            {
                tmp = true;
            }
            bool reset;
            try
            {
                reset = (bool)tmp;
            }
            catch (InvalidCastException)
            {
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    "Key reset is required to be a boolean"
                );
                throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
            }

            Debug.WriteLine("Retrieve Simulation Contract: " + name, this.GetType().Name);
            ISimulationProducerContract contract = null;
            try
            {
                contract = ProducerSimulationContract.Get((string)name);
            }
            catch (Exception ex)
            {
                string msg = String.Format("Failed to get Simulation {0}: {1}",
                    name, ex.StackTrace.ToString());
                Debug.WriteLine(msg, this.GetType().Name);
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    msg
                );
                throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
            }
            Object inputs;

            if (!inputData.TryGetValue("Inputs", out inputs))
            {
                Debug.WriteLine("Missing input data", this.GetType().Name);
                throw new ArgumentException("No Input data name");
            }
            Debug.WriteLine("Found Inputs", this.GetType().Name);
            Newtonsoft.Json.Linq.JObject inputsD = null;
            try
            {
                inputsD = (Newtonsoft.Json.Linq.JObject)inputs;
            }
            catch (InvalidCastException)
            {
                var detail = new ErrorDetail( 
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    "Key Inputs is required to be a dictionary"
                );
                throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
            }
            Debug.WriteLine("Cast Inputs", this.GetType().Name);

            Dictionary<String, Object> dd = new Dictionary<string, object>();
            foreach (var v in inputsD)
            {
                Debug.WriteLine("VALUE: " + v);
                dd[v.Key] = v.Value;
            }
            var jobContract = contract.NewJob(sessionID, initialize, reset);
            jobContract.Process.Input = dd;

            var d = DataMarshal.GetJobDict(jobContract.Id, true);
            json = Newtonsoft.Json.JsonConvert.SerializeObject(d);
            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        }

        /*
        public Stream SubmitJob(string id)
        {
            Debug.WriteLine("submit job " + id, this.GetType().Name);
            int myid = Convert.ToInt32(id);
            IJobProducerContract contract = new AspenJobProducerContract(myid);
            try
            {
                contract.Submit();
            }
            catch (Exception ex)
            {
                var detail = new ErrorDetail( 
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    "Failed to Submit job " + id + ", traceback " + ex.StackTrace.ToString()
                );
                throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
            }
            var d = DataMarshal.GetJobDict(contract.Id, true);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(d);
            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        }


        public Stream CancelJob(string id)
        {
            Debug.WriteLine("cancel job " + id, this.GetType().Name);
            int myid = Convert.ToInt32(id);
            IJobProducerContract contract = new AspenJobProducerContract(myid);
            try
            {
                contract.Cancel();
            }
            catch (Exception ex)
            {
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.JOB_STATE_ERROR,
                    "Failed to Cancel job " + id + ", " + ex.Message
                );
                throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
            }
            var d = DataMarshal.GetJobDict(contract.Id, true);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(d);
            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        }
         */

        public Stream TerminateJob(string id)
        {
            Debug.WriteLine("terminate job " + id, this.GetType().Name);
            int myid = Convert.ToInt32(id);
            IJobProducerContract contract = new ProducerJobContract(myid);
            try
            {
                contract.Terminate();
            }
            catch (Exception ex)
            {
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.JOB_STATE_ERROR,
                    "Failed to Terminate job " + id + ", " + ex.Message
                );
                throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
            }
            var d = DataMarshal.GetJobDict(contract.Id, true);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(d);
            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        }

        public Stream KillJob(string id)
        {
            Debug.WriteLine("kill job " + id, this.GetType().Name);
            int myid = Convert.ToInt32(id);
            IJobProducerContract contract = new ProducerJobContract(myid);
            try
            {
                contract.Kill();
            }
            catch (Exception ex)
            {
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.JOB_STATE_ERROR,
                    "Failed to Kill job " + id + ", " + ex.Message
                );
                throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
            }
            var d = DataMarshal.GetJobDict(contract.Id, true);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(d);
            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        }

        public Stream SubmitJob(string id)
        {
            throw new NotImplementedException();
        }

        public Stream CancelJob(string id)
        {
            throw new NotImplementedException();
        }
        /*
        public Stream TerminateJob(string id)
        {
            throw new NotImplementedException();
        }
        */

        public Stream CreateJob(Stream data)
        {
            throw new NotImplementedException();
        }
    }
}