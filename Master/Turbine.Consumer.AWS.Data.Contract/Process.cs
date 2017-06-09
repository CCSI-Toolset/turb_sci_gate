using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Consumer.AWS.Data.Contract
{
    class Process : Turbine.Data.Contract.Behaviors.IProcess
    {
        private Guid jobid;

        public Process(Guid jobid)
        {
            // TODO: Complete member initialization
            this.jobid = jobid;
        }
        public void AddStdout(string data)
        {
            Turbine.Consumer.Contract.Behaviors.IConsumerContext ctx = Turbine.Consumer.AppUtility.GetConsumerContext();
            //var msg = AddStdoutMessage(jobid, ctx.Id, data);
        }

        public void AddStderr(string data)
        {
            throw new NotImplementedException();
        }

        public void SetStatus(int status)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, object> Input
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

        public IDictionary<string, object> Output
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

        public string WorkingDirectory
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

        public void AddConvergenceError(Dictionary<string, List<string>> dictionary)
        {
            throw new NotImplementedException();
        }

        public void AddBlockError(Dictionary<string, List<string>> dictionary)
        {
            throw new NotImplementedException();
        }

        public void AddStreamError(Dictionary<string, List<string>> dictionary)
        {
            throw new NotImplementedException();
        }

        public void AddConvergenceWarning(Dictionary<string, List<string>> dictionary)
        {
            throw new NotImplementedException();
        }

        public void AddBlockWarning(Dictionary<string, List<string>> dictionary)
        {
            throw new NotImplementedException();
        }

        public void AddStreamWarning(Dictionary<string, List<string>> dictionary)
        {
            throw new NotImplementedException();
        }
    }
}
