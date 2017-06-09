using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Turbine.Lite.Web.Resources;
using Turbine.Web.Contracts;
using Turbine.Data.Serialize;
 
namespace Turbine.Data.Test
{
    [TestClass]
    public class SimulationServiceTest : DataTestScenario
    {
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            DataTestScenario.ClassInit(context);
        }

        [TestMethod]
        public void TestGetSimulations()
        {
            var comparer = StringComparer.OrdinalIgnoreCase;
            byte[] bArray = null;
            String data = null;
            var service = new SimulationResource();
            Simulations sims = service.GetSimulations();

            Assert.AreEqual(sims.Count, 10);
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(sims[i].Name, String.Format("testSim{0}",i+1));
                //data = sims[i].Backup;
                bArray = System.Text.Encoding.ASCII.GetBytes(data);
                Assert.IsNotNull(SimulationBackupMD5);
                Assert.IsTrue(0 == comparer.Compare(GetMd5Hash(bArray), SimulationBackupMD5),
                    "simulation backup array failed hash comparison");
                //data = sims[i].Config;
                bArray = System.Text.Encoding.ASCII.GetBytes(data);
                Assert.IsNotNull(SimulationConfigMD5);
                Assert.IsTrue(0 == comparer.Compare(GetMd5Hash(bArray), SimulationConfigMD5),
                    "simulation config array failed hash comparison");
            }
        }
    }
}
