using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Turbine.Producer.Contracts;

namespace Turbine.Web.Behaviors
{
    class MyHTTPContext : IProducerContext
    {
        public string UserName
        {
            get { return System.Web.HttpContext.Current.User.Identity.Name; }
        }
    }
}