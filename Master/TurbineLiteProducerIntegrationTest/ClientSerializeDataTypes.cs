using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TurbineLiteProducerIntegrationTest
{

    internal class JobDescriptionList : List<JobDescription>
    {
        public JobDescriptionList() { }
        public JobDescriptionList(IEnumerable<JobDescription> source) : base(source) { }
    }


    internal class JobDescription
    {
        [JsonProperty("Id")]
        public int Id { get; set; }
        [JsonProperty("Guid")]
        public Guid Guid { get; set; }
        [JsonProperty("Simulation")]
        public Guid Simulation { get; set; }
        [JsonProperty("State")]
        public string State { get; set; }
        [JsonProperty("Input")]
        public JObject Input { get; set; }
        //[JsonProperty("Output")]
        //public JObject Output { get; set; }
        [JsonProperty("Session")]
        public Guid Session { get; set; }
        [JsonProperty("Initialize")]
        public Boolean Initialize { get; set; }
        [JsonProperty("Reset")]
        public Boolean Reset { get; set; }
        [JsonProperty("Create")]
        public DateTime Create { get; set; }
        [JsonProperty("Submit")]
        public DateTime? Submit { get; set; }
        [JsonProperty("Setup")]
        public DateTime? Setup { get; set; }
        [JsonProperty("Running")]
        public DateTime? Running { get; set; }
        [JsonProperty("Finished")]
        public DateTime? Finished { get; set; }
    }

    internal class JobRequest
    {
        [JsonProperty("Initialize")]
        public Boolean Initialize { get; set; }

        [JsonProperty("Reset")]
        public Boolean Reset { get; set; }

        [JsonProperty("Simulation")]
        public string Simulation { get; set; }

        [JsonProperty("Input")]
        public JObject Input { get; set; }
    }

    internal class JobRequestList : List<JobRequest>
    {
        public JobRequestList() { }
        public JobRequestList(IEnumerable<JobRequest> source) : base(source) { }
    }

    internal class Application
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        //[JsonProperty("accounts")]
        //public InputTypeList Inputs { get; set; }
    }

    internal class ApplicationList : List<Application>
    {
        public ApplicationList() { }
        public ApplicationList(IEnumerable<Application> source) : base(source) { }
    }

    internal class Simulation
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Application")]
        public string Application { get; set; }

        [JsonProperty("StagedInputs")]
        public List<string> StagedInputs { get; set; }
    }
    internal class SimulationStagedInputList : List<string>
    {
    }

    internal class SimulationList : List<Simulation>
    {
        public SimulationList() { }
        public SimulationList(IEnumerable<Simulation> source) : base(source) { }
    }
    internal class InputType
    {
    }

    internal class InputTypeList : List<InputType>
    {
        public InputTypeList() { }
        public InputTypeList(IEnumerable<InputType> source) : base(source) { }
    }
    /*
     * Test Name:	SessionResourceTest
Session Status: {"create":1,"running":0,"setup":0,"submit":0,"pause":0,"locked":0,
     * "error":0,"cancel":0,"success":0,"terminate":0}


     */



    internal class SessionStatusDict
    {
        [JsonProperty("create")]
        public int Create { get; set; }

        [JsonProperty("submit")]
        public int Submit { get; set; }

        [JsonProperty("setup")]
        public int Setup { get; set; }

        [JsonProperty("running")]
        public int Running { get; set; }

        [JsonProperty("pause")]
        public int Pause { get; set; }

        [JsonProperty("locked")]
        public int Locked { get; set; }

        [JsonProperty("error")]
        public int Error { get; set; }

        [JsonProperty("cancel")]
        public int Cancel { get; set; }

        [JsonProperty("success")]
        public int Success { get; set; }

        [JsonProperty("terminate")]
        public int Terminate { get; set; }
    }
}
