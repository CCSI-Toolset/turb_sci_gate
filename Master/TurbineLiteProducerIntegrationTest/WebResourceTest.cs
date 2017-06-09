using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using Turbine.Web.Contracts;
using System.IO;
using System.ServiceModel.Web;
using WebOperationContext = System.ServiceModel.Web.MockedWebOperationContext;
using Moq;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
//using Turbine.Consumer.Data.Contract.Behaviors;
using System.Data.Entity.Core;
using Turbine.Lite.Web.Resources.Contracts;
using Turbine.Data.Contract.Behaviors;
using Turbine.Data.Serialize;
//using Turbine.Consumer.Contract.Behaviors;


namespace TurbineLiteProducerIntegrationTest
{

    internal class MockSimulationResource : Turbine.Lite.Web.Resources.SimulationResource
    {
        override protected string OutgoingWebResponseContext_ContentType
        {
            get
            {
                return WebOperationContext.Current.OutgoingResponse.ContentType;
            }
            set
            {
                WebOperationContext.Current.OutgoingResponse.ContentType = value;
            }
        }
        override protected System.Net.HttpStatusCode OutgoingWebResponseContext_StatusCode
        {
            get
            {
                return WebOperationContext.Current.OutgoingResponse.StatusCode;
            }
            set
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = value;
            }
        }
    }

    internal class MockSessionResource : Turbine.Lite.Web.Resources.SessionResource
    {
        override protected string OutgoingWebResponseContext_ContentType
        {
            get
            {
                return WebOperationContext.Current.OutgoingResponse.ContentType;
            }
            set
            {
                WebOperationContext.Current.OutgoingResponse.ContentType = value;
            }
        }
        override protected System.Net.HttpStatusCode OutgoingWebResponseContext_StatusCode
        {
            get
            {
                return WebOperationContext.Current.OutgoingResponse.StatusCode;
            }
            set
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = value;
            }
        }

        /// <summary>
        /// return default value
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        override protected int QueryParameters_GetPage(int p)
        {
            return p;
        }

        override protected int QueryParameters_GetPageSize(int p)
        {
            return p;
        }

        override protected bool QueryParameters_GetVerbose(bool p)
        {
            return p;
        }
        override protected bool QueryParameters_GetMetaData(bool p)
        {
            return p;
        }
    } 

    [TestClass]
    public class WebResourceTest
    {

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
                        Type = "application/octet-stream"
                    });
                    item.InputFileTypes.Add(new Turbine.Data.Entities.InputFileType
                    {
                        Id = Guid.NewGuid(),
                        Name = "any",
                        Required = false,
                        Type = "application/octet-stream"
                    });
                    db.SaveChanges();
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                Debug.WriteLine("Ignore Application entries already made");
            }
            try
            {
                using (var db = new Turbine.DataEF6.ProducerContext())
                {
                    var item = db.Applications.Single(s => s.Name == "AspenPlus");
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
                        Type = "application/octet-stream"
                    });
                    db.SaveChanges();
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                Debug.WriteLine("Ignore Application entries already made");
            }
        }
        public static void CleanUpDatabase()
        {
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
        }

        [TestMethod]
        public void TestSimulationUpdate()
        {
            Mock<IWebOperationContext> mockContext = new Mock<IWebOperationContext> { DefaultValue = DefaultValue.Mock };
            var simulationName = "RandomName1";
            Debug.WriteLine("Test");
            ISimulationResource iSimulation = new MockSimulationResource();
            iSimulation.UpdateSimulation(simulationName, new Turbine.Data.Serialize.Simulation { Application = "ACM", Name = simulationName });
            using (new MockedWebOperationContext(mockContext.Object))
            {
                using (var fstream = File.Open(@"models\Hybrid_v0.51_rev1.1_UQ_0809_sinter.json", FileMode.Open))
                {
                    iSimulation.UpdateStagedInputFile(simulationName, "configuration", fstream);
                }
            }
            //mockContext.VerifySet(c => c.OutgoingResponse.ContentType, "application/atom+xml");
            using (new MockedWebOperationContext(mockContext.Object))
            {
                using (var fstream = File.Open(@"models\Hybrid_v0.51_rev1.1_UQ_0809.acmf", FileMode.Open))
                {
                    iSimulation.UpdateStagedInputFile(simulationName, "aspenfile", fstream);
                }
            }
            //mockContext.VerifySet(c => c.OutgoingResponse.ContentType, "application/atom+xml");

            using (new MockedWebOperationContext(mockContext.Object))
            {
                using (var fstream = File.Open(@"models\Hybrid_v0.51_rev1.1_UQ_0809_sinter.json", FileMode.Open))
                {
                    iSimulation.UpdateStagedInputFile(simulationName, "configuration", fstream);
                }
            }

            var mySimulation = iSimulation.GetSimulation(simulationName);
            Assert.AreEqual(mySimulation.Application, "ACM");
            Assert.AreEqual(mySimulation.Name, simulationName);
            Assert.AreEqual(mySimulation.StagedInputs.Count, 2);
        }

        [TestMethod]
        public void TestSimulationUpdate_50MB()
        {
            Mock<IWebOperationContext> mockContext = new Mock<IWebOperationContext> { DefaultValue = DefaultValue.Mock };
            var simulationName = Guid.NewGuid().ToString();
            Debug.WriteLine("Test");
            ISimulationResource iSimulation = new MockSimulationResource();
            iSimulation.UpdateSimulation(simulationName, new Turbine.Data.Serialize.Simulation { Application = "ACM", Name = simulationName });

            using (new MockedWebOperationContext(mockContext.Object))
            {
                var sizeInBytes = 50 * 1048576;
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        while (writer.BaseStream.Length <= sizeInBytes)
                        {
                            writer.Write("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                        }
                        Debug.WriteLine("LENGTH: " + writer.BaseStream.Length);
                        iSimulation.UpdateStagedInputFile(simulationName, "aspenfile", stream);
                        writer.Close();
                    }
                }
            }
        }

        [TestMethod]
        public void TestSimulationUpdate_ACM_Directory_Any()
        {
            Mock<IWebOperationContext> mockContext = new Mock<IWebOperationContext> { DefaultValue = DefaultValue.Mock };
            var simulationName = "RandomName2";
            Debug.WriteLine("Test");
            ISimulationResource iSimulation = new MockSimulationResource();
            iSimulation.UpdateSimulation(simulationName, new Turbine.Data.Serialize.Simulation { Application = "ACM", Name = simulationName });

            using (new MockedWebOperationContext(mockContext.Object))
            {
                var sizeInBytes = 500;
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        while (writer.BaseStream.Length <= sizeInBytes)
                        {
                            writer.Write("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                        }
                        Debug.WriteLine("LENGTH: " + writer.BaseStream.Length);
                        iSimulation.UpdateStagedInputFile(simulationName, "var/librarytest.dll", stream);
                        writer.Close();
                    }
                }
            }
            var mySimulation = iSimulation.GetSimulation(simulationName);
            Assert.AreEqual(mySimulation.Application, "ACM");
            Assert.AreEqual(mySimulation.Name, simulationName);
            Assert.AreEqual(mySimulation.StagedInputs.Count, 3);

        }

        [TestMethod]
        public void TestSimulationUpdate_ACM_Any()
        {
            Mock<IWebOperationContext> mockContext = new Mock<IWebOperationContext> { DefaultValue = DefaultValue.Mock };
            var simulationName = "RandomName3";
            Debug.WriteLine("Test");
            ISimulationResource iSimulation = new MockSimulationResource();
            iSimulation.UpdateSimulation(simulationName, new Turbine.Data.Serialize.Simulation { Application = "ACM", Name = simulationName });

            using (new MockedWebOperationContext(mockContext.Object))
            {
                var sizeInBytes = 500;
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        while (writer.BaseStream.Length <= sizeInBytes)
                        {
                            writer.Write("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                        }
                        Debug.WriteLine("LENGTH: " + writer.BaseStream.Length);
                        iSimulation.UpdateStagedInputFile(simulationName, "librarytest.dll", stream);
                        writer.Close();
                    }
                }
            }
            var mySimulation = iSimulation.GetSimulation(simulationName);
            Assert.AreEqual(mySimulation.Application, "ACM");
            Assert.AreEqual(mySimulation.Name, simulationName);
            Assert.AreEqual(mySimulation.StagedInputs.Count, 3);

        }

        [TestMethod]
        //[ExpectedException(typeof(WebFaultException<ErrorDetail>), "Wildcard was not configured for AspenPlus")]
        public void TestSimulationUpdate_AspenPlus_NoAny()
        {
            Mock<IWebOperationContext> mockContext = new Mock<IWebOperationContext> { DefaultValue = DefaultValue.Mock };
            var simulationName = "RandomName4";
            Debug.WriteLine("Test");
            ISimulationResource iSimulation = new MockSimulationResource();
            iSimulation.UpdateSimulation(simulationName, new Turbine.Data.Serialize.Simulation { Application = "AspenPlus", Name = simulationName });

            using (new MockedWebOperationContext(mockContext.Object))
            {
                var sizeInBytes = 500;
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        while (writer.BaseStream.Length <= sizeInBytes)
                        {
                            writer.Write("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                        }
                        Debug.WriteLine("LENGTH: " + writer.BaseStream.Length);
                        iSimulation.UpdateStagedInputFile(simulationName, "librarytest.dll", stream);
                        writer.Close();
                    }
                }
            }
        }
    }
}
