using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Turbine.DataEF6;
using System.Data.Entity.Validation;


namespace DatabaseUnitTest
{
    [TestClass]
    public class GeneratorClassTest
    {
        static Guid sessionId = Guid.NewGuid();
        static Guid generatorId = Guid.NewGuid();
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


            List<Turbine.Data.Entities.Job> jobsList = new List<Turbine.Data.Entities.Job>();
            // Session
            using (var db = new ProducerContext())
            {
                var item = db.Sessions.Single(s => s.Id == sessionId);
                var user = db.Users.Single(u => u.Name == userName);
                var simulation = db.Simulations.Single(s => s.Name == simulationName);

                for (int i = 0; i < 10; i++)
                {
                    var job = new Turbine.Data.Entities.Job
                    {
                        Id = Guid.NewGuid(),
                        Session = item,
                        Create = DateTime.UtcNow,
                        State = "success",
                        User = user,
                        Simulation = simulation
                    };
                    jobsList.Add(job);
                    db.Jobs.Add(job);
                }

                db.SaveChanges();
            }


            using (var db = new ProducerContext())
            {
                var session = db.Sessions.Single(s => s.Id == sessionId);
                var item = new Turbine.Data.Entities.Generator
                {
                    Id = generatorId,
                    Page = 1,
                    Session = session,
                    Create = DateTime.UtcNow
                };
                db.Generators.Add(item);
                db.SaveChanges();
            }

            using (var db = new ProducerContext())
            {
                var generator = db.Generators.Single(g => g.Id == generatorId);
                var session = db.Sessions.Single(s => s.Id == sessionId);

                Debug.WriteLine("Generator Page num: " + generator.Page);

                foreach (var job in session.Jobs)
                {
                    var item = new Turbine.Data.Entities.GeneratorJob
                    {
                        Id = Guid.NewGuid(),
                        Page = 2,
                        Generator = generator,
                        Job = job
                    };
                    db.GeneratorJobs.Add(item);
                }

                db.SaveChanges();
            }
        }

        [TestMethod]
        public void TestGenerator_Create()
        {
            Debug.WriteLine("Start Generator Test", "GeneratorClassTest.TestGenerator_Create");
            
            using (var db = new ProducerContext())
            {
                var entity = db.GeneratorJobs.Where(g => g.GeneratorId == generatorId);
                foreach (var gen in entity)
                {
                    Debug.WriteLine("entity.GeneratorJobs: " + gen.Id.ToString() 
                        ,
                        "GeneratorClassTest.TestGenerator_Create");
                    Debug.WriteLine("Job number: " + gen.Job.Count
                        + ", in page: " + gen.Page.ToString());
                    Debug.WriteLine("Generator: " + gen.GeneratorId.ToString()
                        + ", Job: " + gen.Job.Id.ToString());
                }
            }           
            
        }

        [TestMethod]
        public void TestGenerator_OtherJobs()
        {
            using (var db = new ProducerContext())
            {
                var item = db.Sessions.Single(s => s.Id == sessionId);
                var user = db.Users.Single(u => u.Name == userName);
                var simulation = db.Simulations.Single(s => s.Name == simulationName);

                for (int i = 0; i < 10; i++)
                {
                    var job = new Turbine.Data.Entities.Job
                    {
                        Id = Guid.NewGuid(),
                        Session = item,
                        Create = DateTime.UtcNow,
                        State = "error",
                        User = user,
                        Simulation = simulation
                    };
                    db.Jobs.Add(job);
                }
                db.SaveChanges();
            }

            using (var db = new ProducerContext())
            {
                // Getting Jobs that are not in the Generators table
                // By doing a left join and getting all the nulls
                var jobs = from j in db.Jobs
                           join g in db.GeneratorJobs on j.Id equals g.JobId into joinedtable
                           from x in joinedtable.DefaultIfEmpty()
                           where x.JobId == null
                           select j;

                foreach (var job in jobs)
                {
                    Debug.WriteLine("Job: " + job.Count + " is not in Generator"
                        , "GeneratorClassTest.TestGenerator_Create");
                }
            }


            // Add Another generator
            Guid generatorId2 = Guid.NewGuid();
            using (var db = new ProducerContext())
            {
                var session = db.Sessions.Single(s => s.Id == sessionId);
                var item = new Turbine.Data.Entities.Generator
                {
                    Id = generatorId2,
                    Page = 1,
                    Session = session,
                    Create = DateTime.UtcNow
                };
                db.Generators.Add(item);
                db.SaveChanges();
            }
            
            using (var db = new ProducerContext())
            {
                var item = db.Sessions.Single(s => s.Id == sessionId);
                var user = db.Users.Single(u => u.Name == userName);
                var simulation = db.Simulations.Single(s => s.Name == simulationName);

                for (int i = 0; i < 10; i++)
                {
                    var job = new Turbine.Data.Entities.Job
                    {
                        Id = Guid.NewGuid(),
                        Session = item,
                        Create = DateTime.UtcNow,
                        State = "terminate",
                        User = user,
                        Simulation = simulation
                    };
                    db.Jobs.Add(job);
                }
                db.SaveChanges();
            }

            using (var db = new ProducerContext())
            {
                var generator = db.Generators.Single(g => g.Id == generatorId2);
                var session = db.Sessions.Single(s => s.Id == sessionId);

                Debug.WriteLine("Generator Page num: " + generator.Page);

                foreach (var job in session.Jobs.Where(j => j.Count > 20))
                {
                    var item = new Turbine.Data.Entities.GeneratorJob
                    {
                        Id = Guid.NewGuid(),
                        Page = 2,
                        Generator = generator,
                        Job = job
                    };
                    db.GeneratorJobs.Add(item);
                }

                db.SaveChanges();
            }


            using (var db = new ProducerContext())
            {
                // Getting Jobs that are not in the Generators table
                // By doing a left join and getting all the nulls
                /*var jobs = from j in db.Jobs
                           join g in db.GeneratorJobs.Where(y => y.GeneratorId == generatorId2) on j.Id equals g.JobId into joinedtable
                           from x in joinedtable.DefaultIfEmpty()
                           where x.JobId == null
                           select j;*/

                var jobs = from j in db.Jobs
                           join g in db.GeneratorJobs.Where(y => y.GeneratorId == generatorId2) on j.Id equals g.JobId into joinedtable
                           from x in joinedtable.DefaultIfEmpty()
                           where x.JobId == null && j.SessionId == sessionId && (j.State == "success" || j.State == "error" || j.State == "terminate")
                           select j;

                foreach (var job in db.Jobs.OrderBy(j => j.Count))
                {
                    Debug.WriteLine("Job: " + job.Count
                        , "GeneratorClassTest.TestGenerator_Create");
                }

                foreach (var job in jobs)
                {
                    Debug.WriteLine("Job: " + job.Count + " is not in Generator"
                        , "GeneratorClassTest.TestGenerator_Create");
                }
            }
        }
    }
}
