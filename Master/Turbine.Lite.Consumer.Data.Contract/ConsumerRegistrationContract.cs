using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Turbine.Consumer;
using Turbine.Consumer.Data.Contract.Behaviors;
using Turbine.Consumer.Contract.Behaviors;
using Turbine.DataEF6;
using Turbine.Data.Entities;


namespace Turbine.Lite.Consumer.Data.Contract
{
    class JobQueueException : Exception
    {
        public JobQueueException(string message) : base(message)
        {
        }
    }

    class DBJobQueue : IJobQueue
    {
        //internal Guid consumerId = Guid.Empty;
        private string[] apps = new string[] { };
        public DBJobQueue()
        {
        }
        public IJobConsumerContract GetNext(IConsumerRun run)
        {
            apps = run.SupportedApplications;
            IJobConsumerContract contract = null;
            if (apps.Length == 0)
                throw new JobQueueException("Invalid JobQueue configuration:  no applications specified");

            IConsumerContext appContext = AppUtility.GetConsumerContext();
            string simulationName= appContext.BindSimulationName;

            using (ProducerContext db = new ProducerContext())
            {
                Job job = null;
                foreach(var appName in apps) 
                {
                    Debug.WriteLine("Get Job for Application: " + appName, "DBJobQueue.GetNext");

                    if (String.IsNullOrEmpty(simulationName))
                    {
                        job = db.Jobs.OrderByDescending(j => j.Submit).
                            Where(j => j.State == "submit" && j.ConsumerId == run.ConsumerId
                        && j.Simulation.ApplicationName == appName).FirstOrDefault();

                        if (job == null)
                        {
                            job = db.Jobs.OrderByDescending(j => j.Submit).
                                Where(j => j.State == "submit" && j.ConsumerId == null
                            && j.Simulation.ApplicationName == appName).FirstOrDefault();
                        }
                    }
                    else
                    {
                        job = db.Jobs.OrderByDescending(j => j.Submit).
                            Where(j => j.State == "submit" && j.ConsumerId == run.ConsumerId
                        && j.Simulation.Name == simulationName && j.Simulation.ApplicationName == appName).FirstOrDefault();

                        if (job == null)
                        {
                            job = db.Jobs.OrderByDescending(j => j.Submit).
                                Where(j => j.State == "submit" && j.ConsumerId == null
                            && j.Simulation.Name == simulationName && j.Simulation.ApplicationName == appName).FirstOrDefault();
                        }
                    }

                    var consumer = db.Consumers.Single(c => c.Id == run.ConsumerId);
                    if (job != null)
                    {
                        job.State = "locked";
                        job.Consumer = consumer;
                        db.SaveChanges();
                        contract = new ConsumerJobContract(job.Count, run.ConsumerId);
                        Debug.WriteLine(String.Format("Found Job({0}): {1},{2}", job.Id, job.Simulation.Name, job.Simulation.ApplicationName), "DBJobQueue.GetNext");
                        break;
                    }
                }
            }
            return contract;
        }

        public void SetSupportedApplications(IConsumerRun run)
        {
            apps = run.SupportedApplications;
        }
    }

    public class ConsumerRegistrationContract : IConsumerRegistrationContract
    {
        //private Guid consumerId = Guid.NewGuid();
        private IConsumerRun run = null;
        private IJobQueue queue = null;
        public IJobQueue Queue
        {
            get { return queue; }
        }

        public IJobQueue Register(IConsumerRun run)
        {
            this.run = run;
            IConsumerContext ctx = Turbine.Consumer.AppUtility.GetConsumerContext();
            //Guid consumerId = ctx.Id;
            //Guid consumerId = Guid.NewGuid();
            String hostname = ctx.Hostname;
            String appName = run.SupportedApplications.ElementAtOrDefault(0);
            Debug.WriteLine(String.Format("Register({0}) as {1}, {2}", appName, run.ConsumerId, hostname), this.GetType().Name);
            using (ProducerContext db = new ProducerContext())
            {
                // TODO: Registering as a single application is dubious.  
                // IF support multiple apps in single consumer need to expose that via the database ( update SCHEMA!! )                
                var app = db.Applications.Single(a => a.Name == appName);
                var consumer = new Turbine.Data.Entities.JobConsumer
                {
                    Application = app,
                    Id = run.ConsumerId,
                    hostname = hostname,
                    processId = System.Diagnostics.Process.GetCurrentProcess().Id,
                    keepalive = DateTime.UtcNow,
                    status = "up"
                };
                db.Consumers.Add(consumer);
                db.SaveChanges();
            }
            queue = AppUtility.GetJobQueue(run);
            //((DBJobQueue)queue).consumerId = consumerId;
            return queue;
        }

        /// <summary>
        /// Return Consumer Status
        /// </summary>
        public string GetStatus()
        {
            //Debug.WriteLine(String.Format("Getting Status of Consumer {0}", run.ConsumerId), this.GetType().Name);

            String status = null;
            using (ProducerContext db = new ProducerContext())
            {
                var consumer = db.Consumers.Single<JobConsumer>(s => s.Id == run.ConsumerId);
                if (consumer != null)
                {
                    status = consumer.status;
                }
            }

            return status;
        }

        /// <summary>
        /// If state is "up" move to "down", else leave alone.
        /// </summary>
        public void UnRegister()
        {
            //IConsumerContext ctx = AppUtility.GetConsumerContext();
            //Guid consumerId = ctx.Id;
            Debug.WriteLine(String.Format("UnRegister Consumer {0}", run.ConsumerId), this.GetType().Name);
            using (ProducerContext db = new ProducerContext())
            {
                var consumer = db.Consumers.Single<JobConsumer>(s => s.Id == run.ConsumerId);
                if (consumer.status.Equals("up"))
                {
                    consumer.status = "down";
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (System.Data.Entity.Validation.DbEntityValidationException e)
                    {
                        foreach (var eve in e.EntityValidationErrors)
                        {
                            Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                eve.Entry.Entity.GetType().Name, eve.Entry.State);
                            foreach (var ve in eve.ValidationErrors)
                            {
                                Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                    ve.PropertyName, ve.ErrorMessage);
                            }
                        }
                        throw;
                    }
                }
                foreach (var job in consumer.Jobs.Where(j => j.State == "locked" || j.State == "setup" || j.State == "running"))
                {
                    Debug.WriteLine(String.Format("Moving job {0},{1} to state submit", job.Count, job.State));
                    job.Messages.Add(new Message { Create = DateTime.UtcNow, Id = Guid.NewGuid(), 
                        Value = String.Format("Resubmit (Id={0},State={1}):  UnRegister Consumer {2}", job.Id, job.State, consumer.Id) });
                    job.State = "submit";
                    //job.Consumer = null;
                    db.SaveChanges();
                }
            }
        }

        public void Error()
        {
            //IConsumerContext ctx = AppUtility.GetConsumerContext();
            //Guid consumerId = ctx.Id;
            Debug.WriteLine(String.Format("Error as {0}, {1}", run.ConsumerId), this.GetType().Name);
            using (ProducerContext db = new ProducerContext())
            {
                var consumer = db.Consumers.Single<JobConsumer>(s => s.Id == run.ConsumerId);
                if (!consumer.status.Equals("error"))
                {
                    consumer.status = "error";
                    db.SaveChanges();
                }
            }
        }


        public void KeepAlive()
        {
            //IConsumerContext ctx = AppUtility.GetConsumerContext();
            //Guid consumerId = ctx.Id;
            Debug.WriteLine(String.Format("KeepAlive Consumer {0}", run.ConsumerId), this.GetType().Name);
            using (ProducerContext db = new ProducerContext())
            {
                var consumer = db.Consumers.Single<JobConsumer>(s => s.Id == run.ConsumerId);
                if (consumer.status.Equals("up"))
                {
                    consumer.keepalive = DateTime.UtcNow;
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (System.Data.Entity.Validation.DbEntityValidationException e)
                    {
                        foreach (var eve in e.EntityValidationErrors)
                        {
                            Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                eve.Entry.Entity.GetType().Name, eve.Entry.State);
                            foreach (var ve in eve.ValidationErrors)
                            {
                                Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                    ve.PropertyName, ve.ErrorMessage);
                            }
                        }
                        throw;
                    }
                }
            }      
        }
    }
}
