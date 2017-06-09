using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Data.Test
{
    class AppTestConsumerContext : Turbine.Consumer.Contract.Behaviors.IConsumerContext
    {
        private static Guid guid = Guid.NewGuid();
        public Guid Id
        {
            get
            {
                return AppTestConsumerContext.guid;
            }
        }
        public string Hostname
        {
            get
            {
                return "DataIntegrationTests";
            }
        }
    }
}
