using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Data.Contract.Behaviors
{
    public interface IProcess
    {
        void AddStdout(String data);
        void AddStderr(String data);
        //void SetInput(byte[] data);
        //void SetInput(List<string> list);
        void SetStatus(int status);
        //void SetWorkingDirectory(String path);

        //string[] Input { get; set; }
        IDictionary<String, Object> Input { get; set; }
        IDictionary<String, Object> Output { get; set; }

        String WorkingDirectory { get; set;  }

        void AddConvergenceError(Dictionary<string, List<string>> dictionary);

        void AddBlockError(Dictionary<string, List<string>> dictionary);

        void AddStreamError(Dictionary<string, List<string>> dictionary);

        void AddConvergenceWarning(Dictionary<string, List<string>> dictionary);

        void AddBlockWarning(Dictionary<string, List<string>> dictionary);

        void AddStreamWarning(Dictionary<string, List<string>> dictionary);
    }
}
