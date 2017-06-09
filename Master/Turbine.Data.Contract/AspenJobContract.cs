using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Common;
using Turbine.Data.Contract.Behaviors;
using System.IO;
using Newtonsoft.Json;

namespace Turbine.Data.Contract
{
    public class AspenJobContract
    {
        private AspenJobContract() {}

        // Consumer grabs job in submit state off queue
        public static IJobConsumerContract GetNext()
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Job obj = container.Jobs.FirstOrDefault<Job>(s => s.State == "submit");
                if (obj == null) return null;
                return new AspenJobConsumerContract(obj.Id);
            }
        }

        public static IJobProducerContract Create(string simulationName, Guid sessionID, bool initialize)
        {
            int id = -1;
            IContext ctx = AppUtility.GetAppContext();
            String userName = ctx.UserName;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Simulation obj = container.Simulations.Single<Simulation>(s => s.Name == simulationName);
                User user = container.Users.Single<User>(u => u.Name == userName);
                Session session = container.Sessions.SingleOrDefault<Session>(i => i.guid == sessionID);
                if (session == null)
                {
                    session = new Session() { guid = sessionID, Create = DateTime.UtcNow, User = user };
                    container.Sessions.AddObject(session);
                    container.SaveChanges();
                }

                Job job = new Job()
                {
                    Simulation = obj,
                    State = "create",
                    Create = DateTime.UtcNow,
                    Process = new SinterProcess(),
                    User = user,
                    Session = session, 
                    Initialize = initialize
                };
                container.SaveChanges();
                id = job.Id;
            }
            return new AspenJobProducerContract(id);
        }

        public static IJobConsumerContract GetConsumerContract(int id)
        {
            return new AspenJobConsumerContract(id);
        }
        public static IJobProducerContract GetProducerContract(int id)
        {
            return new AspenJobProducerContract(id);
        }
    }
}
