using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using Microsoft.Practices.Unity;
using Turbine.Common;
using Turbine.Data.Contract.Behaviors;
using Turbine.Consumer;
using sinter;
using System.Threading;
using Turbine.Data.Contract;
using System.Threading.Tasks;
using Turbine.Consumer.Data.Contract.Behaviors;
using Turbine.Consumer.Data.Contract;


namespace Turbine.Console
{
    class ConsoleContext : IContext
    {
        public string BaseWorkingDirectory
        {
            get { return AspenSinterConsumerConsole.baseWorkDir; }
        }
    }

    class ConsumerContext : IConsumerContext
    {
        public Guid Id
        {
            get { return AspenSinterConsumerConsole.guid; }
        }
        private static string hostname = System.Net.Dns.GetHostName();
        public string Hostname
        {
            get
            {
                int id = Thread.CurrentThread.ManagedThreadId;
                return String.Format("{0} - {1}", hostname, id);
            }
        }
    }

    class AspenSinterConsumerConsole
    {
        public static Guid guid = Guid.NewGuid();
        public static string baseWorkDir;
        private static string tag = "AspenSinterConsumerConsole";
        static AspenSinterConsumer consumer = null;

        static void OnExit(int retCode)
        {
            Debug.WriteLine("Unregister Consumer", tag);
            IConsumerRegistrationContract cc = new ConsumerRegistrationContract();
            cc.UnRegister();
            System.Console.Write("[return]");
            System.Console.ReadLine();
            Environment.Exit(retCode);
        }

        static void Main(string[] args)
        {
            int timeSleepInterval = 1000;
            int iterations = 60 * 15;
            int setupIterations = 20;
            bool finish = false;

            System.Console.CancelKeyPress += delegate
            {
                OnExit(0);
            };

            string dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "TurbineDevelopment");

            // Register as a consumer, else can't use JobContract
            IConsumerRegistrationContract cc = new ConsumerRegistrationContract();
            cc.Register();

            while (true)
            {
                if (!Directory.Exists(dir))
                {
                    Debug.WriteLine(String.Format("Base Directory does not exist: {0}", dir), tag);
                    OnExit(1);
                }
                try
                {
                    var acl = Directory.GetAccessControl(dir);
                }
                catch (UnauthorizedAccessException)
                {
                    Debug.WriteLine(String.Format("Base Directory is not writable: {0}", dir), tag);
                    OnExit(1);
                }

                Debug.WriteLine("> New Task", tag);
                baseWorkDir = dir;
                consumer = new AspenSinterConsumer();
                var task = new Task<Boolean>(() => consumer.Run());
                finish = false;
                int i = 0;
                task.Start();

                while (true)
                {
                    i++;
                    Debug.WriteLine("top");
                    try
                    {
                        finish = task.Wait(timeSleepInterval);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception caught from Run: " + ex.Message, tag);
                        Debug.WriteLine(ex.StackTrace, tag);
                        System.Threading.Thread.Sleep(4000);
                        break;
                    }
                    Debug.WriteLine(">> Waiting " + i, tag);
                    if (finish)
                    {
                        if (task.IsFaulted)
                        {
                            Debug.WriteLine("Faulted: " + task.Exception, tag);
                        }
                        else if (task.IsCompleted)
                            Debug.WriteLine("Completed", tag);
                        else
                            throw new Exception("Task Unexpected status?");

                        if (!task.Result)
                        {
                            // Nothing on Job Queue
                            System.Threading.Thread.Sleep(4000);
                        }
                        break;
                    }
                    // DETECT HANGS
                    if (consumer.IsSimulationInitializing)
                    {
                        Debug.WriteLine("> Simulation Initializing: " + setupIterations);
                        if (i >= setupIterations)
                        {
                            // Allows SinterConsumer to attempt to reopen the simulation
                            // IF attempts max out in SinterConsumer, throw Exception
                            // and task.IsFaulted and job will move to error
                            Debug.WriteLine("HANG SETUP, TRY AGAIN", tag);
                            int num = consumer.MonitorTerminateAPWNByName();
                            Debug.WriteLine("Killed apwn instances: " + num, tag);
                            i = 0;
                            continue;
                        }
                    }
                    else if (consumer.IsEngineRunning)
                    {
                        Debug.WriteLine("> Engine Running: " + iterations);
                        if (i >= iterations)
                        {
                            // Assume Simulation is done Initializing
                            Debug.WriteLine("HANG RUNNING EXIT", tag);
                            consumer.MonitorTerminateAPWN();
                            try
                            {
                                consumer.MonitorTerminateAspenEngine();
                            }
                            catch (TerminateException ex)
                            {
                                Debug.WriteLine(ex.Message, tag);
                            }
                        }
                    }
                }
            }
        }
    }
}
