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
using Turbine.Common;
using Turbine.Consumer.Data.Contract.Behaviors;
using Turbine.Consumer.Data.Contract;


namespace Turbine.Data.Test
{
    public class DataTestScenario
    {
        private static String simulationBackupFile = Properties.Settings.Default.simulationBackup;
        private static String simulationConfigFile = Properties.Settings.Default.sinterConfiguration;
        private static String simulationBackupMD5;
        private static String simulationConfigMD5;


        public static string SimulationBackupMD5 { 
            get { return simulationBackupMD5; }
        }

        public static string SimulationConfigMD5 { 
            get { return simulationConfigMD5; }
        }
        
        //[ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                foreach (User obj in container.Users)
                {
                    container.Users.DeleteObject(obj);
                }
                foreach (Simulation obj in container.Simulations)
                {
                    container.Simulations.DeleteObject(obj);
                }
                foreach (Job obj in container.Jobs)
                {
                    container.Jobs.DeleteObject(obj);
                }
                foreach (AspenProcessError obj in container.AspenProcessErrors)
                {
                    container.AspenProcessErrors.DeleteObject(obj);
                }
                foreach (SinterProcess obj in container.SinterProcesses)
                {
                    container.SinterProcesses.DeleteObject(obj);
                }
                foreach (Consumer obj in container.Consumers)
                {
                    container.Consumers.DeleteObject(obj);
                }
                foreach (Session obj in container.Sessions)
                {
                    container.Sessions.DeleteObject(obj);
                }
                foreach (Message obj in container.Messages)
                {
                    container.Messages.DeleteObject(obj);
                }
                container.SaveChanges();

                byte[] bt = System.Text.Encoding.ASCII.GetBytes(
                    File.ReadAllText(simulationBackupFile)
                    );
                byte[] cb = System.Text.Encoding.ASCII.GetBytes(
                    File.ReadAllText(simulationConfigFile)
                    );

                simulationBackupMD5 = GetMd5Hash(bt);
                simulationConfigMD5 = GetMd5Hash(cb);
            
                var jsonDefaults = @"{""P_abs_top"":15.0,""abs_ht_wash"":5.0,""abs_ht_mid"":10.7,""abs_ic_dT"":-11.3,""P_sol_pump"":30.0,""lean_load"":0.178,""P_regen_top"":21.2,""cond_T_regen"":121.0,""ht_regen"":13.0,""slv_cool_01"":130.0,""lr_rich_T"":207.0,""input_s[0,0]"":134.4,""input_s[0,1]"":16.0,""input_s[0,2]"":959553.0,""input_s[0,3]"":0.071,""input_s[0,4]"":0.0,""input_s[0,5]"":0.0,""input_s[0,6]"":0.211,""input_s[0,7]"":0.0,""input_s[0,8]"":0.0,""input_s[0,9]"":0.0,""input_s[0,10]"":0.0,""input_s[0,11]"":0.0,""input_s[0,12]"":0.0,""input_s[0,13]"":0.0,""input_s[0,14]"":0.0,""input_s[0,15]"":0.718,""input_s[1,0]"":126.0,""input_s[1,1]"":30.0,""input_s[1,2]"":4319500.4116,""input_s[1,3]"":0.66207726067,""input_s[1,4]"":0.28374739743,""input_s[1,5]"":0.0,""input_s[1,6]"":0.0541753419,""input_s[1,7]"":0.0,""input_s[1,8]"":0.0,""input_s[1,9]"":0.0,""input_s[1,10]"":0.0,""input_s[1,11]"":0.0,""input_s[1,12]"":0.0,""input_s[1,13]"":0.0,""input_s[1,14]"":0.0,""input_s[1,15]"":0.0,""input_s[2,0]"":120.0,""input_s[2,1]"":30.0,""input_s[2,2]"":10000.0,""input_s[2,3]"":1.0,""input_s[2,4]"":0.0,""input_s[2,5]"":0.0,""input_s[2,6]"":0.0,""input_s[2,7]"":0.0,""input_s[2,8]"":0.0,""input_s[2,9]"":0.0,""input_s[2,10]"":0.0,""input_s[2,11]"":0.0,""input_s[2,12]"":0.0,""input_s[2,13]"":0.0,""input_s[2,14]"":0.0,""input_s[2,15]"":0.0,""eq_par[0,0]"":0.7996,""eq_par[0,1]"":-8094.81,""eq_par[0,2]"":0.0,""eq_par[0,3]"":-0.007484,""eq_par[1,0]"":98.566,""eq_par[1,1]"":1353.8,""eq_par[1,2]"":-14.3043,""eq_par[1,3]"":0.0,""eq_par[2,0]"":216.049,""eq_par[2,1]"":-12431.7,""eq_par[2,2]"":-35.4819,""eq_par[2,3]"":0.0,""eq_par[3,0]"":1.282562,""eq_par[3,1]"":-3456.179,""eq_par[3,2]"":0.0,""eq_par[3,3]"":0.0,""eq_par[4,0]"":132.899,""eq_par[4,1]"":-13445.9,""eq_par[4,2]"":-22.4773,""eq_par[4,3]"":0.0}";
                System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
                Byte[] defaultBytes = encoding.GetBytes(jsonDefaults);

                User u = null;
                Simulation s = null;
                Job job = null;
                SinterProcess p = null;
                Session session = null;

                //var consumer = new JobConsumer();
                //consumer.guid = Guid.NewGuid();
                //consumer.hostname = "test.host";
                //container.Consumers.AddObject(consumer);
                //container.SaveChanges();

                IConsumerRegistrationContract reg = new ConsumerRegistrationContract();
                reg.Register();

                IConsumerContext ctx = AppUtility.GetConsumerContext();

                JobConsumer consumer = (JobConsumer)container.Consumers.First<Consumer>(r => r.guid == ctx.Id);

                for (int i = 1; i < 11; i++)
                {
                    Debug.WriteLine("{0}", i);
                    u = new User() { Name = "test" + i, Token = "qwerty" };
                    container.Users.AddObject(u);
                    s = new Simulation()
                    {
                        Name = "testSim" + i,
                        Backup = bt,
                        Configuration = cb,
                        Defaults = defaultBytes,
                        User = u
                    };
                    session = new Session();
                    var guid = Guid.NewGuid();
                    Debug.WriteLine("GUID: " + guid);
                    session.guid = guid;
                    session.Create = DateTime.UtcNow;
                    session.User = u;
                    container.Sessions.AddObject(session);

                    p = new SinterProcess();
                    job = new Job()
                    {
                        State = "create",
                        Create = DateTime.UtcNow,
                        Simulation = s,
                        User = u,
                        Process = p,
                        Session = session,
                        JobConsumer = consumer
                    };
                    var msg = new Message();
                    msg.Value = "test harness";
                    job.Messages.Add(msg);

                    for (int j = 1; j < 4; j++)
                    {
                        p.Errors.Add(new AspenProcessError()
                        {
                            Name = String.Format("Block{0}{1}", i, j),
                            Type = "block",
                            Error = "TEST" + i
                        });
                    }
                }
                container.SaveChanges();
            }
        }
        // Hash an input string and return the hash as
        // a 32 character hexadecimal string.
        // http://msdn.microsoft.com/en-us/library/s02tk69a.aspx
        public static string GetMd5Hash(byte[] input)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(input);

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
}
