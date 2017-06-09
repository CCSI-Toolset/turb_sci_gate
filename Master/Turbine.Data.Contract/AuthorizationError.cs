using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Turbine.Data.Contract
{
    public class AuthorizationError : Exception
    {
        public AuthorizationError(string msg) :
            base(msg)
        {
        }
    }
}