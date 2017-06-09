using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
//using Newtonsoft.Json;
using Turbine.Data.Contract.Behaviors;
using Turbine.Producer.Data.Contract.Behaviors;
using Turbine.Producer.Contracts;
using Turbine.Data;

namespace Turbine.Producer.Data.Contract
{
    public class AspenJobContract
    {
        private AspenJobContract() {}

        public static IJobProducerContract Create(string simulationName, Guid sessionID, bool initialize, bool reset)
        {
            int id = -1;
            IProducerContext ctx = Container.GetAppContext();
            String userName = ctx.UserName;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Simulation obj = container.Simulations.OrderByDescending(q => q.Create).First<Simulation>(s => s.Name == simulationName);
                User user = container.Users.Single<User>(u => u.Name == userName);
                Session session = container.Sessions.SingleOrDefault<Session>(i => i.Id == sessionID);
                if (session == null)
                {
                    session = new Session() { Id = sessionID, Create = DateTime.UtcNow, User = user };
                    container.Sessions.AddObject(session);
                    container.SaveChanges();
                }
                System.Diagnostics.Debug.WriteLine(String.Format("simulation {0}, User {1}, Session {2}",
                    simulationName, user.Name, session.Id), "AspenJobContract");
                Job job = new Job()
                {
                    guid = Guid.NewGuid(),
                    Simulation = obj,
                    State = "create",
                    Create = DateTime.UtcNow,
                    Process = new SinterProcess() { Id = Guid.NewGuid() },
                    User = user,
                    Session = session,
                    Initialize = initialize,
                    Reset = reset,
                };
                container.SaveChanges();
                id = job.Id;
            }
            return new AspenJobProducerContract() { Id = id };
        }
    }
}
