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
using Turbine.Lite.Web.Resources.Contracts;
using Turbine.Producer.Data.Contract;
//using Turbine.Producer.Data.Contract;
//using Turbine.Data.Contract;


namespace Turbine.Lite.Web.Resources
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, IncludeExceptionDetailInFaults = true)]
    public class SessionResource : BaseResource, ISessionResource
    {
        public List<Guid> GetSessionList()
        {
            int page = QueryParameters_GetPage(1);
            int rpp = QueryParameters_GetPageSize(1000);
            Debug.WriteLine("GET Session List");
            return DataMarshal.GetSessions(page, rpp);
        }

        public Guid CreateSession()
        {
            ISessionProducerContract contract = new ProducerSessionContract();
            return contract.Create();
        }

        /// <summary>
        /// Returns List of Jobs in the Session
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns>List of Jobs in the Session</returns>
        public Stream GetSession(string sessionID)
        {
            int page = QueryParameters_GetPage(1);
            int rpp = QueryParameters_GetPageSize(1000);
            Guid session = new Guid(sessionID);
            bool verbose = QueryParameters_GetVerbose(false);
            bool meta = QueryParameters_GetMetaData(false);
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
            var jobs = DataMarshal.GetJobs(session, simulation, Guid.Empty, null, null, page, rpp, verbose);
            json = Newtonsoft.Json.JsonConvert.SerializeObject(jobs);
            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        }

        public int DeleteSession(string sessionID)
        {
            Guid session = new Guid(sessionID);
            ISessionProducerContract contract = new ProducerSessionContract();
            try
            {
                return contract.Delete(session);
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
            {
                var msg = "Failed to Delete session " + sessionID + ", UpdateException: " + ex.Message + ", InnerException: " + ex.InnerException;
                Debug.WriteLine(msg, GetType().Name);
                var detail = new ErrorDetail(ErrorDetail.Codes.JOB_STATE_ERROR, msg);
                //throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
                throw;
            }
            catch (Exception ex)
            {
                var msg = "Failed to Delete session " + sessionID + ", " + ex + ": " + ex.Message + ", InnerException: " + ex.InnerException;
                Debug.WriteLine(msg, GetType().Name);
                var detail = new ErrorDetail(ErrorDetail.Codes.JOB_STATE_ERROR, msg);
                throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
            }
        }

        /*
         
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns></returns>
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
         */

        /// <summary>
        /// Append new Jobs to session, in 'Create' state.
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="data">list of job requests  [ {"Simulation":.., "Input":..., "Initialize":..., "Reset":..., } ]</param>
        /// <returns></returns>
        public List<int> AppendJobs(string sessionID, System.IO.Stream data)
        {
            Debug.WriteLine(String.Format("append jobs to session {0}", sessionID), this.GetType().Name);
            Object name = "";
            Object inputs = "";
            Object initialize = false;
            Object reset = true;
            Object visible = false;
            Object jobId = "";
            Object consumerId = "";

            StreamReader sr = new StreamReader(data);
            string json = sr.ReadToEnd();
            List<Dictionary<string, Object>> jobsInputList =
                Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, Object>>>(json);
            List<Dictionary<String, Object>> jobDictList = new List<Dictionary<string, object>>();
            List<string> simulationList = new List<string>();
            List<bool> initializeList = new List<bool>();
            List<bool> resetList = new List<bool>();
            List<bool> visibleList = new List<bool>();
            List<Guid> jobIdList = new List<Guid>();
            List<Guid> consumerIdList = new List<Guid>();

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
                if (!inputData.TryGetValue("Visible", out visible))
                {
                    visible = false;
                }

                if (inputData.TryGetValue("Guid", out jobId))
                {
                    try
                    {
                        jobIdList.Add(Guid.Parse((string)jobId));
                    }
                    catch (InvalidCastException)
                    {
                        var detail = new ErrorDetail(
                            ErrorDetail.Codes.DATAFORMAT_ERROR,
                            "Key jobId is required to be a string/GUID"
                        );
                        throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
                    }
                    catch (FormatException)
                    {
                        var detail = new ErrorDetail(
                            ErrorDetail.Codes.DATAFORMAT_ERROR,
                            String.Format("Key jobId {0} is required to be a formatted as a GUID", jobId)
                        );
                        throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
                    }
                }
                else
                {
                    jobIdList.Add(Guid.Empty);
                }

                if (inputData.TryGetValue("ConsumerId", out consumerId))
                {
                    try
                    {
                        consumerIdList.Add(Guid.Parse((string)consumerId));
                    }
                    catch (InvalidCastException)
                    {
                        var detail = new ErrorDetail(
                            ErrorDetail.Codes.DATAFORMAT_ERROR,
                            "Key ConsumerId is required to be a string/GUID"
                        );
                        throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
                    }
                    catch (FormatException)
                    {
                        var detail = new ErrorDetail(
                            ErrorDetail.Codes.DATAFORMAT_ERROR,
                            String.Format("Key ConsumerId {0} is required to be a formatted as a GUID", consumerId)
                        );
                        throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
                    }
                }
                else
                {
                    consumerIdList.Add(Guid.Empty);
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
                    //Debug.WriteLine("VALUE: " + v);
                    dd[v.Key] = v.Value;
                }
                jobDictList.Add(dd);
                simulationList.Add(String.Format("{0}", name));
                try
                {
                    Debug.WriteLine("initialize String: " + initialize.ToString(), "SessionResource.AppendJobs");
                    Debug.WriteLine("initialize Type: " + initialize.GetType().ToString(), "SessionResource.AppendJobs");
                    initializeList.Add(Convert.ToBoolean(initialize));
                }
                catch (InvalidCastException e)
                {
                    Debug.WriteLine("Error: "  + e.Message, "SessionResource.AppendJobs");
                    Debug.WriteLine("StackTrace: " + e.StackTrace, "SessionResource.AppendJobs");
                    var detail = new ErrorDetail(
                        ErrorDetail.Codes.DATAFORMAT_ERROR,
                        "Key initialize is required to be a boolean"
                    );
                    throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
                }
                try
                {
                    resetList.Add(Convert.ToBoolean(reset));
                }
                catch (InvalidCastException)
                {
                    var detail = new ErrorDetail(
                        ErrorDetail.Codes.DATAFORMAT_ERROR,
                        "Key Reset is required to be a boolean"
                    );
                    throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
                }
                try
                {
                    visibleList.Add(Convert.ToBoolean(visible));
                }
                catch (InvalidCastException)
                {
                    var detail = new ErrorDetail(
                        ErrorDetail.Codes.DATAFORMAT_ERROR,
                        "Key Visible is required to be a boolean"
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
                    contract = ProducerSimulationContract.Get(simulationName);
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

                if (contract == null)
                {
                    string msg = String.Format("Failed to create new job {0}: No Simulation {1}",
                        i, simulationName);
                    var detail = new ErrorDetail(
                        ErrorDetail.Codes.DATAFORMAT_ERROR,
                        msg
                    );
                    throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.NotFound);
                }

                IJobProducerContract jobContract;
                try
                {
                    jobContract = contract.NewJob(new Guid(sessionID), initializeList[i], resetList[i], visibleList[i], jobIdList[i], consumerIdList[i]);
                    jobContract.Process.Input = dd;
                }
                catch (Exception ex)
                {
                    string msg = String.Format("Failed to create new job {0}: {1} {2}",
                        i, ex, ex.StackTrace.ToString());
                    Debug.WriteLine(msg, this.GetType().Name);
                    var detail = new ErrorDetail(
                        ErrorDetail.Codes.DATAFORMAT_ERROR,
                        msg
                    );
                    throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
                }
                
                jobList.Add(jobContract.Id);
            }

            return jobList;
        }


        /// <summary>
        ///  Copy session to a new session with the desired job state.
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns>Json session's new Guid and new jobs Count numbers</returns>
        public Stream CopySession(string sessionid)
        {
            string state = QueryParameters_GetData("error", "state");

            Dictionary<string, string> d = null;

            ISessionProducerContract contract = new ProducerSessionContract();

            try
            {
                Debug.WriteLine("Copying Session, jobs with state: " + state, "SessionResource.CopySession");
                d = contract.Copy(new Guid(sessionid), state);
            }
            catch (Exception ex)
            {
                var msg = "Failed to Copy session: " + sessionid + ", UpdateException: "
                    + ex.Message + ", InnerException: " + ex.InnerException + ", StackTrace: " + ex.StackTrace.ToString();
                Debug.WriteLine(msg, "SessionResource.CopySession");
                var detail = new ErrorDetail(
                            ErrorDetail.Codes.DATAFORMAT_ERROR,
                            String.Format("Couldn't Copy Session")
                        );
                throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
            }

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(d);

            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));

            //return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(result ?? ""));
        }
        

        /*

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
         */



        public int StartSession(string sessionID)
        {
            Debug.WriteLine(String.Format("start session {0}", sessionID), this.GetType().Name);
            ISessionProducerContract contract = new ProducerSessionContract();
            int total = 0;
            total += contract.Unpause(new Guid(sessionID));
            total += contract.Submit(new Guid(sessionID));
            Debug.WriteLine(String.Format("total {0}", total), this.GetType().Name);
            return total;
        }


        public int StopSession(string sessionID)
        {
            ISessionProducerContract contract = new ProducerSessionContract();
            return contract.Pause(new Guid(sessionID));
        }

        public int KillSession(string sessionID)
        {
            ISessionProducerContract contract = new ProducerSessionContract();
            return contract.Terminate(new Guid(sessionID));
        }

        /// <summary>
        ///  example: {"finished":100, "submit":10}
        ///  Query Parameters:
        ///  ?state="finished"&state="submit"
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns>JSON object with counts of all jobs in various states</returns>
        public Stream StatusSession(string sessionID)
        {
            ISessionProducerContract contract = new ProducerSessionContract();
            Dictionary<string, int> d = contract.Status(new Guid(sessionID));
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(d);
            //WebOperationContext.Current.OutgoingResponse.ContentType = "application/json; charset=utf-8";
            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));

        }
        public bool UpdateDescription(string sessionid, Stream data)
        {
            throw new NotImplementedException();
        }

        public Stream GetDescription(string sessionid)
        {
            throw new NotImplementedException();
        }

        public Guid CreateSessionResults(string sessionid)
        {
            Debug.WriteLine("Creating a new Generator", "SessionResource.CreateSessionResults");
            Guid generator = Guid.Empty;
            try
            {
                Guid sessionGuid = new Guid(sessionid);
                ISessionProducerContract contract = new ProducerSessionContract();
                generator = contract.CreateGenerator(sessionGuid);
            }
            catch (Exception ex)
            {
                var msg = "Failed to Create a new generator of session: " + sessionid + ", UpdateException: "
                    + ex.Message + ", InnerException: " + ex.InnerException + ", StackTrace: " + ex.StackTrace.ToString();
                Debug.WriteLine(msg, "SessionResource.CreateSessionResults");
                var detail = new ErrorDetail(
                            ErrorDetail.Codes.DATAFORMAT_ERROR,
                            String.Format("Couldn't Create a new generator")
                        );
                throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
            }
            return generator;
        }

        public int CreateSessionResultsPage(string sessionid, string generatorid)
        {
            Guid sessionGuid = new Guid(sessionid);
            Guid generatorGuid = new Guid(generatorid);

            int pagenum = 0;
            //try
            //{
                ISessionProducerContract contract = new ProducerSessionContract();
                pagenum = contract.CreateResultPage(sessionGuid, generatorGuid);

                if (pagenum == -1)
                {
                    var msg = "No resources available";
                    Debug.WriteLine(msg, "SessionResource.CreateSessionResultsPage");
                    var detail = new ErrorDetail(
                                ErrorDetail.Codes.DATAFORMAT_ERROR,
                                String.Format(msg)
                            );
                    throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.NotFound);
                }
                else if(pagenum == -2)
                {
                    var msg = "Bad Request, one or more Jobs are still in 'create' or 'pause' state";
                    
                    Debug.WriteLine(msg, "SessionResource.CreateSessionResultsPage");
                    var detail = new ErrorDetail(
                                ErrorDetail.Codes.DATAFORMAT_ERROR,
                                String.Format(msg)
                            );
                    throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
                }
            //}
            /*catch (Exception ex)
            {
                var msg = "Failed to Create a new result page " + ", UpdateException: "
                    + ex.Message + ", InnerException: " + ex.InnerException + ", StackTrace: " + ex.StackTrace.ToString();
                Debug.WriteLine(msg, "SessionResource.CreateSessionResultsPage");
                var detail = new ErrorDetail(
                            ErrorDetail.Codes.DATAFORMAT_ERROR,
                            String.Format(msg)
                        );
                throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
            }*/
                        
            return pagenum;
        }

        public Stream StartSessionGeneratorResults(string sessionid, string generatorid, string pagenum)
        {
            bool verbose = JobQueryParameters.GetVerbose(false);
            int page = 0;
            int sub_page = QueryParameters_GetPage(1);
            int rpp = QueryParameters_GetPageSize(1000);

            Int32.TryParse(pagenum, out page);

            Guid sessionGuid = new Guid(sessionid);
            Guid generatorGuid = new Guid(generatorid);

            string json = "";

            List<Dictionary<string, object>> joblist = null; 
            try
            {
                joblist = DataMarshal.GetGeneratorPage(generatorGuid, page, sub_page, rpp, verbose);
                json = Newtonsoft.Json.JsonConvert.SerializeObject(joblist);
            }
            catch (Exception ex)
            {
                var msg = "Failed to get page: " + pagenum + ", UpdateException: "
                    + ex.Message + ", InnerException: " + ex.InnerException + ", StackTrace: " + ex.StackTrace.ToString();
                Debug.WriteLine(msg, "SessionResource.StartSessionGeneratorResults");
                var detail = new ErrorDetail(
                            ErrorDetail.Codes.DATAFORMAT_ERROR,
                            String.Format("Failed to get page")
                        );
                throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
            }

            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        }

        public string DeleteSessionGeneratorResults(string sessionid, string generatorid)
        {
            Guid sessionGuid = new Guid(sessionid);
            Guid generatorGuid = new Guid(generatorid);

            string output = "Failed to Delete Generator";

            try
            {
                output = DataMarshal.DeleteGenerator(generatorGuid) ? "Generator has been successfully deleted"
                    : output;
            }
            catch (Exception ex)
            {
                var msg = "Failed to Delete Generator: " + generatorid + ", UpdateException: "
                    + ex.Message + ", InnerException: " + ex.InnerException + ", StackTrace: " + ex.StackTrace.ToString();
                Debug.WriteLine(msg, "SessionResource.DeleteSessionGeneratorResults");
                var detail = new ErrorDetail(
                            ErrorDetail.Codes.DATAFORMAT_ERROR,
                            String.Format("Failed to Delete Generator")
                        );
                throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.BadRequest);
            }

            return output;            
        }
    }
}