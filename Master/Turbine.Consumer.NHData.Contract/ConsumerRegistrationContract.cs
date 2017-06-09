using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Turbine.Common;
using Turbine.Consumer.Data.Contract.Behaviors;
using Turbine.Data.POCO.Domain;
using Turbine.NHData.Contract;


namespace Turbine.Consumer.NHData.Contract
{
    public class ConsumerRegistrationContract : IConsumerRegistrationContract
    {
        public void Register()
        {
            IConsumerContext ctx = AppUtility.GetConsumerContext();
            Guid consumerId = ctx.Id;
            String hostname = ctx.Hostname;
            Debug.WriteLine(String.Format("Register as {0}, {1}", consumerId, hostname), this.GetType().Name);

            using (NHibernate.ISession session = SessionFactory.OpenSession())
            {
                using (NHibernate.ITransaction trans = session.BeginTransaction())
                {
                    var consumer = new JobConsumer() { Id=consumerId, hostname=hostname, status="up" };

                }
            }
        }

        /* UnRegister
         *     If state is "up" move to "down", else leave alone.
         */
        public void UnRegister()
        {
            /*
            IConsumerContext ctx = AppUtility.GetConsumerContext();
            Guid consumerId = ctx.Id;
            Debug.WriteLine(String.Format("UnRegister Consumer {0}", consumerId), this.GetType().Name);

            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                JobConsumer consumer = (JobConsumer)container.Consumers.First<Turbine.Data.Consumer>(s => s.guid == consumerId);
                if (consumer.status == "up")
                    consumer.status = "down";
                container.SaveChanges();
            }
             */
        }

        public void Error()
        {
            IConsumerContext ctx = AppUtility.GetConsumerContext();
            Guid consumerId = ctx.Id;
            Debug.WriteLine(String.Format("Error as {0}, {1}", consumerId), this.GetType().Name);
            /*
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                JobConsumer consumer = (JobConsumer)container.Consumers.First<Turbine.Data.Consumer>(s => s.guid == consumerId);
                consumer.status = "error";
                container.SaveChanges();
            }
             * */
        }

        public bool Shutdown()
        {
            return false;
        }
    }
}
