using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
//using Newtonsoft.Json;
using Turbine.Data.Contract.Behaviors;
using Turbine.Consumer.Data.Contract.Behaviors;
using Turbine.Consumer.Data.Contract;
using Turbine.Data.POCO;
using Turbine.Data.Contract;
using Turbine.Data.POCO.Domain;
using NHibernate.Criterion;
using Turbine.NHData.Contract;


namespace Turbine.Consumer.NHData.Contract
{
    public class JobContract
    {
        private JobContract() {}

        // Consumer grabs job in submit state off queue
        public static IJobConsumerContract GetNext()
        {

            using (NHibernate.ISession session = SessionFactory.OpenSession())
            {
                using (NHibernate.ITransaction trans = session.BeginTransaction())
                {
                    var job = session
                        .CreateCriteria<Job>()
                        .Add(Restrictions.Eq("state", "submit"))
                        .AddOrder(Order.Asc("create"))
                        .UniqueResult<Job>();

                    if (job == null) return null;
                    return new JobConsumerContract(job.Id);
                }
            }
        }
    }
}
