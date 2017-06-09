using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Consumer.Data.Contract.Behaviors;

namespace SinterIntegrationTest
{
    class InMemorySinterConsumerGProms : Turbine.Consumer.GProms.GPromsSinterConsumer
    {
        public IJobConsumerContract job = null;
        override protected IJobConsumerContract GetNextJob()
        {
            if (job == null) 
                throw new ArgumentNullException("Set Job First");
            return job;
        }
    }
}
