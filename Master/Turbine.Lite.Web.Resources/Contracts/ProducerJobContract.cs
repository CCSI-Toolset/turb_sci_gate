using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
//using Turbine.Data.Serialize;
using Turbine.Data.Entities;
using Turbine.DataEF6;
using Turbine.Producer.Data.Contract.Behaviors;

namespace Turbine.Lite.Web.Resources.Contracts
{
    class ProducerJobContract : Turbine.Producer.Data.Contract.Behaviors.IJobProducerContract
    {

        public ProducerJobContract(int myid)
        {
            // TODO: Complete member initialization
            Id = myid;
        }

        public int Id { get; set; }
        private Guid jobID = Guid.Empty;

        /// <summary>
        /// factory method
        /// </summary>
        /// <param name="simulationNameOrID"></param>
        /// <param name="sessionID"></param>
        /// <param name="initialize"></param>
        /// <param name="reset"></param>
        /// <returns></returns>
        public static IJobProducerContract Create(string simNameOrID, Guid sessionID, bool initialize, bool reset, bool visible)
        {
            Guid jobID = Guid.NewGuid();
            return Create(simNameOrID, sessionID, initialize, reset, visible, jobID, Guid.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simNameOrID"></param>
        /// <param name="sessionID"></param>
        /// <param name="initialize"></param>
        /// <param name="reset"></param>
        /// <param name="jobID"></param>
        /// <returns></returns>
        public static IJobProducerContract Create(string simNameOrID, Guid sessionID, bool initialize, bool reset, bool visible, Guid jobID, Guid consumerID)
        {
            int id = -1;
            bool isConsumerSet = true;

            Guid simulationId = Guid.Empty;
            bool isGuid = Guid.TryParse(simNameOrID, out simulationId);

            Turbine.Producer.Contracts.IProducerContext ctx = Turbine.Producer.Container.GetAppContext();
            String userName = ctx.UserName;
            if (jobID.Equals(Guid.Empty))
            {
                jobID = Guid.NewGuid();
            }

            if (consumerID.Equals(Guid.Empty))
            {
                isConsumerSet = false;
            }
            using (var db = new ProducerContext())
            {
                Simulation obj = null;
                
                if (isGuid == true)
                {
                    obj = db.Simulations.SingleOrDefault<Simulation>(s => s.Id == simulationId);
                }
                else
                {
                    obj = db.Simulations.OrderByDescending(q => q.Count).FirstOrDefault<Simulation>(s => s.Name == simNameOrID);
                }

                User user = db.Users.Single<User>(u => u.Name == userName);
                Session session = db.Sessions.SingleOrDefault<Session>(i => i.Id == sessionID);
                if (session == null)
                {
                    session = new Session() { 
                        Id = sessionID,
                        Create = DateTime.UtcNow, 
                        //User = user 
                    };
                    db.Sessions.Add(session);
                    db.SaveChanges();
                }
                Debug.WriteLine(String.Format("simulation {0}, User {1}, Session {2}",
                    simNameOrID, "user.Name", session.Id), "ProducerJobContract.Create");

                Job job = null;

                if (isConsumerSet == true)
                {
                    job = new Job()
                    {
                        Id = jobID,
                        Simulation = obj,
                        State = "create",
                        Create = DateTime.UtcNow,
                        Process = new Turbine.Data.Entities.Process() { Id = Guid.NewGuid() },
                        User = user,
                        Session = session,
                        ConsumerId = consumerID,
                        Initialize = initialize,
                        Reset = reset,
                        Visible = visible
                    };
                }
                else
                {
                    job = new Job()
                    {
                        Id = jobID,
                        Simulation = obj,
                        State = "create",
                        Create = DateTime.UtcNow,
                        Process = new Turbine.Data.Entities.Process() { Id = Guid.NewGuid() },
                        User = user,
                        Session = session,
                        Initialize = initialize,
                        Reset = reset,
                        Visible = visible
                    };
                }
                db.Jobs.Add(job);
                db.SaveChanges();
                job = db.Jobs.Single<Job>(j => j.Id == jobID);
                Debug.WriteLine(String.Format("Job {0} with Id {1} is created", 
                    job.Count.ToString(), job.Id.ToString()), "ProducerJobContract.Create()");
                Debug.WriteLine(String.Format("Job {0} state is {1}"
                    , job.Count.ToString(), job.State), "ProducerJobContract.Create()");
                id = job.Count;
            }
            var contract = new ProducerJobContract(id) { jobID = jobID };
            return contract;
        }

        public void Submit()
        {
            throw new NotImplementedException();
        }

        public void Terminate()
        {
            using (var db = new ProducerContext())
            {
                var entity = db.Jobs.Single<Turbine.Data.Entities.Job>(j => j.Count == Id);
                if (new List<string>() { "create", "submit", "locked", "setup", "running" }.Contains<string>(entity.State))
                {
                    entity.State = "terminate";
                    entity.Finished = DateTime.UtcNow;
                    db.SaveChanges();
                }
                else
                {
                    throw new InvalidStateChangeException(String.Format("Cannot Terminate a job in state {0}", entity.State));
                }
            }
        }

        public void Kill()
        {
            using (var db = new ProducerContext())
            {
                var entity = db.Jobs.Single<Turbine.Data.Entities.Job>(j => j.Count == Id);
                if (new List<string>() { "create", "submit", "locked", "setup", "running", "terminate" }.Contains<string>(entity.State))
                {
                    entity.State = "down";
                    entity.Finished = DateTime.UtcNow;
                    db.SaveChanges();
                }
                else
                {
                    throw new InvalidStateChangeException(String.Format("Cannot kill a job in state {0}", entity.State));
                }
            }
        }

        private ProducerProcessContract process = null;

        public Data.Contract.Behaviors.IProcess Process
        {
            get
            {
                if (process == null)
                {
                    using (var db = new ProducerContext())
                    {
                        var entity = db.Jobs.Single<Turbine.Data.Entities.Job>(j => j.Id == jobID);
                        process = new ProducerProcessContract(entity.Process.Id);
                    }
                }
                return process;
            }
        }


        public void Cancel()
        {
            throw new NotImplementedException();
        }
    }
}
