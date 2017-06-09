using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Turbine.Data.Contract.Behaviors;

namespace Turbine.Orchestrator.AWS
{
    public class SimpleScheduler : Turbine.Orchestrator.Contract.Behaviors.IScheduler
    {
        public void resetQueue()
        {
            IAWSOrchestratorContext ctx = AppUtility.GetContext();
        }
    }
}
