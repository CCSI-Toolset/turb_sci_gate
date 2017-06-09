using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Turbine.Data.Test
{

    [TestClass]
    public class DataEncodingTest : DataTestScenario
    {
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            DataTestScenario.ClassInit(context);
        }

        [TestMethod]
        public void BaseEncodingTest()
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                var entity = container.Simulations.First<Simulation>();
                byte[] data = entity.Configuration;
                string config = System.Text.Encoding.ASCII.GetString(data);
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                File.WriteAllBytes(Path.Combine(path, "test_sinter.txt"), data);

            }
        }
    }
}
