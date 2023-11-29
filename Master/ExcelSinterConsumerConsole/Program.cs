using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using Unity;
using Turbine.Data.Contract.Behaviors;
using sinter;
using System.Threading;
using Turbine.Data.Contract;
using System.Threading.Tasks;
using Turbine.Consumer.Data.Contract.Behaviors;
using Turbine.Consumer.Data.Contract;
using Turbine.Consumer;
using Turbine.Consumer.SimSinter;
using Turbine.Consumer.Contract.Behaviors;


namespace ExcelSinterConsumerConsole
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
    /// ExelSinterConsumerConsole
    /// 
    /// Set up all implementations in app.config
    /// 
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Turbine.Consumer.Console.SinterConsumerConsole.Run();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                int ERROR_ACCESS_DENIED = 5;
                Environment.Exit(ERROR_ACCESS_DENIED);
            }
        }
    }
}
