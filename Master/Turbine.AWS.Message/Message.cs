using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.AWS.Messages
{
    public class Message
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
