using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Turbine.DataEF6;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace DatabaseUnitTest
{
    /// <summary>
    ///  Test Top-Level Producer Database Resource Contracts
    ///  Resources
    ///     Applications
    ///     Simulations
    ///     Jobs
    ///     Session
    ///     JobConsumers
    /// </summary>
    [TestClass]
    public class ProducerReadTest
    {
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
        }

        [TestInitialize()]
        public void Initialize()
        {
        }

        [TestMethod]
        public void TestApplication_ReadAll()
        {
            var guid = Guid.NewGuid();
            using (var db = new ProducerContext())
            {
                var query = from b in db.Applications
                            orderby b.Name
                            select b;

                Console.WriteLine("Applications in the database:");
                foreach (var item in query)
                {
                    Console.WriteLine(String.Format("    {0}", item.Name));
                }
            }
        }

        [TestMethod]
        public void TestSimulation_ReadAll()
        {
            var guid = Guid.NewGuid();
            using (var db = new ProducerContext())
            {
                // Display all Blogs from the database 
                var query = from b in db.Simulations
                            orderby b.Name
                            select b;

                Console.WriteLine("Simulations in the database:");
                foreach (var item in query)
                {
                    Console.WriteLine(String.Format("    {0}", item.Name));
                }
            }
        }

        [TestMethod]
        public void TestSession_ReadAll()
        {
            var guid = Guid.NewGuid();
            using (var db = new ProducerContext())
            {
                // Display all Blogs from the database 
                var query = from b in db.Sessions
                            orderby b.Create
                            select b;

                Console.WriteLine("Simulations in the database:");
                foreach (var item in query)
                {
                    Console.WriteLine(String.Format("    {0}", item.Id));
                }
            }
        }

        [TestMethod]
        public void TestJobs_ReadAll()
        {
            var guid = Guid.NewGuid();
            using (var db = new ProducerContext())
            {
                // Display all Blogs from the database 
                var query = from b in db.Jobs
                            orderby b.Submit
                            select b;

                Console.WriteLine("Simulations in the database:");
                foreach (var item in query)
                {
                    Console.WriteLine(String.Format("    {0}", item.Id));
                }
            }
        }

        [TestMethod]
        public void TestConsumer_ReadAll()
        {
            var guid = Guid.NewGuid();
            using (var db = new ProducerContext())
            {
                // Display all Blogs from the database 
                var query = from b in db.Consumers
                            orderby b.Id
                            select b;

                Console.WriteLine("Simulations in the database:");
                foreach (var item in query)
                {
                    Console.WriteLine(String.Format("    {0}", item.Id));
                }
            }
        }
    }
}
