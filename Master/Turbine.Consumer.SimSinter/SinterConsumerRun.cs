using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Data.Contract.Behaviors;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using Turbine.Consumer.Data.Contract.Behaviors;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Turbine.Consumer.Contract.Behaviors;
using System.Management;


namespace Turbine.Consumer.SimSinter
{
    /// <summary>
    /// SinterConsumer
    /// </summary>
    abstract public class SinterConsumerRun : IConsumerRun
    {
        protected string configFileName = "sinter_configuration.txt";
        protected sinter.ISimulation sim = null;
        protected Guid last_simulation_name = Guid.Empty;
        protected bool last_simulation_success = false;
        protected bool isSetup = false;
        protected bool isTerminated = false;
        protected bool running = false;
        protected bool visible = false;
        protected string[] applications;
        protected Guid consumerId;
        private IJobConsumerContract job;


        string[] IConsumerRun.SupportedApplications
        {
            get { return applications; }
        }

        Guid IConsumerRun.ConsumerId
        {
            get { return consumerId; }
        }

        public bool IsEngineRunning
        {
            get { return (sim != null && running); }
        }
        public bool IsSimulationInitializing
        {
            get { return (sim != null && sim.IsInitializing); }
        }
        public bool IsSimulationOpened
        {
            get { return (sim != null && sim.ProcessID > 0); }
        }

        abstract protected void copyFilesToDisk(IJobConsumerContract job);
        abstract protected void InitializeSinter(IJobConsumerContract job, string workingDir, string config);
        abstract public void CleanUp();
        abstract protected bool RunNoReset(IJobConsumerContract job);

        protected virtual IJobConsumerContract GetNextJob()
        {
            IJobQueue queue = AppUtility.GetJobQueue(this);
            var job = queue.GetNext(this);
            if (job != null && IsSupported(job) == false)
            {
                Debug.WriteLine(String.Format("Application {0} is not supported", job.ApplicationName), GetType().Name);
                Debug.WriteLine("QUEUE: " + queue, GetType().Name);
                //return null;
                throw new Exception(String.Format("Application {0} is not supported", job.ApplicationName));
            }
            return job;
        }

        /// <summary>
        /// Returns true if application of job is supported
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        abstract protected bool IsSupported(IJobConsumerContract job);

        /// <summary>
        /// Stop is called while simulation is running.  The "running" thread is blocking on "Run", and when 
        /// "stopSim" is called it will eventually exit.  The only valid "runStatus" values are STOPPED and STOP_FAILED.
        /// This monitor thread waits for the exit to happen and returns the
        /// runStatus.  If this runStatus is not normal, terminate may need to be called to clean up stray processes.
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            if (sim != null)
            {
                sinter.sinter_InteractiveSim isim = ((sinter.sinter_InteractiveSim)sim);
                sim.closeSim();

                try
                {
                    isim.stopSim();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Message: " + ex.Message, "SinterConsumerRun.Stop");
                    Debug.WriteLine("StackTrace: " + ex.StackTrace, "SinterConsumerRun.Stop");
                }
                
                // Wait for Stop to work
                // According to developer this will always return
                Debug.WriteLine("Waiting for Stop", this.GetType().Name);
                int wait_seconds = 60;
                if (Monitor.TryEnter(this, new TimeSpan(0, 0, wait_seconds)))
                {
                    Debug.WriteLine(String.Format("Obtained Lock, Stop simulation status: {0}", sim.runStatus), "SinterConsumerRun.Stop");
                    Monitor.Exit(this);
                    return (sim.runStatus == sinter.sinter_AppError.si_SIMULATION_STOPPED);
                }
            }

            // NEVER GOT LOCK
            Debug.WriteLine(String.Format("Failure to Obtain Lock"), "SinterConsumerRun.Stop");
            return false;
        }

        /// <summary>
        /// setIsTerminated:  sets a boolean value to isTerminated.
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public void setIsTerminated(bool terminated)
        {
            isTerminated = terminated;
        }


        /// <summary>
        /// terminateSimAndJob:  Terminate both job and simulation
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public bool terminateSimAndJob()
        {
            bool success = false;
            if (sim != null)
            {
                if (sim.ProcessID > 0)
                {
                    Debug.WriteLine("Terminating the Simulation",
                        "SinterConsumerRun.terminateSimAndJob()");
                    //KillProcessAndChildren(sim.ProcessID);
                    success = sim.terminate();
                    Debug.WriteLine("sim.terminate() returns " + success.ToString(),
                        "SinterConsumerRun.terminateSimAndJob()");
                }
                job = null;
                isTerminated = success;
                Debug.WriteLine("Job termination result : {0}", success);
                Debug.WriteLine(String.Format("Simulation Terminate ( sinter:{0}, processID:{1} ) {2}", sim.GetType().Name, sim.ProcessID, success), GetType().Name);
            }
            return success;
        }

        /// <summary>
        /// Kill a process, and all of its children, grandchildren, etc.
        /// </summary>
        /// <param name="pid">Process ID.</param>
        private static void KillProcessAndChildren(int pid)
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher
                  ("Select * From Win32_Process Where ParentProcessID=" + pid);
                ManagementObjectCollection moc = searcher.Get();
                foreach (ManagementObject mo in moc)
                {
                    KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(String.Format("Exception 1 (processId={0}): {1} ", pid, ex.Message),
                    "Application.KillProcessAndChildren");
                Debug.WriteLine(ex.StackTrace, "Application.KillProcessAndChildren");
                return;
            }

            Process proc;
            string processName;
            try
            {
                proc = Process.GetProcessById(pid);
                processName = proc.ProcessName;
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine(String.Format("ArgumentException (processId={0}): {1}", pid, ex.Message),
                    "Application.KillProcessAndChildren");
                Debug.WriteLine(ex.StackTrace, "Application.KillProcessAndChildren");
                return;
            }
            catch (System.InvalidOperationException ex)
            {
                Debug.WriteLine(String.Format("InvalidOperationException (processId={0}): {1}", pid, ex.Message),
                    "Application.KillProcessAndChildren");
                Debug.WriteLine(ex.StackTrace, "Application.KillProcessAndChildren");
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(String.Format("Exception 2 (processId={0}): {1} ", pid, ex.Message),
                    "Application.KillProcessAndChildren");
                Debug.WriteLine(ex.StackTrace, "Application.KillProcessAndChildren");
                return;
            }

            Debug.WriteLine(String.Format("Send Kill: processId={0}, name={1}", pid, processName),
                "Application.KillProcessAndChildren");
            try
            {
                proc.Kill();
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                Debug.WriteLine(String.Format("Win32Exception (pid={0}, name={1}):  {2}",
                    pid, processName, ex.Message), "Application.KillProcessAndChildren");
                Debug.WriteLine(ex.StackTrace, "Application.KillProcessAndChildren");
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(String.Format("Win32Exception (pid={0}, name={1}):  {2}",
                    pid, processName, ex.Message), "Application.KillProcessAndChildren");
                Debug.WriteLine(ex.StackTrace, "Application.KillProcessAndChildren");
                return;
            }
            Debug.WriteLine(String.Format("Kill: processId={0}, name={1}", pid, processName),
                "Application.KillProcessAndChildren");
        }


        /// <summary>
        /// isJobTerminated:  Checks if the job is terminated 
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public bool isJobTerminated()
        {
            if (job != null)
            {
                return job.IsTerminated();
            }
            return false;
        }


        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        /// <summary>
        /// copyFromCache:  copies simulation files from cache, return true if cache exists.
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        private bool copyFromCache(IJobConsumerContract job)
        {
            string cacheDir = Path.Combine(AppUtility.GetAppContext().BaseWorkingDirectory, job.SimulationId.ToString());
            Debug.WriteLine(String.Format("Copying From Cache: {0}", cacheDir), "SinterConsumerRun.CopyFromCache");
            //string shortConfigFileName = "sinter_configuration.txt";
            //configFileName = Path.Combine(job.Process.WorkingDirectory, shortConfigFileName);

            if (Directory.Exists(cacheDir))
            {
                // Shallow Copy cacheDir to Working Directory
                var source = new DirectoryInfo(cacheDir);
                var target = new DirectoryInfo(job.Process.WorkingDirectory);
                /*foreach (FileInfo finfo in source.GetFiles())
                {
                    Debug.WriteLine(String.Format("Found Cached file {0}: {1}", job.SimulationId, finfo.FullName),
                        "SinterConsumer.copyFromCache");
                    finfo.CopyTo(Path.Combine(target.ToString(), finfo.Name), true);
                }*/

                Debug.WriteLine(String.Format("Copying Cached files from {0} to {1}", source.ToString(), target.ToString())
                    , "SinterConsumerRun.CopyFromCache");
                DirectoryCopy(source.ToString(), target.ToString(), true);
                return true;
            }
            Debug.WriteLine(String.Format("Directory {0} doesn't exist", cacheDir), "SinterConsumerRun.CopyFromCache");
            return false;
        }

        /// <summary>
        /// SetupWorkingDir creates the working directory
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        private string SetupWorkingDir(IProcess process)
        {
            //Sets the dir in the function to MyDocs, but doesn't actually chdir
            string dir = process.WorkingDirectory;
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
            Debug.WriteLine("The Working directory is: " + dir, "SinterConsumerRun.SetupWorkingDir");
            var dinfo = Directory.CreateDirectory(dir);
            return dir;
        }

        /// <summary>
        /// DoSetup setup up the working directory, returns an IProcess interface.
        /// </summary>
        /// <param name="job"></param>
        /// <param name="configFileName"></param>
        /// <returns></returns>
        private IProcess DoSetup(IJobConsumerContract job)
        {
            Debug.WriteLine("DoSetup Called on " + job.Id, "SinterConsumer");
            // NOTE: expect OptimisticConcurrencyException
            // to occur ONLY here when running multiple consumers
            IProcess process = null;
            try
            {
                process = job.Setup();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SinterConsumer.DoSetup");
                throw;
            }

            //Sets the dir in the function to MyDocs, but doesn't actually chdir
            try
            {
                SetupWorkingDir(process);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to setup working directory", "SinterConsumer.Run");
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                job.Error(String.Format("Failed to setup working directory: {0}", ex.StackTrace));
                throw;
            }

            bool isCached = false;
            try
            {
                isCached = copyFromCache(job);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("copyFromCache Failed to copy files to working directory", "SinterConsumer.Run");
                Debug.WriteLine(ex.StackTrace, "SinterConsumer.Run");
                job.Error(String.Format("copyFromCache Failed to copy files to working directory: {0}", ex.StackTrace));
                throw;
            }

            if (isCached == false)
            {
                try
                {
                    copyFilesToDisk(job);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("copyFilesToDisk Failed to copy files to working directory", "SinterConsumerRun.DoSetup");
                    Debug.WriteLine(ex.Message, "SinterConsumerRun.DoSetup");
                    Debug.WriteLine(ex.StackTrace, "SinterConsumerRun.DoSetup");
                    job.Error(String.Format("copyFilesToDisk Failed to copy files to working directory: {0} StackTrace: {1}"
                        , ex.Message, ex.StackTrace));
                    throw;
                }

            }
            job.Message("working directory setup finished");

            return process;

        }

        /// <summary>
        ///  DoConfigure starts up COM process server.  Occasionally readSetup fail, this
        ///  will attempt maxAttemptsReadSetup times to open the
        ///  simulation.  Also readSetup will hang in unmanaged code
        /// </summary>
        /// <param name="job"></param>
        /// <param name="process"></param>
        /// <param name="maxAttemptsReadSetup"></param>
        /// <returns></returns>
        private void DoConfigure(IJobConsumerContract job,
            IProcess process, int maxAttemptsReadSetup)
        {
            int attempts = 1;
            int timeout = 1000;
            string setupString = null;

            var configFilePath = Path.Combine(process.WorkingDirectory, configFileName);
            while (true)
            {
                Debug.WriteLine(String.Format("Attempt to Read {0}", configFilePath),
                    "SinterConsumer.DoConfigure");
                setupString = "";
                try
                {
                    StreamReader inFileStream = new StreamReader(configFilePath);
                    setupString = inFileStream.ReadToEnd();
                    inFileStream.Close();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message, "SinterConsumer.DoConfigure");
                    Debug.WriteLine(ex.StackTrace, "SinterConsumer.DoConfigure");
                    job.Error(String.Format("Failed to read configuration file: {0}", ex.StackTrace));
                    throw;
                }

                try
                {
                    InitializeSinter(job, process.WorkingDirectory, setupString);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message, "SinterConsumer.DoConfigure.InitializeSinter");
                    Debug.WriteLine(ex.StackTrace, "SinterConsumer.DoConfigure.InitializeSinter");
                    job.Error(String.Format("Failed to InitializeSinter: {0}", ex.StackTrace));
                    throw;

                }

                Debug.WriteLine(String.Format("readSetup {0}", configFilePath),
                    "SinterConsumerRun.DoConfigure");
                Debug.WriteLine(String.Format("model {0}", sim.setupFile.aspenFilename),
                    "SinterConsumerRun.DoConfigure");

                try
                {
                    lock (this)
                    {
                        sim.openSim();
                        isSetup = true;
                        if (isTerminated)
                        {
                            throw new TerminateException(
                                "AspenModeler was terminated, try readSetup again");
                        }
                    }
                }
                catch (Exception ex)
                {
                    //System.Exception: Cannot create ActiveX component
                    var msg = String.Format("Failed to open Simulation ({0}: {1}", attempts, ex.Message);
                    Debug.WriteLine(msg, "SinterConsumerRun.DoConfigure");
                    Debug.WriteLine(ex.StackTrace, "SinterConsumerRun.DoConfigure");
                    try
                    {
                        sim.closeSim();
                    }
                    catch (NullReferenceException e2)
                    {
                        Debug.WriteLine(e2.Message, "SinterConsumerRun.DoConfigure");
                        Debug.WriteLine(ex.StackTrace, "SinterConsumerRun.DoConfigure");
                    }
                    if (job.IsTerminated())
                    {
                        throw new TerminateException("Job was terminated by producer");
                    }
                    else if (attempts >= maxAttemptsReadSetup)
                    {
                        job.Error(msg);
                        throw;
                    }
                    job.Message(msg);
                    isTerminated = false;
                    attempts += 1;
                    System.Threading.Thread.Sleep(timeout);
                    timeout *= 2;
                    continue;
                }
                job.Message("sinter read setup finished");

                // One of these commands hangs infrequently..
                // However this job will not be retired as above, just abandoned and left in setup.
                Debug.WriteLine("set simulation layout", "SinterConsumer.DoConfigure");
                try
                {
                    sim.Vis = visible; // operates on Aspen instance
                }
                catch (Exception ex)
                {
                    sim.closeSim();
                    var msg = String.Format("Failed to Layout Simulation ({0}: {1}", attempts, ex.Message);
                    Debug.WriteLine(msg, "SinterConsumer.DoConfigure");
                    Debug.WriteLine(ex.StackTrace, "SinterConsumer.DoConfigure");
                    if (job.IsTerminated())
                    {
                        throw new TerminateException("Job was terminated by producer");
                        Debug.WriteLine("Job was terminated by producer", "SinterConsumer.DoConfigure");
                    }
                    else if (attempts >= maxAttemptsReadSetup)
                    {
                        job.Error(msg);
                        throw;
                    }
                    job.Message(msg);
                    attempts += 1;
                    System.Threading.Thread.Sleep(timeout);
                    timeout *= 2;
                    continue;
                }

                Debug.WriteLine("set simulation reset", "SinterConsumer.DoConfigure");
                try
                {
                    sim.dialogSuppress = true;  // operates on Aspen instance
                    //sim.resetSim(); // operates on Aspen instance
                }
                catch (Exception ex)
                {
                    sim.closeSim();
                    var msg = String.Format("Failed to Reset Simulation ({0}: {1}", attempts, ex.Message);
                    Debug.WriteLine(msg, "SinterConsumer.DoConfigure");
                    Debug.WriteLine(ex.StackTrace, "SinterConsumer.DoConfigure");
                    if (job.IsTerminated())
                    {
                        throw new TerminateException("Job was terminated by producer");
                    }
                    else if (attempts >= maxAttemptsReadSetup)
                    {
                        job.Error(msg);
                        throw;
                    }
                    job.Message(msg);
                    attempts += 1;
                    System.Threading.Thread.Sleep(timeout);
                    timeout *= 2;
                    continue;
                }
                break;
            }
        }

        /// <summary>
        /// DoInitialize initializes the Aspen Engine if specified in the job contract.
        /// </summary>
        /// <param name="job"></param>
        /// <param name="stest"></param>
        /// <param name="defaultsDict"></param>
        private void DoInitialize(IJobConsumerContract job, sinter.ISimulation stest, JObject defaultsDict)
        {
            if (!job.Initialize())
                return;

            Debug.WriteLine(String.Format("Start warm-up run on job {0}", job.Id),
                "SinterConsumerRun.DoInitialize");
            try
            {
                stest.sendInputs(defaultsDict);
            }
            catch (Exception ex)
            {
                job.Error(String.Format(
                    "Error sending Defaults to Aspen for warm-up run: {0}, traceback {1}",
                    ex.Message, ex.StackTrace));
                Debug.WriteLine(String.Format("Moved Job {0} to Error", job.Id),
                    "SinterConsumer.DoInitialize");
                Debug.WriteLine(String.Format(
                    "Error sending Defaults to Aspen for warm-up run: {0}, traceback {1}",
                    ex.Message, ex.StackTrace), "SinterConsumerRun.DoInitialize");
                throw;
            }
            job.Message("Warm-up inputs sent, running simulation");
            Debug.WriteLine(String.Format("Calling run Simulation for warm-up on Job {0}", job.Id),
                "SinterConsumerRun.DoInitialize");
            try
            {
                running = true;
                lock (this)
                {
                    stest.sendInputsToSim();
                    stest.runSim();
                }
                running = false;
                if (isTerminated)
                {
                    throw new TerminateException(
                        "Monitor Terminated Initializing Simulation");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(String.Format("Moving Job {0} to Error", job.Id),
                    "SinterConsumer.Do");
                Debug.WriteLine(String.Format(
                    "Error running warm-up simulation: {0}, traceback {1}",
                    ex.Message, ex.StackTrace), "SinterConsumerRun.DoInitialize");
                job.Error(String.Format(
                    "Error running warm-up simulation: {0}, traceback {1}",
                    ex.Message, ex.StackTrace));
                throw;
            }

            lock (this)
            {
                if (stest.runStatus == sinter.sinter_AppError.si_OKAY)
                {
                    stest.recvOutputsFromSim();
                    job.Message("Warm-up complete and successful.  Reseting Sim for real run.");
                    Debug.WriteLine(String.Format("Warm-up complete and successful.  Reseting Sim for real run {0}", job.Id),
                        "SinterConsumer.Run");
                }
                else if (stest.runStatus == sinter.sinter_AppError.si_SIMULATION_WARNING)
                {
                    stest.recvOutputsFromSim();
                    job.Message("Warm-up with warnings.  Reseting Sim for real run.");
                    Debug.WriteLine(String.Format("Warm-up with warnings.  Reseting Sim for real run {0}", job.Id),
                        "SinterConsumer.Run");
                }
                else
                {
                    job.Message("Warm-up complete and failed.  Reseting Sim for real run.");
                    Debug.WriteLine(String.Format("Warm-up complete and failed.  Reseting Sim for real run {0}", job.Id),
                        "SinterConsumer.Run");
                }
            }

        }

        /// <summary>
        /// DoRun sends inputs specified in job contract to aspen simulation and runs
        /// the simulation.
        /// </summary>
        /// <param name="job"></param>
        /// <param name="stest"></param>
        /// <param name="combinedDict"></param>
        protected void DoRun(IJobConsumerContract job, sinter.ISimulation stest, JObject defaultsDict, JObject inputDict)
        {
            Debug.WriteLine(String.Format("Send Inputs to Job {0}", job.Id),
                "SinterConsumer.DoRun");

            try
            {
                Debug.WriteLine("Default: " + JsonConvert.SerializeObject(defaultsDict), "SinterConsumerRun.DoRun");
                Debug.WriteLine("Input: " + JsonConvert.SerializeObject(inputDict), "SinterConsumerRun.DoRun");

                if (defaultsDict != null)
                {
                    stest.sendDefaults(defaultsDict);
                }
                stest.sendInputs(inputDict);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(String.Format(
                    "Error sending Inputs: {0}, traceback {1}",
                    ex.Message, ex.StackTrace), "SinterConsumerRun.DoRun");
                if (job.IsTerminated())
                {
                    throw new TerminateException("Job was terminated by producer");
                }
                job.Error(String.Format(
                    "Error sending Inputs: {0}, traceback {1}",
                    ex.Message, ex.StackTrace));
                Debug.WriteLine(String.Format("Moved Job {0} to Error", job.Id),
                    "SinterConsumer.DoRun");
                throw;
            }
            job.Message("sinter inputs sent, running simulation");
            Debug.WriteLine(String.Format("Job {0}: sendInputsToSim ", job.Id),
                "SinterConsumer.DoRun");
            try
            {
                running = true;
                lock (this)
                {
                    stest.sendInputsToSim();
                    Debug.WriteLine(String.Format("Job {0}: runSim ", job.Id),
                        "SinterConsumer.DoRun");
                    stest.runSim();
                    Debug.WriteLine(String.Format("Job {0}: recvOutputsFromSim ", job.Id),
                        "SinterConsumer.DoRun");
                    if (stest.runStatus == sinter.sinter_AppError.si_OKAY)
                    {
                        stest.recvOutputsFromSim();
                        job.Message("Real Run was complete and successful.");
                        Debug.WriteLine(String.Format("Real Run was complete and successful. Job Id: {0}", job.Id),
                            "SinterConsumer.Run");
                    }
                    else if (stest.runStatus == sinter.sinter_AppError.si_SIMULATION_WARNING)
                    {
                        stest.recvOutputsFromSim();
                        job.Message("Real Run was completed with warning.");
                        Debug.WriteLine(String.Format("Real Run was completed with warning. Job Id: {0}", job.Id),
                            "SinterConsumer.Run");
                    }
                    else
                    {
                        job.Message(String.Format("Real Run failed, runStatus={0}", stest.runStatus));
                        Debug.WriteLine(String.Format("Real Run was complete and failed. Job Id: {0}, runStatus={1}", 
                            job.Id, stest.runStatus), "SinterConsumer.Run");
                    }
                }
                running = false;
                if (isTerminated)
                {
                    throw new TerminateException(
                        "Monitor thread Terminated Simulation");
                }
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                Debug.WriteLine(String.Format("Moving Job {0} to Error", job.Id),
                    "SinterConsumerRun.DoRun");
                Debug.WriteLine(ex.Message, "SinterConsumerRun.DoRun");
                Debug.WriteLine(ex.StackTrace, "SinterConsumerRun.DoRun");

                if (this.terminateSimAndJob() == false)
                {
                    Debug.WriteLine("Failed to terminate the simulation",
                        "SinterConsumerRun.DoRun()");
                    if (sim != null)
                    {
                        Debug.WriteLine("1) Simulation is not Null",
                        "SinterConsumerRun.DoRun()");
                        sim.closeSim();
                        if (sim != null)
                        {
                            KillProcessAndChildren(sim.ProcessID);
                        }
                        sim = null;
                        //sim.terminate();
                    }
                }

                job.Error(String.Format(
                    "Error running simulation: {0}, traceback {1}",
                    ex.Message, ex.StackTrace));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(String.Format("Moving Job {0} to Error", job.Id),
                    "SinterConsumerRun.DoRun");
                Debug.WriteLine(ex.Message, "SinterConsumerRun.DoRun");
                Debug.WriteLine(ex.StackTrace, "SinterConsumerRun.DoRun");
                if (job.IsTerminated())
                {
                    throw new TerminateException("Job was terminated by producer");
                }
                job.Error(String.Format(
                    "Error running simulation: {0}, traceback {1}",
                    ex.Message, ex.StackTrace));
                throw;
            }
        }

        /// <summary>
        /// DoFinalize sends outputs to the database, sets final run status and
        /// warnings and/or errors in the job and process.
        /// </summary>
        /// <param name="stest"></param>
        /// <param name="job"></param>
        /// <param name="process"></param>
        protected virtual void DoFinalize(sinter.ISimulation stest,
             IJobConsumerContract job,
             IProcess process)
        {
            int runStatus = (int)stest.runStatus;
            process.SetStatus(runStatus);
            IDictionary<string, Object> myDict = null;
            JObject outputDict;
            try
            {
                if (stest.runStatus == sinter.sinter_AppError.si_OKAY || 
                    stest.runStatus == sinter.sinter_AppError.si_SIMULATION_WARNING ||
                    stest.runStatus == sinter.sinter_AppError.si_NONCONVERGENCE_ERROR)
                {
                    var superDict = stest.getOutputs();
                    outputDict = (JObject)superDict["outputs"];
                    // HACK: Inefficient Just making it work w/o covariance issues
                    string data = outputDict.ToString(Newtonsoft.Json.Formatting.None);
                    myDict = JsonConvert.DeserializeObject<IDictionary<string, Object>>(data);
                }
                else
                {
                    Debug.WriteLine(String.Format("Sinter runstatus={0}, did not retrieve outputs.", stest.runStatus), 
                        "SinterConsumer.DoFinalize");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SinterConsumer.DoFinalize");
                process.Output = new Dictionary<string, Object>() {
                    {"exception",ex.Message}
                };
                if (job.IsTerminated())
                {
                    throw new TerminateException("Job was terminated by producer");
                }
                job.Error(String.Format("Failed to retrieve the outputs: {0}   {1}", ex.Message, ex.StackTrace));
                return;
            }
            Debug.WriteLine("Outputs: " + myDict, this.GetType());
            process.Output = myDict;

            if (stest.runStatus == sinter.sinter_AppError.si_OKAY)
            {
                job.Success();
            }
            else if (stest.runStatus == sinter.sinter_AppError.si_SIMULATION_ERROR)
            {
                var msg = String.Join(",", stest.errorsBasic());
                Debug.WriteLine("Error: si_SIMULATION_ERROR", "SinterConsumerRun.DoFinalize");
                Debug.WriteLine(msg, "SinterConsumerRun.DoFinalize");

                // Truncate the msg if it's more than 4000 characters.
                // The last 4000 characters are more interesting for debugging.
                if (msg.Length > 4000)
                {
                    msg = msg.Substring(msg.Length - 4000);
                }

                job.Error(String.Format("Error: si_SIMULATION_ERROR: {0}", msg));
                if (sim != null)
                {
                    Debug.WriteLine("2) Simulation is not Null",
                    "SinterConsumerRun.DoFinalize()");
                    sim.closeSim();
                    if (sim != null)
                    {
                        KillProcessAndChildren(sim.ProcessID);
                    }
                    sim = null;
                    //sim.terminate();
                }
            }
            else if (stest.runStatus == sinter.sinter_AppError.si_SIMULATION_WARNING)
            {
                job.Message(String.Join(",", stest.warningsBasic()));
                job.Success();
            }
            else if (stest.runStatus == sinter.sinter_AppError.si_COM_EXCEPTION)
            {
                Debug.WriteLine("Error: si_COM_EXCEPTION", "SinterConsumerRun.DoFinalize");
                job.Error("COM Exception, Server was Terminated");
            }
            else if (stest.runStatus == sinter.sinter_AppError.si_SIMULATION_NOT_RUN)
            {
                Debug.WriteLine("Error: si_SIMULATION_NOT_RUN", "SinterConsumerRun.DoFinalize");
                job.Error("Simulation was not Run");
            }
            else
            {
                Debug.WriteLine("Error: Unknown", "SinterConsumerRun.DoFinalize");
                job.Error(string.Format("Aspen Process Status unknown code {0}.", (int)stest.runStatus));
            }
        }


        /// <summary>
        /// Run pops a job descriptor off the queue and manages the underlying AspenSinter
        /// </summary>
        /// <returns></returns>
        public bool Run()
        {
            Debug.WriteLine("Starting", "SinterConsumerRun.Run");
            // RESET ALL instance variables except sim
            bool prevJobIsTerminated = isTerminated;
            isTerminated = false;
            isSetup = false;
            running = false;
            var job = GetNextJob();
            this.job = job;

            if (job == null)
            {
                Debug.WriteLine(DateTime.Now.ToString() + " - No submitted jobs in queue",
                    "SinterConsumerRun.Run");
                return false;
            }

            visible = job.Visible;

            IProcess process = null;
            if (job.Reset == false && sim != null && prevJobIsTerminated == false
                && RunNoReset(job))
            {
                Debug.WriteLine(String.Format("{0} - Found JOB(noreset): {1}", DateTime.Now.ToString(), job.Id),
                    "SinterConsumerRun.Run");
                return true;
            }

            Debug.WriteLine(String.Format("{0} - Found JOB(reset): {1}", DateTime.Now.ToString(), job.Id),
                "SinterConsumerRun.Run");
            CleanUp();
            //sim = null;
            if (sim != null)
            {
                Debug.WriteLine("3) Simulation is not Null",
                "SinterConsumerRun.Run()");
                sim.closeSim();
                if (sim != null)
                {
                    KillProcessAndChildren(sim.ProcessID);
                }
                sim = null;
                //sim.terminate();
            }

            Debug.WriteLine(String.Format("Setup Sinter Process {0}", job.Id),
                "SinterConsumerRun.Run");
            process = DoSetup(job);

            var maxAttemptsReadSetup = 5;
            DoConfigure(job, process, maxAttemptsReadSetup);
            Debug.WriteLine(String.Format("Move Job {0} to state Running", job.Id),
                "SinterConsumerRun.Run");

            job.Running();

            IDictionary<String, Object> inputDict = null;
            String data = null;
            JObject defaults = null;
            JObject inputs = null;

            // NOTE: Quick Fix. Serializing Dicts into string json
            // and back to JObject because of API change to sinter.
            // Better to refactor interfaces, etc to hand back strings
            try
            {
                inputDict = process.Input;
                data = JsonConvert.SerializeObject(inputDict);
                inputs = JObject.Parse(data);
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                Debug.WriteLine("SqlException: " + ex.Message, "SinterConsumer.Run");
                Debug.WriteLine(ex.StackTrace, "SinterConsumer.Run");
                if (sim != null)
                {
                    Debug.WriteLine("4) Simulation is not Null",
                    "SinterConsumerRun.Run()");
                    sim.closeSim();
                    if (sim != null)
                    {
                        KillProcessAndChildren(sim.ProcessID);
                    }
                    sim = null;
                    //sim.terminate();
                }
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Serializing Dicts into string json Exception Close: " + ex.Message, "SinterConsumer.Run");
                Debug.WriteLine("StackTrace: " + ex.StackTrace, "SinterConsumer.Run");
                if (sim != null)
                {
                    Debug.WriteLine("5) Simulation is not Null",
                    "SinterConsumerRun.Run()");
                    sim.closeSim();
                    if (sim != null)
                    {
                        KillProcessAndChildren(sim.ProcessID);
                    }
                    sim = null;
                    //sim.terminate();
                }
                job.Error("Failed to Create Inputs to Simulation (Bad Simulation defaults or Job inputs)");
                job.Message(ex.StackTrace.ToString());
                throw;
            }

            Debug.WriteLine("Initialize", "SinterConsumer.Run");
            try
            {
                DoInitialize(job, sim, defaults);
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                Debug.WriteLine("SqlException: " + ex.Message, "SinterConsumer.Run");
                Debug.WriteLine(ex.StackTrace, "SinterConsumer.Run");
                if (sim != null)
                {
                    Debug.WriteLine("6) Simulation is not Null",
                    "SinterConsumerRun.Run()");
                    sim.closeSim();
                    if (sim != null)
                    {
                        KillProcessAndChildren(sim.ProcessID);
                    }
                    sim = null;
                    //sim.terminate();
                }
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DoInitialize Exception Close: " + ex.Message, "SinterConsumer.Run");
                Debug.WriteLine("DoInitialize StackTrace: " + ex.StackTrace, "SinterConsumer.Run");
                if (sim != null)
                {
                    Debug.WriteLine("7) Simulation is not Null",
                    "SinterConsumerRun.Run()");
                    sim.closeSim();
                    if (sim != null)
                    {
                        KillProcessAndChildren(sim.ProcessID);
                    }
                    sim = null;
                    //sim.terminate();
                }
                throw;
            }
            Debug.WriteLine(String.Format("Run: ProcessID {0}", sim.ProcessID), "SinterConsumer.Run");
            try
            {
                DoRun(job, sim, defaults, inputs);
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                Debug.WriteLine("SqlException: " + ex.Message, "SinterConsumer.Run");
                Debug.WriteLine(ex.StackTrace, "SinterConsumer.Run");
                if (sim != null)
                {
                    Debug.WriteLine("8) Simulation is not Null",
                    "SinterConsumerRun.Run()");
                    sim.closeSim();
                    if (sim != null)
                    {
                        KillProcessAndChildren(sim.ProcessID);
                    }
                    sim = null;
                    //sim.terminate();
                }
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DoRun Exception Close", "SinterConsumer.Run");
                Debug.WriteLine("DoRun Exception: " + ex.Message, "SinterConsumer.Run");
                Debug.WriteLine(ex.StackTrace, "SinterConsumer.Run");
                job.Error("Exception: " + ex.Message);
                if (sim != null)
                {
                    Debug.WriteLine("9) Simulation is not Null",
                    "SinterConsumerRun.Run()");
                    sim.closeSim();
                    if (sim != null)
                    {
                        KillProcessAndChildren(sim.ProcessID);
                    }
                    sim = null;
                    //sim.terminate();
                }
                throw;
            }

            Debug.WriteLine(String.Format("Finalize: ProcessID {0}", sim.ProcessID), "SinterConsumer.Run");
            try
            {
                DoFinalize(sim, job, process);
                if (sim != null)
                {
                    Debug.WriteLine(String.Format("Done Finalize: ProcessID {0}", sim.ProcessID), "SinterConsumer.Run");
                }
                last_simulation_name = job.SimulationId;
                last_simulation_success = job.IsSuccess();
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                Debug.WriteLine("SqlException: " + ex.Message, "SinterConsumer.Run");
                Debug.WriteLine(ex.StackTrace, "SinterConsumer.Run");
                if (sim != null)
                {
                    Debug.WriteLine("10) Simulation is not Null",
                    "SinterConsumerRun.Run()");
                    sim.closeSim();
                    if (sim != null)
                    {
                        KillProcessAndChildren(sim.ProcessID);
                    }
                    sim = null;
                    //sim.terminate();
                }
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DoFinalize Exception Close", "SinterConsumerRun.Run");
                Debug.WriteLine(ex.Message, "SinterConsumerRun.Run");
                Debug.WriteLine(ex.StackTrace, "SinterConsumerRun.Run");
                job.Error("Exception: " + ex.Message);
                if (sim != null)
                {
                    Debug.WriteLine("11) Simulation is not Null",
                    "SinterConsumerRun.Run()");
                    sim.closeSim();
                    if (sim != null)
                    {
                        KillProcessAndChildren(sim.ProcessID);
                    }
                    sim = null;
                    //sim.terminate();
                }
                throw;
            }

            return true;
        }
    }
}