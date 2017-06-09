using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Data.Contract.Behaviors;
using Turbine.Common;

namespace Turbine.Data.Contract
{
    class AspenJobProducerContract : IJobProducerContract
    {
        private int id;

        public AspenJobProducerContract(int id)
        {
            this.id = id;
        }

        public int Identity
        {
            get { return id; }
        }

        /* Submit
         * 
         * IJobProducerContract event requires user name check
         * Better solution to use DIJ for Producer.CheckUser
         * and State Checks, throw Application Specific
         * Exceptions.  Move to POCO Data Model first.
         */
        public void Submit()
        {
            string user = AppUtility.GetAppContext().UserName;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Job obj = container.Jobs.Single<Job>(s => s.Id == id);  //Pulls the job that matches id 
                if (obj.User.Name != user)
                    throw new ArgumentException(
                        String.Format("Authorization Denied For {0}, Owner {1}", user, obj.User.Name)
                        );
                if (obj.State != "create")
                    throw new InvalidStateChangeException("Violation of state machine");

                // Backup and Config must be valid to submit/run
                ValidateBackup(obj.Simulation);
                ValidateConfig(obj.Simulation);

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
            string user = AppUtility.GetAppContext().UserName;
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
            string user = AppUtility.GetAppContext().UserName;
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
            get { return SinterProcessContract.Get(id); }
        }

        private void ValidateConfig(Simulation sim)
        {
            if (sim.Configuration == null | sim.Configuration.Length == 0)
            {
                throw new ArgumentException("Simulation contains no Configuration");
            }
        }

        private void ValidateBackup(Simulation sim)
        {
            if (sim.Backup == null | sim.Backup.Length == 0)
            {
                throw new ArgumentException("Simulation contains no Backup");
            }
        }
    }
}
