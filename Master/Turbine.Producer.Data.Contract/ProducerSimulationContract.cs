using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Turbine.Data.Contract.Behaviors;
using Turbine.Producer.Data.Contract.Behaviors;
using Turbine.Data;
using Turbine.Data.Contract;
using System.Diagnostics;

namespace Turbine.Producer.Data.Contract
{
    /* AspenSimulationContract:  Used by producer to access, update and create simulations.  
     *    Also factory method for creating a new job for the simulation.
     * 
     *
     */
    public class ProducerSimulationContract : ISimulationProducerContract
    {
        public static string[] ListOfNames()
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                List<string> l = new List<string>();
                foreach (Simulation sim in container.Simulations)
                {
                    l.Add(sim.Name);
                }
                return l.ToArray<string>();
            }
        }
        public static ISimulationProducerContract Get(string name)
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Simulation obj = container.Simulations
                    .OrderByDescending(q => q.Create).First(s => s.Name == name);
            }
            ProducerSimulationContract sc = new ProducerSimulationContract();
            sc.name = name;
            return sc;
        }


        public static ISimulationProducerContract Create(string simulationName, string applicationName)
        {
            string owner = Container.GetAppContext().UserName;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                User user = container.Users.Single<User>(s => s.Name == owner);
                Simulation obj = container.Simulations.OrderByDescending(q => q.Create)
                    .FirstOrDefault(s => s.Name == simulationName);
                if (obj != null)
                    throw new InvalidStateChangeException(String.Format(
                        "Simulation with Name '{0}' already exists", simulationName));

                Application app = container.Applications.SingleOrDefault<Application>(s => s.Name == applicationName);
                if (app == null)
                    throw new ArgumentException(String.Format(
                        "Application '{0}' does not exist", applicationName));

                var sim = new Simulation() { 
                    Name = simulationName,
                    Id = Guid.NewGuid(),
                    User = user,
                    Application = app,
                    Create = DateTime.UtcNow,
                    Update = DateTime.UtcNow
                };
                
                foreach (var input in app.InputFileTypes) 
                {
                    if ("any".Equals(input.Name)) continue;
                    sim.SimulationStagedInputs.Add(new SimulationStagedInput() { 
                        Id=Guid.NewGuid(), Name = input.Name, InputFileType=input, Hash=null, Simulation=sim });
                }
                container.Simulations.AddObject(sim);
                container.SaveChanges();
            }
            ProducerSimulationContract sc = new ProducerSimulationContract();
            sc.name = simulationName;
            return sc;
        }

        private string name;
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
            string owner = Container.GetAppContext().UserName;
            var provider = System.Security.Cryptography.MD5CryptoServiceProvider.Create();
            byte[] hash = provider.ComputeHash(data);
            var comparer = StringComparer.OrdinalIgnoreCase;
            var sb = new StringBuilder();
            foreach (byte b in hash)
                sb.Append(b.ToString("X2"));
            string hval = sb.ToString();
            Debug.WriteLine(String.Format("UpdateInput: inputFileName '{0}'", inputFileName), 
                this.GetType().Name);

            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Simulation obj = container.Simulations
                    .OrderByDescending(q => q.Create).First(s => s.Name == name);

                if (obj.User.Name != owner)
                    throw new ArgumentException(String.Format("Only owner {0} can update simulation", obj.User.Name));

                var input = obj.SimulationStagedInputs.SingleOrDefault<SimulationStagedInput>(i => i.Name == inputFileName);
                if (input == null)
                {
                    Debug.WriteLine("UpdateInput:  input is null", this.GetType().Name);
                    var ifType = obj.Application.InputFileTypes.SingleOrDefault<InputFileType>(f => f.Name == inputFileName);
                    if (ifType == null)
                    {
                        // wildcard:  if not then error
                        ifType = obj.Application.InputFileTypes.Single<InputFileType>(f => f.Name == "any");
                    }
                    input = new SimulationStagedInput() { Name = inputFileName, Id = Guid.NewGuid() };
                    input.Content = data;
                    input.Simulation = obj;
                    input.InputFileType = ifType;
                    input.Hash = hval;
                    obj.Update = DateTime.UtcNow;
                    container.SaveChanges();
                }
                else if (input.Hash == null || comparer.Compare(input.Hash, hval) != 0)
                {
                    // if simulation is changed, make new simulation resource
                    var sim = new Simulation()
                    {
                        Name = name,
                        Id = Guid.NewGuid(),
                        User = obj.User,
                        Application = obj.Application,
                        Create = DateTime.UtcNow,
                        Update = DateTime.UtcNow
                    };
                    // copy all files
                    foreach (var i in obj.SimulationStagedInputs.Where(s => s.Id != input.Id))
                    {
                        sim.SimulationStagedInputs.Add(new SimulationStagedInput()
                        {
                            Id = Guid.NewGuid(),
                            Name = i.Name,
                            InputFileType = i.InputFileType,
                            Hash = i.Hash,
                            Content = i.Content
                        });
                    }

                    sim.SimulationStagedInputs.Add(new SimulationStagedInput()
                    {
                        Id = Guid.NewGuid(),
                        Name = input.Name,
                        InputFileType = input.InputFileType,
                        Hash = hval,
                        Content = data
                    });

                    Debug.WriteLine(String.Format("UpdateInput: action update, hash values do not match '{0}' != '{1}'", input.Hash, hval),
                        this.GetType().Name);

                    container.Simulations.AddObject(sim);
                    container.SaveChanges();
                }
                else
                {
                    Debug.WriteLine(String.Format("UpdateInput: Simulation '{0}' hash values match '{1}'", name, inputFileName),
                        this.GetType().Name);
                }
            }
            return true;
        }

        public IJobProducerContract NewJob(Guid sessionID, bool initialize, bool reset)
        {
            return (IJobProducerContract)AspenJobContract.Create(name, sessionID, initialize, reset);
        }

        public bool Delete()
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Simulation entity = container.Simulations.Single<Simulation>(s => s.Name == name);
                container.Simulations.DeleteObject(entity);
            }
            return true;
        }

        public bool Validate()
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
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
    }
}
