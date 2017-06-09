using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
//using Newtonsoft.Json;
using Turbine.Data.Contract.Behaviors;
using Turbine.Consumer.Data.Contract.Behaviors;
using Turbine.Consumer.Data.Contract;
using Turbine.Data;
using Turbine.Consumer.Contract.Behaviors;
using Turbine.Data.Contract;
using System.Diagnostics;

namespace Turbine.Consumer.Data.Contract
{
    /// <summary>
    /// DBJobQueue 
    /// 
    /// If specify apps GetNext will look for a job from that particular application
    /// else it will just grab the next job.
    /// </summary>
    public class DBJobQueue : IJobQueue
    {
        string[] applications;
        public DBJobQueue()
        {
            applications = null;
        }
        public DBJobQueue(string[] apps)
        {
            applications = apps;
        }

        public void SetSupportedApplications(string[] apps)
        {
            applications = apps;
        }

        // Consumer grabs job in submit state off queue
        public IJobConsumerContract GetNext()
        {
            IJobConsumerContract contract = new AspenJobConsumerContract();;
            IConsumerContext ctx = AppUtility.GetConsumerContext();
            Guid consumerId = ctx.Id;

            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                var consumer = container.JobConsumers
                    .SingleOrDefault<Turbine.Data.JobConsumer>(c => c.Id == consumerId);

                if (consumer == null)
                    throw new IllegalAccessException("GetNext failed, Consumer is not registered");
                if (consumer.status != "up")
                    throw new IllegalAccessException(String.Format(
                        "GetNext failed, Consumer status {0} != up", consumer.status));

                //Job obj = container.Jobs.OrderBy("it.Submit").
                //    FirstOrDefault<Job>(s => s.State == "submit");
                //obj.Simulation.ApplicationName

                Job obj = null;
                // TODO: CHange the WHERE clause to do an OR between all available applications
                if (applications == null)
                {
                    System.Data.Objects.ObjectQuery<Turbine.Data.Job> query = container.Jobs;
                    obj = query.OrderBy("it.Submit").FirstOrDefault<Job>(s => s.State == "submit");
                }
                else
                {
                    foreach (string appName in applications)
                    {
                        System.Data.Objects.ObjectQuery<Turbine.Data.Job> query = container.Jobs;
                        query = query.Where("it.Simulation.ApplicationName = @appName",
                            new System.Data.Objects.ObjectParameter("appName", appName));
                        obj = query.OrderBy("it.Submit").FirstOrDefault<Job>(s => s.State == "submit");
                        if (obj != null) break;
                    }
                }

                if (obj == null) return null;

                // Need to Check that Consumer is up
                obj.State = "locked";
                try
                {
                    container.SaveChanges();
                }
                catch (System.Data.OptimisticConcurrencyException)
                {
                    Debug.WriteLine("OptimisticConcurrencyException:  Failed attempt to lock job", GetType().Name);
                    return null;
                }
                contract.Id = obj.Id;
            }
            return contract;
        }

        public int Count()
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                return container.Jobs.Count<Job>(s => s.State == "submit"); ;
            }
        }
    }
}
