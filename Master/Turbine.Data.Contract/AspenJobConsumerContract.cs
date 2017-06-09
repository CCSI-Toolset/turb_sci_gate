using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Data.Contract.Behaviors;
using Turbine.Common;
using Newtonsoft.Json;
using System.IO;

namespace Turbine.Data.Contract
{
    class AspenJobConsumerContract : IJobConsumerContract
    {
        private int id;

        public AspenJobConsumerContract(int id)
        {
            this.id = id;
        }

        public int Identity
        {
            get { return id; }
        }

        public byte[] GetSimulationConfiguration()
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Job obj = container.Jobs.Single<Job>(s => s.Id == id);
                return obj.Simulation.Configuration;
            }
        }

        public byte[] GetSimulationBackup()
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Job obj = container.Jobs.Single<Job>(s => s.Id == id);
                return obj.Simulation.Backup;
            }
        }

        public Dictionary<String, Object> GetSimulationDefaults()
        {
            var dict = new Dictionary<string,object>();
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Job obj = container.Jobs.Single<Job>(s => s.Id == id);
                if (obj.Simulation.Defaults != null)
                {
                    String data = System.Text.Encoding.ASCII.GetString(obj.Simulation.Defaults);
                    dict = JsonConvert.DeserializeObject<Dictionary<string, Object>>(data);
                }
            }
            return dict;
        }

        /*  Message: Consumer changes job message without changing state.
         * 
         * IJobConsumerContract
         *
         */
        public void Message(string msg)
        {
            Message w = new Message();
            w.Value = msg;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Job obj = container.Jobs.Single<Job>(s => s.Id == id);
                AuthorizeConsumer(obj);
                obj.Messages.Add(w);
                container.SaveChanges();
            }
        }

        /*  Setup: Moves job from submit to setup, concurrency issues to be aware of.
         *     Consumer must be registered before calling this method.
         * 
         *      throws OptimisticConcurrencyException if job has already been 
         *      grabbed by another consumer.
         *
         */
        public IProcess Setup()
        {
            string baseDir = AppUtility.GetAppContext().BaseWorkingDirectory;
            IConsumerContext ctx = AppUtility.GetConsumerContext();
            Guid consumerId = ctx.Id;

            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Consumer consumer = container.Consumers
                    .SingleOrDefault<Consumer>(c => c.guid == consumerId);

                if (consumer == null)
                    throw new IllegalAccessException("Setup failed, Consumer is not registered");

                Job obj = container.Jobs.Single<Job>(s => s.Id == id);
                if (obj.State != "submit")
                    throw new InvalidStateChangeException("Violation of state machine");
          
                if (obj.Process == null)
                    throw new InvalidStateChangeException("Process configuration missing");
                
                obj.State = "setup";
                obj.Setup = DateTime.UtcNow;
                obj.Process.WorkingDir = Path.Combine(baseDir, id.ToString());
                obj.JobConsumer = (JobConsumer)consumer;

                container.SaveChanges();
            }
            return SinterProcessContract.Get(id);
        }

        /*  Initialize: Some jobs specify an initialize step,
         *     if so return true.
         *
         */
        public bool Initialize()
        {
            bool val = false;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Job obj = container.Jobs.Single<Job>(s => s.Id == id);
                val = obj.Initialize;
            }

            return val;
        }
        /*  Running: Moves job from running
         *    Only can be performed by declared consumer
         *
         */
        public void Running()
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Job obj = container.Jobs.Single<Job>(s => s.Id == id);
                AuthorizeConsumer(obj);

                if (obj.State != "setup")
                    throw new InvalidStateChangeException("Violation of state machine");
                if (obj.Process == null)
                    throw new InvalidStateChangeException("Process configuration missing");
                if (obj.Process.WorkingDir == null)
                    throw new InvalidStateChangeException("Process configuration missing working directory");
                if (obj.Process.Input == null)
                    throw new InvalidStateChangeException("Process configuration missing Input BLOB");
                if (obj.Process.Configuration == null)
                    obj.Process.Configuration = obj.Simulation.Configuration;
                if (obj.Process.Backup == null)
                    obj.Process.Backup = obj.Simulation.Backup;

                obj.State = "running";
                obj.Running = DateTime.UtcNow;
                container.SaveChanges();
            }
        }
        public void Success()
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Job obj = container.Jobs.Single<Job>(s => s.Id == id);
                AuthorizeConsumer(obj);

                if (obj.State != "running")
                    throw new InvalidStateChangeException("Violation of state machine");
                obj.State = "success";
                obj.Finished = DateTime.UtcNow;
                container.SaveChanges();
            }
        }
        public void Error(string msg)
        {
            var w = new Message();
            w.Value = msg;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Job obj = container.Jobs.Single<Job>(s => s.Id == id);
                AuthorizeConsumer(obj);

                if (obj.State == "error")
                    throw new InvalidStateChangeException("Violation of state machine");
                obj.State = "error";
                obj.Messages.Add(w);
                obj.Finished = DateTime.UtcNow;
                container.SaveChanges();
            }
        }

        public void Warning(string msg)
        {
            var w = new Message();
            w.Value = msg;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Job obj = container.Jobs.Single<Job>(s => s.Id == id);
                AuthorizeConsumer(obj);

                if (obj.State != "running")
                    throw new InvalidStateChangeException("Violation of state machine");
                obj.State = "warning";
                obj.Messages.Add(w);
                obj.Finished = DateTime.UtcNow;
                container.SaveChanges();
            }
        }

        public IProcess Process
        {
            get { return SinterProcessContract.Get(id); }
        }

        private void AuthorizeConsumer(Job obj)
        {
            IConsumerContext ctx = AppUtility.GetConsumerContext();
            Guid consumerGUID = ctx.Id;
            Guid jobGUID =  obj.JobConsumer.guid;

            if (jobGUID != Guid.Empty && jobGUID != consumerGUID)
            {
                throw new IllegalAccessException(String.Format(
                    "job({0}) consumer access denied {1} != {2}", obj.Id, jobGUID, consumerGUID)
                    );
            }
        }

    }
}
