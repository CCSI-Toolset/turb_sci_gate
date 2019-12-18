using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Data.Contract.Behaviors;
using System.IO;
using Turbine.Consumer.SimSinter;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using Turbine.Consumer.Data.Contract.Behaviors;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using sinter;
using System.Management;

namespace Turbine.Consumer.AspenTech
{
    /// <summary>
    /// AspenSinterConsumer
    /// </summary>
    public class AspenSinterConsumer : Turbine.Consumer.SimSinter.SinterConsumerRun
    {
        private string applicationName = "";

        public AspenSinterConsumer()
        {
            applications = new string[] { "ACM", "AspenPlus" };
            consumerId = Guid.NewGuid();
        }

        /// <summary>
        /// Cleans up any processes
        /// </summary>
        public override void CleanUp()
        {
            /*
            if (sim != null && sim.GetType() == typeof(sinter.sinter_SimACM) && ((sinter.sinter_SimACM)sim).closed == false)
            {
                Debug.WriteLine("Close ACM simulation", GetType());
                sim.closeSim();
            }
             */

            if (sim != null)
            {
                Debug.WriteLine("Cleanup: Close simulation", GetType());
                sim.closeSim();
                Debug.WriteLine("Cleanup: Finished Close simulation", GetType());
            }
        }

        /// <summary>
        /// copyFilesToDisk copies files from data contract to working directory.  Assumes the sinter configuration file
        /// is named 'configuration' in the job description.  Currently handles all the mapping from 'resource' names 
        /// specified in the database schema to file names.
        /// </summary>
        /// <param name="job"></param>
        protected override void copyFilesToDisk(IJobConsumerContract job)
        {
            string cacheDir = Path.Combine(AppUtility.GetAppContext().BaseWorkingDirectory, job.SimulationId.ToString());
            Directory.CreateDirectory(cacheDir);
            // NOTE: Aspen implementations only
            // Find 'aspenfile'
            // configuration file

            var input_files = job.GetSimulationInputFiles();

            SimpleFile config = input_files.Single(f => f.name == "configuration");
            string content = System.Text.Encoding.ASCII.GetString(config.content);

            var cacheFile = Path.Combine(cacheDir, configFileName);
            var filepath = Path.Combine(job.Process.WorkingDirectory, configFileName);
            File.WriteAllBytes(cacheFile, config.content);
            File.Copy(cacheFile, filepath);

            Dictionary<String, Object> jsonConfig = JsonConvert.DeserializeObject<Dictionary<String, Object>>(content);
            object file_version;
            string aspenfilename;
            // Handling the new format of SinterConfig file
            if (jsonConfig.TryGetValue("filetype-version", out file_version) && (double)file_version >= 0.3)
            {
                Debug.WriteLine("file_version: " + ((double)file_version).ToString(), "AspenSinterConsumer.copyFilesToDisk");
                var aspenfileObject = JObject.FromObject(jsonConfig["model"]);
                aspenfilename = (String)aspenfileObject["file"];
            }
            else
            {
                Debug.WriteLine("Key 'filetype-version' wasn't specified or < 0.3", "AspenSinterConsumer.copyFilesToDisk");
                aspenfilename = (String)jsonConfig["aspenfile"];
            }

            SimpleFile aspenfile = input_files.Single(g => g.name == "aspenfile");
            cacheFile = Path.Combine(cacheDir, aspenfilename);
            filepath = Path.Combine(job.Process.WorkingDirectory, aspenfilename);
            File.WriteAllBytes(cacheFile, aspenfile.content);
            File.Copy(cacheFile, filepath);

            foreach (SimpleFile sf in input_files)
            {
                if (sf.name == "configuration" || sf.name == "aspenfile")
                    continue;
                cacheFile = Path.Combine(cacheDir, sf.name);
                filepath = Path.Combine(job.Process.WorkingDirectory, sf.name);
                try
                {
                    File.WriteAllBytes(cacheFile, sf.content);
                }
                catch (DirectoryNotFoundException)
                {
                    string[] dirs = cacheFile.Split('/');
                    string[] d = dirs.Take<string>(dirs.Length - 1).ToArray<string>();
                    string dirpath = Path.Combine(d);
                    Debug.WriteLine(String.Format("copyFilesToDisk: create cache directory {0}", dirpath), GetType().Name);
                    Directory.CreateDirectory(dirpath);
                    File.WriteAllBytes(cacheFile, sf.content);
                }
                try
                {
                    File.Copy(cacheFile, filepath, true);
                }
                catch (DirectoryNotFoundException)
                {
                    string[] dirs = filepath.Split('/');
                    string[] d = dirs.Take<string>(dirs.Length - 1).ToArray<string>();
                    string dirpath = Path.Combine(d);
                    Debug.WriteLine(String.Format("copyFilesToDisk: create directory {0}", dirpath), GetType().Name);
                    Directory.CreateDirectory(dirpath);
                    File.Copy(cacheFile, filepath);
                }
            }
        }

        protected override bool IsSupported(IJobConsumerContract job)
        {
            return (job.ApplicationName == "ACM" || job.ApplicationName == "AspenPlus");
        }

        /// <summary>
        ///  InitializeSinter: MUST be called before openSim (working dir and setupFile must be done).
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="workingDir"></param>
        /// <param name="config"></param>
        protected override void InitializeSinter(IJobConsumerContract job, string workingDir, string config)
        {
            applicationName = job.ApplicationName;
            if (job.ApplicationName == "ACM")
            {
                sim = new sinter_SimACM();
                sim.setupFile = new sinter_JsonSetupFile();
            }
            else if (job.ApplicationName == "AspenPlus")
            {
                sim = new sinter_SimAspen();
                sim.setupFile = new sinter_JsonSetupFile();
            }
            else
            {
                throw new Exception(String.Format("Unsupported Application Type: {0}",job.ApplicationName));
            }

            sim.workingDir = workingDir;
            sim.setupFile.parseFile(config);
        }


        /// <summary>
        /// RunNoReset doesn't close underlying COM object, doesn't change Sinter working directory. 
        /// Grabs new inputs and sends them via sinter, and runs again.
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        protected override bool RunNoReset(IJobConsumerContract job)
        {
            if (job.SimulationId != last_simulation_name)
                return false;
            
            if ((sim.GetType() == typeof(sinter_SimACM)) && ((sinter_SimACM)sim).closed)
                return false;
            if ((sim.GetType() == typeof(sinter_SimAspen))) {
                // hack to see whether Aspen has already been initialized
                // should implement an interface "closed"
                try
                {
                    bool test = ((sinter_SimAspen)sim).Vis;
                }
                catch (NullReferenceException)
                {
                    return false;
                }
            }

            if (last_simulation_success == false)
                return false;

            IProcess process = null;
            Debug.WriteLine(String.Format("Reusing Sinter({0}) : Job {1}", job.ApplicationName, job.Id),
                "AspenSinterConsumer.RunNoReset");

            try
            {
                process = job.Setup();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "AspenSinterConsumer.RunNoReset.DoSetup");
                Debug.WriteLine(ex.StackTrace, "AspenSinterConsumer.RunNoReset.DoSetup");
                throw;
            }

            job.Message("Setup: Reusing Sinter ACM");
            Debug.WriteLine(String.Format("Move Job {0} to state Running", job.Id),
                "AspenSinterConsumer.RunNoReset");

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
            catch (Exception ex)
            {
                sim.closeSim();
                job.Error("Failed to Create Inputs to Simulation (Bad Simulation defaults or Job inputs)");
                job.Message(ex.StackTrace.ToString());
                throw;
            }

            Debug.WriteLine("Run", "AspenSinterConsumer.RunNoReset");
            try
            {
                DoRun(job, sim, defaults, inputs);
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                Debug.WriteLine("SqlException: " + ex.Message, "AspenSinterConsumer.RunNoReset");
                Debug.WriteLine(ex.StackTrace, "AspenSinterConsumer.RunNoReset");
                sim.closeSim();
                sim = null;
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DoRun Exception Close", "AspenSinterConsumer.RunNoReset");
                //if (CheckTerminate() == false)
                if (isTerminated == false )
                {
                    job.Error("Exception: " + ex.Message);
                    sim.closeSim();
                }
                sim = null;
                throw;
            }

            Debug.WriteLine("Finalize", "AspenSinterConsumer.RunNoReset");
            try
            {
                DoFinalize(sim, job, process);
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                Debug.WriteLine("SqlException: " + ex.Message, "AspenSinterConsumer.RunNoReset");
                Debug.WriteLine(ex.StackTrace, "AspenSinterConsumer.Run");
                sim.closeSim();
                sim = null;
                throw;
            }
            catch (DbEntityValidationException dbEx)
            {
                Debug.WriteLine("DbEntityValidationException: " + dbEx.Message, "AspenSinterConsumer.RunNoReset");
                Debug.WriteLine(dbEx.StackTrace, "AspenSinterConsumer.RunNoReset");

                var msg = String.Empty;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        msg += String.Format("Property: {0} Error: {1}",
                                                validationError.PropertyName,
                                                validationError.ErrorMessage);
                        Debug.WriteLine(msg, "AspenSinterConsumer.RunNoReset");
                        Trace.TraceInformation("Property: {0} Error: {1}",
                                                validationError.PropertyName,
                                                validationError.ErrorMessage);
                        msg += ". \n";
                    }
                }

                job.Error("Exception: " + msg);
                sim.closeSim();
                sim = null;
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DoFinalize Exception Close", "AspenSinterConsumer.RunNoReset");
                job.Error("Exception: " + ex.Message);
                sim.closeSim();
                sim = null;
                throw;
            }
            return true;
        }
    }
}