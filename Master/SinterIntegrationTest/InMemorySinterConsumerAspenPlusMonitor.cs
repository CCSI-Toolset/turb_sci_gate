using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Consumer.Data.Contract.Behaviors;
using Turbine.Consumer.SimSinter;

namespace SinterIntegrationTest
{
    class InMemorySinterConsumerAspenPlusMonitor : Turbine.Consumer.SimSinter.SinterConsumerMonitor
    {
        public InMemorySinterConsumerAspenPlusMonitor()
        {
            consumerId = new Guid();
        }
    }
}
