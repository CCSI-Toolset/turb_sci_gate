using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Consumer.Data.Contract.Behaviors;

namespace SinterIntegrationTest
{
    class JobQueue : IJobQueue
    {
        public IJobConsumerContract GetNext(Turbine.Consumer.Contract.Behaviors.IConsumerRun run)
        {
            throw new NotImplementedException();
        }

        public void SetSupportedApplications(Turbine.Consumer.Contract.Behaviors.IConsumerRun run)
        {
            throw new NotImplementedException();
        }
    }
}
