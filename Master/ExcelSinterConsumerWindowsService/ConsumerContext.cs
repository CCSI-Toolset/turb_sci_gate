using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Turbine.Consumer.Contract.Behaviors;


namespace ExcelSinterConsumerWindowsService
{
    class ConsumerContext : IConsumerContext
    {
        private static Guid guid = Guid.NewGuid();
        public Guid Id
        {
            get
            {
                return ConsumerContext.guid;
            }
        }

        private static string hostname = System.Net.Dns.GetHostName();
        public string Hostname
        {
            get
            {
                return hostname;
            }
        }


        public string BindSimulationName { get; set; } 
    }
}
