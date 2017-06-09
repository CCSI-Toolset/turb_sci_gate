using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Consumer.Contract.Behaviors;

namespace Turbine.Consumer.Data.Contract.Behaviors
{
    /* IConsumerRegistrationContract: Consumers must register themselves before processing.
     * 
     */
    public interface IConsumerRegistrationContract
    {

        IJobQueue Queue { get; }

        /// <summary>
        ///  Register:  Consumer must register itself before popping a job off the queue.
        /// </summary>
        /// <returns></returns>
        IJobQueue Register(IConsumerRun run);

        /// <summary>
        /// UnRegister:  Consumer must unregister before going down.  This can occur anytime after
        ///    it has pulled its last job off the queue.
        /// </summary>
        void UnRegister();

        /// <summary>
        /// Error:  Consumer encountered a fatal error and will shutdown.
        /// </summary>
        void Error();

        /// <summary>
        /// KeepAlive:  Sends Keepalive notification
        /// </summary>
        void KeepAlive();

        /// <summary>
        /// GetStatus:  Get Consumer Status
        /// </summary>
        string GetStatus();
    }
}
