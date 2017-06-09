using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using Turbine.Data.Contract;
using Turbine.Data.Contract.Behaviors;
using Turbine.Producer.Data.Contract;
using Turbine.Consumer.Data.Contract;
using Turbine.Producer.Data.Contract.Behaviors;
using Turbine.Consumer.Data.Contract.Behaviors;


namespace Turbine.Data.Test
{
    [TestClass]
    public class DataContractTest : DataTestScenario
    {
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            DataTestScenario.ClassInit(context);
        }

        [TestMethod]
        public void TestSimulationContract()
        {
            var simContract = AspenSimulationContract.Get("testSim1"); //testSim1 is the first in Simulations
            var data = new Byte[] {0XAA};    // Just a BS holder value in place of the aspen.bkp file
            //""abs_ht_wash"":5.0, 
            var json = @"{""P_abs_top"":16.0,""abs_ht_mid"":10.7,""abs_ic_dT"":-11.3,""P_sol_pump"":30.0,""lean_load"":0.178,""P_regen_top"":21.2,""cond_T_regen"":121.0,""ht_regen"":13.0,""slv_cool_01"":130.0,""lr_rich_T"":207.0,""reg_dia_in"":17.37,""input_s[0,0]"":134.4,""input_s[0,1]"":16.0,""input_s[0,2]"":959553.0,""input_s[0,3]"":0.071,""input_s[0,4]"":0.0,""input_s[0,5]"":0.0,""input_s[0,6]"":0.211,""input_s[0,7]"":0.0,""input_s[0,8]"":0.0,""input_s[0,9]"":0.0,""input_s[0,10]"":0.0,""input_s[0,11]"":0.0,""input_s[0,12]"":0.0,""input_s[0,13]"":0.0,""input_s[0,14]"":0.0,""input_s[0,15]"":0.718,""input_s[1,0]"":126.0,""input_s[1,1]"":30.0,""input_s[1,2]"":4319500.4116,""input_s[1,3]"":0.66207726067,""input_s[1,4]"":0.28374739743,""input_s[1,5]"":0.0,""input_s[1,6]"":0.0541753419,""input_s[1,7]"":0.0,""input_s[1,8]"":0.0,""input_s[1,9]"":0.0,""input_s[1,10]"":0.0,""input_s[1,11]"":0.0,""input_s[1,12]"":0.0,""input_s[1,13]"":0.0,""input_s[1,14]"":0.0,""input_s[1,15]"":0.0,""input_s[2,0]"":120.0,""input_s[2,1]"":30.0,""input_s[2,2]"":10000.0,""input_s[2,3]"":1.0,""input_s[2,4]"":0.0,""input_s[2,5]"":0.0,""input_s[2,6]"":0.0,""input_s[2,7]"":0.0,""input_s[2,8]"":0.0,""input_s[2,9]"":0.0,""input_s[2,10]"":0.0,""input_s[2,11]"":0.0,""input_s[2,12]"":0.0,""input_s[2,13]"":0.0,""input_s[2,14]"":0.0,""input_s[2,15]"":0.0,""eq_par[0,0]"":0.7996,""eq_par[0,1]"":-8094.81,""eq_par[0,2]"":0.0,""eq_par[0,3]"":-0.007484,""eq_par[1,0]"":98.566,""eq_par[1,1]"":1353.8,""eq_par[1,2]"":-14.3043,""eq_par[1,3]"":0.0,""eq_par[2,0]"":216.049,""eq_par[2,1]"":-12431.7,""eq_par[2,2]"":-35.4819,""eq_par[2,3]"":0.0,""eq_par[3,0]"":1.282562,""eq_par[3,1]"":-3456.179,""eq_par[3,2]"":0.0,""eq_par[3,3]"":0.0,""eq_par[4,0]"":132.899,""eq_par[4,1]"":-13445.9,""eq_par[4,2]"":-22.4773,""eq_par[4,3]"":0.0}";
            var initialize = false;

            Dictionary<string, Object> inputData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Object>>(json);
   

            simContract.UpdateAspenBackup(data);  // Set the aspen.bkp file to BS value (really this should've already been set when the sim was created)

            // Producer: Produces jobs to be run from simulations
            IJobProducerContract producer = simContract.NewJob(Guid.NewGuid(), initialize); // Ask the AspenSimulationContract to create a new job based on the selected simulation
            producer.Submit();  //Basically just sets state to submit
            int jobId = producer.Id;  

            // Consumer: Pops submitted jobs off the 'queue' and runs them
            IJobConsumerContract consumer = new AspenJobConsumerContract() { Id = jobId };
            IProcess process = consumer.Setup();  //Setup the job (change state, copy setup files)  Returns SinterProcessContract
            process.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);  //Sets the dir in the function to MyDocs, but doesn't actually chdir
            //process.Input = inputData.ToArray<string>(); //The varaible inputs are a file?  I actually didn't expect that.

            process.Input = inputData;

            consumer.Running(); //Does some checking and changes state to running.  Run will actually be done outside contract
            consumer.Success(); //Changes state to success. (Other option is error)

            Byte[] d2 = consumer.GetSimulationBackup();  //Final checks to make sure that all kinda worked.
            Assert.IsTrue(data.SequenceEqual(d2));
            process = consumer.Process;
            Assert.IsTrue(inputData.SequenceEqual(process.Input));
        }
    }
}
