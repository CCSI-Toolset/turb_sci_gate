using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Orchestrator.AWS.Data.Contract.Messages
{
    /// <summary>
    /// SubmitJobMessage One way message, serialized by Orchestrator deserialized by Consumer
    /// </summary>
    public class SubmitJobMessage
    {
        private Guid id = Guid.Empty;
        private Guid simulationId = Guid.Empty;
        private string simulationName = null;
        private bool reset = false;
        private bool initialize = false;
        private int inc = -1;

        public Guid Id
        {
            get
            {
                return id;
            }
            set
            {
                Id = Id;
            }
        }
        public string SimulationName
        {
            get
            {
                return simulationName;
            }
            set
            {
                simulationName = SimulationName;
            }
        }
        public Guid SimulationId
        {
            get
            {
                return simulationId;
            }
            set
            {
                SimulationId = simulationId;
            }
        }

        public bool Reset
        {
            get
            {
                return reset;
            }
            set
            {
                Reset = reset;
            }
        }

        public bool Initialize
        {
            get
            {
                return initialize;
            }
            set
            {
                Initialize = initialize;
            }
        }

        public int Inc
        {
            get
            {
                return inc;
            }
            set
            {
                Inc = inc;
            }
        }
    }

}
