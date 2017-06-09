using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Turbine.Client;
using Turbine.Data.Serialize;
using System.Diagnostics;
using System.IO;

namespace IntegrationTests
{
    [TestClass]
    public class SimulationTest
    {
        static string simulation_mea_uq = "mea-uq-billet-1";

        [TestMethod]
        public void TestGetSimulation()
        {
            String addr = Properties.Settings.Default.simulationAddress;

            Uri uri = new Uri(Path.Combine(addr, simulation_mea_uq));
            Debug.WriteLine("Address: " + addr, this.GetType());
            SimulationClient cli = new SimulationClient();
            cli.SetBasicAuth(Properties.Settings.Default.username,
                Properties.Settings.Default.password);
            Simulation sim = cli.GetSimulation(uri);
            Assert.AreEqual(simulation_mea_uq, sim.Name);
        }
        [TestMethod]
        public void TestGetAllSimulations()
        {
            String addr = Properties.Settings.Default.simulationAddress;
            Debug.WriteLine("Address: " + addr, this.GetType());
            SimulationClient cli = new SimulationClient();
            cli.SetBasicAuth(Properties.Settings.Default.username,
                Properties.Settings.Default.password);
            Simulations sims = cli.GetSimulationRoot(new Uri(addr));
            //Assert.AreEqual(10, sims.Count);
            Assert.IsTrue(sims.Count >= 1);
            Debug.WriteLine("Name: " + simulation_mea_uq, this.GetType());
            Simulation sim = sims.Single(s => s.Name == simulation_mea_uq);
            Assert.IsNotNull(sim);
            Assert.AreEqual<String>(sim.Name, simulation_mea_uq);
        }
    }
}