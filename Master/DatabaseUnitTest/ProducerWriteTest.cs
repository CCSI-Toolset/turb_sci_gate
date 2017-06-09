using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Turbine.DataEF6;
using System.IO;


namespace DatabaseUnitTest
{
    [TestClass]
    public class ProducerWriteTest
    {
        static string username = "testuser";

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            BaseDatabaseTest.CleanUpDatabase();
        }

        [TestInitialize()]
        public void Initialize()
        {
            Console.WriteLine("Initialize");
        }

        [TestCleanup()]
        public void Cleanup()
        {
            Console.WriteLine("Cleanup");
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
            Console.WriteLine("ClassCleanup");
        }
                 
        [TestMethod]
        //[DeploymentItem(@"models\Hybrid_v0.51_rev1.1_UQ_0809.acmf")]
        //[DeploymentItem(@"models\Hybrid_v0.51_rev1.1_UQ_0809_sinter.json")]
        public void TestApplication_Write()
        {
            //AppDomain.CurrentDomain.SetData("DataDirectory", 
            //    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            var guid = Guid.NewGuid();
            var simulationName = "Test";
            using (var db = new ProducerContext())
            {
                var app = new Turbine.Data.Entities.Application { Name = "ACM", Version = "7.3" };
                db.Applications.Add(app);
                var user = new Turbine.Data.Entities.User { Name = "testuser" };
                db.Users.Add(user);
                db.SaveChanges();
            }
            using (var db = new ProducerContext())
            {
                var item = db.Applications.Single(s => s.Name == "ACM");
                item.InputFileTypes.Add(new Turbine.Data.Entities.InputFileType { 
                    Id=Guid.NewGuid(), Name = "configuration", Required = true, Type = "plain/text" });
                item.InputFileTypes.Add(new Turbine.Data.Entities.InputFileType { 
                    Id=Guid.NewGuid(), Name = "aspenfile", Required = true, Type = "plain/text" });
                db.SaveChanges();
            }

            using (var db = new ProducerContext())
            {
                var app = db.Applications.Single(s => s.Name == "ACM");
                var user = db.Users.Single(u => u.Name == username);
                var sim = new Turbine.Data.Entities.Simulation {
                    Id = Guid.NewGuid(), 
                    Application = app, 
                    Create = DateTime.UtcNow, 
                    Name = simulationName, 
                    Update = DateTime.UtcNow,
                    User = user 
                };
                db.Simulations.Add(sim);
                db.SaveChanges();
            }

            byte[] dataACMF;
            using (var fstream = File.Open(@"models\Hybrid_v0.51_rev1.1_UQ_0809.acmf", FileMode.Open))
            {
                using (var ms = new MemoryStream())
                {
                    fstream.CopyTo(ms);
                    dataACMF = ms.ToArray();
                }
            }
            byte[] dataConfig;
            using (var fstream = File.Open(@"models\Hybrid_v0.51_rev1.1_UQ_0809_sinter.json", FileMode.Open))
            {
                using (var ms = new MemoryStream())
                {
                    fstream.CopyTo(ms);
                    dataConfig = ms.ToArray();
                }
            }
            using (var db = new ProducerContext())
            {
                var sim = db.Simulations.Single(s => s.Name == simulationName);
                Console.WriteLine("App: " + sim.Application.Name);
                Console.WriteLine("Sim: " + sim.Name);
                foreach (var i in sim.Application.InputFileTypes)
                {
                    Console.WriteLine("IT: " + i.Name);
                }
                var it = sim.Application.InputFileTypes.Single(s => s.Name == "aspenfile");
 
                var item = new Turbine.Data.Entities.SimulationStagedInput
                {
                    //InputFileType = it,
                    Simulation = sim,
                    Content = dataACMF,
                    Hash = "",
                    Id = Guid.NewGuid(),
                    Name = it.Name
                };
                sim.SimulationStagedInputs.Add(item);
                
                it = sim.Application.InputFileTypes.Single(s => s.Name == "configuration");
                item = new Turbine.Data.Entities.SimulationStagedInput
                {
                    //InputFileType = it,
                    Simulation = sim,
                    Content = dataConfig,
                    Hash = "",
                    Id = Guid.NewGuid(),
                    Name = it.Name
                };
                sim.SimulationStagedInputs.Add(item);

                db.SaveChanges();
            }

            // Session
            var sessionId = Guid.NewGuid();
            using (var db = new ProducerContext())
            {
                var item = new Turbine.Data.Entities.Session { Id = sessionId, Create=DateTime.UtcNow, Description="testing simulation" };
                db.Sessions.Add(item);
                db.SaveChanges();
            }
            var jobId = Guid.NewGuid();
            var workingDir = String.Format("WorkingDir_{0}", Guid.NewGuid());
            using (var db = new ProducerContext())
            {
                var item = db.Sessions.Single(s => s.Id == sessionId);
                var user = db.Users.Single(u => u.Name == username);
                var simulation = db.Simulations.Single(s => s.Name == simulationName);
                var job = new Turbine.Data.Entities.Job
                {
                    Id = jobId,
                    Session = item,
                    Create = DateTime.UtcNow,
                    Setup = DateTime.UtcNow,
                    State = "setup",
                    User = user,
                    Simulation = simulation,
                    Process = new Turbine.Data.Entities.Process { Input="{}", WorkingDir=workingDir },
                };
                var msg = new Turbine.Data.Entities.Message
                {
                    Id = Guid.NewGuid(),
                    Create = DateTime.UtcNow,
                    Value = "Test Job"
                };
                job.Messages.Add(msg);
                db.Jobs.Add(job);
                db.SaveChanges();
            }
            using (var db = new ProducerContext())
            {
                var job = db.Jobs.Single(j => j.Id == jobId);
                foreach (var f in job.Simulation.SimulationStagedInputs) {
                    job.StagedInputFiles.Add(new Turbine.Data.Entities.StagedInputFile
                    {
                        Content = f.Content,
                        Name = f.Name
                    });
                }
            }

            // READ JOB Save to Filesystem
            using (var db = new ProducerContext())
            {
                var job = db.Jobs.Single(j => j.State == "setup");
                Assert.AreEqual(job.Id, jobId);
                var dinfo = Directory.CreateDirectory(job.Process.WorkingDir);
                Console.WriteLine("Working Directory: " + dinfo.FullName);
                foreach (var f in job.StagedInputFiles)
                {
                    Console.WriteLine("Save File: " + f.Name);
                    var fd = File.Create(Path.Combine(dinfo.FullName, f.Name));
                    fd.Write(f.Content, 0, f.Content.Length);
                    fd.Close();
                }
            }
        }
    }
}
