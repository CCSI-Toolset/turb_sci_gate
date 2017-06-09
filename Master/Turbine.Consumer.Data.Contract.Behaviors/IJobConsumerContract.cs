using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Data.Contract.Behaviors;


namespace Turbine.Consumer.Data.Contract.Behaviors
{
    public class SimpleFile
    {
        public String name;
        public byte[] content;
    }

    public interface IJobConsumerContract : IJob
    {
        string ApplicationName { get; }
        string SimulationName { get; }
        Guid SimulationId { get; }
        bool Reset { get; }
        bool Visible { get; }
        bool IsSuccess();

        // utility methods
        //byte[] GetSimulationConfiguration();
        //byte[] GetSimulationBackup();
        IEnumerable<SimpleFile> GetSimulationInputFiles();
        void Message(string msg);

        // consumer state methods
        IProcess Setup();
        bool Initialize();
        void Running();
        void Error(string msg);
        void Success();
        void Warning(string p);

        /// <summary>
        /// IsTerminated:  Check if the job has been marked for termination
        /// </summary>
        /// <returns></returns>
        bool IsTerminated();
    }
}
