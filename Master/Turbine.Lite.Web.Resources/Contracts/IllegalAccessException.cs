using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Lite.Web.Resources.Contracts
{
    public class IllegalAccessException : Exception
    {
        public IllegalAccessException(string msg)
            : base(msg)
        {
        }
    }
}
