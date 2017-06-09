using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Turbine.DataEF6;

namespace DatabaseUnitTest
{
    [TestClass]
    public class SessionCreateTest
    {
        [TestMethod]
        public void TestSession_Create()
        {
            // Session
            using (var db = new ProducerContext())
            {
                var item = new Turbine.Data.Entities.Session
                {
                    Id = Guid.NewGuid(),
                    Description = "testing simulation",
                    Create = DateTime.UtcNow
                };
                db.Sessions.Add(item);
                db.SaveChanges();
            }
        }
        [TestMethod]
        [ExpectedException(typeof(System.Data.Entity.Infrastructure.DbUpdateException),
            "Create DateTime is required")]
        public void TestSession_No_Create()
        {
            // Session
            using (var db = new ProducerContext())
            {
                var item = new Turbine.Data.Entities.Session
                {
                    Id = Guid.NewGuid(),
                    Description = "testing simulation"
                };
                db.Sessions.Add(item);
                db.SaveChanges();
            }
        }
    }
}
