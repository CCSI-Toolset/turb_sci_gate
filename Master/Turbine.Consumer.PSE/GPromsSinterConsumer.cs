using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Turbine.Consumer;
using Turbine.Consumer.Data.Contract.Behaviors;
using Turbine.Consumer.Data.Contract;
using Turbine.Consumer.Contract;
using Turbine.Data.Contract.Behaviors;



namespace Turbine.Consumer.PSE
{
    /// <summary>
    /// AspenSinterConsumer
    /// </summary>
    public class GPromsSinterConsumer : Turbine.Consumer.Contract.SinterConsumer
    {
        protected override void InitializeSinter(IJobConsumerContract job, String workingDir, String config)
        {
            if (IsSupported(job) == false)
            {
                throw new ArgumentException(String.Format("gPROMS:  Found wrong Application {0}", job.ApplicationName));
            }
            sim = new sinter.PSE.sinter_simGPROMS();
            sim.workingDir = workingDir;
            sim.setupFile.parseFile(config);
        }

        public override void CleanUp()
        {
        }

        protected override bool IsSupported(IJobConsumerContract job)
        {
            return (job.ApplicationName != null && job.ApplicationName.ToLower().Equals("gPROMS"));
        }

        protected override string[] SupportedApplications()
        {
            return new string[] { "gPROMS" };
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
            // NOTE: Aspen implementations only
            // Find 'aspenfile'
            // configuration file
            SimpleFile config = job.GetSimulationInputFiles().Single(f => f.name == "configuration");
            string content = System.Text.Encoding.ASCII.GetString(config.content);

            var cacheFile = Path.Combine(cacheDir, configFileName);
            var filepath = Path.Combine(job.Process.WorkingDirectory, configFileName);
            File.WriteAllBytes(cacheFile, config.content);
            File.Copy(cacheFile, filepath);

            Dictionary<String, Object> jsonConfig = JsonConvert.DeserializeObject<Dictionary<String, Object>>(content);
            string modelfilename = (String)jsonConfig["model"];

            SimpleFile model = job.GetSimulationInputFiles().Single(g => g.name == "model");
            cacheFile = Path.Combine(cacheDir, modelfilename);
            filepath = Path.Combine(job.Process.WorkingDirectory, modelfilename);
            File.WriteAllBytes(cacheFile, model.content);
            File.Copy(cacheFile, filepath);
        }

        protected override bool RunNoReset(IJobConsumerContract job)
        {
            throw new NotImplementedException();
        }
    }
}