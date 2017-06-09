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
using Turbine.Producer.Data.Contract;
using Turbine.Data.Contract;
using Turbine.Data.Marshal;


namespace Turbine.Web.Resources
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, IncludeExceptionDetailInFaults = true)]
    public class SessionResource : ISessionResource
    {
        public List<Guid> GetSessionList()
        {
            int page = QueryParameters.GetPage(1);
            int rpp = QueryParameters.GetPageSize(1000);
            return DataMarshal.GetSessions(page, rpp);
        }

        public Guid CreateSession()
        {
            ISessionProducerContract contract = new AspenSessionProducerContract();
            return contract.Create();
        }

        public Stream GetSession(string sessionID)
        {
            int page = QueryParameters.GetPage(1);
            int rpp = QueryParameters.GetPageSize(1000);
            Guid session = new Guid(sessionID);
            bool verbose = QueryParameters.GetVerbose(false);
            bool meta = QueryParameters.GetMetaData(false);
            string json = "";

            if (meta)
            {
                Debug.WriteLine(String.Format("get session {0} metadata", sessionID), this.GetType().Name);
                var sessionMeta = DataMarshal.GetSessionMeta(session);
                json = Newtonsoft.Json.JsonConvert.SerializeObject(sessionMeta);
                return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
            }
            string simulation = null;
            Debug.WriteLine(String.Format("get session {0} job resources {1},{2}", sessionID, page, rpp), this.GetType().Name);
            var jobs = DataMarshal.GetJobs(session, simulation, Guid.Empty, null, page, rpp, verbose);
            json = Newtonsoft.Json.JsonConvert.SerializeObject(jobs);
            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        }

        public int DeleteSession(string sessionID)
        {
            Guid session = new Guid(sessionID);
            ISessionProducerContract contract = new AspenSessionProducerContract();
            try
            {
                return contract.Delete(session);
            }
            catch (System.Data.UpdateException ex)
            {
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.JOB_STATE_ERROR,
                    "Failed to Delete session " + sessionID + ", " + ex.Message + ", InnerException: " + ex.InnerException
                );
                throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.JOB_STATE_ERROR,
                    "Failed to Delete session " + sessionID + ", " + ex.Message +
                    ", Inner Exception: " + ex.InnerException
                );
                throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
            }
        }

        public int StartSession(string sessionID)
        {
            Debug.WriteLine(String.Format("start session {0}", sessionID), this.GetType().Name);
            ISessionProducerContract contract = new AspenSessionProducerContract();
            int total = 0;
            total += contract.Unpause(new Guid(sessionID));
            total += contract.Submit(new Guid(sessionID));
            Debug.WriteLine(String.Format("total {0}", total), this.GetType().Name);
            return total;
        }

        public int StopSession(string sessionID)
        {
            ISessionProducerContract contract = new AspenSessionProducerContract();
            return contract.Pause(new Guid(sessionID));
        }

        /* KillSession:
         *     Returns total number of jobs moved to cancel or terminate
         *     QueryParameters:
         *         ?jobid=100&jobid=120
         */
        public int KillSession(string sessionID)
        {
            ISessionProducerContract contract = new AspenSessionProducerContract();
            int total = 0;
           
            try
            {
                total += contract.Cancel(new Guid(sessionID));
                total += contract.Terminate(new Guid(sessionID));
            }
            catch (AuthorizationError ex)
            {
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.AUTHORIZATION_ERROR,
                    ex.Message
                );
                throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.Unauthorized);
            }
            return total;
        }

        /*  AppendJobs
         * 
         * Each job request has a "Simulation" and "Input" 
         * 
         */
        public List<int> AppendJobs(string sessionID, System.IO.Stream data)
        {
            Debug.WriteLine(String.Format("append jobs to session {0}", sessionID), this.GetType().Name);
            Object name = "";
            Object inputs = "";
            Object initialize = false;
            Object reset = true;

            StreamReader sr = new StreamReader(data);
            string json = sr.ReadToEnd();
            List<Dictionary<string, Object>> jobsInputList =
                Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, Object>>>(json);
            List<Dictionary<String, Object>> jobDictList = new List<Dictionary<string, object>>();
            List<string> simulationList = new List<string>();
            List<bool> initializeList = new List<bool>();
            List<bool> resetList = new List<bool>();

            foreach (var inputData in jobsInputList)
            {
                if (!inputData.TryGetValue("Simulation", out name))
                {
                    Debug.WriteLine("Missing simulation name", this.GetType().Name);
                    var detail = new ErrorDetail(
                        ErrorDetail.Codes.DATAFORMAT_ERROR,
                        "Key Simulation is required"
                    );
                    throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
                }

                if (!inputData.TryGetValue("Input", out inputs))
                {
                    Debug.WriteLine("Missing input data", this.GetType().Name);
                    throw new ArgumentException("No Input data name");
                }

                if (!inputData.TryGetValue("Initialize", out initialize))
                {
                    initialize = false;
                }
                if (!inputData.TryGetValue("Reset", out reset))
                {
                    reset = true;
                }

                Newtonsoft.Json.Linq.JObject inputsD = null;
                try
                {
                    inputsD = (Newtonsoft.Json.Linq.JObject)inputs;
                }
                catch (InvalidCastException)
                {
                    var detail = new ErrorDetail(
                        ErrorDetail.Codes.DATAFORMAT_ERROR,
                        "Key Input is required to be a dictionary"
                    );
                    throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
                }

                Dictionary<String, Object> dd = new Dictionary<string, object>();
                foreach (var v in inputsD)
                {
                    Debug.WriteLine("VALUE: " + v);
                    dd[v.Key] = v.Value;
                }
                jobDictList.Add(dd);
                simulationList.Add(String.Format("{0}", name));
                try
                {
                    initializeList.Add((bool)initialize);
                }
                catch (InvalidCastException)
                {
                    var detail = new ErrorDetail(
                        ErrorDetail.Codes.DATAFORMAT_ERROR,
                        "Key initialize is required to be a boolean"
                    );
                    throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
                }
                try
                {
                    resetList.Add((bool)reset);
                }
                catch (InvalidCastException)
                {
                    var detail = new ErrorDetail(
                        ErrorDetail.Codes.DATAFORMAT_ERROR,
                        "Key Reset is required to be a boolean"
                    );
                    throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
                }
            }

            List<int> jobList = new List<int>();
            // Formatting complete
            for (int i = 0; i < jobDictList.Count; i++)
            {
                ISimulationProducerContract contract = null;
                string simulationName = simulationList[i];
                Dictionary<String, Object> dd = jobDictList[i];

                try
                {
                    contract = AspenSimulationContract.Get(simulationName);
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

                IJobProducerContract jobContract;
                try
                {
                    jobContract = contract.NewJob(new Guid(sessionID), initializeList[i], resetList[i]);
                }
                catch (Exception ex)
                {
                    string msg = String.Format("Failed to create new job {0}: {1}",
                        i, ex.StackTrace.ToString());
                    Debug.WriteLine(msg, this.GetType().Name);
                    var detail = new ErrorDetail(
                        ErrorDetail.Codes.DATAFORMAT_ERROR,
                        msg
                    );
                    throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
                }
                jobContract.Process.Input = dd;
                jobList.Add(jobContract.Id);
            }

            return jobList;
        }

        /* Status:
         *  Returns a JSON object with counts of all jobs in various states
         *      {"finished":100, "submit":10}
         *  Query Parameters:
         *      ?state="finished"&state="submit"
         *      
         * 
         */
        public Stream StatusSession(string sessionID)
        {
            ISessionProducerContract contract = new AspenSessionProducerContract();
            Dictionary<string,int> d = contract.Status(new Guid(sessionID));
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(d);
            //WebOperationContext.Current.OutgoingResponse.ContentType = "application/json; charset=utf-8";
            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));

        }


        public bool UpdateDescription(string sessionID, Stream data)
        {
            var content_type = WebOperationContext.Current.IncomingRequest.ContentType;

            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                data.CopyTo(ms);
                bytes = ms.ToArray();
            }
            var s = System.Text.Encoding.UTF8.GetString(bytes);

            return DataMarshal.UpdateDescription(sessionID, s);
        }

        public Stream GetDescription(string sessionID)
        {
            var sessionMeta = DataMarshal.GetSessionMeta(new Guid(sessionID));
            Debug.WriteLine("GetDescription: " + sessionMeta);
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(sessionMeta.Descrption);
            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        }
    }
}