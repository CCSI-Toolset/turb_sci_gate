using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Turbine.Consumer;
using Turbine.Consumer.Contract.Behaviors;
using Turbine.Consumer.Data.Contract.Behaviors;
using Turbine.Data.Entities;
using Turbine.DataEF6;


namespace Turbine.Lite.Consumer.Data.Contract
{
    public class ConsumerJobContract : IJobConsumerContract
    {
        private int id;
        private bool reset;
        private bool visible;
        private Guid processId = Guid.Empty;
        private string simulationName;
        private string applicationName;
        private Guid simulationId;
        private int MAX_MESSAGE_LENGTH = 2048;
        private enum States { CREATE, SUBMIT, LOCKED, SUCCESS, ERROR, RUNNING, SETUP };
        private States state;

        public ConsumerJobContract(int id, Guid consumerId)
        {
            this.id = id;
            ConsumerId = consumerId;
            state = States.CREATE;
            using (ProducerContext db = new ProducerContext())
            {
                var job = db.Jobs.Single<Job>(i => i.Count == id);
                simulationName = job.Simulation.Name;
                applicationName = job.Simulation.ApplicationName;
                simulationId = job.SimulationId;
                reset = job.Reset;
                visible = job.Visible;
            }
        }

        public Guid ConsumerId { get; set; }

        public int Id
        {
            get { return id; }
            set { throw new NotImplementedException(); }
        }

        public string ApplicationName
        {
            get { return applicationName; }
        }

        public string SimulationName
        {
            get { return simulationName; }
        }

        public Guid SimulationId
        {
            get { return simulationId; }
        }

        public bool Reset
        {
            get { return reset; }
        }

        public bool Visible
        {
            get { return visible; }
        }

        public IEnumerable<SimpleFile> GetSimulationInputFiles()
        {
            var files = new List<SimpleFile>();

            using (ProducerContext db = new ProducerContext())
            {
                var simulation = db.Simulations.Single(i => i.Id == simulationId);
                foreach (var input in simulation.SimulationStagedInputs)
                {
                    Debug.WriteLine("FILE: " + input.Name, this.GetType().Name);
                    var f = new SimpleFile() { content = input.Content, name = input.Name };
                    yield return f;
                }
            }
        }

        public void Message(string msg)
        {
            Message w = new Message() { Id = Guid.NewGuid(), Value = msg, Create=DateTime.UtcNow };
            using (ProducerContext db = new ProducerContext())
            {
                Job obj = db.Jobs.Single<Job>(s => s.Count == id);
                obj.Messages.Add(w);
                db.SaveChanges();
            }
        }

        public Turbine.Data.Contract.Behaviors.IProcess Setup()
        {
            string baseDir = AppUtility.GetAppContext().BaseWorkingDirectory;
            Guid pid = Guid.Empty;

            using (ProducerContext db = new ProducerContext())
            {
                var job = db.Jobs.Single(j => j.Count == id);
                var consumer = job.Consumer;

                if (consumer == null)
                    throw new IllegalAccessException("Setup failed, Consumer is not registered");
                if (consumer.status != "up")
                    throw new IllegalAccessException(String.Format(
                        "Setup failed, Consumer status {0} != up", consumer.status));
                if (job.ConsumerId != ConsumerId)
                {
                    throw new IllegalAccessException(String.Format(
                        "job({0}) consumer access denied {1} != {2}", job.Id, job.ConsumerId, ConsumerId)
                        );
                }
                Debug.WriteLine(String.Format("Setup consumer GUID {0}", consumer.Id), this.GetType().Name);

                if (job.State != "locked")
                    throw new InvalidStateChangeException("Violation of state machine");

                if (job.Process == null)
                    throw new InvalidStateChangeException("Process configuration missing");

                processId = job.Process.Id;
                job.State = "setup";
                job.Setup = DateTime.UtcNow;
                job.Process.WorkingDir = System.IO.Path.Combine(baseDir, id.ToString());
                job.Consumer = (JobConsumer)consumer;
                job.Messages.Add(new Turbine.Data.Entities.Message { 
                    Id = Guid.NewGuid(), 
                    Create = DateTime.UtcNow, 
                    Value = String.Format("event=setup,consumer={0}", consumer.Id) 
                });  
                pid = job.Process.Id;
                simulationName = job.Simulation.Name;
                db.SaveChanges();
            }
            state = States.SETUP;
            return Turbine.DataEF6.Contract.ProcessContract.Get(pid);
        }

        public bool Initialize()
        {
            bool val = false;
            using (ProducerContext db = new ProducerContext())
            {
                Job obj = db.Jobs.Single<Job>(s => s.Count == id);
                val = obj.Initialize;
            }
            return val;
        }

        public void Running()
        {
            using (ProducerContext db = new ProducerContext())
            {
                Job obj = db.Jobs.Single<Job>(s => s.Count == id);

                if (obj.ConsumerId != ConsumerId)
                {
                    throw new IllegalAccessException(String.Format(
                        "job({0}) consumer access denied {1} != {2}", obj.Id, obj.ConsumerId, ConsumerId)
                        );
                }

                Debug.WriteLine("obj.State : " + obj.State, "ConsumerJobContract.Running");
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
                obj.Messages.Add(new Turbine.Data.Entities.Message {  
                    Id=Guid.NewGuid(), 
                    Create = DateTime.UtcNow, 
                    Value = String.Format("event=running,consumer={0}", obj.ConsumerId) 
                });  
                db.SaveChanges();
            }
            state = States.RUNNING;
        }

        public void Error(string msg)
        {
            using (ProducerContext db = new ProducerContext())
            {
                Job obj = db.Jobs.Single<Job>(s => s.Count == id);

                if (obj.ConsumerId != ConsumerId)
                {
                    throw new IllegalAccessException(String.Format(
                        "job({0}) consumer access denied {1} != {2}", obj.Id, obj.ConsumerId, ConsumerId)
                        );
                }

                //Debug.WriteLine("Error: " + msg, "ConsumerJobContract.Error");
                if (!obj.State.Equals("running") && !obj.State.Equals("setup") && !obj.State.Equals("error"))
                    throw new InvalidStateChangeException(String.Format("Violation of state machine: job id={0},state={1} cannot be moved to error", obj.Id, obj.State));

                if (!obj.State.Equals("error"))
                {
                    obj.State = "error";
                    state = States.ERROR;
                    obj.Finished = DateTime.UtcNow;
                }
                if (msg.Length > 3800)
                {
                    msg = msg.Substring(0, 3800);
                }
                obj.Messages.Add(new Turbine.Data.Entities.Message { 
                    Id=Guid.NewGuid(),
                    Create = DateTime.UtcNow, 
                    Value = String.Format("event=error,consumer={0},msg=\"{1}\"", obj.ConsumerId, msg) 
                });
                db.SaveChanges();
            }
        }

        public void Success()
        {
            using (ProducerContext db = new ProducerContext())
            {
                Job obj = db.Jobs.Single<Job>(s => s.Count == id);

                if (obj.ConsumerId != ConsumerId)
                {
                    throw new IllegalAccessException(String.Format(
                        "job({0}) consumer access denied {1} != {2}", obj.Id, obj.ConsumerId, ConsumerId)
                        );
                }

                if (obj.State != "running")
                    throw new InvalidStateChangeException("Violation of state machine");
                if (obj.Process == null)
                    throw new InvalidStateChangeException("Process configuration missing");
                if (obj.Process.WorkingDir == null)
                    throw new InvalidStateChangeException("Process configuration missing working directory");
                if (obj.Process.Input == null)
                    throw new InvalidStateChangeException("Process configuration missing Input BLOB");

                obj.State = "success";
                obj.Finished = DateTime.UtcNow;
                obj.Messages.Add(new Turbine.Data.Entities.Message {
                    Id = Guid.NewGuid(),
                    Create = DateTime.UtcNow, 
                    Value = String.Format("event=sucess,consumer={0}", obj.ConsumerId) 
                });  
                db.SaveChanges();
            }
            state = States.SUCCESS;
        }

        public void Warning(string p)
        {
            throw new NotImplementedException();
        }

        public Turbine.Data.Contract.Behaviors.IProcess Process
        {
            get { return Turbine.DataEF6.Contract.ProcessContract.Get(processId); }
        }

        public bool IsSuccess()
        {
            return (state == States.SUCCESS);
        }


        public bool IsTerminated()
        {
            using (ProducerContext db = new ProducerContext())
            {
                Job obj = db.Jobs.Single<Job>(s => s.Count == id);
                if (obj.ConsumerId != ConsumerId)
                {
                    throw new IllegalAccessException(String.Format(
                        "job({0}) consumer access denied {1} != {2}", obj.Id, obj.ConsumerId, ConsumerId)
                        );
                }
                return "terminate".Equals(obj.State);
            }
        }
    }
}
