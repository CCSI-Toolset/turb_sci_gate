using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Turbine.Data.Contract;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Turbine.Consumer.Data.Contract;

namespace Turbine.Data.Test
{
    [TestClass]
    public class JobSweeperTest : DataTestScenario
    {
        static int jobCount = 0;

        [ClassInitialize()]
        public new static void ClassInit(TestContext context)
        {
            int ms = -5000;
            DataTestScenario.ClassInit(context);
            var msg = new Message();
            msg.Value = "Job Sweeper Test";
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                jobCount = container.Jobs.Count();
                foreach (Job obj in container.Jobs.OrderBy(s => s.Submit))
                {
                    ms -= 500;
                    obj.Messages.Add(msg);
                    obj.State = "submit";
                    obj.Submit = DateTime.UtcNow.AddMilliseconds(ms);
                }
                container.SaveChanges();
            }
        }

        [TestMethod]
        public void TestSweep()
        {
            int count = JobSweeper.SweepSubmitQueue(5000, 5);
            Assert.AreEqual(count, 5);
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                count = container.Jobs.Count(s => s.State == "expired");
            }

            Assert.AreEqual(count, 5);
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                jobCount = container.Jobs.Count();
                foreach (Job obj in container.Jobs.OrderBy(s => s.Submit))
                {
                    Console.WriteLine(String.Format("job({0}) submit {1} state {2}", obj.Id, obj.Submit, obj.State));
                }
                container.SaveChanges();
            }

            count = JobSweeper.SweepSubmitQueue(5000, 5);
            Assert.AreEqual(count, 5);
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                count = container.Jobs.Count(s => s.State == "expired");
            }
            Assert.AreEqual(count, 10);
        }
    }
}
