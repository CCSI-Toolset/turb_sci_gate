using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Turbine.Consumer.Data.Contract.Behaviors;
using Turbine.Data.Contract.Behaviors;


namespace Sinter_Aspen73_IntegrationTest
{
    abstract class InMemoryJob : IJobConsumerContract
    {
        Guid simId = Guid.NewGuid();
        int state = 0;

        protected List<SimpleFile> inputFiles = new List<SimpleFile>();
        protected IProcess process;
        protected string applicationName;
        protected string simulationName;
        protected int jobId;

        public InMemoryJob()
        {
            SetupTestParamters();
        }

        protected abstract void SetupTestParamters();

        public string ApplicationName
        {
            get { return applicationName; }
        }

        public string SimulationName
        {
            get { return simulationName; }
        }

        public Guid SimulationId
        {
            get { return simId; }
        }

        public bool Reset
        {
            get { return false; }
        }

        public bool Visible
        {
            get { return false; }
        }

        public bool IsSuccess()
        {
            return state == 4;
        }

        public IEnumerable<SimpleFile> GetSimulationInputFiles()
        {
            return inputFiles;
        }

        public void Message(string msg)
        {
            Debug.WriteLine(msg);
        }

        public IProcess Setup()
        {
            return process;
        }

        public bool Initialize()
        {
            state = 1;
            return true;
        }

        public void Running()
        {
            state = 3;
        }

        public void Error(string msg)
        {
            Debug.WriteLine("ERROR: " + msg);
        }

        public void Success()
        {
            state = 4;
        }

        public void Warning(string p)
        {
            Debug.WriteLine("WARNING: " + p);
        }

        public IProcess Process
        {
            get { return process; }
        }

        public int Id
        {
            get
            {
                return jobId;
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public bool IsTerminated()
        {
            throw new NotImplementedException();
        }
    }
}
