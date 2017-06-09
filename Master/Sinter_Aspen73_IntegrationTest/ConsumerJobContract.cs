using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Consumer.Data.Contract.Behaviors;

namespace Sinter_Aspen73_IntegrationTest
{
    class ConsumerJobContract : IJobConsumerContract
    {
        public string ApplicationName
        {
            get { throw new NotImplementedException(); }
        }

        public string SimulationName
        {
            get { throw new NotImplementedException(); }
        }

        public Guid SimulationId
        {
            get { throw new NotImplementedException(); }
        }

        public bool Reset
        {
            get { throw new NotImplementedException(); }
        }

        public bool Visible
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsSuccess()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SimpleFile> GetSimulationInputFiles()
        {
            throw new NotImplementedException();
        }

        public void Message(string msg)
        {
            throw new NotImplementedException();
        }

        public Turbine.Data.Contract.Behaviors.IProcess Setup()
        {
            throw new NotImplementedException();
        }

        public bool Initialize()
        {
            throw new NotImplementedException();
        }

        public void Running()
        {
            throw new NotImplementedException();
        }

        public void Error(string msg)
        {
            throw new NotImplementedException();
        }

        public void Success()
        {
            throw new NotImplementedException();
        }

        public void Warning(string p)
        {
            throw new NotImplementedException();
        }

        public Turbine.Data.Contract.Behaviors.IProcess Process
        {
            get { throw new NotImplementedException(); }
        }

        public int Id
        {
            get
            {
                throw new NotImplementedException();
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

        public bool IsDown()
        {
            throw new NotImplementedException();
        }
    }
}
