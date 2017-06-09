using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Turbine.DataEF6;
using System.IO;


namespace DatabaseUnitTest
{
    [TestClass]
    public class JobTest
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
        public void TestJob_StateCreate()
        {
            using (var db = new ProducerContext())
            {
                var item = db.Sessions.Single(s => s.Id == sessionId);
                var user = db.Users.Single(u => u.Name == userName);
                var simulation = db.Simulations.Single(s => s.Name == simulationName);
                var job = new Turbine.Data.Entities.Job
                {
                    Id = Guid.NewGuid(),
                    Session = item,
                    Create = DateTime.UtcNow,
                    State = "create",
                    User = user,
                    Simulation = simulation
                };
                db.Jobs.Add(job);
                db.SaveChanges();
            }
        }

        [TestMethod]
        public void TestJob_UpdateSubmit()
        {
            Guid jobId = Guid.NewGuid();
            Guid consumerId = Guid.NewGuid();
            string createMsg = "Created new Job";
            string submitMsg = "Job Moved to Submit";
            using (var db = new ProducerContext())
            {
                var item = db.Sessions.Single(s => s.Id == sessionId);
                var user = db.Users.Single(u => u.Name == userName);
                var simulation = db.Simulations.Single(s => s.Name == simulationName);
                var job = new Turbine.Data.Entities.Job { 
                    Id = jobId, 
                    Session=item, 
                    Create=DateTime.UtcNow, 
                    State="create", 
                    User=user,
                    Simulation=simulation
                };
                var msg = new Turbine.Data.Entities.Message
                {
                    Id = Guid.NewGuid(),
                    Create = DateTime.UtcNow,
                    Value = createMsg
                };
                job.Messages.Add(msg);
                db.Jobs.Add(job);
                db.SaveChanges();
            }
            using (var db = new ProducerContext())
            {
                var app = db.Applications.Single(a => a.Name == "ACM");
                var consumer = new Turbine.Data.Entities.JobConsumer
                {
                    Id = consumerId,
                    keepalive = DateTime.UtcNow,
                    status = "up",
                    Application = app
                };
                db.Consumers.Add(consumer);
                db.SaveChanges();
            }

            using (var db = new ProducerContext())
            {
                var job = db.Jobs.Single(j => j.Id == jobId);
                job.State = "submit";
                job.Submit = DateTime.UtcNow;
                job.ConsumerId = consumerId;

                Assert.IsFalse(job.Initialize);
                Assert.IsFalse(job.Reset);

                var msg = new Turbine.Data.Entities.Message
                {
                    Id = Guid.NewGuid(),
                    Create = DateTime.UtcNow,
                    Value = submitMsg
                };
                job.Messages.Add(msg);

                try
                {
                    db.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }
            }
            using (var db = new ProducerContext())
            {
                var job = db.Jobs.Single(j => j.Id == jobId);
                Assert.IsTrue(job.Messages.Count == 2);
                Assert.AreEqual<string>(job.Messages.ElementAt(0).Value, createMsg);
                Assert.AreEqual<string>(job.Messages.ElementAt(1).Value, submitMsg);
            }
        }


        [TestMethod]
        public void TestJob_UpdateSetup()
        {
            Guid jobId = Guid.NewGuid();
            Guid consumerId = Guid.NewGuid();
            string createMsg = "Created new Job";
            string submitMsg = "Job Moved to Submit";
            string setupMsg = "Job Moved to Setup";
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
                var msg = new Turbine.Data.Entities.Message
                {
                    Id = Guid.NewGuid(),
                    Create = DateTime.UtcNow,
                    Value = createMsg
                };
                job.Messages.Add(msg);
                db.Jobs.Add(job);
                db.SaveChanges();
            }
            using (var db = new ProducerContext())
            {
                var app = db.Applications.Single(a => a.Name == "ACM");
                var consumer = new Turbine.Data.Entities.JobConsumer
                {
                    Id = consumerId,
                    keepalive = DateTime.UtcNow,
                    status = "up",
                    Application = app
                };
                db.Consumers.Add(consumer);
                db.SaveChanges();
            }

            int count = 0;
            using (var db = new ProducerContext())
            {
                var job = db.Jobs.Single(j => j.Id == jobId);
                count = job.Count;
                Assert.IsTrue(count > 0);

                job.State = "submit";
                job.Submit = DateTime.UtcNow;
                job.ConsumerId = consumerId;

                Assert.IsFalse(job.Initialize);
                Assert.IsFalse(job.Reset);

                var msg = new Turbine.Data.Entities.Message
                {
                    Id = Guid.NewGuid(),
                    Create = DateTime.UtcNow,
                    Value = submitMsg
                };
                job.Messages.Add(msg);

                try
                {
                    db.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }
            }
            using (var db = new ProducerContext())
            {
                var job = db.Jobs.Single(j => j.Id == jobId);
                Assert.AreEqual(count, job.Count);

                job.State = "setup";
                job.Setup = DateTime.UtcNow;
                var msg = new Turbine.Data.Entities.Message
                {
                    Id = Guid.NewGuid(),
                    Create = DateTime.UtcNow,
                    Value = setupMsg
                };
                job.Messages.Add(msg);

                try
                {
                    db.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }
            }
            using (var db = new ProducerContext())
            {
                var job = db.Jobs.Single(j => j.Id == jobId);
                Assert.AreEqual(count, job.Count);
                Assert.IsTrue(job.Messages.Count == 3);
                Assert.AreEqual<string>(job.Messages.ElementAt(0).Value, createMsg);
                Assert.AreEqual<string>(job.Messages.ElementAt(1).Value, submitMsg);
                Assert.AreEqual<string>(job.Messages.ElementAt(2).Value, setupMsg);
            }
        }
        [TestMethod]
        public void TestJob_UpdateRunning()
        {
            Guid jobId = Guid.NewGuid();
            Guid consumerId = Guid.NewGuid();
            string createMsg = "Created new Job";
            string submitMsg = "Job Moved to Submit";
            string setupMsg = "Job Moved to Setup";
            string runMsg = "Job Moved to Running";
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
                var msg = new Turbine.Data.Entities.Message
                {
                    Id = Guid.NewGuid(),
                    Create = DateTime.UtcNow,
                    Value = createMsg
                };
                job.Messages.Add(msg);
                db.Jobs.Add(job);
                db.SaveChanges();
            }
            using (var db = new ProducerContext())
            {
                var app = db.Applications.Single(a => a.Name == "ACM");
                var consumer = new Turbine.Data.Entities.JobConsumer
                {
                    Id = consumerId,
                    keepalive = DateTime.UtcNow,
                    status = "up",
                    Application = app
                };
                db.Consumers.Add(consumer);
                db.SaveChanges();
            }

            using (var db = new ProducerContext())
            {
                var job = db.Jobs.Single(j => j.Id == jobId);
                job.State = "submit";
                job.Submit = DateTime.UtcNow;
                job.ConsumerId = consumerId;

                Assert.IsFalse(job.Initialize);
                Assert.IsFalse(job.Reset);

                var msg = new Turbine.Data.Entities.Message
                {
                    Id = Guid.NewGuid(),
                    Create = DateTime.UtcNow,
                    Value = submitMsg
                };
                job.Messages.Add(msg);

                try
                {
                    db.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }
            }
            using (var db = new ProducerContext())
            {
                var job = db.Jobs.Single(j => j.Id == jobId);
                job.State = "setup";
                job.Setup = DateTime.UtcNow;
                var msg = new Turbine.Data.Entities.Message
                {
                    Id = Guid.NewGuid(),
                    Create = DateTime.UtcNow,
                    Value = setupMsg
                };
                job.Messages.Add(msg);

                try
                {
                    db.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }
            }
            using (var db = new ProducerContext())
            {
                var job = db.Jobs.Single(j => j.Id == jobId);
                job.State = "running";
                job.Running = DateTime.UtcNow;
                var msg = new Turbine.Data.Entities.Message
                {
                    Id = Guid.NewGuid(),
                    Create = DateTime.UtcNow,
                    Value = runMsg
                };
                job.Messages.Add(msg);
                job.Process = new Turbine.Data.Entities.Process
                {
                };
                try
                {
                    db.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }
            }
            using (var db = new ProducerContext())
            {
                var job = db.Jobs.Single(j => j.Id == jobId);
                Assert.IsTrue(job.Messages.Count == 4);
                Assert.AreEqual<string>(job.Messages.ElementAt(0).Value, createMsg);
                Assert.AreEqual<string>(job.Messages.ElementAt(1).Value, submitMsg);
                Assert.AreEqual<string>(job.Messages.ElementAt(2).Value, setupMsg);
                Assert.AreEqual<string>(job.Messages.ElementAt(3).Value, runMsg);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.Data.Entity.Infrastructure.DbUpdateException),
            "Simulation is required")]
        public void TestJob_NoSimulation()
        {
            using (var db = new ProducerContext())
            {
                var item = db.Sessions.Single(s => s.Id == sessionId);
                var app = db.Applications.Single(a => a.Name == "ACM");
                var consumer = new Turbine.Data.Entities.JobConsumer
                {
                    Id = Guid.NewGuid(),
                    keepalive = DateTime.UtcNow,
                    status = "up",
                    Application = app
                };
                var user = db.Users.Single(u => u.Name == userName);
                var job = new Turbine.Data.Entities.Job
                {
                    Id = Guid.NewGuid(),
                    Session = item,
                    Create = DateTime.UtcNow,
                    State = "create",
                    User = user,
                    Consumer = consumer
                };
                db.Jobs.Add(job);
                try
                {
                    db.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }
            }
        }
        [TestMethod]
        [ExpectedException(typeof(System.Data.Entity.Infrastructure.DbUpdateException),
            "Session is required")]
        public void TestJob_NoSession()
        {
            using (var db = new ProducerContext())
            {
                var item = db.Sessions.Single(s => s.Id == sessionId);
                var app = db.Applications.Single(a => a.Name == "ACM");
                var consumer = new Turbine.Data.Entities.JobConsumer
                {
                    Id = Guid.NewGuid(),
                    keepalive = DateTime.UtcNow,
                    status = "up",
                    Application = app
                };
                var user = db.Users.Single(u => u.Name == userName);
                var simulation = db.Simulations.Single(s => s.Name == simulationName);
                var job = new Turbine.Data.Entities.Job
                {
                    Id = Guid.NewGuid(),
                    //Session = item,
                    Create = DateTime.UtcNow,
                    State = "create",
                    User = user,
                    Consumer = consumer,
                    Simulation = simulation
                };
                db.Jobs.Add(job);
                try
                {
                    db.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.Data.Entity.Infrastructure.DbUpdateException),
            "Create DateTime is required")]
        public void TestJob_NoCreate()
        {
            using (var db = new ProducerContext())
            {
                var item = db.Sessions.Single(s => s.Id == sessionId);
                var app = db.Applications.Single(a => a.Name == "ACM");
                var consumer = new Turbine.Data.Entities.JobConsumer
                {
                    Id = Guid.NewGuid(),
                    keepalive = DateTime.UtcNow,
                    status="up",
                    Application=app
                };
                var user = db.Users.Single(u => u.Name == userName);
                var simulation = db.Simulations.Single(s => s.Name == simulationName);
                var job = new Turbine.Data.Entities.Job
                {
                    Id = Guid.NewGuid(),
                    Session = item,
                    //Create = DateTime.UtcNow,
                    State = "create",
                    User = user,
                    Consumer = consumer,
                    Simulation = simulation
                };
                db.Jobs.Add(job);
                try
                {
                    db.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.Data.Entity.Validation.DbEntityValidationException),
            "State is required")]
        public void TestJob_NoState()
        {
            using (var db = new ProducerContext())
            {
                var item = db.Sessions.Single(s => s.Id == sessionId);
                var app = db.Applications.Single(a => a.Name == "ACM");
                var consumer = new Turbine.Data.Entities.JobConsumer
                {
                    Id = Guid.NewGuid(),
                    keepalive = DateTime.UtcNow,
                    status = "up",
                    Application=app
                };
                var user = db.Users.Single(u => u.Name == userName);
                var simulation = db.Simulations.Single(s => s.Name == simulationName);
                var job = new Turbine.Data.Entities.Job
                {
                    Id = Guid.NewGuid(),
                    Session = item,
                    Create = DateTime.UtcNow,
                    //State = "create",
                    User = user,
                    Consumer = consumer,
                    Simulation = simulation
                };
                db.Jobs.Add(job);
                try
                {
                    db.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(System.Data.Entity.Validation.DbEntityValidationException),
            "User is required")]
        public void TestJob_NoUser()
        {
            using (var db = new ProducerContext())
            {
                var item = db.Sessions.Single(s => s.Id == sessionId);
                var app = db.Applications.Single(a => a.Name == "ACM");
                var consumer = new Turbine.Data.Entities.JobConsumer
                {
                    Id = Guid.NewGuid(),
                    keepalive = DateTime.UtcNow,
                    status = "up",
                    Application = app
                };
                var user = db.Users.Single(u => u.Name == userName);
                var simulation = db.Simulations.Single(s => s.Name == simulationName);
                var job = new Turbine.Data.Entities.Job
                {
                    Id = Guid.NewGuid(),
                    Session = item,
                    Create = DateTime.UtcNow,
                    State = "create",
                    //User = user,
                    Consumer = consumer,
                    Simulation = simulation
                };
                db.Jobs.Add(job);
                try
                {
                    db.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }
            }
        }
    }
}
