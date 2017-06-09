using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using Turbine.Consumer.Data.Contract.Behaviors;
using Turbine.Data.Contract.Behaviors;
using Turbine.Data;
using Turbine.Data.Contract;
using System.Diagnostics;
using Turbine.Consumer.Contract.Behaviors;


namespace Turbine.Consumer.Data.Contract
{
    public class AspenJobConsumerContract : IJobConsumerContract
    {
        private int id;
        private Guid processId = Guid.Empty;
        private string simulationName = null;
        private string applicationName = null;
        private Guid simulationId = Guid.Empty;
        private int MAX_MESSAGE_LENGTH = 2048;
        private void _setup()
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Job obj = container.Jobs.Single<Job>(s => s.Id == id);
                simulationName = obj.Simulation.Name;
                applicationName = obj.Simulation.ApplicationName;
                simulationId = obj.Simulation.Id;
            }
        }
        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                this.id = value;
            }
        }

        public string ApplicationName
        {
            get
            {
                if (applicationName != null)
                    return applicationName;
                _setup();
                return applicationName;
            }
        }

        public string SimulationName
        {
            get 
            {
                if (simulationName != null) 
                    return simulationName;
                _setup();
                return simulationName;
            }
        }

        public Guid SimulationId
        {
            get
            {
                if (simulationId != Guid.Empty)
                    return simulationId;
                _setup();
                return simulationId;
            }
        }

        public bool Reset
        {
            get
            {
                using (TurbineModelContainer container = new TurbineModelContainer())
                {
                    Job obj = container.Jobs.Single<Job>(s => s.Id == id);
                    return obj.Reset;
                }
            }
        }

        public IEnumerable<SimpleFile> GetSimulationInputFiles()
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Job obj = container.Jobs.Single<Job>(s => s.Id == id);
                foreach (var input in obj.Simulation.SimulationStagedInputs)
                {
                    Debug.WriteLine("FILE: " + input.Name, this.GetType().Name);
                    var f = new SimpleFile() { content = input.Content, name = input.Name };
                    yield return f;
                }
            }
        }


        /*  Message: Consumer changes job message without changing state.
         * 
         * IJobConsumerContract
         *
         */
        public void Message(string msg)
        {
            Message w = new Message() { Id = Guid.NewGuid(), Value = msg };
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
            Guid pid = Guid.Empty;

            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                var consumer = container.JobConsumers
                    .SingleOrDefault<Turbine.Data.JobConsumer>(c => c.Id == consumerId);

                Debug.WriteLine(String.Format("Setup consumer GUID {0}", consumerId), this.GetType().Name);

                if (consumer == null)
                    throw new IllegalAccessException("Setup failed, Consumer is not registered");
                if (consumer.status != "up")
                    throw new IllegalAccessException(String.Format(
                        "Setup failed, Consumer status {0} != up", consumer.status));

                Job obj = container.Jobs.Single<Job>(s => s.Id == id);
                if (obj.State != "locked")
                    throw new InvalidStateChangeException("Violation of state machine");
          
                if (obj.Process == null)
                    throw new InvalidStateChangeException("Process configuration missing");
                
                obj.State = "setup";
                obj.Setup = DateTime.UtcNow;
                obj.Process.WorkingDir = Path.Combine(baseDir, id.ToString());
                obj.Consumer = (JobConsumer)consumer;
                pid = obj.Process.Id;
                simulationName = obj.Simulation.Name;
                container.SaveChanges();
            }
            return SinterProcessContract.Get(pid);
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
            if (msg.Length > MAX_MESSAGE_LENGTH)
                msg = msg.Substring(0, MAX_MESSAGE_LENGTH);

            var w = new Message() { Id = Guid.NewGuid(), Value = msg };
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
            if (msg.Length > MAX_MESSAGE_LENGTH)
                msg = msg.Substring(0, MAX_MESSAGE_LENGTH);

            var w = new Message() { Id = Guid.NewGuid(), Value = msg };
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
            get {
                if (processId == Guid.Empty)
                {
                    using (TurbineModelContainer container = new TurbineModelContainer())
                    {
                        Job obj = container.Jobs.Single<Job>(s => s.Id == id);
                        processId = obj.Process.Id;
                    } 
                }
                return SinterProcessContract.Get(processId);
            }
        }

        private void AuthorizeConsumer(Job obj)
        {
            IConsumerContext ctx = AppUtility.GetConsumerContext();
            Guid consumerGUID = ctx.Id;
            Guid jobGUID =  obj.Consumer.Id;

            if (jobGUID != Guid.Empty && jobGUID != consumerGUID)
            {
                throw new IllegalAccessException(String.Format(
                    "job({0}) consumer access denied {1} != {2}", obj.Id, jobGUID, consumerGUID)
                    );
            }
        }
    }
}
