using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Turbine.DataEF6;
using System.IO;

namespace DatabaseUnitTest
{
    [TestClass]
    public class SimulationTest
    {
        static string username = "testuser";

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            BaseDatabaseTest.CleanUpDatabase();

            using (var db = new ProducerContext())
            {
                var app = new Turbine.Data.Entities.Application { Name = "ACM", Version = "7.3" };
                db.Applications.Add(app);
                var user = new Turbine.Data.Entities.User { Name = username };
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
                var user = db.Users.Single(u => u.Name == username);
                var sim = new Turbine.Data.Entities.Simulation
                {
                    Id = Guid.NewGuid(),
                    Application = app,
                    Create = DateTime.UtcNow,
                    Name = "Test",
                    Update = DateTime.UtcNow,
                    User = user
                };
                db.Simulations.Add(sim);
                db.SaveChanges();
            }
        }


        [TestMethod]
        public void TestCreateSimulation()
        {
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

            Assert.IsTrue(dataACMF.Length > 4000);
            Assert.IsTrue(dataConfig.Length > 0);
            using (var db = new ProducerContext())
            {
                var sim = db.Simulations.Single(s => s.Name == "Test");
                var it = sim.Application.InputFileTypes.Single(s => s.Name == "aspenfile");
                var item = new Turbine.Data.Entities.SimulationStagedInput
                {
                    Id = Guid.NewGuid(),
                    Name = it.Name,
                    Content = dataACMF,
                    Hash = "",
                    Simulation = sim
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
        public void TestCreateSimulationEmptyInputs()
        {
            CreateSimulationEmptyInputs("Test2");
        }
        private void CreateSimulationEmptyInputs(string simulationName)
        {
            byte[] dataACMF = new Byte[] {};
            byte[] dataConfig = new Byte[] { };
            Console.WriteLine("Testing");
            /*
            using (var db = new ProducerContext())
            {
                var app = db.Applications.Single(s => s.Name == "ACM");
                var user = db.Users.Single(u => u.Name == username);
                var sim = new Turbine.Data.Entities.Simulation
                {
                    Id = Guid.NewGuid(),
                    Application = app,
                    Create = DateTime.UtcNow,
                    Name = "Test3",
                    Update = DateTime.UtcNow,
                    User = user
                };
                db.Simulations.Add(sim);
                db.SaveChanges();
            }
             */

            using (var db = new ProducerContext())
            {
                var app = db.Applications.Single(s => s.Name == "ACM");
                var user = db.Users.Single(u => u.Name == username);
                Console.WriteLine("User: {0}", user.Name);
                var sim = new Turbine.Data.Entities.Simulation()
                {
                    Name = simulationName,
                    Id = Guid.NewGuid(),
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
                var sim = db.Simulations.Single(s => s.Name == simulationName);
                Console.WriteLine("Simulation: {0}", sim.Name);
                
                var it = sim.Application.InputFileTypes.Single(s => s.Name == "aspenfile");
                var item = new Turbine.Data.Entities.SimulationStagedInput
                {
                    Id = Guid.NewGuid(),
                    Name = it.Name,
                    Content = dataACMF,
                    Hash = "",
                    Simulation = sim
                };
                Console.WriteLine("SimulationStagedInput: {0}", item.Name);
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
                Console.WriteLine("SimulationStagedInput: {0}", item.Name);
                sim.SimulationStagedInputs.Add(item);

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
        public void TestCreateSimulations()
        {
            CreateSimulationEmptyInputs("Test3");
            CreateSimulationEmptyInputs("Test4");

            using (var db = new ProducerContext())
            {
                var obj = db.Simulations.Single(i => i.Name == "Test3");
                var input = obj.SimulationStagedInputs.SingleOrDefault(i => i.Name == "configuration");
                Assert.IsNotNull(input);
            }

        }
    }
}
