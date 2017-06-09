using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Producer.Data.Contract.Behaviors;
using Turbine.Data.Contract.Behaviors;
using Turbine.Data.Contract;
using Turbine.Data;
using System.Diagnostics;

namespace Turbine.Producer.Data.Contract
{
    /*
     *  Note this implementation requires one to set "Id" before using in DIJ.  Rather
     *  accomplish this via the DIJ framework and specify the Id in the constructor.
     */
    public class AspenJobProducerContract : IJobProducerContract
    {
        private int id;
        private Guid processId = Guid.Empty;

        public AspenJobProducerContract()
        {
            id = -1;
        }
        public AspenJobProducerContract(int id)
        {
            this.id = id;
        }

        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        /* Submit
         * 
         * IJobProducerContract event requires user name check
         * Better solution to use DIJ for Producer.CheckUser
         * and State Checks, throw Application Specific
         * Exceptions.  Move to POCO Data Model first.
         */
        public virtual void Submit()
        {
            string user = Container.GetAppContext().UserName;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Job obj = container.Jobs.Single<Job>(s => s.Id == id);  //Pulls the job that matches id 
                if (obj.User.Name != user)
                    throw new ArgumentException(
                        String.Format("Authorization Denied For {0}, Owner {1}", user, obj.User.Name)
                        );
                if (obj.State != "create")
                    throw new InvalidStateChangeException("Violation of state machine");

                Debug.WriteLine("SimulationId: " + obj.SimulationId);
                // Simple check 
                ValidateConfig(obj.Simulation);
                // TODO: DIJ Application Validators
                // container.GetApplication(obj.Simulation.Application).Validate(obj.Simulation)

                obj.State = "submit";
                obj.Submit = DateTime.UtcNow;

                container.SaveChanges();
            }
        }

        /* Cancel: 
         * 
         * IJobProducerContract
         */
        public void Cancel()
        {
            string user = Container.GetAppContext().UserName;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Job obj = container.Jobs.Single<Job>(s => s.Id == id);

                if (obj.User.Name != user)
                    throw new AuthorizationError(
                        String.Format("Cancel Authorization Denied For {0}", user)
                        );

                if (obj.State == "create" || obj.State == "submit" || obj.State == "setup" || obj.State == "pause")
                {
                    obj.State = "cancel";
                    obj.Finished = DateTime.UtcNow;
                }
                else
                {
                    throw new InvalidStateChangeException(
                        String.Format("Cannot cancel a job {0} in state {1}", id, obj.State)
                        );
                }
                container.SaveChanges();
            }
        }

        /* Terminate: Administrator function, for killing jobs.
         * IJobProducerContract
         */
        public void Terminate()
        {
            string user = Container.GetAppContext().UserName;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Job obj = container.Jobs.Single<Job>(s => s.Id == id);

                if (obj.User.Name != user)
                    throw new AuthorizationError(
                        String.Format("Termination Authorization Denied For {0}", user)
                        );

                if (obj.State == "running" || obj.State == "setup")
                {
                    obj.State = "terminate";
                    obj.Finished = DateTime.UtcNow;
                }
                else
                {
                    throw new InvalidStateChangeException(
                        String.Format("Cannot terminate a job {0} in state {1}", id, obj.State)
                        );
                }
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

        protected void ValidateConfig(Turbine.Data.Simulation sim)
        {
            var config = sim.SimulationStagedInputs.First<SimulationStagedInput>(s => s.Name == "configuration");
            if (config == null)
            {
                throw new ArgumentException("Simulation file 'configuration' is null");
            }
            if (config.Content == null)
            {
                throw new ArgumentException("Simulation file 'configuration' Content is null");
            }
            if (config.Content.Length == 0)
            {
                throw new ArgumentException("Simulation file 'configuration' Content length is zero");
            }
        }
    }
}
