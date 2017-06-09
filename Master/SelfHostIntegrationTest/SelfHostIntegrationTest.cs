using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Collections.Generic;
using System.ServiceModel.Web;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Net;
using Newtonsoft.Json;
using System.Linq;
using Turbine.Web.Contracts;
using System.ServiceModel.Description;
using System.ServiceModel;
using System.IO;


namespace SelfHostIntegrationTest
{
    [TestClass]
    public class SelfHostIntegrationTest
    {
        static List<System.ServiceModel.ServiceHost> hosts = new List<System.ServiceModel.ServiceHost>();

        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            Console.WriteLine("assembly init");
            CleanUpDatabase();

            // DIJ
            TestProducerContext.name = "Administrator";
            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                db.Users.Add(new Turbine.Data.Entities.User { Name = TestProducerContext.name });
                db.SaveChanges();
            }
            // Create the ServiceHost.
            try
            {
                using (var db = new Turbine.DataEF6.ProducerContext())
                {
                    Debug.WriteLine("Application ProducerContext", "TurbineLite");
                    db.Applications.Add(new Turbine.Data.Entities.Application { Name = "ACM", Version = "7.3" });
                    db.Applications.Add(new Turbine.Data.Entities.Application { Name = "AspenPlus" });
                    db.Applications.Add(new Turbine.Data.Entities.Application { Name = "Excel" });
                    db.SaveChanges();
                }
                using (var db = new Turbine.DataEF6.ProducerContext())
                {
                    var item = db.Applications.Single(s => s.Name == "ACM");
                    item.InputFileTypes.Add(new Turbine.Data.Entities.InputFileType
                    {
                        Id = Guid.NewGuid(),
                        Name = "configuration",
                        Required = true,
                        Type = "plain/text"
                    });
                    item.InputFileTypes.Add(new Turbine.Data.Entities.InputFileType
                    {
                        Id = Guid.NewGuid(),
                        Name = "aspenfile",
                        Required = true,
                        Type = "plain/text"
                    });
                    item.InputFileTypes.Add(new Turbine.Data.Entities.InputFileType
                    {
                        Id = Guid.NewGuid(),
                        Name = "any",
                        Required = true,
                        Type = "application/octet-stream"
                    });
                    db.SaveChanges();
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                Debug.WriteLine("Ignore Application entries already made");
            }

            hosts.Add(new WebServiceHost(typeof(Turbine.Lite.Web.Resources.ApplicationResource), new Uri("http://localhost:8000/TurbineLite/application")));
            hosts.Add(new WebServiceHost(typeof(Turbine.Lite.Web.Resources.JobResource), new Uri("http://localhost:8000/TurbineLite/job")));
            hosts.Add(new WebServiceHost(typeof(Turbine.Lite.Web.Resources.SessionResource), new Uri("http://localhost:8000/TurbineLite/session")));
            hosts.Add(new WebServiceHost(typeof(Turbine.Lite.Web.Resources.SimulationResource), new Uri("http://localhost:8000/TurbineLite/simulation")));
            foreach (var host in hosts)
            {
                ServiceDebugBehavior stp = host.Description.Behaviors.Find<ServiceDebugBehavior>();
                stp.HttpHelpPageEnabled = false;
                stp.IncludeExceptionDetailInFaults = true;
                host.Open();
            }
        }
        public static void CleanUpDatabase()
        {
            Debug.WriteLine("CleanUpDatabase");
            using (var db = new Turbine.DataEF6.ProducerContext())
            {
                db.Database.ExecuteSqlCommand("delete FROM Processes");
                db.Database.ExecuteSqlCommand("delete FROM Messages");
                db.Database.ExecuteSqlCommand("delete FROM Jobs");
                db.Database.ExecuteSqlCommand("delete FROM Sessions");
                db.Database.ExecuteSqlCommand("delete FROM InputFileTypes");
                db.Database.ExecuteSqlCommand("delete FROM SimulationStagedInputs");
                db.Database.ExecuteSqlCommand("delete FROM Applications");
                db.Database.ExecuteSqlCommand("delete FROM Simulations");
                db.Database.ExecuteSqlCommand("delete FROM Users");
            }
        }
        [AssemblyCleanup()]
        public static void AssemblyCleanup()
        {
            Console.WriteLine("assembly cleanup");
            foreach (var host in hosts)
            {
                Console.WriteLine("Closing ServiceHost {0}", host.BaseAddresses);
                host.Close();
            }
        }
    }

    internal class JobDescriptionList : List<JobDescription>
    {
        public JobDescriptionList() { }
        public JobDescriptionList(IEnumerable<JobDescription> source) : base(source) { }
    }


    internal class JobDescription
    {
        [JsonProperty("Id")]
        public int Id { get; set; }
        [JsonProperty("Guid")]
        public Guid Guid { get; set; }
        [JsonProperty("Simulation")]
        public Guid Simulation { get; set; }
        [JsonProperty("State")]
        public string State { get; set; }
        [JsonProperty("Input")]
        public JObject Input { get; set; }
        //[JsonProperty("Output")]
        //public JObject Output { get; set; }
        [JsonProperty("Session")]
        public Guid Session { get; set; }
        [JsonProperty("Initialize")]
        public Boolean Initialize { get; set; }
        [JsonProperty("Reset")]
        public Boolean Reset { get; set; }
        [JsonProperty("Create")]
        public DateTime Create { get; set; }
        [JsonProperty("Submit")]
        public DateTime? Submit { get; set; }
        [JsonProperty("Setup")]
        public DateTime? Setup { get; set; }
        [JsonProperty("Running")]
        public DateTime? Running { get; set; }
        [JsonProperty("Finished")]
        public DateTime? Finished { get; set; }
    }

    internal class JobRequest
    {
        [JsonProperty("Initialize")]
        public Boolean Initialize { get; set; }
        
        [JsonProperty("Reset")]
        public Boolean Reset { get; set; }

        [JsonProperty("Simulation")]
        public string Simulation { get; set; }

        [JsonProperty("Input")]
        public JObject Input { get; set; }
    }

    internal class JobRequestList: List<JobRequest>
    {
        public JobRequestList() { }
        public JobRequestList(IEnumerable<JobRequest> source) : base(source) { }
    }

    public class Application
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        //[JsonProperty("accounts")]
        //public InputTypeList Inputs { get; set; }
    }

    public class ApplicationList : List<Application>
    {
        public ApplicationList() { }
        public ApplicationList(IEnumerable<Application> source) : base(source) { }
    }

    public class Simulation
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Application")]
        public string Application { get; set; }

        [JsonProperty("StagedInputs")]
        public List<SimpleStagedInputFile> StagedInputs { get; set; }
    }

    public class SimpleStagedInputFile
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("MD5Sum")]
        public string MD5Sum { get; set; }
    }

    public class SimulationStagedInputList : List<string>
    {
    }

    public class SimulationList : List<Simulation>
    {
        public SimulationList() { }
        public SimulationList(IEnumerable<Simulation> source) : base(source) { }
    }
    public class InputType
    {
    }

    public class InputTypeList : List<InputType>
    {
        public InputTypeList() { }
        public InputTypeList(IEnumerable<InputType> source) : base(source) { }
    }

    public class User
    {
        /// <summary>
        /// A User's username. eg: "sergiotapia, mrkibbles, matumbo"
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; set; }

        /// <summary>
        /// A User's name. eg: "Sergio Tapia, John Cosack, Lucy McMillan"
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// A User's location. eh: "Bolivia, USA, France, Italy"
        /// </summary>
        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("endorsements")]
        public int Endorsements { get; set; } //Todo.

        [JsonProperty("team")]
        public string Team { get; set; } //Todo.

        /// <summary>
        /// A collection of the User's linked accounts.
        /// </summary>
        [JsonProperty("accounts")]
        public Account Accounts { get; set; }

        /// <summary>
        /// A collection of the User's awarded badges.
        /// </summary>
        [JsonProperty("badges")]
        public List<Badge> Badges { get; set; }
    }

    public class Account
    {
        public string github;
    }

    public class Badge
    {
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("description")]
        public string Description;
        [JsonProperty("created")]
        public string Created;
        [JsonProperty("badge")]
        public string BadgeUrl;
    }

}
