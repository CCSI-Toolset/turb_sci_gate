using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Data.Contract.Behaviors;


namespace Turbine.Producer.Data.Contract.Behaviors
{
    public interface IJobProducerContract : IJob
    {
        // producer state methods
        void Submit();
        void Cancel();
        void Terminate();
        void Kill();
    }
}
