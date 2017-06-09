using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Net;
using Turbine.Data.Serialize;
using Newtonsoft.Json;

namespace Turbine.Client
{
    public class SimulationClient
    {
        private HTTPClient transport;

        public SimulationClient()
        {
            transport = new HTTPClient();
        }
        public void SetBasicAuth(String userName, String password) 
        {
            transport.AuthHandler = new BasicAuthHandler(userName, password);
        }

        public Simulation GetSimulation(Uri addr)
        {
            string data = transport.doGet(addr);
            return JSON.Deserialise<Simulation>(data);
        }

        public Simulations GetSimulationRoot(Uri addr)
        {
            string data = transport.doGet(addr);
            return JSON.Deserialise<Simulations>(data);
        }

        public Job GetJob(Uri uri)
        {
            string data = transport.doGet(uri);
            return JSON.Deserialise<Job>(data);
        }

        public Jobs GetJobs(Uri uri)
        {
            string data = transport.doGet(uri);
            return JSON.Deserialise<Jobs>(data);
        }

        public Job CreateJob(Uri uri, JobRequestMsg msg)
        {
            string data = transport.doPOST(uri, JSON.Serialize(msg));
            return JSON.Deserialise<Job>(data);
        }
        public Job CreateJob(Uri uri, Dictionary<String,Object> msg)
        {
            string data = transport.doPOST(uri, JsonConvert.SerializeObject(msg));
            return JSON.Deserialise<Job>(data);
        }
        public Job SubmitJob(Uri uri, int jobId)
        {
            Uri jobUri = new Uri(uri, Path.Combine(uri.PathAndQuery, jobId.ToString()));
            string data = transport.doPOST(jobUri, "");
            return JSON.Deserialise<Job>(data);
        }
        public Job CancelJob(Uri uri, int jobId)
        {
            Uri jobUri = new Uri(uri, Path.Combine(uri.PathAndQuery, jobId.ToString(), "cancel"));
            string data = transport.doPOST(jobUri, "");
            return JSON.Deserialise<Job>(data);
        }
    }
}