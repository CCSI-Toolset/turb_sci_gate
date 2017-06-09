using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Turbine.Producer.Data.Contract;
using System.Diagnostics;
using Turbine.Data.Serialize;
using Turbine.Data.Marshal;

namespace ConsumerDataIntegrationTests
{
    [TestClass]
    public class DataMarshalTest
    {
        [TestInitialize()]
        public void MyTestInitialize()
        {
            Debug.WriteLine("Initialize", this.GetType().Name);
            ConsumerContext.id = Guid.NewGuid();
            using (var container = new Turbine.Data.TurbineModelContainer())
            {
                container.Jobs.ToList().ForEach(m => container.DeleteObject(m));
                container.SinterProcesses.ToList().ForEach(s => container.DeleteObject(s));
                container.Simulations.ToList().ForEach(n => container.DeleteObject(n));
                container.Applications.ToList().ForEach(i => container.DeleteObject(i));
                container.InputFileTypes.ToList().ForEach(j => container.DeleteObject(j));
                container.StagedInputFiles.ToList().ForEach(k => container.DeleteObject(k));
                container.SimulationStagedInputs.ToList().ForEach(l => container.DeleteObject(l));
                container.Sessions.ToList().ForEach(m => container.DeleteObject(m));
                container.SaveChanges();

            }

            var contract = new Turbine.Producer.Data.Contract.AspenSessionProducerContract();
            for (int i = 0; i <= 1000; i++)
            {
                System.Threading.Thread.Sleep(1);
                contract.Create();
            }
        }
        [TestMethod]
        public void SessionCreateOrder()
        {
            int rpp = 100;
            HashSet<Guid> s = new HashSet<Guid>();

            for (int page = 1; page <= 10; page++)
            {
                List<Guid> sessionList = DataMarshal.GetSessions(page, rpp);
                Turbine.Data.Session entity = null;
                DateTime dt = DateTime.Now;

                foreach (Guid guid in sessionList)
                {
                    Assert.IsFalse(s.Contains<Guid>(guid));
                    s.Add(guid);
                    entity = DataMarshal.GetSessionMeta(guid);
                    Debug.WriteLine(entity.Id);
                    // test Create Timestamp
                    Assert.IsTrue(DateTime.Compare(dt, entity.Create) <= 0, "Session '" + guid + "'Out of Order");
                    dt = entity.Create;
                }

                Assert.IsTrue(sessionList.Count() == 100);
            }
        }
        [TestMethod]
        public void SessionCreateOrderLastPage()
        {
            int page = -1;
            int rpp = 100;
            List<Guid> sessionList = DataMarshal.GetSessions(page, rpp);
            Assert.IsTrue(sessionList.Count() == 1, "Count was " + sessionList.Count());
            List<Guid> sessionList2 = DataMarshal.GetSessions(11, rpp);
            Assert.IsTrue(sessionList.Count() == 1);

            for (int i = 0; i < sessionList.Count(); i++)
            {
                Assert.AreEqual(sessionList.ElementAt<Guid>(i), sessionList2.ElementAt<Guid>(i));
            }

        }

        [TestMethod]
        public void SessionPageLargeRpp()
        {
            int page = 1;
            int rpp = 10000;
            List<Guid> sessionList = DataMarshal.GetSessions(page, rpp);
            Assert.IsTrue(sessionList.Count() == 1001, "Count was " + sessionList.Count());
        }

        [TestMethod]
        public void SessionPageLargeRppLast()
        {
            int page = -1;
            int rpp = 10000;
            List<Guid> sessionList = DataMarshal.GetSessions(page, rpp);
            Assert.IsTrue(sessionList.Count() == 1001, "Count was " + sessionList.Count());
        }

        [TestMethod]
        public void SessionPageLargeRppZeroResults()
        {
            int page = 2;
            int rpp = 10000;
            List<Guid> sessionList = DataMarshal.GetSessions(page, rpp);
            Assert.IsTrue(sessionList.Count() == 0, "Count was " + sessionList.Count());
        }
    }
}
