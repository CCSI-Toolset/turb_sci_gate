using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Consumer.Contract.Behaviors
{
    public interface IConsumerMonitor
    {
        // 
        bool IsEngineRunning { get; }
        bool IsSimulationInitializing { get; }
        bool IsSimulationOpened { get; }

        int Monitor(bool cancelKeyPressed);

        void CleanUp();

        void setConsumerRun(IConsumerRun iconsumerRun);

        /// <summary>
        /// MonitorTerminate:  Terminate job processId and children.  Note called from monitor thread, lock on consumer
        /// </summary>
        void MonitorTerminate();

        /// <summary>
        /// CheckTerminate:  If job has been marked for terminate, call MonitorTerminate.  Note called from monitor thread, lock on consumer
        /// </summary>
        /// <returns></returns>
        int CheckTerminate();

        Guid ConsumerId { get; }
    }
}
