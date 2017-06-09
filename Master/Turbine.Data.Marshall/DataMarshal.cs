using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Diagnostics;


namespace Turbine.Data.Marshal
{
    public static class JSON
    {
        public static string Serialize<T>(T obj)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, obj);
                return Encoding.Default.GetString(ms.ToArray());
            }
        }
        public static T Deserialise<T>(string json)
        {
            T obj = Activator.CreateInstance<T>();
            using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
                obj = (T)serializer.ReadObject(ms);
                return obj;
            }
        }
    }

    public class DataMarshal
    {
        public static Serialize.Application GetApplication(string appname)
        {
            Serialize.Application con = null;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                var entity = container.Applications.Single<Turbine.Data.Application>(s => s.Name == appname);
                con = new Serialize.Application();
                con.Name = entity.Name;
                con.Inputs = new Serialize.InputTypeList();

                foreach (InputFileType i in entity.InputFileTypes)
                {
                    con.Inputs.Add(new Serialize.InputType() { Name = i.Name, Type = i.Type, Required = i.Required });
                }        
            }
            return con;
        }

        public static Serialize.ApplicationList GetApplicationList()
        {
            Serialize.Application con = null;
            Serialize.ApplicationList coll = new Serialize.ApplicationList();

            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                System.Data.Objects.ObjectQuery<Turbine.Data.Application> query = container.Applications;
                foreach (Turbine.Data.Application entity in query)
                {
                    con = new Serialize.Application();
                    con.Name = entity.Name;
                    con.Inputs = new Serialize.InputTypeList();

                    foreach (InputFileType i in entity.InputFileTypes) 
                    {
                        con.Inputs.Add(new Serialize.InputType() { Name = i.Name, Type = i.Type, Required = i.Required });
                    }
                    coll.Add(con);
                }
            }
            return coll;
        }

        public static Serialize.Consumers GetConsumers(String status)
        {
            Serialize.Consumer con = null;
            Serialize.Consumers coll = new Serialize.Consumers();
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                System.Data.Objects.ObjectQuery<Turbine.Data.JobConsumer> query = container.JobConsumers;
                if (status != null)
                {
                    query = container.JobConsumers.Where("it.status = @status",
                        new System.Data.Objects.ObjectParameter("status", status));
                }
                foreach (Turbine.Data.JobConsumer entity in query)
                {
                    con = GetRepresentation(entity);
                    coll.Add(con);
                }
            }
            return coll;
        }

        public static Serialize.Consumer GetConsumer(Guid guid)
        {
            Serialize.Consumer con = null;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                System.Data.Objects.ObjectQuery<Turbine.Data.JobConsumer> query = container.JobConsumers;
                var entity = query.Single<Turbine.Data.JobConsumer>(c => c.Id == guid);
                con = GetRepresentation(entity);
            }
            return con;
        }

        public static Serialize.Simulations GetSimulations()
        {
            Serialize.Simulation sim = null;
            Serialize.Simulations coll = new Serialize.Simulations();
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                var esim = container.Simulations.OrderByDescending(q => q.Create);
                if (esim != null)
                {
                    List<string> l = new List<string>();
                    foreach (Turbine.Data.Simulation entity in esim)
                    {
                        if (l.Contains<string>(entity.Name) == false)
                        {
                            l.Add(entity.Name);
                            sim = GetRepresentation(entity);
                            coll.Add(sim);
                        }
                    }
                }
            }
            return coll;
        }

        public static Serialize.Simulation GetSimulation(string name)
        {
            Serialize.Simulation sim = null;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Turbine.Data.Simulation entity = container.Simulations
                    .OrderByDescending(q => q.Create).First(s => s.Name == name);
                sim = GetRepresentation(entity);
            }
            return sim;
        }

        public static Serialize.JobResourceSummary GetJobResourceSummary()
        {
            Serialize.JobResourceSummary summary = new Serialize.JobResourceSummary();
            summary.Id = new List<int>{};
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                foreach (var entity in container.Jobs.Select( s => new { s.Id }))
                {
                    summary.Id.Add(entity.Id);
                }
            }
            return summary;
        }
        /*
        public static Jobs GetJobs()
        {
            bool verbose = false;
            Serialize.Job job = null;
            Serialize.Jobs col = new Serialize.Jobs();
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                foreach (Turbine.Data.Job entity in container.Jobs)
                {
                    job = GetRepresentation(entity, verbose);
                    col.Add(job);
                }
            }
            return col;
        }
        */
        /* GetJobs
         *   Returns Jobs collection
         *   Arguments
         *     page - starting at 1
         *     rpp -- results per page, must be > 0
         */
        public static List<Dictionary<string,Object>> GetJobs(Guid sessionID, string simulation, 
            Guid consumerID, string state, Int32 page, Int32 rpp, bool verbose)
        {
            List<Dictionary<string, Object>> jobs = new List<Dictionary<string, Object>>();
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                System.Data.Objects.ObjectQuery<Turbine.Data.Job> jobQuery = container.Jobs;
                if (!Guid.Empty.Equals(sessionID))
                {
                    jobQuery = jobQuery.Where("it.SessionId = @session",
                        new System.Data.Objects.ObjectParameter("session", sessionID));
                }
                if (!Guid.Empty.Equals(consumerID))
                {
                    jobQuery = jobQuery.Where("it.ConsumerId = @consumer",
                        new System.Data.Objects.ObjectParameter("consumer", consumerID));
                }
                // NOTE: This option will return all jobs for all versions of Simulation
                if (!String.IsNullOrEmpty(simulation))
                {
                    foreach (Turbine.Data.Simulation entity in container.Simulations.Where(i => i.Name == simulation)) 
                    {
                        jobQuery = jobQuery.Where("it.SimulationId = @simulationId",
                            new System.Data.Objects.ObjectParameter("simulationId", entity.Id));
                        // TODO: ADD A "OR"
                        break;
                    }
                }
                if (!String.IsNullOrEmpty(state))
                {
                    jobQuery = jobQuery.Where("it.State = @state",
                        new System.Data.Objects.ObjectParameter("state", state));
                }
                jobQuery = jobQuery
                    .Skip("it.Id", "@skip", new System.Data.Objects.ObjectParameter("skip", (page - 1) * rpp))
                    .Top("@limit", new System.Data.Objects.ObjectParameter("limit", rpp));

                foreach (Turbine.Data.Job entity in jobQuery)
                {
                    var job = GetJobRepresentation(entity, verbose);
                    jobs.Add(job);
                }
            }
            return jobs;
        }

        private static string ConvertDateTime(DateTime stamp) 
        {
            var fmt = System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat;
            return stamp.ToString("O", fmt);
        }

        private static Serialize.Simulation GetRepresentation(Turbine.Data.Simulation entity)
        {
            var sim = new Serialize.Simulation()
            {
                Name = entity.Name,
                StagedInputs = null,
                Application = entity.Application.Name
            };
            //var config = entity.SimulationStagedInputs.SingleOrDefault<SimulationStagedInput>(
            //    s => s.Name == "configuration");
            var list = new List<String>();
            foreach (var e in entity.SimulationStagedInputs) 
            {
                list.Add(e.Name);
            }
            sim.StagedInputs = list.ToArray<string>();
            Debug.WriteLine(
                String.Format("===== Simulation {0}",
                sim.Name),
                "DataMarshall");
            return sim;
        }

        private static Serialize.Consumer GetRepresentation(Turbine.Data.JobConsumer entity)
        {
            var con = new Serialize.Consumer()
            {
                Id = entity.Id.ToString(),
                instanceID = entity.instance,
                hostname = entity.hostname,
                status = entity.status,
                AMI = entity.AMI,
                processID = entity.processId
            };
            return con;
        }

        /// <summary>
        /// GetSessions:  WHERE is evaluated before Order By, SQLServer 2008 does not provide
        /// a 'Limit' functionality.  Paging based on a column must be programmed.  Example below implements
        /// "skip" by using WHERE row_number, thus order happens after the skip...
        ///              
        ///        System.Data.Objects.ObjectQuery<Turbine.Data.Session> query = container.Sessions
        ///            .OrderBy("it.Create")
        ///            .Skip("it.Id", "@skip", new System.Data.Objects.ObjectParameter("skip", (page - 1) * rpp))
        ///            .Top("@limit", new System.Data.Objects.ObjectParameter("limit", rpp));
        /// </summary>
        /// <param name="page"></param>
        /// <param name="rpp"></param>
        /// <returns></returns>
        public static List<Guid> GetSessions(Int32 page, Int32 rpp)
        {
            List<Guid> l = new List<Guid>();
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                System.Data.Objects.ObjectQuery<Turbine.Data.Session> query = container.Sessions
                   .OrderBy("it.Create");
                int count = query.Count<Turbine.Data.Session>();
                if (count == 0) return l;
                  
                if (page > 0)
                {
                    query = query.Top("@top", new System.Data.Objects.ObjectParameter("top", rpp*page));
                }
                else if (page == -1)
                {
                    page = count / rpp;
                    Debug.WriteLine("Page " + page);
                    Debug.WriteLine("Count " + count);
                    Debug.WriteLine("Modulo " + count % rpp);
                    if (count % rpp > 0)
                        page += 1;
                }
                Debug.WriteLine("Page " + page);
                Debug.WriteLine(query.ToTraceString());
                int i = 1;
                int skip = (page - 1) * rpp;
                foreach (Turbine.Data.Session entity in query)
                {
                    if (i > skip)
                    {
                        l.Add(entity.Id);
                        if ((rpp - (i - skip)) == 0)
                            break;
                    }
                    i += 1;
                }
            }
            return l;
        }

        public static Dictionary<string, object> GetJobDict(int id, bool verbose)
        {
            var dict = new Dictionary<string, Object>();
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Turbine.Data.Job entity = container.Jobs.Single(s => s.Id == id);
                dict = GetJobRepresentation(entity, verbose);
            }
            return dict;
        }

        private static Dictionary<string, object> GetJobRepresentation(Data.Job entity, bool verbose)
        {
            var dict = new Dictionary<string, Object>();
            string input = entity.Process.Input;
            IDictionary<string,Object> inputDict = new Dictionary<string, Object>() { };
            if (input != null)
                    inputDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Object>>(input);

            string output = entity.Process.Output;
            IDictionary<string, Object> outputDict = null;
            if (output != null)
            {
                outputDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Object>>(output);
            }
            string[] msg_array = null;
            if (verbose)
            {
                msg_array = entity.Messages.Select<Message, string>(s => s.Value).ToArray<string>();
            }

            Serialize.ProcessErrorList errorList = new Serialize.ProcessErrorList();

            foreach (var error in entity.Process.Errors)
            {
                var pe = new Serialize.ProcessError();
                pe.Error = error.Error;
                pe.Type = error.Type;
                pe.Name = error.Name;
                errorList.Add(pe);
            }

            int status;
            try
            {
                status = (int)entity.Process.Status;
            }
            catch (Exception)
            {
                status = -1;
            }

            dict["Id"] = entity.Id;
            dict["Simulation"] = entity.Simulation.Name;
            dict["State"] = entity.State;
            dict["Messages"] = msg_array;
            dict["Input"] = inputDict;
            dict["Output"] = outputDict;
            dict["Errors"] = errorList;
            dict["Status"] = status;
            dict["Session"] = entity.Session.Id;
            dict["Initialize"] = entity.Initialize;
            dict["Reset"] = entity.Reset;

            if (entity.Consumer != null)
                dict["Consumer"] = entity.Consumer.Id;
            if (entity.Create != null)
                dict["Create"] = ConvertDateTime((DateTime)entity.Create);
            if (entity.Submit != null)
                dict["Submit"] = ConvertDateTime((DateTime)entity.Submit);
            if (entity.Setup != null)
                dict["Setup"] = ConvertDateTime((DateTime)entity.Setup);
            if (entity.Running != null)
                dict["Running"] = ConvertDateTime((DateTime)entity.Running);
            if (entity.Finished != null)
                dict["Finished"] = ConvertDateTime((DateTime)entity.Finished);

            return dict;
        }

        public static SimulationStagedInput GetStagedInputFile(string name, string inputName)
        {
            SimulationStagedInput ssi = null;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Turbine.Data.Simulation entity = container.Simulations
                    .OrderByDescending(q => q.Create).First(s => s.Name == name);
                var si = entity.SimulationStagedInputs.
                    SingleOrDefault<SimulationStagedInput>(f => f.Name == inputName);
                if (si != null)
                {
                    ssi = new SimulationStagedInput() {Content=si.Content, Name=si.Name, InputFileType=si.InputFileType};
                }

            }

            return ssi;
        }

        public static string[] GetStagedInputs(string name)
        {
            List<string> l = new List<string>();
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                var se = container.Simulations
                    .OrderByDescending(q => q.Create).First(s => s.Name == name);
                foreach (var entity in se.SimulationStagedInputs)
                {
                    l.Add(entity.Name);
                }
            }
            return l.ToArray<string>();
        }

        public static Session GetSessionMeta(Guid id)
        {
            Session session = null;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Turbine.Data.Session entity = container.Sessions.Single(s => s.Id == id);
                session = new Session() { Id = entity.Id, 
                    Create = entity.Create, 
                    Finished = entity.Finished, 
                    Descrption = entity.Descrption };
            }
            return session;
        }

        public static bool UpdateDescription(string sessionid, string description)
        {
            Guid id = Guid.Parse(sessionid);
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Turbine.Data.Session entity = container.Sessions.Single(s => s.Id == id);
                entity.Descrption = description;
                container.SaveChanges();
            }
            return true;
        }
    }
}