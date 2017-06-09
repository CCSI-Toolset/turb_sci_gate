using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using Microsoft.Practices.Unity;
using Turbine.Data.Contract.Behaviors;
using System.Threading;
using Turbine.Data.Contract;
using System.Threading.Tasks;
using Turbine.Consumer.Data.Contract.Behaviors;
using Turbine.Consumer.Data.Contract;
using Turbine.Consumer;
using Turbine.Consumer.AspenTech73;
using Turbine.Consumer.Contract.Behaviors;


namespace Turbine.Console
{
    class ConsoleContext : IContext
    {
        public string UserName
        {
            get { return "never used"; }
        }
        public string BaseWorkingDirectory
        {
            get { return Properties.Settings.Default.BaseDirectory; }
        }
    }
    /// <summary>
    /// AspenSinterConsumerConsole
    /// 
    /// Set up all implementations in app.config
    /// 
    /// </summary>
    class AspenSinter73ConsumerConsole
    {
        static void Main(string[] args)
        {
            string dir = Properties.Settings.Default.BaseDirectory;
            Turbine.Consumer.Console.SinterConsumerConsole.Run();
        }
    }
}
