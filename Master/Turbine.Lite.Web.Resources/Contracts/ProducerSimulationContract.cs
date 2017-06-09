using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Turbine.Data.Entities;
using Turbine.DataEF6;
using Turbine.Producer.Data.Contract.Behaviors;

namespace Turbine.Lite.Web.Resources.Contracts
{
    public class ProducerSimulationContract : Turbine.Producer.Data.Contract.Behaviors.ISimulationProducerContract
    {
        public static string[] ListOfNames()
        {
            using (var db = new ProducerContext())
            {
                Debug.WriteLine("ListOfNames", "ProducerSimulationContract");
                List<string> l = new List<string>();
                foreach (Simulation sim in db.Simulations)
                {
                    l.Add(sim.Name);
                }
                return l.ToArray<string>();
            }
        }
        public static ISimulationProducerContract Get(string nameOrID)
        {
            Guid simulationId = Guid.Empty;
            bool isGuid = Guid.TryParse(nameOrID, out simulationId);

            ProducerSimulationContract sc = new ProducerSimulationContract();
            using (var db = new ProducerContext())
            {
                Simulation obj = null;
                if (isGuid == true)
                {
                    obj = db.Simulations.SingleOrDefault(s => s.Id == simulationId);
                }
                else
                {
                    obj = db.Simulations
                         .OrderByDescending(q => q.Count).First(s => s.Name == nameOrID);
                }

                if (obj == null)
                {
                    return null;
                }

                sc.simId = obj.Id;
                sc.name = obj.Name;
                sc.nameIsGuid = isGuid;
            }

            return sc;
        }

        public static ISimulationProducerContract Create(string simNameOrID, string simName, string applicationName)
        {
            Guid simulationId = Guid.Empty;
            bool isGuid = Guid.TryParse(simNameOrID, out simulationId);

            ProducerSimulationContract sc = new ProducerSimulationContract();

            string owner = Turbine.Producer.Container.GetAppContext().UserName;
            using (var db = new ProducerContext())
            {
                Debug.WriteLine("Owner: " + owner, "ProducerSimulationContract.Create");
                User user = db.Users.Single<User>(s => s.Name == owner);
                Simulation obj = null;
                Guid simIdUsed = Guid.Empty;

                if (isGuid == true)
                {
                    simIdUsed = simulationId;
                    obj = db.Simulations.SingleOrDefault(s => s.Id == simulationId);
                }
                else
                {
                    simIdUsed = Guid.NewGuid();
                    obj = db.Simulations
                         .OrderBy(q => q.Count).FirstOrDefault(s => s.Name == simNameOrID);
                }

                if (obj != null)
                    throw new InvalidStateChangeException(String.Format(
                        "Simulation with Name or Id '{0}' already exists", simNameOrID));

                Application app = db.Applications.SingleOrDefault<Application>(s => s.Name == applicationName);
                if (app == null)
                    throw new ArgumentException(String.Format(
                        "Application '{0}' does not exist", applicationName));

                Debug.WriteLine("create simulation", "ProducerSimulationContract");
                var sim = new Simulation() {
                    Name = simName,
                    Id = simIdUsed,
                    User = user,
                    Application = app,
                    Create = DateTime.UtcNow,
                    Update = DateTime.UtcNow
                };
                db.Simulations.Add(sim);
                db.SaveChanges();
            }
            using (var db = new ProducerContext())
            {
                Debug.WriteLine("create simulation inputs", "ProducerSimulationContract");
                Simulation sim = null;

                if (isGuid == true)
                {
                    sim = db.Simulations.Single<Simulation>(s => s.Id == simulationId);
                }
                else
                {
                    sim = db.Simulations.Single<Simulation>(s => s.Name == simNameOrID);                    
                }
                
                foreach (var input in sim.Application.InputFileTypes) 
                {
                    Debug.WriteLine("create simulation input " + input.Name, "ProducerSimulationContract");
                    if ("any".Equals(input.Name)) continue;
                    var ssi = new SimulationStagedInput()
                    {
                        Id = Guid.NewGuid(),
                        Name = input.Name,
                        Content = new Byte[] { },
                        Hash = "",
                        Simulation = sim
                    };
                    sim.SimulationStagedInputs.Add(ssi);
                }

                sc.simId = sim.Id;
                sc.name = sim.Name;
                sc.nameIsGuid = isGuid;

                Debug.WriteLine("create save", "ProducerSimulationContract");
                db.SaveChanges();
            }            
            
            return sc;
        }

        private string name;
        private Guid simId;
        private bool nameIsGuid;
        private ProducerSimulationContract() { }

        private bool UpdateInput(string inputFileName, string filePath, string content_type)
        {
            byte[] data = System.Text.Encoding.ASCII.GetBytes(
                    File.ReadAllText(filePath)
                    );
            return UpdateInput(inputFileName, data, content_type);
        }

        public bool UpdateInput(string inputFileName, byte[] data, string content_type)
        {
            //string owner = Container.GetAppContext().UserName;
            Debug.WriteLine("UpdateInput", this.GetType().Name);
            var provider = System.Security.Cryptography.MD5CryptoServiceProvider.Create();
            byte[] hash = provider.ComputeHash(data);
            var comparer = StringComparer.OrdinalIgnoreCase;
            var sb = new StringBuilder();
            foreach (byte b in hash)
                sb.Append(b.ToString("X2"));
            string hval = sb.ToString();
            Debug.WriteLine(String.Format("UpdateInput: inputFileName '{0}'", inputFileName), 
                this.GetType().Name);

            using (var db = new ProducerContext())
            {
                Simulation obj = db.Simulations.SingleOrDefault(s => s.Id == simId);

                //Simulation obj = db.Simulations.OrderByDescending(q => q.Count).First(s => s.Name == name);

                ///if (obj.User.Name != owner)
                //    throw new ArgumentException(String.Format("Only owner {0} can update simulation", obj.User.Name));

                /**NOTE (BUG EF6/SQLCompact): Not sure why this is returning multiple objects with i.Name
                 * var input = obj.SimulationStagedInputs.SingleOrDefault<SimulationStagedInput>(i => i.Name == inputFileName);
                 */
                var input = obj.SimulationStagedInputs.SingleOrDefault<SimulationStagedInput>(i => i.Name == inputFileName && i.SimulationId == obj.Id);
                /*if (input == null)
                {
                    Debug.WriteLine("UpdateInput: Expecting Wildcard", this.GetType().Name);
                    var ifType = obj.Application.InputFileTypes.SingleOrDefault<InputFileType>(f => f.Name == inputFileName);
                    if (ifType != null)
                    {
                        throw new Exception("Expecting Wildcard, Found Application InputFileType: " + inputFileName);
                    }
                    ifType = obj.Application.InputFileTypes.Single<InputFileType>(f => f.Name == "any");
                    input = new SimulationStagedInput() { Name = inputFileName, Id = Guid.NewGuid() };
                    input.Content = data;
                    input.Simulation = obj;
                    //input.InputFileType = ifType;
                    input.Hash = hval;
                    obj.Update = DateTime.UtcNow;
                    obj.SimulationStagedInputs.Add(input);
                    db.SaveChanges();
                }
                else if (input.Hash == null || comparer.Compare(input.Hash, hval) != 0)
                {*/
                if (input == null || input.Hash == null || comparer.Compare(input.Hash, hval) != 0)
                {
                    // if simulation is changed, make new simulation resource
                    Debug.WriteLine("New Simulation Version:  input hash is null or doesn't match", this.GetType().Name);
                    Simulation sim = null;

                    if (nameIsGuid == true)
                    {
                        sim = obj;
                    }
                    else
                    {
                        sim = new Simulation()
                        {
                            Name = name,
                            Id = Guid.NewGuid(),
                            User = obj.User,
                            Application = obj.Application,
                            Create = obj.Create,
                            Update = DateTime.UtcNow,
                            SimulationStagedInputs = new List<SimulationStagedInput>()
                        };
                        db.Simulations.Add(sim);

                        // copy all files
                        Debug.WriteLine("Copying Inputs to the new Simulation", this.GetType().Name);
                        foreach (var i in obj.SimulationStagedInputs)
                        {
                            if (input != null && input.Id == i.Id)
                            {
                                Debug.WriteLine("Copying Process: Skipping Input '" + input.Id.ToString() + "' to be modified later",
                                    this.GetType().Name);
                                continue;
                            }
                            sim.SimulationStagedInputs.Add(new SimulationStagedInput()
                            {
                                Id = Guid.NewGuid(),
                                Name = i.Name,
                                //InputFileType = i.InputFileType,
                                Hash = i.Hash,
                                Content = i.Content
                            });
                        }
                    }
                                        

                    if (input != null && nameIsGuid == true)
                    {
                        input.Name = inputFileName;
                        input.Hash = hval;
                        input.Content = data;
                        input.Simulation = sim;

                        Debug.WriteLine(String.Format("UpdateInput: action update, hash values do not match '{0}' != '{1}'", input.Hash, hval),
                                this.GetType().Name);
                    }
                    else
                    {
                        sim.SimulationStagedInputs.Add(new SimulationStagedInput()
                        {
                            Id = Guid.NewGuid(),
                            Name = inputFileName,
                            //InputFileType = input.InputFileType,
                            Hash = hval,
                            Content = data,
                            Simulation = sim
                        });


                        Debug.WriteLine(String.Format("UpdateInput: action update, New input has been added"),
                                this.GetType().Name);
                    }


                    //db.Simulations.Add(sim);
                    db.SaveChanges();
                }
                else
                {
                    Debug.WriteLine(String.Format("UpdateInput: Simulation '{0}' hash values match '{1}'", name, inputFileName),
                        this.GetType().Name);
                }
            }
            return true;
        }

        public IJobProducerContract NewJob(Guid sessionID, bool initialize, bool reset, bool visible)
        {
            return (IJobProducerContract)ProducerJobContract.Create(simId.ToString(), sessionID, initialize, reset, visible);
        }

        public IJobProducerContract NewJob(Guid sessionID, bool initialize, bool reset, bool visible, Guid jobID, Guid consumerID)
        {
            return (IJobProducerContract)ProducerJobContract.Create(simId.ToString(), sessionID, initialize, reset, visible, jobID, consumerID);
        }

        public bool DeleteAll()
        {
            Debug.WriteLine(String.Format(
                "Deleting All Simulations with name '{0}'", name),
                "ProducerSimulationContract.DeleteAll()");
            using (var db = new ProducerContext())
            {
                foreach (var entity in db.Simulations.Where<Simulation>(s => s.Name == name))
                {
                    foreach (var jobEntity in db.Jobs.Where(s => s.SimulationId == entity.Id))
                    {
                        db.Processes.Remove(jobEntity.Process);
                        db.Messages.RemoveRange(jobEntity.Messages);
                        db.StagedInputFiles.RemoveRange(jobEntity.StagedInputFiles);
                        db.StagedOutputFiles.RemoveRange(jobEntity.StagedOutputFiles);
                        db.GeneratorJobs.RemoveRange(db.GeneratorJobs.Where(g => g.JobId == jobEntity.Id));
                        db.Generators.RemoveRange(db.Generators.Where(g => g.SessionId == jobEntity.SessionId));

                        Session session = db.Sessions.Single<Session>(s => s.Id == jobEntity.SessionId);

                        db.Jobs.Remove(jobEntity);

                        if (session.Jobs.Count == 0)
                        {
                            db.Sessions.Remove(session);
                        }
                    }
                    Debug.WriteLine(String.Format(
                        "Simulation '{0}' Id '{1}' removed", name, entity.Id.ToString()),
                        "ProducerSimulationContract.DeleteAll()");
                    db.Simulations.Remove(entity);
                }
                db.SaveChanges();
            }
            return true;
        }

        public bool Delete()
        {
            Debug.WriteLine(String.Format(
                "Deleting Simulation '{0}' Id '{1}'", name, simId.ToString()),
                "ProducerSimulationContract.Delete()");
            using (var db = new ProducerContext())
            {
                foreach (var entity in db.Simulations.Where<Simulation>(s => s.Id == simId))
                {
                    foreach (var jobEntity in db.Jobs.Where(s => s.SimulationId == entity.Id))
                    {
                        db.Processes.Remove(jobEntity.Process);
                        db.Messages.RemoveRange(jobEntity.Messages);
                        db.StagedInputFiles.RemoveRange(jobEntity.StagedInputFiles);
                        db.StagedOutputFiles.RemoveRange(jobEntity.StagedOutputFiles);
                        db.GeneratorJobs.RemoveRange(db.GeneratorJobs.Where(g => g.JobId == jobEntity.Id));
                        db.Generators.RemoveRange(db.Generators.Where(g => g.SessionId == jobEntity.SessionId));

                        Session session = db.Sessions.Single<Session>(s => s.Id == jobEntity.SessionId);

                        db.Jobs.Remove(jobEntity);

                        if (session.Jobs.Count == 0)
                        {
                            db.Sessions.Remove(session);
                        }
                    }
                    Debug.WriteLine(String.Format(
                        "Simulation '{0}' Id '{1}' removed", name, simId.ToString()),
                        "ProducerSimulationContract.Delete()");
                    db.Simulations.Remove(entity);
                }
                db.SaveChanges();
            }
            return true;
        }
        /*
        public bool Validate()
        {
            using (var container = new ProducerContext())
            {
                Simulation entity = container.Simulations.Single<Simulation>(s => s.Name == name);
                foreach (var input in entity.SimulationStagedInputs) {
                    if (input.Content == null & input.InputFileType.Required) {
                        throw new ArgumentException(String.Format(
                            "Simulation '{0}' input file '{1}' has null content", name, input.Name));
                    }
                }
            }
            return true;
        }
         * */


        public bool ValidateAll()
        {
            Debug.WriteLine(String.Format(
                "Validating All Simulations with name '{0}'", name),
                "ProducerSimulationContract.ValidateAll()");
            using (var db = new ProducerContext())
            {
                foreach (var entity in db.Simulations.Where<Simulation>(s => s.Name == name))
                {
                    foreach (var input in entity.SimulationStagedInputs)
                    {
                        if (input.Content == null || String.IsNullOrEmpty(input.Content.ToString()) || String.IsNullOrEmpty(input.Hash))
                        {
                            Debug.WriteLine(String.Format(
                                "Simulation '{0}' input file '{1}' has null content or null hash", name, input.Name),
                                "ProducerSimulationContract.ValidateAll()");
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public bool Validate()
        {
            Debug.WriteLine(String.Format(
                "Validating Simulation '{0}' Id '{1}'", name, simId.ToString()),
                "ProducerSimulationContract.Validate()");
            using (var db = new ProducerContext())
            {
                foreach (var entity in db.Simulations.Where<Simulation>(s => s.Id == simId))
                {
                    foreach (var input in entity.SimulationStagedInputs)
                    {
                        if (input.Content == null || String.IsNullOrEmpty(input.Content.ToString()) || String.IsNullOrEmpty(input.Hash))
                        {
                            Debug.WriteLine(String.Format(
                                "Simulation '{0}' input file '{1}' has null content or null hash", name, input.Name),
                                "ProducerSimulationContract.Validate()");
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
