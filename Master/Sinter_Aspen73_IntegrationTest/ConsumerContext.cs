using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Consumer.Contract.Behaviors;

namespace Sinter_Aspen73_IntegrationTest
{

    class ConsumerContext : IConsumerContext
    {
 
        public Guid Id
        {
            get { throw new NotImplementedException(); }
        }

        public string Hostname
        {
            get { return "localhost"; }
        }


        public string BindSimulationName { get; set; } 
    }
}
