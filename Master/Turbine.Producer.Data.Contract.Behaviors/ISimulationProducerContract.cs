using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Producer.Data.Contract.Behaviors
{
    public interface ISimulationProducerContract
    {
        /*
        bool UpdateConfiguration(string filePath);
        bool UpdateConfiguration(byte[] data);
        bool UpdateAspenBackup(string filePath);
        bool UpdateAspenBackup(byte[] data);
         */
        IJobProducerContract NewJob(Guid sessionID, bool initialize, bool reset, bool visible);
        IJobProducerContract NewJob(Guid sessionID, bool initialize, bool reset, bool visible, Guid jobID, Guid consumerID);
        bool DeleteAll();
        bool Delete();
        bool UpdateInput(string inputFileName, byte[] data, string content_type);
        bool Validate();
        bool ValidateAll();
    }
}
