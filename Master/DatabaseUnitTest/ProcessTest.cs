using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Turbine.DataEF6;
using System.IO;


namespace DatabaseUnitTest
{
    [TestClass]
    public class ProcessTest
    {
        static Guid sessionId = Guid.NewGuid();
        static string userName = "testuser";
        static string simulationName = "test";


        [ClassInitialize()]
        public static void Initialize(TestContext context)
        {
            BaseDatabaseTest.CleanUpDatabase();

            using (var db = new ProducerContext())
            {
                var app = new Turbine.Data.Entities.Application { Name = "ACM", Version = "7.3" };
                db.Applications.Add(app);
                var user = new Turbine.Data.Entities.User { Name = userName };
                db.Users.Add(user);
                db.SaveChanges();
            }
            using (var db = new ProducerContext())
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
                db.SaveChanges();
            }
            using (var db = new ProducerContext())
            {
                var app = db.Applications.Single(s => s.Name == "ACM");
                var user = db.Users.Single(u => u.Name == userName);
                var sim = new Turbine.Data.Entities.Simulation
                {
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
            using (var db = new ProducerContext())
            {
                var item = new Turbine.Data.Entities.Session
                {
                    Id = sessionId,
                    Description = "testing simulation",
                    Create = DateTime.UtcNow
                };
                db.Sessions.Add(item);
                db.SaveChanges();
            }
        }

        [TestMethod]
        public void TestProcess_Empty()
        {
            Guid jobId = Guid.NewGuid();
            using (var db = new ProducerContext())
            {
                var item = db.Sessions.Single(s => s.Id == sessionId);
                var user = db.Users.Single(u => u.Name == userName);
                var simulation = db.Simulations.Single(s => s.Name == simulationName);
                var job = new Turbine.Data.Entities.Job
                {
                    Id = jobId,
                    Session = item,
                    Create = DateTime.UtcNow,
                    State = "create",
                    User = user,
                    Simulation = simulation
                };
                db.Jobs.Add(job);
                db.SaveChanges();
            }
            using (var db = new ProducerContext())
            {
                var item = db.Jobs.Single(s => s.Id == jobId);
                item.Process = new Turbine.Data.Entities.Process
                {
                };
                db.SaveChanges();
            }
        }

        [TestMethod]
        public void TestProcess_Full()
        {
            Guid jobId = Guid.NewGuid();
            using (var db = new ProducerContext())
            {
                var item = db.Sessions.Single(s => s.Id == sessionId);
                var user = db.Users.Single(u => u.Name == userName);
                var simulation = db.Simulations.Single(s => s.Name == simulationName);
                var job = new Turbine.Data.Entities.Job
                {
                    Id = jobId,
                    Session = item,
                    Create = DateTime.UtcNow,
                    State = "create",
                    User = user,
                    Simulation = simulation
                };
                db.Jobs.Add(job);
                db.SaveChanges();
            }
            using (var db = new ProducerContext())
            {
                var item = db.Jobs.Single(s => s.Id == jobId);
                item.Process = new Turbine.Data.Entities.Process
                { 
                    Input = "Input string",
                    Output = "Output string",
                    WorkingDir = "MYDIR",
                    Status = -1,
                    Stdin = "",
                    Stderr = "",
                    Stdout = ""
                };
                db.SaveChanges();
            }
        }
        [TestMethod]
        public void TestProcess_FullProcessErrors()
        {
            Guid jobId = Guid.NewGuid();
            using (var db = new ProducerContext())
            {
                var item = db.Sessions.Single(s => s.Id == sessionId);
                var user = db.Users.Single(u => u.Name == userName);
                var simulation = db.Simulations.Single(s => s.Name == simulationName);
                var job = new Turbine.Data.Entities.Job
                {
                    Id = jobId,
                    Session = item,
                    Create = DateTime.UtcNow,
                    State = "create",
                    User = user,
                    Simulation = simulation
                };
                db.Jobs.Add(job);
                db.SaveChanges();
            }
            using (var db = new ProducerContext())
            {
                var item = db.Jobs.Single(s => s.Id == jobId);
                item.Process = new Turbine.Data.Entities.Process
                {
                    Input = "Input string",
                    Output = "Output string",
                    WorkingDir = "MYDIR",
                    Status = -1,
                    Stdin = "",
                    Stderr = "",
                    Stdout = ""
                };
                db.SaveChanges();
            }
            using (var db = new ProducerContext())
            {
                var item = db.Jobs.Single(s => s.Id == jobId);
                item.Process.Error.Add(new Turbine.Data.Entities.ProcessError
                {
                    Error = "ERROR1",
                    Type = "SystemError",
                    Name = "Bad Working Directory",
                    Id = Guid.NewGuid()
                });
                db.SaveChanges();
            }
            using (var db = new ProducerContext())
            {
                var item = db.Jobs.Single(s => s.Id == jobId);
                item.Process.Error.Add(new Turbine.Data.Entities.ProcessError
                {
                    Error = "ERROR2",
                    Type = "SystemError",
                    Name = "Bad File",
                    Id = Guid.NewGuid()
                });
                db.SaveChanges();
            }
            using (var db = new ProducerContext())
            {
                var item = db.Jobs.Single(s => s.Id == jobId);
                Assert.IsTrue(item.Process.Error.Count == 2);
            }
        }
    }
}
