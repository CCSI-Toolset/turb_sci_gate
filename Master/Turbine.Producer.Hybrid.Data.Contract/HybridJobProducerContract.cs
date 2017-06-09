using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Data;
using Turbine.Producer.Data.Contract.Behaviors;
using Turbine.Data.Contract.Behaviors;
using Turbine.Data.Contract;

namespace Turbine.Producer.Hybrid.Data.Contract
{
    /// <summary>
    /// HybridJobProducerContract : Hybrid architecture, orchestrator roles played by 
    ///   producer and consumer.  Saves job info to DB, places Job description on Queue
    ///   and moves Job to failure if SQS Put fails for any reason to generate a response
    ///   for the entry.
    /// </summary>
    public class HybridJobProducerContract : Turbine.Producer.Data.Contract.AspenJobProducerContract
    {
        public override void Submit()
        {
            // NOTE: If SQS Failure occurs ignore.  
            // Orchestrator will check submit queue
            
            var listener = new Turbine.Orchestrator.AWS.Data.Contract.SimpleMessageListener();   
            var job = new Turbine.Orchestrator.AWS.Data.Contract.Messages.SubmitJobMessage();
            string user = Container.GetAppContext().UserName;

            using (TurbineModelContainer container = new TurbineModelContainer())
            {

                Job obj = container.Jobs.Single<Job>(s => s.Id == this.Id);  //Pulls the job that matches id 
                if (obj.User.Name != user)
                    throw new ArgumentException(
                        String.Format("Authorization Denied For {0}, Owner {1}", user, obj.User.Name)
                        );
                if (obj.State != "create")
                    throw new InvalidStateChangeException("Violation of state machine");

                // Simple check 
                ValidateConfig(obj.Simulation);
                // TODO: DIJ Application Validators
                // container.GetApplication(obj.Simulation.Application).Validate(obj.Simulation)

                obj.State = "submit";
                obj.Submit = DateTime.UtcNow;

                container.SaveChanges();

                job.Id = obj.guid;
                job.Inc = obj.Id;
                job.Initialize = obj.Initialize;
                job.Reset = obj.Reset;
                job.SimulationId = obj.SimulationId;
                job.SimulationName = obj.Simulation.Name;
                var messages = new Turbine.Orchestrator.AWS.Data.Contract.Messages.SubmitJobMessage[] { job };
                var jobIdList = listener.PutOnQueue(messages);

                if (jobIdList.Count != 1)
                {
                    obj.State = "failure";
                    obj.Finished = DateTime.UtcNow;
                    string msg = String.Format("Failed to Add Job {0} to SQS Queue", obj.guid);
                    var w = new Message() { Id = Guid.NewGuid(), Value = msg };
                    obj.Messages.Add(w);
                    container.SaveChanges();
                }
            }
        }
    }
}
