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
using CommandLine;
using CommandLine.Text;
using Sinter;
using System.Data.Common.EntitySql;

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
        }

        static void Main(string[] args)
        {
            string dir = Properties.Settings.Default.BaseDirectory;
            IUnityContainer container = Turbine.Consumer.AppUtility.container;
            IConsumerContext consumerCtx = Turbine.Consumer.AppUtility.GetConsumerContext();
            container.RegisterInstance<IConsumerContext>(consumerCtx);

            var parserResult = Parser.Default.ParseArguments<Options>(args);

            parserResult.WithParsed<Options>(options => RunOptions(options, consumerCtx));
            parserResult.WithNotParsed<Options>(errs =>
            {
                var helpText = HelpText.AutoBuild(parserResult,
                  (HelpText current) => HelpText.DefaultParsingErrorsHandler(parserResult, current));
            });
        }
        static void RunOptions(Options opts, IConsumerContext consumerCtx)
        {
            consumerCtx.BindSimulationName = opts.Simulation;

            int iterations = Properties.Settings.Default.TimeOutIterations;
            int setupIterations = Properties.Settings.Default.TimeOutSetupIterations;
            int postInitIterations = Properties.Settings.Default.TimePostInitIterations;

            Turbine.Consumer.Console.SinterConsumerConsole.setTimeOutParams(iterations,
                setupIterations, postInitIterations);
            Turbine.Consumer.Console.SinterConsumerConsole.Run();
        }
    }
}
