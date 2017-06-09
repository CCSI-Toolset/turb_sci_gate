using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turbine.Producer.AWS.Contracts
{
    public interface IMongoContext
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
