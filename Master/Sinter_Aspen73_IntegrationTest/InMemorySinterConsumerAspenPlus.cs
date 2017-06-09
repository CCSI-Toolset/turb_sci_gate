using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Consumer.Data.Contract.Behaviors;

namespace Sinter_Aspen73_IntegrationTest
{
    class InMemorySinterConsumerAspenPlus : Turbine.Consumer.AspenTech73.AspenSinterConsumer
    {
        public IJobConsumerContract job = null;
        override protected IJobConsumerContract GetNextJob()
        {
            job = new InMemoryJobAspenPlus();
            return job;
        }
    }
}
