using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Turbine.Consumer;
using Turbine.Consumer.Data.Contract.Behaviors;
using Turbine.Data;
using Turbine.Consumer.Contract.Behaviors;


namespace Turbine.Consumer.Hybrid.Data.Contract
{
    /// <summary>
    /// Hybrid version utilizes IAWSContext to discover image and AMI
    /// </summary>
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

            Turbine.Consumer.AWS.IAWSContext awsCtx = null;
            string ami = "";
            string instance = "";
            try
            {
                awsCtx = Turbine.Consumer.AWS.AppUtility.GetContext();
                ami = awsCtx.AmiId;
                instance = awsCtx.InstanceId;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(String.Format("Ignore AWS Context is not available: {0}", ex.Message), this.GetType().Name);
                Debug.WriteLine(String.Format("{0}", ex.StackTrace), this.GetType().Name);
            }

            //
            // NOTE: Amazon InstanceID and AMI removed
            //
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                container.JobConsumers.AddObject(new JobConsumer() { 
                    Id = consumerId, 
                    hostname = hostname, 
                    AMI = ami,
                    instance = instance,
                    processId = Process.GetCurrentProcess().Id.ToString(),
                    status = "up" }
                    );
                container.SaveChanges();
            }
            queue = new Turbine.Consumer.Data.Contract.DBJobQueue();
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
                var consumer = (JobConsumer)container.JobConsumers.First<Turbine.Data.JobConsumer>(s => s.Id == consumerId);
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
                var consumer = (JobConsumer)container.JobConsumers.First<Turbine.Data.JobConsumer>(s => s.Id == consumerId);
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
