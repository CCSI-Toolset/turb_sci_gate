using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using Turbine.Client;
using Turbine.Data.Serialize;
using Newtonsoft.Json;

namespace IntegrationTests
{
    // JobTest
    /// <summary>
    /// Integration tests are against a canned database with
    /// known values.
    /// TODO: 
    ///   1) missing resources
    /// </summary>
    [TestClass]
    public class JobTest
    {
        static string simulation_mea_uq = "mea-uq-billet-1";

        //[TestMethod]
        public void TestGetJob(int jobid)
        {
            String addr = Properties.Settings.Default.jobAddress + jobid;
            Uri uri = new Uri(addr);
            Debug.WriteLine("Address: " + addr, this.GetType());
            SimulationClient cli = new SimulationClient();
            cli.SetBasicAuth(Properties.Settings.Default.username,
                Properties.Settings.Default.password);
            Job job = cli.GetJob(uri);
            int id = Convert.ToInt32(addr.Split('/').Last());
            Assert.AreEqual(id, job.Id);
        }
        [TestMethod]
        public void TestGetAllJobs()
        {
            String addr = Properties.Settings.Default.jobAddress;
            Debug.WriteLine("Address: " + addr, this.GetType());
            SimulationClient cli = new SimulationClient();
            cli.SetBasicAuth(Properties.Settings.Default.username,
                Properties.Settings.Default.password);
            Jobs jobs = cli.GetJobs(new Uri(addr));
            Assert.IsTrue(jobs.Count >= 0);
            Job j ;
            if (jobs.Count > 0)
            {
                j = jobs.First<Job>();
                TestGetJob(j.Id);
            }
        }

        [TestMethod]
        public void TestCreateSubmitJob()
        {
            String addr = Properties.Settings.Default.jobAddress;
            Debug.WriteLine("Address: " + addr, this.GetType());
            SimulationClient cli = new SimulationClient();
            cli.SetBasicAuth(Properties.Settings.Default.username,
                Properties.Settings.Default.password);

            var msg = new Dictionary<String, Object>()
            {
                {"Simulation", simulation_mea_uq},
                {"Inputs", new Dictionary<String,Object>() {
                    {"reactions.input.parameters(4,0)", "1.328578867187e+02"}, 
                    {"absorber.input.main.masstrans.cv", "2.133484375000e-01"}, 
                    {"absorber.input.wash.masstrans.cv", "2.133484375000e-01"}, 
                    {"reactions.input.parameters(1,0)", "9.856565390625e+01"}, 
                    {"reactions.input.parameters(0,0)", "8.745164071484e-01"}
                }},
                {"Session", Guid.NewGuid()}
            };

            Job job = cli.CreateJob(new Uri(addr), msg);
            Assert.AreEqual("create", job.State);

            job = cli.SubmitJob(new Uri(addr), job.Id);
            Assert.AreEqual("submit", job.State);

            int i = 0;
            while (job.State == "submit")
            {
                i++;
                Console.WriteLine("Waiting for Consumer..");
                if (i > 7)
                {
                    cli.CancelJob(new Uri(addr), job.Id);
                    Assert.Fail("Max wait reached job remains in submit state");
                }
                System.Threading.Thread.Sleep(10000);
                job = cli.GetJob(new Uri(addr + job.Id));
            }
            foreach (var state in new string[] { "setup", "running" })
            {
                i = 0;
                while (job.State == state)
                {
                    i++;
                    Assert.IsTrue(i <= 60,
                        String.Format("Max wait reached job remains in {0} state", job.State)
                      );
                    Console.WriteLine("Consumer Got Job:  " + job.State);
                    System.Threading.Thread.Sleep(5000);
                    job = cli.GetJob(new Uri(addr + job.Id));
                }
            }

            // Validate Job Results...
            Assert.AreEqual("success", job.State);
            Assert.AreEqual(8, job.Status);

            Dictionary<string, Object> dict =
                JsonConvert.DeserializeObject<Dictionary<string, Object>>(job.Output);

            Assert.AreEqual(dict["solvent.input.lean_load"], 0.265);
            Assert.AreEqual(dict["solvent.input.massFlow"], 4319500.4116);
            Assert.AreEqual(dict["gas.input.stream(0,0)"], 134.4);
            Assert.AreEqual(dict["gas.input.stream(0,1)"], 16.0);
            Assert.AreEqual(dict["gas.input.stream(0,2)"], 959553.0);
            Assert.AreEqual(dict["gas.input.stream(0,3)"], 0.071);
            Assert.AreEqual(dict["gas.input.stream(0,4)"], 0.0);
            Assert.AreEqual(dict["gas.input.stream(0,5)"], 0.0);
            Assert.AreEqual(dict["gas.input.stream(0,6)"], 0.211);
            Assert.AreEqual(dict["gas.input.stream(0,7)"], 0.718);
            Assert.AreEqual(dict["reactions.input.parameters(0,0)"], 0.8745164071484);
            Assert.AreEqual(dict["reactions.input.parameters(0,1)"], -8094.81);
            Assert.AreEqual(dict["reactions.input.parameters(0,2)"], 0.0);
            Assert.AreEqual(dict["reactions.input.parameters(0,3)"], -0.007484);
            Assert.AreEqual(dict["reactions.input.parameters(1,0)"], 98.56565390625);
            Assert.AreEqual(dict["reactions.input.parameters(1,1)"], 1353.8);
            Assert.AreEqual(dict["reactions.input.parameters(1,2)"], -14.3043);
            Assert.AreEqual(dict["reactions.input.parameters(1,3)"], 0.0);
            Assert.AreEqual(dict["reactions.input.parameters(2,0)"], 216.049);
            Assert.AreEqual(dict["reactions.input.parameters(2,1)"], -12431.7);
            Assert.AreEqual(dict["reactions.input.parameters(2,2)"], -35.4819);
            Assert.AreEqual(dict["reactions.input.parameters(2,3)"], 0.0);
            Assert.AreEqual(dict["reactions.input.parameters(3,0)"], 1.282562);
            Assert.AreEqual(dict["reactions.input.parameters(3,1)"], -3456.179);
            Assert.AreEqual(dict["reactions.input.parameters(3,2)"], 0.0);
            Assert.AreEqual(dict["reactions.input.parameters(3,3)"], 0.0);
            Assert.AreEqual(dict["reactions.input.parameters(4,0)"], 132.8578867187);
            Assert.AreEqual(dict["reactions.input.parameters(4,1)"], -13445.9);
            Assert.AreEqual(dict["reactions.input.parameters(4,2)"], -22.4773);
            Assert.AreEqual(dict["reactions.input.parameters(4,3)"], 0.0);
            Assert.AreEqual(dict["absorber.input.wash.massTrans.CL"], 2.3);
            Assert.AreEqual(dict["absorber.input.wash.massTrans.CV"], 0.2133484375);
            Assert.AreEqual(dict["absorber.input.main.massTrans.CL"], 2.3);
            Assert.AreEqual(dict["absorber.input.main.massTrans.CV"], 0.2133484375);
            Assert.AreEqual(dict["absorber.input.top.P"], 15.0);
            Assert.AreEqual(dict["absorber.input.wash.ht"], 5.0);
            Assert.AreEqual(dict["absorber.input.main.ht"], 12.6);
            Assert.AreEqual(dict["absorber.input.ic.dT"], -17.9);
            Assert.AreEqual(dict["absorber.input.dia"], 26.32);
            Assert.AreEqual(dict["absorber.output.ic.duty"], -59816093.3);
            Assert.AreEqual(dict["absorber.output.capacity"], 0.775799804);
            Assert.AreEqual(dict["absorber.output.dia"], 26.32);
            Assert.AreEqual(dict["absorber.output.profile.TandP(0,0)"], 141.597683);
            Assert.AreEqual(dict["absorber.output.profile.TandP(0,1)"], 141.764559);
            Assert.AreEqual(dict["absorber.output.profile.TandP(0,2)"], 15.0);
            Assert.AreEqual(dict["absorber.output.profile.TandP(1,0)"], 141.827969);
            Assert.AreEqual(dict["absorber.output.profile.TandP(1,1)"], 141.829792);
            Assert.AreEqual(dict["absorber.output.profile.TandP(1,2)"], 15.0049576);
            Assert.AreEqual(dict["absorber.output.profile.TandP(2,0)"], 141.844656);
            Assert.AreEqual(dict["absorber.output.profile.TandP(2,1)"], 141.844704);
            Assert.AreEqual(dict["absorber.output.profile.TandP(2,2)"], 15.009917);
            Assert.AreEqual(dict["absorber.output.profile.TandP(3,0)"], 141.978332);
            Assert.AreEqual(dict["absorber.output.profile.TandP(3,1)"], 141.979304);
            Assert.AreEqual(dict["absorber.output.profile.TandP(3,2)"], 15.0148748);
            Assert.AreEqual(dict["absorber.output.profile.TandP(4,0)"], 134.342593);
            Assert.AreEqual(dict["absorber.output.profile.TandP(4,1)"], 146.865808);
            Assert.AreEqual(dict["absorber.output.profile.TandP(4,2)"], 15.0198286);
            Assert.AreEqual(dict["absorber.output.profile.TandP(5,0)"], 142.128677);
            Assert.AreEqual(dict["absorber.output.profile.TandP(5,1)"], 150.946921);
            Assert.AreEqual(dict["absorber.output.profile.TandP(5,2)"], 15.0220426);
            Assert.AreEqual(dict["absorber.output.profile.TandP(6,0)"], 148.422859);
            Assert.AreEqual(dict["absorber.output.profile.TandP(6,1)"], 153.735156);
            Assert.AreEqual(dict["absorber.output.profile.TandP(6,2)"], 15.0244809);
            Assert.AreEqual(dict["absorber.output.profile.TandP(7,0)"], 152.823783);
            Assert.AreEqual(dict["absorber.output.profile.TandP(7,1)"], 155.368976);
            Assert.AreEqual(dict["absorber.output.profile.TandP(7,2)"], 15.027437);
            Assert.AreEqual(dict["absorber.output.profile.TandP(8,0)"], 155.520751);
            Assert.AreEqual(dict["absorber.output.profile.TandP(8,1)"], 156.110675);
            Assert.AreEqual(dict["absorber.output.profile.TandP(8,2)"], 15.0308568);
            Assert.AreEqual(dict["absorber.output.profile.TandP(9,0)"], 156.934547);
            Assert.AreEqual(dict["absorber.output.profile.TandP(9,1)"], 156.220656);
            Assert.AreEqual(dict["absorber.output.profile.TandP(9,2)"], 15.0346235);
            Assert.AreEqual(dict["absorber.output.profile.TandP(10,0)"], 157.470065);
            Assert.AreEqual(dict["absorber.output.profile.TandP(10,1)"], 155.903946);
            Assert.AreEqual(dict["absorber.output.profile.TandP(10,2)"], 15.0386077);
            Assert.AreEqual(dict["absorber.output.profile.TandP(11,0)"], 157.433097);
            Assert.AreEqual(dict["absorber.output.profile.TandP(11,1)"], 155.303144);
            Assert.AreEqual(dict["absorber.output.profile.TandP(11,2)"], 15.0427013);
            Assert.AreEqual(dict["absorber.output.profile.TandP(12,0)"], 157.029937);
            Assert.AreEqual(dict["absorber.output.profile.TandP(12,1)"], 154.509919);
            Assert.AreEqual(dict["absorber.output.profile.TandP(12,2)"], 15.0468263);
            Assert.AreEqual(dict["absorber.output.profile.TandP(13,0)"], 156.39133);
            Assert.AreEqual(dict["absorber.output.profile.TandP(13,1)"], 153.57957);
            Assert.AreEqual(dict["absorber.output.profile.TandP(13,2)"], 15.0509315);
            Assert.AreEqual(dict["absorber.output.profile.TandP(14,0)"], 155.596701);
            Assert.AreEqual(dict["absorber.output.profile.TandP(14,1)"], 152.543034);
            Assert.AreEqual(dict["absorber.output.profile.TandP(14,2)"], 15.0549848);
            Assert.AreEqual(dict["absorber.output.profile.TandP(15,0)"], 154.69233);
            Assert.AreEqual(dict["absorber.output.profile.TandP(15,1)"], 151.415288);
            Assert.AreEqual(dict["absorber.output.profile.TandP(15,2)"], 15.0589664);
            Assert.AreEqual(dict["absorber.output.profile.TandP(16,0)"], 153.703501);
            Assert.AreEqual(dict["absorber.output.profile.TandP(16,1)"], 150.200767);
            Assert.AreEqual(dict["absorber.output.profile.TandP(16,2)"], 15.0628643);
            Assert.AreEqual(dict["absorber.output.profile.TandP(17,0)"], 152.642144);
            Assert.AreEqual(dict["absorber.output.profile.TandP(17,1)"], 148.896658);
            Assert.AreEqual(dict["absorber.output.profile.TandP(17,2)"], 15.0666708);
            Assert.AreEqual(dict["absorber.output.profile.TandP(18,0)"], 151.511453);
            Assert.AreEqual(dict["absorber.output.profile.TandP(18,1)"], 147.494803);
            Assert.AreEqual(dict["absorber.output.profile.TandP(18,2)"], 15.0703806);
            Assert.AreEqual(dict["absorber.output.profile.TandP(19,0)"], 150.308545);
            Assert.AreEqual(dict["absorber.output.profile.TandP(19,1)"], 145.982685);
            Assert.AreEqual(dict["absorber.output.profile.TandP(19,2)"], 15.0739898);
            Assert.AreEqual(dict["absorber.output.profile.TandP(20,0)"], 149.025841);
            Assert.AreEqual(dict["absorber.output.profile.TandP(20,1)"], 144.343868);
            Assert.AreEqual(dict["absorber.output.profile.TandP(20,2)"], 15.077495);
            Assert.AreEqual(dict["absorber.output.profile.TandP(21,0)"], 147.651607);
            Assert.AreEqual(dict["absorber.output.profile.TandP(21,1)"], 142.558069);
            Assert.AreEqual(dict["absorber.output.profile.TandP(21,2)"], 15.0808928);
            Assert.AreEqual(dict["absorber.output.profile.TandP(22,0)"], 146.169844);
            Assert.AreEqual(dict["absorber.output.profile.TandP(22,1)"], 140.601056);
            Assert.AreEqual(dict["absorber.output.profile.TandP(22,2)"], 15.0841795);
            Assert.AreEqual(dict["absorber.output.profile.TandP(23,0)"], 144.559637);
            Assert.AreEqual(dict["absorber.output.profile.TandP(23,1)"], 138.444492);
            Assert.AreEqual(dict["absorber.output.profile.TandP(23,2)"], 15.087351);
            Assert.AreEqual(dict["absorber.output.profile.TandP(24,0)"], 142.793905);
            Assert.AreEqual(dict["absorber.output.profile.TandP(24,1)"], 136.055918);
            Assert.AreEqual(dict["absorber.output.profile.TandP(24,2)"], 15.0904025);
            Assert.AreEqual(dict["absorber.output.profile.TandP(25,0)"], 140.837413);
            Assert.AreEqual(dict["absorber.output.profile.TandP(25,1)"], 133.399096);
            Assert.AreEqual(dict["absorber.output.profile.TandP(25,2)"], 15.0933284);
            Assert.AreEqual(dict["absorber.output.profile.TandP(26,0)"], 138.643717);
            Assert.AreEqual(dict["absorber.output.profile.TandP(26,1)"], 130.435164);
            Assert.AreEqual(dict["absorber.output.profile.TandP(26,2)"], 15.0961222);
            Assert.AreEqual(dict["absorber.output.profile.TandP(27,0)"], 123.938349);
            Assert.AreEqual(dict["absorber.output.profile.TandP(27,1)"], 127.125313);
            Assert.AreEqual(dict["absorber.output.profile.TandP(27,2)"], 15.0987765);
            Assert.AreEqual(dict["absorber.output.profile.TandP(28,0)"], 126.000636);
            Assert.AreEqual(dict["absorber.output.profile.TandP(28,1)"], 128.279357);
            Assert.AreEqual(dict["absorber.output.profile.TandP(28,2)"], 15.1013225);
            Assert.AreEqual(dict["absorber.output.profile.TandP(29,0)"], 127.270418);
            Assert.AreEqual(dict["absorber.output.profile.TandP(29,1)"], 129.117581);
            Assert.AreEqual(dict["absorber.output.profile.TandP(29,2)"], 15.1040461);
            Assert.AreEqual(dict["absorber.output.profile.TandP(30,0)"], 127.997037);
            Assert.AreEqual(dict["absorber.output.profile.TandP(30,1)"], 129.815586);
            Assert.AreEqual(dict["absorber.output.profile.TandP(30,2)"], 15.106888);
            Assert.AreEqual(dict["absorber.output.profile.TandP(31,0)"], 128.350728);
            Assert.AreEqual(dict["absorber.output.profile.TandP(31,1)"], 130.525711);
            Assert.AreEqual(dict["absorber.output.profile.TandP(31,2)"], 15.1098051);
            Assert.AreEqual(dict["absorber.output.profile.TandP(32,0)"], 128.442493);
            Assert.AreEqual(dict["absorber.output.profile.TandP(32,1)"], 131.398159);
            Assert.AreEqual(dict["absorber.output.profile.TandP(32,2)"], 15.1127673);
            Assert.AreEqual(dict["absorber.output.profile.TandP(33,0)"], 128.341542);
            Assert.AreEqual(dict["absorber.output.profile.TandP(33,1)"], 132.603306);
            Assert.AreEqual(dict["absorber.output.profile.TandP(33,2)"], 15.1157548);
            Assert.AreEqual(dict["absorber.output.profile.CO2(0,0)"], 0.0307164007);
            Assert.AreEqual(dict["absorber.output.profile.CO2(0,1)"], 0.2030085);
            Assert.AreEqual(dict["absorber.output.profile.CO2(1,0)"], 0.0307055539);
            Assert.AreEqual(dict["absorber.output.profile.CO2(1,1)"], 0.203279174);
            Assert.AreEqual(dict["absorber.output.profile.CO2(2,0)"], 0.0307055874);
            Assert.AreEqual(dict["absorber.output.profile.CO2(2,1)"], 0.203277499);
            Assert.AreEqual(dict["absorber.output.profile.CO2(3,0)"], 0.0307077111);
            Assert.AreEqual(dict["absorber.output.profile.CO2(3,1)"], 0.203233468);
            Assert.AreEqual(dict["absorber.output.profile.CO2(4,0)"], 0.0307898154);
            Assert.AreEqual(dict["absorber.output.profile.CO2(4,1)"], 0.201597016);
            Assert.AreEqual(dict["absorber.output.profile.CO2(5,0)"], 0.0391570209);
            Assert.AreEqual(dict["absorber.output.profile.CO2(5,1)"], 0.222882846);
            Assert.AreEqual(dict["absorber.output.profile.CO2(6,0)"], 0.0477685062);
            Assert.AreEqual(dict["absorber.output.profile.CO2(6,1)"], 0.239363941);
            Assert.AreEqual(dict["absorber.output.profile.CO2(7,0)"], 0.0554587654);
            Assert.AreEqual(dict["absorber.output.profile.CO2(7,1)"], 0.250098247);
            Assert.AreEqual(dict["absorber.output.profile.CO2(8,0)"], 0.061742411);
            Assert.AreEqual(dict["absorber.output.profile.CO2(8,1)"], 0.255547121);
            Assert.AreEqual(dict["absorber.output.profile.CO2(9,0)"], 0.0667527283);
            Assert.AreEqual(dict["absorber.output.profile.CO2(9,1)"], 0.256886112);
            Assert.AreEqual(dict["absorber.output.profile.CO2(10,0)"], 0.0708165874);
            Assert.AreEqual(dict["absorber.output.profile.CO2(10,1)"], 0.255347177);
            Assert.AreEqual(dict["absorber.output.profile.CO2(11,0)"], 0.0742358886);
            Assert.AreEqual(dict["absorber.output.profile.CO2(11,1)"], 0.251910191);
            Assert.AreEqual(dict["absorber.output.profile.CO2(12,0)"], 0.0772357948);
            Assert.AreEqual(dict["absorber.output.profile.CO2(12,1)"], 0.24725292);
            Assert.AreEqual(dict["absorber.output.profile.CO2(13,0)"], 0.0799721555);
            Assert.AreEqual(dict["absorber.output.profile.CO2(13,1)"], 0.241806259);
            Assert.AreEqual(dict["absorber.output.profile.CO2(14,0)"], 0.0825505358);
            Assert.AreEqual(dict["absorber.output.profile.CO2(14,1)"], 0.235827161);
            Assert.AreEqual(dict["absorber.output.profile.CO2(15,0)"], 0.0850429752);
            Assert.AreEqual(dict["absorber.output.profile.CO2(15,1)"], 0.229458117);
            Assert.AreEqual(dict["absorber.output.profile.CO2(16,0)"], 0.0875001524);
            Assert.AreEqual(dict["absorber.output.profile.CO2(16,1)"], 0.222768239);
            Assert.AreEqual(dict["absorber.output.profile.CO2(17,0)"], 0.0899596692);
            Assert.AreEqual(dict["absorber.output.profile.CO2(17,1)"], 0.215779321);
            Assert.AreEqual(dict["absorber.output.profile.CO2(18,0)"], 0.0924516099);
            Assert.AreEqual(dict["absorber.output.profile.CO2(18,1)"], 0.208481331);
            Assert.AreEqual(dict["absorber.output.profile.CO2(19,0)"], 0.0950023438);
            Assert.AreEqual(dict["absorber.output.profile.CO2(19,1)"], 0.200840917);
            Assert.AreEqual(dict["absorber.output.profile.CO2(20,0)"], 0.0976372647);
            Assert.AreEqual(dict["absorber.output.profile.CO2(20,1)"], 0.192805231);
            Assert.AreEqual(dict["absorber.output.profile.CO2(21,0)"], 0.100382967);
            Assert.AreEqual(dict["absorber.output.profile.CO2(21,1)"], 0.184302447);
            Assert.AreEqual(dict["absorber.output.profile.CO2(22,0)"], 0.103269212);
            Assert.AreEqual(dict["absorber.output.profile.CO2(22,1)"], 0.1752396);
            Assert.AreEqual(dict["absorber.output.profile.CO2(23,0)"], 0.106331006);
            Assert.AreEqual(dict["absorber.output.profile.CO2(23,1)"], 0.165497784);
            Assert.AreEqual(dict["absorber.output.profile.CO2(24,0)"], 0.109611093);
            Assert.AreEqual(dict["absorber.output.profile.CO2(24,1)"], 0.154924227);
            Assert.AreEqual(dict["absorber.output.profile.CO2(25,0)"], 0.11316325);
            Assert.AreEqual(dict["absorber.output.profile.CO2(25,1)"], 0.143320051);
            Assert.AreEqual(dict["absorber.output.profile.CO2(26,0)"], 0.117056967);
            Assert.AreEqual(dict["absorber.output.profile.CO2(26,1)"], 0.13042156);
            Assert.AreEqual(dict["absorber.output.profile.CO2(27,0)"], 0.121384413);
            Assert.AreEqual(dict["absorber.output.profile.CO2(27,1)"], 0.115871241);
            Assert.AreEqual(dict["absorber.output.profile.CO2(28,0)"], 0.127091838);
            Assert.AreEqual(dict["absorber.output.profile.CO2(28,1)"], 0.118863554);
            Assert.AreEqual(dict["absorber.output.profile.CO2(29,0)"], 0.130971522);
            Assert.AreEqual(dict["absorber.output.profile.CO2(29,1)"], 0.120383961);
            Assert.AreEqual(dict["absorber.output.profile.CO2(30,0)"], 0.133666858);
            Assert.AreEqual(dict["absorber.output.profile.CO2(30,1)"], 0.120787109);
            Assert.AreEqual(dict["absorber.output.profile.CO2(31,0)"], 0.135616191);
            Assert.AreEqual(dict["absorber.output.profile.CO2(31,1)"], 0.120318938);
            Assert.AreEqual(dict["absorber.output.profile.CO2(32,0)"], 0.137114954);
            Assert.AreEqual(dict["absorber.output.profile.CO2(32,1)"], 0.11912235);
            Assert.AreEqual(dict["absorber.output.profile.CO2(33,0)"], 0.138364949);
            Assert.AreEqual(dict["absorber.output.profile.CO2(33,1)"], 0.117251177);
            Assert.AreEqual(dict["absorber.output.profile.kv(0,0)"], 0.0);
            Assert.AreEqual(dict["absorber.output.profile.kv(1,0)"], 0.0);
            Assert.AreEqual(dict["absorber.output.profile.kv(2,0)"], 0.0);
            Assert.AreEqual(dict["absorber.output.profile.kv(3,0)"], 0.0);
            Assert.AreEqual(dict["absorber.output.profile.kv(4,0)"], 11519.2795);
            Assert.AreEqual(dict["absorber.output.profile.kv(5,0)"], 11896.8063);
            Assert.AreEqual(dict["absorber.output.profile.kv(6,0)"], 12238.2616);
            Assert.AreEqual(dict["absorber.output.profile.kv(7,0)"], 12507.2302);
            Assert.AreEqual(dict["absorber.output.profile.kv(8,0)"], 12692.4041);
            Assert.AreEqual(dict["absorber.output.profile.kv(9,0)"], 12805.1159);
            Assert.AreEqual(dict["absorber.output.profile.kv(10,0)"], 12864.545);
            Assert.AreEqual(dict["absorber.output.profile.kv(11,0)"], 12888.2089);
            Assert.AreEqual(dict["absorber.output.profile.kv(12,0)"], 12889.0366);
            Assert.AreEqual(dict["absorber.output.profile.kv(13,0)"], 12875.626);
            Assert.AreEqual(dict["absorber.output.profile.kv(14,0)"], 12853.3713);
            Assert.AreEqual(dict["absorber.output.profile.kv(15,0)"], 12825.5306);
            Assert.AreEqual(dict["absorber.output.profile.kv(16,0)"], 12794.0072);
            Assert.AreEqual(dict["absorber.output.profile.kv(17,0)"], 12759.864);
            Assert.AreEqual(dict["absorber.output.profile.kv(18,0)"], 12723.6451);
            Assert.AreEqual(dict["absorber.output.profile.kv(19,0)"], 12685.5697);
            Assert.AreEqual(dict["absorber.output.profile.kv(20,0)"], 12645.6471);
            Assert.AreEqual(dict["absorber.output.profile.kv(21,0)"], 12603.7409);
            Assert.AreEqual(dict["absorber.output.profile.kv(22,0)"], 12559.6038);
            Assert.AreEqual(dict["absorber.output.profile.kv(23,0)"], 12512.8925);
            Assert.AreEqual(dict["absorber.output.profile.kv(24,0)"], 12463.1706);
            Assert.AreEqual(dict["absorber.output.profile.kv(25,0)"], 12409.9037);
            Assert.AreEqual(dict["absorber.output.profile.kv(26,0)"], 12352.4511);
            Assert.AreEqual(dict["absorber.output.profile.kv(27,0)"], 12546.3124);
            Assert.AreEqual(dict["absorber.output.profile.kv(28,0)"], 12658.2131);
            Assert.AreEqual(dict["absorber.output.profile.kv(29,0)"], 12728.815);
            Assert.AreEqual(dict["absorber.output.profile.kv(30,0)"], 12772.1985);
            Assert.AreEqual(dict["absorber.output.profile.kv(31,0)"], 12797.8207);
            Assert.AreEqual(dict["absorber.output.profile.kv(32,0)"], 12811.8862);
            Assert.AreEqual(dict["absorber.output.profile.kv(33,0)"], 12818.4364);
            Assert.AreEqual(dict["absorber.output.profile.kl(0,0)"], 6710.46166);
            Assert.AreEqual(dict["absorber.output.profile.kl(1,0)"], 6719.10062);
            Assert.AreEqual(dict["absorber.output.profile.kl(2,0)"], 6704.05042);
            Assert.AreEqual(dict["absorber.output.profile.kl(3,0)"], 6124.74362);
            Assert.AreEqual(dict["absorber.output.profile.kl(4,0)"], 130233.286);
            Assert.AreEqual(dict["absorber.output.profile.kl(5,0)"], 136618.312);
            Assert.AreEqual(dict["absorber.output.profile.kl(6,0)"], 141723.379);
            Assert.AreEqual(dict["absorber.output.profile.kl(7,0)"], 145216.205);
            Assert.AreEqual(dict["absorber.output.profile.kl(8,0)"], 147256.415);
            Assert.AreEqual(dict["absorber.output.profile.kl(9,0)"], 148203.579);
            Assert.AreEqual(dict["absorber.output.profile.kl(10,0)"], 148406.278);
            Assert.AreEqual(dict["absorber.output.profile.kl(11,0)"], 148128.198);
            Assert.AreEqual(dict["absorber.output.profile.kl(12,0)"], 147546.664);
            Assert.AreEqual(dict["absorber.output.profile.kl(13,0)"], 146773.275);
            Assert.AreEqual(dict["absorber.output.profile.kl(14,0)"], 145875.101);
            Assert.AreEqual(dict["absorber.output.profile.kl(15,0)"], 144890.634);
            Assert.AreEqual(dict["absorber.output.profile.kl(16,0)"], 143840.404);
            Assert.AreEqual(dict["absorber.output.profile.kl(17,0)"], 142733.64);
            Assert.AreEqual(dict["absorber.output.profile.kl(18,0)"], 141572.258);
            Assert.AreEqual(dict["absorber.output.profile.kl(19,0)"], 140353.149);
            Assert.AreEqual(dict["absorber.output.profile.kl(20,0)"], 139069.374);
            Assert.AreEqual(dict["absorber.output.profile.kl(21,0)"], 137710.637);
            Assert.AreEqual(dict["absorber.output.profile.kl(22,0)"], 136263.236);
            Assert.AreEqual(dict["absorber.output.profile.kl(23,0)"], 134709.575);
            Assert.AreEqual(dict["absorber.output.profile.kl(24,0)"], 133027.222);
            Assert.AreEqual(dict["absorber.output.profile.kl(25,0)"], 131187.427);
            Assert.AreEqual(dict["absorber.output.profile.kl(26,0)"], 129152.871);
            Assert.AreEqual(dict["absorber.output.profile.kl(27,0)"], 118263.038);
            Assert.AreEqual(dict["absorber.output.profile.kl(28,0)"], 119739.81);
            Assert.AreEqual(dict["absorber.output.profile.kl(29,0)"], 120631.66);
            Assert.AreEqual(dict["absorber.output.profile.kl(30,0)"], 121119.989);
            Assert.AreEqual(dict["absorber.output.profile.kl(31,0)"], 121328.432);
            Assert.AreEqual(dict["absorber.output.profile.kl(32,0)"], 121336.803);
            Assert.AreEqual(dict["absorber.output.profile.kl(33,0)"], 121193.586);
            Assert.AreEqual(dict["solvent.output.stream.mass(0,0)"], 126.0);
            Assert.AreEqual(dict["solvent.output.stream.mass(0,1)"], 30.0);
            Assert.AreEqual(dict["solvent.output.stream.mass(0,2)"], 4319500.41);
            Assert.AreEqual(dict["solvent.output.stream.mass(0,3)"], 0.659077004);
            Assert.AreEqual(dict["solvent.output.stream.mass(0,4)"], 0.136174919);
            Assert.AreEqual(dict["solvent.output.stream.mass(0,5)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mass(0,6)"], 8.30298642E-07);
            Assert.AreEqual(dict["solvent.output.stream.mass(0,7)"], 0.0028313652);
            Assert.AreEqual(dict["solvent.output.stream.mass(0,8)"], 0.110796535);
            Assert.AreEqual(dict["solvent.output.stream.mass(0,9)"], 0.0839017153);
            Assert.AreEqual(dict["solvent.output.stream.mass(0,10)"], 0.00720352408);
            Assert.AreEqual(dict["solvent.output.stream.mass(0,11)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mass(0,12)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mass(0,13)"], 3.58185609E-11);
            Assert.AreEqual(dict["solvent.output.stream.mass(0,14)"], 5.93486908E-06);
            Assert.AreEqual(dict["solvent.output.stream.mass(0,15)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mass(1,0)"], 120.0);
            Assert.AreEqual(dict["solvent.output.stream.mass(1,1)"], 30.0);
            Assert.AreEqual(dict["solvent.output.stream.mass(1,2)"], 10000.0);
            Assert.AreEqual(dict["solvent.output.stream.mass(1,3)"], 0.999999992);
            Assert.AreEqual(dict["solvent.output.stream.mass(1,4)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mass(1,5)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mass(1,6)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mass(1,7)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mass(1,8)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mass(1,9)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mass(1,10)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mass(1,11)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mass(1,12)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mass(1,13)"], 4.18485006E-09);
            Assert.AreEqual(dict["solvent.output.stream.mass(1,14)"], 3.74161301E-09);
            Assert.AreEqual(dict["solvent.output.stream.mass(1,15)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mass(2,0)"], 128.341542);
            Assert.AreEqual(dict["solvent.output.stream.mass(2,1)"], 15.1157548);
            Assert.AreEqual(dict["solvent.output.stream.mass(2,2)"], 4445153.46);
            Assert.AreEqual(dict["solvent.output.stream.mass(2,3)"], 0.62898987);
            Assert.AreEqual(dict["solvent.output.stream.mass(2,4)"], 0.0473127369);
            Assert.AreEqual(dict["solvent.output.stream.mass(2,5)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mass(2,6)"], 0.000142183519);
            Assert.AreEqual(dict["solvent.output.stream.mass(2,7)"], 0.0168401046);
            Assert.AreEqual(dict["solvent.output.stream.mass(2,8)"], 0.175861727);
            Assert.AreEqual(dict["solvent.output.stream.mass(2,9)"], 0.127259139);
            Assert.AreEqual(dict["solvent.output.stream.mass(2,10)"], 0.00252086652);
            Assert.AreEqual(dict["solvent.output.stream.mass(2,11)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mass(2,12)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mass(2,13)"], 2.80568698E-10);
            Assert.AreEqual(dict["solvent.output.stream.mass(2,14)"], 2.79548953E-07);
            Assert.AreEqual(dict["solvent.output.stream.mass(2,15)"], 0.00107309274);
            Assert.AreEqual(dict["solvent.output.stream.mole(0,0)"], 178811.338);
            Assert.AreEqual(dict["solvent.output.stream.mole(0,1)"], 0.883758509);
            Assert.AreEqual(dict["solvent.output.stream.mole(0,2)"], 0.0538530262);
            Assert.AreEqual(dict["solvent.output.stream.mole(0,3)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mole(0,4)"], 4.5574656E-07);
            Assert.AreEqual(dict["solvent.output.stream.mole(0,5)"], 0.00112093026);
            Assert.AreEqual(dict["solvent.output.stream.mole(0,6)"], 0.0257150484);
            Assert.AreEqual(dict["solvent.output.stream.mole(0,7)"], 0.0326438697);
            Assert.AreEqual(dict["solvent.output.stream.mole(0,8)"], 0.0028997308);
            Assert.AreEqual(dict["solvent.output.stream.mole(0,9)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mole(0,10)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mole(0,11)"], 4.54857255E-11);
            Assert.AreEqual(dict["solvent.output.stream.mole(0,12)"], 8.42944868E-06);
            Assert.AreEqual(dict["solvent.output.stream.mole(0,13)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mole(1,0)"], 555.084351);
            Assert.AreEqual(dict["solvent.output.stream.mole(1,1)"], 0.999999992);
            Assert.AreEqual(dict["solvent.output.stream.mole(1,2)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mole(1,3)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mole(1,4)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mole(1,5)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mole(1,6)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mole(1,7)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mole(1,8)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mole(1,9)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mole(1,10)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mole(1,11)"], 3.96323154E-09);
            Assert.AreEqual(dict["solvent.output.stream.mole(1,12)"], 3.96323154E-09);
            Assert.AreEqual(dict["solvent.output.stream.mole(1,13)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mole(2,0)"], 176862.183);
            Assert.AreEqual(dict["solvent.output.stream.mole(2,1)"], 0.877514724);
            Assert.AreEqual(dict["solvent.output.stream.mole(2,2)"], 0.0194672395);
            Assert.AreEqual(dict["solvent.output.stream.mole(2,3)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mole(2,4)"], 8.11991795E-05);
            Assert.AreEqual(dict["solvent.output.stream.mole(2,5)"], 0.00693650655);
            Assert.AreEqual(dict["solvent.output.stream.mole(2,6)"], 0.042466436);
            Assert.AreEqual(dict["solvent.output.stream.mole(2,7)"], 0.0515149269);
            Assert.AreEqual(dict["solvent.output.stream.mole(2,8)"], 0.00105578583);
            Assert.AreEqual(dict["solvent.output.stream.mole(2,9)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mole(2,10)"], 0.0);
            Assert.AreEqual(dict["solvent.output.stream.mole(2,11)"], 3.70697383E-10);
            Assert.AreEqual(dict["solvent.output.stream.mole(2,12)"], 4.13103819E-07);
            Assert.AreEqual(dict["solvent.output.stream.mole(2,13)"], 0.000962768719);
            Assert.AreEqual(dict["gas.output.stream.mass(0,0)"], 134.4);
            Assert.AreEqual(dict["gas.output.stream.mass(0,1)"], 16.0);
            Assert.AreEqual(dict["gas.output.stream.mass(0,2)"], 959553.0);
            Assert.AreEqual(dict["gas.output.stream.mass(0,3)"], 0.071);
            Assert.AreEqual(dict["gas.output.stream.mass(0,4)"], 0.0);
            Assert.AreEqual(dict["gas.output.stream.mass(0,5)"], 0.0);
            Assert.AreEqual(dict["gas.output.stream.mass(0,6)"], 0.211);
            Assert.AreEqual(dict["gas.output.stream.mass(0,7)"], 0.718);
            Assert.AreEqual(dict["gas.output.stream.mass(1,0)"], 141.764559);
            Assert.AreEqual(dict["gas.output.stream.mass(1,1)"], 15.0);
            Assert.AreEqual(dict["gas.output.stream.mass(1,2)"], 843843.344);
            Assert.AreEqual(dict["gas.output.stream.mass(1,3)"], 0.138139369);
            Assert.AreEqual(dict["gas.output.stream.mass(1,4)"], 9.23428954E-20);
            Assert.AreEqual(dict["gas.output.stream.mass(1,5)"], 0.0);
            Assert.AreEqual(dict["gas.output.stream.mass(1,6)"], 0.0510601339);
            Assert.AreEqual(dict["gas.output.stream.mass(1,7)"], 0.810800497);
            Assert.AreEqual(dict["gas.output.stream.mole(0,0)"], 32976.0017);
            Assert.AreEqual(dict["gas.output.stream.mole(0,1)"], 0.114680163);
            Assert.AreEqual(dict["gas.output.stream.mole(0,2)"], 0.0);
            Assert.AreEqual(dict["gas.output.stream.mole(0,3)"], 0.0);
            Assert.AreEqual(dict["gas.output.stream.mole(0,4)"], 0.13950958);
            Assert.AreEqual(dict["gas.output.stream.mole(0,5)"], 0.745810257);
            Assert.AreEqual(dict["gas.output.stream.mole(1,0)"], 31873.082);
            Assert.AreEqual(dict["gas.output.stream.mole(1,1)"], 0.2030085);
            Assert.AreEqual(dict["gas.output.stream.mole(1,2)"], 4.00235659E-20);
            Assert.AreEqual(dict["gas.output.stream.mole(1,3)"], 0.0);
            Assert.AreEqual(dict["gas.output.stream.mole(1,4)"], 0.0307164007);
            Assert.AreEqual(dict["gas.output.stream.mole(1,5)"], 0.766275099);
        }

        [TestMethod]
        public void TestCreateSubmitJobBadUserAuth()
        {
            String addr = Properties.Settings.Default.jobAddress;
            Debug.WriteLine("Address: " + addr, this.GetType());
            var cli = new SimulationClient();
            cli.SetBasicAuth(Properties.Settings.Default.username,
                Properties.Settings.Default.password);

            var msg = new Dictionary<String, Object>()
            {
                {"Simulation", simulation_mea_uq},
                {"Inputs", new Dictionary<String,Object>() {
                    {"reactions.input.parameters(4,0)", "1.328578867187e+02"}, 
                    {"absorber.input.main.masstrans.cv", "2.133484375000e-01"}, 
                    {"absorber.input.wash.masstrans.cv", "2.133484375000e-01"}, 
                    {"reactions.input.parameters(1,0)", "9.856565390625e+01"}, 
                    {"reactions.input.parameters(0,0)", "8.745164071484e-01"}
                }},
                {"Session", Guid.NewGuid()}
            };

            Job job = cli.CreateJob(new Uri(addr), msg);
            Assert.AreEqual("create", job.State);

            // Attempt submit as different user
            var cli2 = new SimulationClient();
            cli2.SetBasicAuth(Properties.Settings.Default.username2,
                Properties.Settings.Default.password2);
            try
            {
                job = cli2.SubmitJob(new Uri(addr), job.Id);
                Assert.Fail("Authorization error Job submit as different user succeeded.");
            }
            catch (System.Net.WebException)
            {
            }

        }
        [TestMethod]
        public void TestSubmitJobBadUser()
        {
            String addr = Properties.Settings.Default.jobAddress;
            Debug.WriteLine("Address: " + addr, this.GetType());
            var cli = new SimulationClient();
            cli.SetBasicAuth("badusername", "badpassword");

            var msg = new Dictionary<String, Object>()
            {
                {"Simulation", simulation_mea_uq},
                {"Inputs", new Dictionary<String,Object>() {
                    {"reactions.input.parameters(4,0)", "1.328578867187e+02"}, 
                    {"absorber.input.main.masstrans.cv", "2.133484375000e-01"}, 
                    {"absorber.input.wash.masstrans.cv", "2.133484375000e-01"}, 
                    {"reactions.input.parameters(1,0)", "9.856565390625e+01"}, 
                    {"reactions.input.parameters(0,0)", "8.745164071484e-01"}
                }},
                {"Session", Guid.NewGuid()}
            };
            Job job;
            try
            {
                job = cli.CreateJob(new Uri(addr), msg);
                Assert.Fail("Authorization error Job create as badusername user succeeded.");
            }
            catch (System.Net.WebException)
            {
            }

        }
        [TestMethod]
        public void TestCreateJobBadRequest()
        {
            String addr = Properties.Settings.Default.jobAddress;
            Debug.WriteLine("Address: " + addr, this.GetType());
            SimulationClient cli = new SimulationClient();
            cli.SetBasicAuth(Properties.Settings.Default.username,
                Properties.Settings.Default.password);

            var msg = new Dictionary<String, Object>()
            {
                {"Simulation", simulation_mea_uq},
                {"Inputs", "aaa"},
                {"Session", Guid.NewGuid()}
            };

            try
            {
                cli.CreateJob(new Uri(addr), msg);
                Assert.Fail("BAD INPUTS FORMAT EXPECTING DICT");
            }
            catch (System.Net.WebException)
            {
            }

        }
    }
}
