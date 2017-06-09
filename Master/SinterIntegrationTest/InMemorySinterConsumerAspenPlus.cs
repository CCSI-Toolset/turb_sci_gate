using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Consumer.Data.Contract.Behaviors;

namespace SinterIntegrationTest
{
    class InMemorySinterConsumerAspenPlus : Turbine.Consumer.AspenTech.AspenSinterConsumer
    {
        public IJobConsumerContract job = null;
        override protected IJobConsumerContract GetNextJob()
        {
            job = new InMemoryJobAspenPlus();
            return job;
        }
    }
}
