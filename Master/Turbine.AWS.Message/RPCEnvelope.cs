using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.AWS.Messages
{
    public class RPCEnvelope
    {
        public string Operation { get; set; } // Operation ( class name )
        public Message Message { get; set; }
        public string ResponseQueueUrl { get; set; }
    }
}
