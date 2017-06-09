using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Turbine.Data.Serialize;
using Turbine.DataEF6;
using System.Data.Entity.Core.Objects;

namespace Turbine.Lite.Web.Resources.Contracts
{
    public class IQueryableExpression
    {
        public IQueryable<T> OrderByField<T>(IQueryable<T> q, string SortField, bool Ascending)
        {
            var param = Expression.Parameter(typeof(T), "p");
            var prop = Expression.Property(param, SortField);
            var exp = Expression.Lambda(prop, param);
            string method = Ascending ? "OrderBy" : "OrderByDescending";
            Type[] types = new Type[] { q.ElementType, exp.Body.Type };
            var mce = Expression.Call(typeof(Queryable), method, types, q.Expression, exp);
            return q.Provider.CreateQuery<T>(mce);
        }
    }
    class DataMarshal
    {
        internal static ApplicationList GetApplicationList()
        {
            var appList = new ApplicationList();
            Debug.WriteLine("Application", "TurbineLite");
            using (var db = new ProducerContext())
            {
                Debug.WriteLine("Application ProducerContext", "TurbineLite");
                foreach (var entity in db.Applications)
                {
                    Debug.WriteLine("Application Add", "TurbineLite");
                    Application app = new Application { Name = entity.Name, Inputs = new InputTypeList() };
                    appList.Add(app);
                    foreach (var i in app.Inputs)
                    {
                        app.Inputs.Add(new InputType() { Name = i.Name, Required = i.Required, Type = i.Type });
                    }
                }
            }
            return appList;
        }

        internal static Application GetApplication(string name)
        {
            Application app = null;
            using (var db = new ProducerContext())
            {
                var entity = db.Applications.Single(a => a.Name == name);
                app = new Application { Name = entity.Name, Inputs=new InputTypeList() };
                foreach (var i in app.Inputs) {
                    app.Inputs.Add(new InputType() { Name = i.Name, Required = i.Required, Type = i.Type });
                }
                
            }
            return app;
        }
        internal static Simulations GetSimulations(bool allSimulations)
        {
            var simList = new Simulations();
            Debug.WriteLine("Simulation", "DataMarshall.GetSimulations");
            using (var db = new ProducerContext())
            {
                Debug.WriteLine("Simulation ProducerContext", "TurbineLite");

                if (allSimulations == true)
                {
                    var query = db.Simulations.OrderByDescending(c => c.Create);

                    var provider = System.Security.Cryptography.MD5CryptoServiceProvider.Create();
                    /*byte[] hash = provider.ComputeHash(data);
                    var comparer = StringComparer.OrdinalIgnoreCase;
                    var sb = new StringBuilder();
                    foreach (byte b in hash)
                        sb.Append(b.ToString("X2"));
                    string hval = sb.ToString();*/

                    foreach (var entity in query)
                    {
                        Debug.WriteLine(String.Format("ENTITY {0}: {1}", entity.Name, entity.Id));

                        var stagedInputList = new SimpleStagedInputFiles();

                        foreach (var input in entity.SimulationStagedInputs)
                        {
                            stagedInputList.Add(new SimpleStagedInputFile
                            {
                                Name = input.Name,
                                Id = input.Id,
                                MD5Sum = input.Hash
                            });
                        }
                                                
                        simList.Add(new Simulation
                        {
                            Id = entity.Id,
                            Name = entity.Name,
                            Application = entity.ApplicationName,
                            //StagedInputs = (from i in entity.SimulationStagedInputs select i.Name).ToArray<string>()
                            StagedInputs = stagedInputList
                        });
                    }
                }
                else
                {
                    var query = from c in db.Simulations
                                group c by c.Name into uniqueIds
                                select uniqueIds.OrderByDescending(c => c.Count).FirstOrDefault();


                    foreach (var entity in query)
                    {
                        Debug.WriteLine(String.Format("ENTITY {0}: {1} {2}", entity.Name, entity.Id, entity.Create.ToString()));

                        var stagedInputList = new SimpleStagedInputFiles();

                        foreach (var input in entity.SimulationStagedInputs)
                        {
                            stagedInputList.Add(new SimpleStagedInputFile
                            {
                                Name = input.Name,
                                Id = input.Id,
                                MD5Sum = input.Hash
                            });
                        }

                        simList.Add(new Simulation
                        {
                            Id = entity.Id,
                            Name = entity.Name,
                            Application = entity.ApplicationName,
                            //StagedInputs = (from i in entity.SimulationStagedInputs select i.Name).ToArray<string>()
                            StagedInputs = stagedInputList
                        });
                    }
                }
            }
            return simList;
        }
        internal static Simulation GetSimulation(string nameOrID, bool isGuid)
        {
            Simulation sim = null;
            var stagedInputList = new SimpleStagedInputFiles();
            using (var db = new ProducerContext())
            {
                Turbine.Data.Entities.Simulation entity = null;
                
                if (isGuid == true)
                {
                    Guid simulationid = new Guid(nameOrID);
                    entity = db.Simulations.OrderByDescending(q => q.Count).Single(a => a.Id == simulationid);
                }
                else
                {
                    entity = db.Simulations.OrderByDescending(q => q.Count).First(a => a.Name == nameOrID);
                }

                if (entity != null)
                {
                    foreach (var input in entity.SimulationStagedInputs)
                    {
                        stagedInputList.Add(new SimpleStagedInputFile
                        {
                            Name = input.Name,
                            Id = input.Id,
                            MD5Sum = input.Hash
                        });
                    }

                    sim = new Simulation
                    {
                        Id = entity.Id,
                        Name = entity.Name,
                        Application = entity.ApplicationName,
                    };

                    //sim.StagedInputs = stagedInputList.ToArray<String>();
                    sim.StagedInputs = stagedInputList;
                }
            }
            return sim;
        }

        internal static string[] GetStagedInputs(string nameOrID, bool isGuid)
        {
            List<String> stagedInputList = new List<String>();
            using (var db = new ProducerContext())
            {
                Turbine.Data.Entities.Simulation entity = null;

                if (isGuid == true)
                {
                    Guid simulationid = new Guid(nameOrID);
                    entity = db.Simulations.OrderByDescending(q => q.Count).Single(a => a.Id == simulationid);
                }
                else
                {
                    entity = db.Simulations.OrderByDescending(q => q.Count).First(a => a.Name == nameOrID);
                }

                foreach (var si in entity.SimulationStagedInputs)
                {
                    stagedInputList.Add(si.Name);
                }
            }
            return stagedInputList.ToArray<String>();;            
        }

        internal static StagedInputFile GetStagedInputFile(string nameOrID, string inputName, bool isGuid)
        {
            Debug.WriteLine("Entered", "DataMarshal.GetStagedInputFile");
            StagedInputFile siFile = null;
            using (var db = new ProducerContext())
            {                
                Turbine.Data.Entities.Simulation entity = null;

                if (isGuid == true)
                {
                    Debug.WriteLine("isGuid: True", "DataMarshal.GetStagedInputFile");
                    Guid simulationid = new Guid(nameOrID);
                    entity = db.Simulations.SingleOrDefault(a => a.Id == simulationid);
                }
                else
                {
                    Debug.WriteLine("isGuid: False", "DataMarshal.GetStagedInputFile");
                    entity = db.Simulations.OrderByDescending(q => q.Count).First(a => a.Name == nameOrID);
                }
                if (entity == null)
                {
                    Debug.WriteLine(String.Format("GetStagedInputFile:  Simulation {0} does not exist", nameOrID), "DataMarshal");
                    return null;
                }
                Turbine.Data.Entities.SimulationStagedInput si = entity.SimulationStagedInputs.SingleOrDefault(s => s.Name == inputName);
                if (si == null)
                {
                    Debug.WriteLine(String.Format("GetStagedInputFile:  Simulation {0} input {1} does not exist", nameOrID, inputName), "DataMarshal");
                    return null;
                }
                //TODO: Add Input File Type to Entity 
                //siFile = new StagedInputFile() { Content = si.Content, Name = si.Name, InputFileType = si.InputFileType };
                siFile = new StagedInputFile() { Content = si.Content, Name = si.Name, InputFileType = null };
            }
            Debug.WriteLine("Returned siFile: " + siFile.ToString(), "DataMarshal.GetStagedInputFile");
            return siFile;
        }

        internal static List<Dictionary<string, object>> GetJobs(Guid sessionID, string simulation, Guid consumerID, string state, string orderString, int page, int rpp, bool verbose)
        {
            List<Dictionary<string, Object>> jobs = new List<Dictionary<string, Object>>();
            using (var db = new ProducerContext())
            {
                //ObjectQuery<Turbine.Data.Entities.Job> jobQuery = db.
                //ObjectSet<Turbine.Data.Entities.Job> jobQuery = db.Jobs;
                IQueryable<Turbine.Data.Entities.Job> jobQuery = null;
                IQueryableExpression orderjobQuery = new IQueryableExpression();

                jobQuery = db.Jobs
                    .OrderBy(j => j.Count)
                    .Where(j => j.SessionId == sessionID || Guid.Empty.Equals(sessionID))
                    .Where(j => j.ConsumerId == consumerID || Guid.Empty.Equals(consumerID))
                    .Where(j => j.Simulation.Name == simulation || String.IsNullOrEmpty(simulation))
                    .Where(j => j.State == state || String.IsNullOrEmpty(state))
                    .Skip((page-1)*rpp)
                    .Take(rpp);

                if (String.IsNullOrEmpty(orderString) == false)
                {
                    string[] orders = orderString.Split(',');
                    foreach (var orderValue in orders)
                    {
                        jobQuery = orderjobQuery.OrderByField(jobQuery, orderValue, true);
                    }
                }
                /*
                if (!Guid.Empty.Equals(sessionID))
                {
                    
                    //jobQuery = jobQuery.Where("it.SessionId = @session",
                    //    new ObjectParameter("session", sessionID));
                }
                if (!Guid.Empty.Equals(consumerID))
                {
                    jobQuery = jobQuery.Where("it.ConsumerId = @consumer",
                        new ObjectParameter("consumer", consumerID));
                }
                // NOTE: This option will return all jobs for all versions of Simulation
                if (!String.IsNullOrEmpty(simulation))
                {
                    foreach (Turbine.Data.Simulation entity in container.Simulations.Where(i => i.Name == simulation)) 
                    {
                        jobQuery = jobQuery.Where("it.SimulationId = @simulationId",
                            new ObjectParameter("simulationId", entity.Id));
                        // TODO: ADD A "OR"
                        break;
                    }
                }
                if (!String.IsNullOrEmpty(state))
                {
                    jobQuery = jobQuery.Where("it.State = @state",
                        new ObjectParameter("state", state));
                }
                jobQuery = jobQuery
                    .Skip("it.Id", "@skip", new System.Data.Objects.ObjectParameter("skip", (page - 1) * rpp))
                    .Top("@limit", new ObjectParameter("limit", rpp));
                                 * */
                foreach (Turbine.Data.Entities.Job entity in jobQuery)
                {
                    var job = GetJobRepresentation(entity, verbose);
                    jobs.Add(job);
                }

            }
            return jobs;
        }
        private static Dictionary<string, object> GetJobRepresentation(Turbine.Data.Entities.Job entity, bool verbose)
        {
            var dict = new Dictionary<string, Object>();
            string input = entity.Process.Input;
            IDictionary<string, Object> inputDict = new Dictionary<string, Object>() { };
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
                msg_array = entity.Messages.Select<Turbine.Data.Entities.Message, string>(s => s.Value).ToArray<string>();
            }
            /*
            Serialize.ProcessErrorList errorList = new Serialize.ProcessErrorList();

            foreach (var error in entity.Process.Errors)
            {
                var pe = new Serialize.ProcessError();
                pe.Error = error.Error;
                pe.Type = error.Type;
                pe.Name = error.Name;
                errorList.Add(pe);
            }
            */
            ProcessErrorList errorList = new ProcessErrorList();
            foreach (var error in entity.Process.Error)
            {
                var pe = new ProcessError() { Error = error.Error, Name = error.Name, Type = error.Type };
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

            dict["Id"] = entity.Count;
            dict["Guid"] = entity.Id;
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
            dict["Visible"] = entity.Visible;

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

        private static string ConvertDateTime(DateTime stamp)
        {
            var fmt = System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat;
            return stamp.ToString("O", fmt);
        }


        public static Dictionary<string, object> GetJobDict(int id, bool verbose)
        {
            var dict = new Dictionary<string, Object>();
            using (var db = new ProducerContext())
            {
                var job = db.Jobs.Single<Turbine.Data.Entities.Job>(j => j.Count == id);
                dict = GetJobRepresentation(job, verbose);
            }
            return dict;
        }


        internal static List<Guid> GetSessions(int page, int rpp)
        {
            var l = new List<Guid>();
            Debug.WriteLine("GetSessions", "Turbine.Lite.Web.Resources.Contracts.DataMarshall");
            using (var db = new ProducerContext())
            {
                foreach (var entity in db.Sessions.OrderBy(s => s.Create).Take(rpp).Skip((page-1)*rpp))
                {
                    l.Add(entity.Id);
                }
            }
            return l;
        }

        internal static object GetSessionMeta(Guid session)
        {
            throw new NotImplementedException();
        }

        internal static List<Guid> GetConsumers(string status, int page, int rpp)
        {
            var l = new List<Guid>();
            Debug.WriteLine("GetConsumers", "Turbine.Lite.Web.Resources.Contracts.DataMarshall");
            using (var db = new ProducerContext())
            {
                if(status == "all")
                {
                    foreach (var entity in db.Consumers.OrderByDescending(c => c.keepalive).Take(rpp).Skip((page - 1) * rpp))
                    {
                        l.Add(entity.Id);
                    }
                }
                else
                {
                    foreach (var entity in db.Consumers.OrderByDescending(c => c.keepalive).Where(c => c.status == status).Take(rpp).Skip((page - 1) * rpp))
                    {
                        l.Add(entity.Id);
                    }
                }                
            }
            return l;
        }

        internal static Consumer GetConsumer(Guid consumerID)
        {
            Consumer consumer = null;
            //Guid consumerid = new Guid(consumerID);

            using (var db = new ProducerContext())
            {
                var entity = db.Consumers.Single(c => c.Id == consumerID);
                consumer = new Consumer
                {
                    Id = entity.Id,
                    hostname = entity.hostname,
                    processID = entity.processId,
                    status = entity.status,
                    keepalive = entity.keepalive.ToString(),
                    Application_Name = entity.Application_Name,
                };
            }
            return consumer;
        }

        internal static int StopConsumer(Guid consumerID)
        {
            // 0 Failed to stop the consumer
            // -1 Consumer is already down
            // 1 Succeeded to stop the consumer 

            Debug.WriteLine("Stopping consumer", "DataMarshall.StopConsumer");

            int stopped = 0;
            using (var db = new ProducerContext())
            {
                var entity = db.Consumers.Single(c => c.Id == consumerID);
                if (entity != null)
                {
                    if (entity.status != "down")
                    {
                        Debug.WriteLine("Changing status to down", "DataMarshall.StopConsumer");
                        entity.status = "down";
                        stopped = 1;
                    }
                    else
                    {
                        Debug.WriteLine("Consumer is already down", "DataMarshall.StopConsumer");
                        stopped = -1;
                    }                    
                }
                db.SaveChanges();
            }
            return stopped;
        }

        internal static List<Dictionary<string, object>> GetGeneratorPage(Guid generatorid, int pagenum, int sub_page, int rpp, bool verbose)
        {
            List<Dictionary<string, Object>> jobList = new List<Dictionary<string, Object>>();

            using (var db = new ProducerContext())
            {
                var generatorJobs = db.GeneratorJobs.Where(g => g.GeneratorId == generatorid)
                    .Where(g => g.Page == pagenum).OrderBy(g => g.Page).Skip((sub_page - 1) * rpp)
                    .Take(rpp);

                foreach (var gen in generatorJobs)
                {                    
                    Debug.WriteLine("Job: " + gen.Job.Count + " belongs to page: " + pagenum, "DataMarshall.GetGeneratorPage");
                    var job = GetJobRepresentation(gen.Job, verbose);
                    jobList.Add(job);
                }
            }
                        
            return jobList;
        }

        internal static bool DeleteGenerator(Guid generatorid)
        {
            bool deleted = false;

            using (var db = new ProducerContext())
            {
                db.GeneratorJobs.RemoveRange(db.GeneratorJobs.Where(g => g.GeneratorId == generatorid));
                db.Generators.RemoveRange(db.Generators.Where(g => g.Id == generatorid));

                Debug.WriteLine("Delete Generators: " + db.Generators.Any(g => g.Id == generatorid).ToString()
                    + ", Delete GeneratorJobs: " + db.GeneratorJobs.Any(g => g.GeneratorId == generatorid).ToString(),
                "DataMarshall.DeleteGenerator");

                deleted = !(db.Generators.Any(g => g.Id == generatorid)
                    || db.GeneratorJobs.Any(g => g.GeneratorId == generatorid));

                db.SaveChanges();
            }

            using (var db = new ProducerContext())
            {
                Debug.WriteLine("Delete Generators: " + db.Generators.Any(g => g.Id == generatorid).ToString()
                    + ", Delete GeneratorJobs: " + db.GeneratorJobs.Any(g => g.GeneratorId == generatorid).ToString(),
                "DataMarshall.DeleteGenerator");

                deleted = !(db.Generators.Any(g => g.Id == generatorid)
                    || db.GeneratorJobs.Any(g => g.GeneratorId == generatorid));
            }
            return deleted;
        }
    }
}
