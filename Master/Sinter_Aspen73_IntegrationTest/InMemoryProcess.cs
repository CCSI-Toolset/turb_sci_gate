using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;


namespace Sinter_Aspen73_IntegrationTest
{
    class InMemoryProcess : Turbine.Data.Contract.Behaviors.IProcess
    {
        string wdir = "test73_" + Guid.NewGuid();
        private int status = 0;
        private IDictionary<string, object> outputD;
        private IDictionary<string, object> inputD = new Dictionary<string,object>();

        public InMemoryProcess()
        {
            System.IO.Directory.CreateDirectory(wdir);
        }

        public void AddStdout(string data)
        {
            throw new NotImplementedException();
        }

        public void AddStderr(string data)
        {
            throw new NotImplementedException();
        }

        public void SetStatus(int status)
        {
            this.status = status;
        }

        public IDictionary<string, object> Input
        {
            get
            {
                return inputD;
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
                Debug.WriteLine(String.Format("get Output: {0}", outputD), this.GetType()); ;
                return this.outputD;
            }
            set
            {
                Debug.WriteLine(String.Format("set Output: {0}", value), this.GetType()); ;
                this.outputD = value;
            }
        }

        public string WorkingDirectory
        {
            get
            {
                return wdir;
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
