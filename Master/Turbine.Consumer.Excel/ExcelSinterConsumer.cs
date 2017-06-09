using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.Entity.Validation;
using Turbine.Consumer.Data.Contract.Behaviors;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Turbine.Data.Contract.Behaviors;

namespace Turbine.Consumer.Excel
{
    public class ExcelSinterConsumer : Turbine.Consumer.SimSinter.SinterConsumerRun
    {
        public ExcelSinterConsumer()
        {
            applications = new string[] { "Excel" };
            consumerId = Guid.NewGuid();
        }

        /*protected override void copyFilesToDisk(Data.Contract.Behaviors.IJobConsumerContract job)
        {
            Debug.WriteLine("Copying files to disk", GetType().Name);
            string cacheDir = Path.Combine(AppUtility.GetAppContext().BaseWorkingDirectory, job.SimulationId.ToString());
            Directory.CreateDirectory(cacheDir);

            // NOTE: Excel implementations only
            // Find 'spreadsheet'
            // configuration file
            SimpleFile config = job.GetSimulationInputFiles().Single(f => f.name == "configuration");
            string content = System.Text.Encoding.ASCII.GetString(config.content);

            var cacheFile = Path.Combine(cacheDir, configFileName);
            var filepath = Path.Combine(job.Process.WorkingDirectory, configFileName);
            Debug.WriteLine("filePath: " + filepath, GetType().Name);
            Debug.WriteLine("cacheFile: " + cacheFile, GetType().Name);
            File.WriteAllBytes(cacheFile, config.content);
            File.Copy(cacheFile, filepath);

            Dictionary<String, Object> jsonConfig = JsonConvert.DeserializeObject<Dictionary<String, Object>>(content);
            object file_version;
            string modelfilename;
            // Handling the new format of SinterConfig file
            if (jsonConfig.TryGetValue("filetype-version", out file_version) && (double)file_version >= 0.3)
            {
                Debug.WriteLine("file_version: " + ((double)file_version).ToString(), "ExcelSinterConsumer.copyFilesToDisk");
                var modelfileObject = JObject.FromObject(jsonConfig["model"]);
                modelfilename = (String)modelfileObject["file"];
            }
            else
            {
                Debug.WriteLine("Key 'filetype-version' wasn't specified or < 0.3", "ExcelSinterConsumer.copyFilesToDisk");
                modelfilename = (String)jsonConfig["spreadsheet"];
            }

            SimpleFile model = job.GetSimulationInputFiles().Single(g => g.name == "spreadsheet");
            cacheFile = Path.Combine(cacheDir, modelfilename);
            filepath = Path.Combine(job.Process.WorkingDirectory, modelfilename);
            File.WriteAllBytes(cacheFile, model.content);
            File.Copy(cacheFile, filepath);
        }*/

        protected override void copyFilesToDisk(IJobConsumerContract job)
        {
            string cacheDir = Path.Combine(AppUtility.GetAppContext().BaseWorkingDirectory, job.SimulationId.ToString());
            Directory.CreateDirectory(cacheDir);
            // NOTE: Excel implementations only
            // Find 'spreadsheet'
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
            string excelfilename;
            // Handling the new format of SinterConfig file
            if (jsonConfig.TryGetValue("filetype-version", out file_version) && (double)file_version >= 0.3)
            {
                Debug.WriteLine("file_version: " + ((double)file_version).ToString(), "ExcelSinterConsumer.copyFilesToDisk");
                var excelfileObject = JObject.FromObject(jsonConfig["model"]);
                excelfilename = (String)excelfileObject["file"];
            }
            else
            {
                Debug.WriteLine("Key 'filetype-version' wasn't specified or < 0.3", "ExcelSinterConsumer.copyFilesToDisk");
                excelfilename = (String)jsonConfig["spreadsheet"];
            }

            SimpleFile excelfile = input_files.Single(g => g.name == "spreadsheet");
            cacheFile = Path.Combine(cacheDir, excelfilename);
            filepath = Path.Combine(job.Process.WorkingDirectory, excelfilename);
            File.WriteAllBytes(cacheFile, excelfile.content);
            File.Copy(cacheFile, filepath);

            foreach (SimpleFile sf in input_files)
            {
                if (sf.name == "configuration" || sf.name == "spreadsheet")
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
                    File.Copy(cacheFile, filepath);
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


        protected override void InitializeSinter(Data.Contract.Behaviors.IJobConsumerContract job,
            string workingDir, string config)
        {
            if (IsSupported(job) == false)
            {
                throw new ArgumentException(String.Format("Excel:  Found wrong Application {0}", job.ApplicationName));
            }
            sim = new sinter.sinter_SimExcel();
            sim.setupFile = new sinter.sinter_JsonSetupFile();
            sim.workingDir = workingDir;
            sim.setupFile.parseFile(config);
        }

        public override void CleanUp()
        {
            if (sim == null) return;
            Debug.WriteLine("Close Excel Sinter", GetType().Name);
            try
            {
                sim.closeSim();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CleanUp: Excel Sinter threw exception: " + ex.Message, GetType().Name);
                Debug.WriteLine(ex.StackTrace, GetType().Name);
            }
        }

        protected override bool RunNoReset(Data.Contract.Behaviors.IJobConsumerContract job)
        {
            //throw new NotImplementedException();
            if (job.SimulationId != last_simulation_name)
                return false;

            if ((sim.GetType() == typeof(sinter.sinter_SimExcel)))
            {
                // hack to see whether Excel has already been initialized
                // should implement an interface "closed"
                try
                {
                    bool test = ((sinter.sinter_SimExcel)sim).Vis;
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
                "ExcelSinterConsumer.RunNoReset");

            try
            {
                process = job.Setup();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "ExcelSinterConsumer.RunNoReset.DoSetup");
                Debug.WriteLine(ex.StackTrace, "ExcelSinterConsumer.RunNoReset.DoSetup");
                throw;
            }

            job.Message("Setup: Reusing Sinter Excel");
            Debug.WriteLine(String.Format("Move Job {0} to state Running", job.Id),
                "ExcelSinterConsumer.RunNoReset");

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
                Debug.WriteLine(String.Format("Move Job {0} to state error", job.Id),
                "ExcelSinterConsumer.RunNoReset");
                Debug.WriteLine(ex.Message, "ExcelSinterConsumer.RunNoReset");
                Debug.WriteLine(ex.StackTrace, "ExcelSinterConsumer.RunNoReset");
                sim.closeSim();
                job.Error("Failed to Create Inputs to Simulation (Bad Simulation defaults or Job inputs)");
                job.Message(ex.StackTrace.ToString());
                throw;
            }

            Debug.WriteLine("Run", "ExcelSinterConsumer.RunNoReset");
            try
            {
                DoRun(job, sim, defaults, inputs);
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                Debug.WriteLine("SqlException: " + ex.Message, "ExcelSinterConsumer.RunNoReset");
                Debug.WriteLine(ex.StackTrace, "ExcelSinterConsumer.RunNoReset");
                sim.closeSim();
                sim = null;
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DoRun Exception Close", "ExcelSinterConsumer.RunNoReset");
                Debug.WriteLine(ex.Message, "ExcelSinterConsumer.RunNoReset");
                Debug.WriteLine(ex.StackTrace, "ExcelSinterConsumer.RunNoReset");
                //if (CheckTerminate() == false)
                if (isTerminated == false )
                {
                    job.Error("Exception: " + ex.Message);
                    sim.closeSim();
                }
                sim = null;
                throw;
            }

            Debug.WriteLine("Finalize", "ExcelSinterConsumer.RunNoReset");
            try
            {
                DoFinalize(sim, job, process);
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                Debug.WriteLine("SqlException: " + ex.Message, "ExcelSinterConsumer.RunNoReset");
                Debug.WriteLine(ex.StackTrace, "ExcelSinterConsumer.RunNoReset");
                sim.closeSim();
                sim = null;
                throw;
            }
            catch (DbEntityValidationException dbEx)
            {
                Debug.WriteLine("DbEntityValidationException: " + dbEx.Message, "ExcelSinterConsumer.RunNoReset");
                Debug.WriteLine(dbEx.StackTrace, "ExcelSinterConsumer.RunNoReset");

                var msg = String.Empty;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        msg += String.Format("Property: {0} Error: {1}",
                                                validationError.PropertyName,
                                                validationError.ErrorMessage);
                        Debug.WriteLine(msg, "ExcelSinterConsumer.RunNoReset");
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
                Debug.WriteLine("DoFinalize Exception Close", "ExcelSinterConsumer.RunNoReset");
                Debug.WriteLine(ex.Message, "ExcelSinterConsumer.RunNoReset");
                Debug.WriteLine(ex.StackTrace, "ExcelSinterConsumer.RunNoReset");
                job.Error("Exception: " + ex.Message);
                sim.closeSim();
                sim = null;
                throw;
            }
            return true;
        }

        protected override bool IsSupported(Data.Contract.Behaviors.IJobConsumerContract job)
        {
            return (job.ApplicationName != null && job.ApplicationName.ToLower().Equals("excel"));
        }

        /// <summary>
        /// DoFinalize:  Close the Excel Simulation everytime at the end of a run
        /// </summary>
        /// <param name="stest"></param>
        /// <param name="job"></param>
        /// <param name="process"></param>
        /*protected override void DoFinalize(sinter.ISimulation stest,
             IJobConsumerContract job,
             IProcess process)
        {
            base.DoFinalize(stest, job, process);
            CleanUp();
        }*/
    }
}
