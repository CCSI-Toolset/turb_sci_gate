using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Security.Cryptography;

namespace Turbine.Data.Test
{
    /// <summary>
    /// EntityModelTest initializes a database with Users, Simulations, Jobs, etc and tests
    /// the values and associations.
    /// </summary>
    [TestClass]
    public class EntityModelTest : DataTestScenario
    {

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            DataTestScenario.ClassInit(context);
        }
        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestUsers()
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                List<User> users = container.Users.ToList<User>();
                Debug.WriteLine("Users: {0}", users.Count);
                Debug.Assert(users.Count == 10, "Expecting 10 users");
                foreach (User u in users)
                {
                    Debug.WriteLine("  {0}:{1}", u.Name, u.Token);
                    Assert.IsTrue(u.Simulations.Count == 1, "unexpected number of simulations");
                }
            }
        }
        [TestMethod]
        public void TestSimulations()
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                StringComparer comparer = StringComparer.OrdinalIgnoreCase;
                byte[] bArray = null;
                //String path = null;
                foreach (Simulation s in container.Simulations)
                {
                    Debug.WriteLine(String.Format("      {0}", s.Name));
                    bArray = s.Backup;
                    //path = Path.Combine("test", s.Name);
                    //Directory.CreateDirectory(path);
                    //File.WriteAllBytes(Path.Combine(path, "test.bkp"), 
                    //    backupArray);
                    Assert.IsTrue(0 == comparer.Compare(GetMd5Hash(bArray), SimulationBackupMD5),
                        "simulation backup array failed hash comparison");
                    bArray = s.Configuration;
                    Assert.IsTrue(0 == comparer.Compare(GetMd5Hash(bArray), SimulationConfigMD5),
                        "simulation config array failed hash comparison");
                }
            }
        }
        [TestMethod]
        public void TestJobs()
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                User user;
                foreach (Job job in container.Jobs)
                {
                    user = job.User;
                    Assert.IsTrue(job.State == "create", "wrong job state");
                    Assert.IsTrue(job.Process.Errors.Count == 3, "expecting 3 errors");
                }
            }
        }
    }
}
