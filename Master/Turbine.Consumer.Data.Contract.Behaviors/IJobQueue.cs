using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Consumer.Data.Contract.Behaviors
{
    public interface IJobQueue
    {
        IJobConsumerContract GetNext(Consumer.Contract.Behaviors.IConsumerRun run);
        void SetSupportedApplications(Consumer.Contract.Behaviors.IConsumerRun run);
    }
}