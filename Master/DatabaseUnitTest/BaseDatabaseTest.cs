using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Turbine.DataEF6;
using System.Collections.Generic;
using System.Data.Entity;
using System.Text;

namespace DatabaseUnitTest
{
    public class BaseDatabaseTest
    {
        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            Console.WriteLine("assembly init");
            CleanUpDatabase();
        }
        [AssemblyCleanup()]
        public static void AssemblyCleanup()
        {
            Console.WriteLine("assembly cleanup");
        }

        public static void CleanUpDatabase() 
        {
            using (var db = new ProducerContext())
            {
                db.Database.ExecuteSqlCommand("delete FROM GeneratorJobs");
                db.Database.ExecuteSqlCommand("delete FROM Generators");
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
    }
}
