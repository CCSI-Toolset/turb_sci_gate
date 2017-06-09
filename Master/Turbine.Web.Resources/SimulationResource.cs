using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using Turbine.Data.Contract;
using Turbine.Data.Serialize;
using System.Diagnostics;
using System.IO;
using Turbine.Data.Contract.Behaviors;
using Turbine.Producer.Data.Contract.Behaviors;
using Turbine.Producer.Data.Contract;


namespace Turbine.Web.Resources
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, IncludeExceptionDetailInFaults=true)]
    public class SimulationResource : Turbine.Web.Contracts.ISimulationResource
    {
        public Simulations GetSimulations()
        {
            Debug.WriteLine("get all simulations", this.GetType().Name);
            return DataMarshal.GetSimulations();
        }

        public Simulation GetSimulation(string name)
        {
            Debug.WriteLine("Find simulation: " + name, this.GetType().Name);
            Simulation sim = null;
            try
            {
                sim = DataMarshal.GetSimulation(name);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("No simulation " + name, this.GetType().Name);
                Debug.WriteLine(ex.ToString());
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    "No simulation " + name + ", traceback: " + ex.StackTrace.ToString()
                );
                throw new WebFaultException<ErrorDetail>(detail, System.Net.HttpStatusCode.NotFound);
            }
            return sim;
        }
        
        public Simulation UpdateSimulation(string name, Simulation sim)
        {
            ISimulationProducerContract contract = null;
            if (sim == null)
            {
                string msg = String.Format("No Application specified in Simulation {0}", name);
                Debug.WriteLine(msg, this.GetType().Name);
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    msg
                );
                throw new WebFaultException<ErrorDetail>(detail,
                    System.Net.HttpStatusCode.InternalServerError);
            }

            string appname = sim.Application;
            try
            {
                contract = Turbine.Lite.Web.Resources.Contracts.ProducerSimulationContract.Create(name, appname);
            }
            catch (Exception ex)
            {
                string msg = String.Format("Failed to Create Simulation {0}: {1} {2}",
                    name, ex.Message, ex.StackTrace.ToString());
                if (ex.InnerException != null)
                    msg += String.Format("    InnerException:    {0} {1}", 
                        ex.InnerException.Message, ex.InnerException.StackTrace.ToString());
                Debug.WriteLine(msg, this.GetType().Name);
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    msg
                );
                throw new WebFaultException<ErrorDetail>(detail, 
                    System.Net.HttpStatusCode.InternalServerError);
            }

            sim = DataMarshal.GetSimulation(name);

            return sim;

        }

        public string[] GetStagedInputs(string name)
        {
            return DataMarshal.GetStagedInputs(name);
        }

        public Stream GetStagedInputFile(string name, string inputName)
        {
            OutgoingWebResponseContext ctx = WebOperationContext.Current.OutgoingResponse;
            var e = DataMarshal.GetStagedInputFile(name, inputName);
            if (e == null) {
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    String.Format("simulation \"{0}\" input \"{1}\" does not exist", name, inputName)
                );
                throw new WebFaultException<ErrorDetail>(detail,
                    System.Net.HttpStatusCode.NotFound);
            }
            if (e.Content == null)
            {
                return new MemoryStream();
            }
            ctx.ContentType = "text/plain";
            if (e.InputFileType != null && e.InputFileType.Type != null) {
                ctx.ContentType = e.InputFileType.Type;
            }
            ctx.StatusCode = System.Net.HttpStatusCode.OK;
            return new MemoryStream(e.Content);
        }

        public bool UpdateStagedInputFile(string name, string inputName, Stream data)
        {
            Debug.WriteLine("UpdateStagedInputFile: " + name, this.GetType());
            //Simulation sim = GetSimulation(name);
            OutgoingWebResponseContext ctx = WebOperationContext.Current.OutgoingResponse;
            var content_type = WebOperationContext.Current.IncomingRequest.ContentType;

            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                data.CopyTo(ms);
                bytes = ms.ToArray();
            }

            //StreamReader sr = new StreamReader(data);
            //string s = sr.ReadToEnd();
            //byte[] bytes = System.Text.Encoding.ASCII.GetBytes(s);
            Debug.WriteLine("content_type " + content_type, this.GetType());

            // TODO: Validate Backup File
            var contract = AspenSimulationContract.Get(name);
            bool success = false;
            try
            {
                success = contract.UpdateInput(inputName, bytes, content_type);
            }
            catch (ArgumentException ex)
            {
                string msg = String.Format("Failed to Update Simulation {0} input {1}: {2}",
                    name, inputName, ex.Message);
                Debug.WriteLine(msg, this.GetType().Name);
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.AUTHORIZATION_ERROR,
                    msg
                );
                throw new WebFaultException<ErrorDetail>(detail,
                    System.Net.HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                string msg = String.Format("Failed to Update Simulation {0} input {1}: {2}, {3}",
                    name, inputName, ex.Message, ex.StackTrace);
                Debug.WriteLine(msg, this.GetType().Name);
                if (ex.InnerException != null)
                {
                    Debug.WriteLine("InnerException: " + ex.InnerException.Message, this.GetType().Name);
                    Debug.WriteLine(ex.InnerException.StackTrace, this.GetType().Name);
                }
                var detail = new ErrorDetail(
                    ErrorDetail.Codes.DATAFORMAT_ERROR,
                    msg
                );
                throw new WebFaultException<ErrorDetail>(detail,
                    System.Net.HttpStatusCode.InternalServerError);
            }

            //sim = GetSimulation(name);
            ctx.ContentType = content_type;
            ctx.StatusCode = System.Net.HttpStatusCode.OK;

            //return new MemoryStream(bytes);
            return success;
        }

        /*
        public bool Validate(string name)
        {
            var contract = AspenSimulationContract.Get(name);
            return contract.Validate();
        }
         */

        public bool Validate(string name)
        {
            throw new NotImplementedException();
        }
    }
}