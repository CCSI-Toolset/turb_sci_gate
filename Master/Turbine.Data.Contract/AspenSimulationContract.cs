using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Data.Contract.Behaviors;
using Turbine.Common;
using System.IO;
using Newtonsoft.Json;

namespace Turbine.Data.Contract
{
    /* AspenSimulationContract:  Used by producer to access, update and create simulations.  
     *    Also factory method for creating a new job for the simulation.
     * 
     *
     */
    public class AspenSimulationContract : ISimulationProducerContract
    {
        public static string[] ListOfNames()
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                List<string> l = new List<string>();
                foreach (Simulation sim in container.Simulations)
                {
                    l.Add(sim.Name);
                }
                return l.ToArray<string>();
            }
        }
        public static ISimulationProducerContract Get(string name)
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Simulation obj = container.Simulations.Single<Simulation>(s => s.Name == name);
            }
            AspenSimulationContract sc = new AspenSimulationContract();
            sc.name = name;
            return sc;
        }


        public static ISimulationProducerContract Create(string name)
        {
            string owner = AppUtility.GetAppContext().UserName;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                User user = container.Users.Single<User>(s => s.Name == owner);
                Simulation obj = container.Simulations.SingleOrDefault<Simulation>(s => s.Name == name);
                if (obj != null)
                    throw new InvalidStateChangeException("Simulation with Name {0} already exists");

                container.Simulations.AddObject(new Simulation() { Name = name, User = user });
                container.SaveChanges();
            }
            AspenSimulationContract sc = new AspenSimulationContract();
            sc.name = name;
            return sc;
        }

        private string name;
        private AspenSimulationContract() { }

        public bool UpdateConfiguration(string filePath)
        {
            byte[] data = System.Text.Encoding.ASCII.GetBytes(
                    File.ReadAllText(filePath)
                    );
            return UpdateConfiguration(data);
        }
        public bool UpdateConfiguration(byte[] data)
        {
            string owner = AppUtility.GetAppContext().UserName;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Simulation obj = container.Simulations.Single<Simulation>(s => s.Name == name);
                if (obj.User.Name != owner)
                    throw new ArgumentException("Only owner {0} can update simulation", obj.User.Name);
                obj.Configuration = data;
                container.SaveChanges();
            }
            return true;
        }
        public bool UpdateAspenBackup(string filePath)
        {
            byte[] data = System.Text.Encoding.ASCII.GetBytes(
                    File.ReadAllText(filePath)
                    );
            return UpdateAspenBackup(data);
        }
        public bool UpdateAspenBackup(byte[] data)
        {
            string owner = AppUtility.GetAppContext().UserName;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Simulation obj = container.Simulations.Single<Simulation>(s => s.Name == name);
                if (obj.User.Name != owner)
                    throw new ArgumentException("Only owner {0} can update simulation", obj.User.Name);
                obj.Backup = data;
                container.SaveChanges();
            }
            return true;
        }

        private void SetDefaults(byte[] data)
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Simulation entity = container.Simulations.Single<Simulation>(s => s.Name == name);
                entity.Defaults = data;
                container.SaveChanges();
            }
        }
        //public string[] Input
        public Dictionary<String, Object> Defaults
        {
            get
            {
                Dictionary<String, Object> dict = null;
                using (TurbineModelContainer container = new TurbineModelContainer())
                {
                    Simulation entity = container.Simulations.Single<Simulation>(s => s.Name == name);
                    String data = System.Text.Encoding.ASCII.GetString(entity.Defaults);
                    if (data != null)
                    {
                        dict = JsonConvert.DeserializeObject<Dictionary<string, Object>>(data);
                    }
                    return dict;
                }
            }
            set
            {
                String data = JsonConvert.SerializeObject(value);
                SetDefaults(System.Text.Encoding.ASCII.GetBytes(data));
            }
        }

        public IJobProducerContract NewJob(Guid sessionID, bool initialize)
        {
            return (IJobProducerContract)AspenJobContract.Create(name, sessionID, initialize);
        }

        public bool Delete(string name)
        {
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                Simulation entity = container.Simulations.Single<Simulation>(s => s.Name == name);
                container.Simulations.DeleteObject(entity);
            }
            return true;
        }

    }



}
