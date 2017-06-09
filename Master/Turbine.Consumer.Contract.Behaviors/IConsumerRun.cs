using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Consumer.Contract.Behaviors
{
    public interface IConsumerRun
    {
        //
        bool IsEngineRunning { get; }
        bool IsSimulationInitializing { get; }
        bool IsSimulationOpened { get; }

        bool Run();

        bool Stop();

        void setIsTerminated(bool terminated);

        bool terminateSimAndJob();

        bool isJobTerminated();

        void CleanUp();

        string[] SupportedApplications { get; }

        Guid ConsumerId { get; }
    }
}