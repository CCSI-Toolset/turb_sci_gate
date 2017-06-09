using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Turbine.Consumer.AWS.Data.Contract.Messages
{
    /// <summary>
    /// JobAddMessage : Adds a Message to the Job, One-Way ( not acknowledged ).
    /// </summary>
    public class JobAddMessage : Turbine.AWS.Messages.Message
    {
        public Guid JobId { get; set; }
        public string Message { get; set; }
    }
}