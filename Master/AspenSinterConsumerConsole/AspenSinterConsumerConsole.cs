using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using Microsoft.Practices.Unity;
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
using CommandLine;
using CommandLine.Text;
using Sinter;

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
    class AspenSinterConsumerConsole
    {
        class Options
        {
            [Option('s', "simulation", Required = false,
              HelpText = "Bind to Simulation")]
            public string Simulation { get; set; }

            [ParserState]
            public IParserState LastParserState { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                return HelpText.AutoBuild(this,
                  (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
            }
        }

        static void Main(string[] args)
        {
            var options = new Options();
            string dir = Properties.Settings.Default.BaseDirectory;
            IUnityContainer container = Turbine.Consumer.AppUtility.container;
            IConsumerContext consumerCtx = Turbine.Consumer.AppUtility.GetConsumerContext();
            container.RegisterInstance<IConsumerContext>(consumerCtx);

            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                // TODO: HACK TO BIND SIMULATION NAME
                consumerCtx.BindSimulationName = options.Simulation;
            }

            int iterations = Properties.Settings.Default.TimeOutIterations;
            int setupIterations = Properties.Settings.Default.TimeOutSetupIterations;
            int postInitIterations = Properties.Settings.Default.TimePostInitIterations;

            Turbine.Consumer.Console.SinterConsumerConsole.setTimeOutParams(iterations, 
                setupIterations, postInitIterations);
            Turbine.Consumer.Console.SinterConsumerConsole.Run();
        }
    }
}
