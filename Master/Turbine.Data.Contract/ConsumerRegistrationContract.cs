using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Data.Contract.Behaviors;
using Turbine.Common;
using System.Diagnostics;

namespace Turbine.Data.Contract
{
    public class ConsumerRegistrationContract : IConsumerRegistrationContract
    {
        public void Register()
        {
            IConsumerContext ctx = AppUtility.GetConsumerContext();
            Guid consumerId = ctx.Id;
            String hostname = ctx.Hostname;
            Debug.WriteLine(String.Format("Register as {0}, {1}", consumerId, hostname), this.GetType().Name);

            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                container.Consumers.AddObject(new JobConsumer() { 
                    guid = consumerId, 
                    hostname = hostname, 
                    status = "up" }
                    );
                container.SaveChanges();
            }
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
                JobConsumer consumer = (JobConsumer)container.Consumers.First<Consumer>(s => s.guid == consumerId);
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
                JobConsumer consumer = (JobConsumer)container.Consumers.First<Consumer>(s => s.guid == consumerId);
                consumer.status = "error";
                container.SaveChanges();
            }
        }

        public bool Shutdown()
        {
            return false;
        }
    }
}
