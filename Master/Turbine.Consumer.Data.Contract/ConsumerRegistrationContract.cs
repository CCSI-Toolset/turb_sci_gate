using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Turbine.Consumer;
using Turbine.Consumer.Data.Contract.Behaviors;
using Turbine.Data;
using Turbine.Consumer.Contract.Behaviors;


namespace Turbine.Consumer.Data.Contract
{
    public class ConsumerRegistrationContract : IConsumerRegistrationContract
    {
        private IJobQueue queue;
        public IJobQueue Queue
        {
            get { return queue; }
        }

        public IJobQueue Register()
        {
            IConsumerContext ctx = Turbine.Consumer.AppUtility.GetConsumerContext();
            Guid consumerId = ctx.Id;
            String hostname = ctx.Hostname;
            Debug.WriteLine(String.Format("Register as {0}, {1}", consumerId, hostname), this.GetType().Name);

            //
            // NOTE: Amazon InstanceID and AMI removed
            //
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                container.JobConsumers.AddObject(new JobConsumer() { 
                    Id = consumerId, 
                    hostname = hostname, 
                    AMI = "",
                    instance = "",
                    processId = Process.GetCurrentProcess().Id.ToString(),
                    status = "up",
                    keepalive = DateTime.UtcNow }
                    );
                container.SaveChanges();
            }
            queue = new DBJobQueue();
            return queue;
        }

        /* UnRegister
         *     If state is "up" move to "down", else leave alone.
         */
        public void UnRegister()
        {
            IConsumerContext ctx = AppUtility.GetConsumerContext();
            Guid consumerId = ctx.Id;
            Debug.WriteLine(String.Format("UnRegister Consumer {0}", consumerId), this.GetType().Name);

            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                var consumer = (JobConsumer)container.JobConsumers.Single<Turbine.Data.JobConsumer>(s => s.Id == consumerId);
                if (consumer.status == "up")
                    consumer.status = "down";
                container.SaveChanges();
            }
        }

        public void Error()
        {
            IConsumerContext ctx = AppUtility.GetConsumerContext();
            Guid consumerId = ctx.Id;
            Debug.WriteLine(String.Format("Error as {0}, {1}", consumerId), this.GetType().Name);

            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                var consumer = (JobConsumer)container.JobConsumers.Single<Turbine.Data.JobConsumer>(s => s.Id == consumerId);
                consumer.status = "error";
                container.SaveChanges();
            }
        }


        public void KeepAlive()
        {
            IConsumerContext ctx = AppUtility.GetConsumerContext();
            Guid consumerId = ctx.Id;
            Debug.WriteLine(String.Format("KeepAlive Consumer {0}", consumerId), this.GetType().Name);

            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                var consumer = (JobConsumer)container.JobConsumers.Single<Turbine.Data.JobConsumer>(s => s.Id == consumerId);
                if (consumer.status == "up")
                    consumer.keepalive = DateTime.UtcNow;
                container.SaveChanges();
            }            
        }
    }
}
