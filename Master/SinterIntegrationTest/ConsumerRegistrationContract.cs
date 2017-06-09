using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Consumer.Data.Contract.Behaviors;

namespace SinterIntegrationTest
{
    class ConsumerRegistrationContract : IConsumerRegistrationContract
    {
        public IJobQueue Queue
        {
            get { throw new NotImplementedException(); }
        }

        public IJobQueue Register(Turbine.Consumer.Contract.Behaviors.IConsumerRun run)
        {
            throw new NotImplementedException();
        }

        public void UnRegister()
        {
            throw new NotImplementedException();
        }

        public void Error()
        {
            throw new NotImplementedException();
        }

        public void KeepAlive()
        {
            throw new NotImplementedException();
        }

        public string GetStatus()
        {
            throw new NotImplementedException();
        }
    }
}
