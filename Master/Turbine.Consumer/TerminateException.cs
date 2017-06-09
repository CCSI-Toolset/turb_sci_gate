using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Consumer
{
    /// <summary>
    /// TerminateException is thrown when state (isSetup & isTerminatedAPWN) are found in
    /// unexpected state.
    /// </summary>
    public class TerminateException : Exception
    {
        public TerminateException(string msg)
            : base(msg)
        {
        }
    }

}
