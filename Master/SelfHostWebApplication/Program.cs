using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Diagnostics;


namespace SelfHostWebApplication
{
    class Program
    {
        /// <summary>
        /// Web Application that runs in Console program.
        /// Configuration:
        ///     Need to add URL ACL for URL registration:
        ///     runas /user:Administrator "netsh http add urlacl url=http://localhost:8000/TurbineLite user=lbl\boverhof"
        /// </summary>
        /// <param name="args"></param>
        /// 
        static void Main(string[] args)
        {
            // Create the ServiceHost.
            try
            {
                using (var db = new Turbine.DataEF6.ProducerContext())
                {
                    Debug.WriteLine("Application ProducerContext", "TurbineLite");
                    db.Applications.Add(new Turbine.Data.Entities.Application { Name = "ACM" });
                    db.Applications.Add(new Turbine.Data.Entities.Application { Name = "AspenPlus" });
                    db.Applications.Add(new Turbine.Data.Entities.Application { Name = "GProms" });
                    db.Applications.Add(new Turbine.Data.Entities.Application { Name = "Excel" });
                    db.SaveChanges();
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                Debug.WriteLine("Ignore Application entries already made");
            }
            try
            {
                using (var db = new Turbine.DataEF6.ProducerContext())
                {
                    var app = db.Applications.Single(s => s.Name == "ACM");
                    app.InputFileTypes.Add(new Turbine.Data.Entities.InputFileType { Name = "aspenfile", Id = Guid.NewGuid(), Type = "text/html" });
                    app.InputFileTypes.Add(new Turbine.Data.Entities.InputFileType { Name = "configuration", Id = Guid.NewGuid(), Type = "text/html" });
                    app.InputFileTypes.Add(new Turbine.Data.Entities.InputFileType { Name = "any", Id = Guid.NewGuid(), Type = "application/octet-stream" });
                    db.SaveChanges();
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                Debug.WriteLine("Ignore Application ACM input entries already made");
            }
            try
            {
                using (var db = new Turbine.DataEF6.ProducerContext())
                {
                    var app = db.Applications.Single(s => s.Name == "AspenPlus");
                    app.InputFileTypes.Add(new Turbine.Data.Entities.InputFileType { Name = "aspenfile", Id = Guid.NewGuid(), Type = "text/html" });
                    app.InputFileTypes.Add(new Turbine.Data.Entities.InputFileType { Name = "configuration", Id = Guid.NewGuid(), Type = "text/html" });
                    app.InputFileTypes.Add(new Turbine.Data.Entities.InputFileType { Name = "any", Id = Guid.NewGuid(), Type = "application/octet-stream" });
                    db.SaveChanges();
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                Debug.WriteLine("Ignore Application AspenPlus input entries already made");
            }
            try
            {
                using (var db = new Turbine.DataEF6.ProducerContext())
                {
                    var app = db.Applications.Single(s => s.Name == "GProms");
                    app.InputFileTypes.Add(new Turbine.Data.Entities.InputFileType { Name = "configuration", Id = Guid.NewGuid(), Type = "text/html" });
                    app.InputFileTypes.Add(new Turbine.Data.Entities.InputFileType { Name = "model", Id = Guid.NewGuid(), Type = "text/html" });
                    app.InputFileTypes.Add(new Turbine.Data.Entities.InputFileType { Name = "any", Id = Guid.NewGuid(), Type = "application/octet-stream" });
                    db.SaveChanges();
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                Debug.WriteLine("Ignore Application GProms input entries already made");
            }
            try
            {
                using (var db = new Turbine.DataEF6.ProducerContext())
                {
                    var app = db.Applications.Single(s => s.Name == "Excel");
                    app.InputFileTypes.Add(new Turbine.Data.Entities.InputFileType { Name = "spreadsheet", Id = Guid.NewGuid(), Type = "application/vnd.ms-excel" });
                    app.InputFileTypes.Add(new Turbine.Data.Entities.InputFileType { Name = "configuration", Id = Guid.NewGuid(), Type = "text/html" });
                    db.SaveChanges();
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                Debug.WriteLine("Ignore Application Excel input entries already made");
            }
            try
            {
                string owner = Turbine.Producer.Container.GetAppContext().UserName;
                Debug.WriteLine("OWNER " + owner);
                using (var db = new Turbine.DataEF6.ProducerContext())
                {
                    Debug.WriteLine("Application ProducerContext", "TurbineLite");
                    db.Users.Add(new Turbine.Data.Entities.User { Name = owner });
                    db.SaveChanges();
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                Debug.WriteLine("Ignore User entries already made");
            }
            var hosts = new List<ServiceHost>();
            try
            {
                hosts.Add(new WebServiceHost(
                    typeof(Turbine.Lite.Web.Resources.ApplicationResource),
                    new Uri(String.Format("http://{0}/{1}/application", Properties.Settings.Default.netloc, Properties.Settings.Default.webdirectory))));
                hosts.Add(
                    new WebServiceHost(
                        typeof(Turbine.Lite.Web.Resources.SimulationResource),
                        new Uri(String.Format("http://{0}/{1}/simulation", Properties.Settings.Default.netloc, Properties.Settings.Default.webdirectory))));
                hosts.Add(
                    new WebServiceHost(
                        typeof(Turbine.Lite.Web.Resources.SessionResource),
                        new Uri(String.Format("http://{0}/{1}/session", Properties.Settings.Default.netloc, Properties.Settings.Default.webdirectory))
                        ));
                hosts.Add(
                    new WebServiceHost(
                        typeof(Turbine.Lite.Web.Resources.JobResource),
                        new Uri(String.Format("http://{0}/{1}/job", Properties.Settings.Default.netloc, Properties.Settings.Default.webdirectory))
                        ));
                hosts.Add(
                    new WebServiceHost(
                        typeof(Turbine.Lite.Web.Resources.ConsumerResource),
                        new Uri(String.Format("http://{0}/{1}/consumer", Properties.Settings.Default.netloc, Properties.Settings.Default.webdirectory))
                        ));

                foreach (var host in hosts)
                {
                    host.Open();
                }
                Console.WriteLine("The service is ready");
                Console.WriteLine("Press <Enter> to stop the service.");
                Console.ReadLine();
                foreach (var host in hosts)
                {
                    host.Close();
                }
            }
            finally
            {
                foreach (var host in hosts) 
                    ((IDisposable)host).Dispose();
            }
            /*
            using (WebServiceHost host = new WebServiceHost(typeof(Turbine.Lite.Web.Resources.ApplicationResource), new Uri("http://localhost:8000/TurbineLite/application")))
            {
                //host.AddServiceEndpoint(typeof(Turbine.Lite.Web.Resources.ApplicationResource), new WebHttpBinding(), new Uri("http://localhost:8000/TurbineLite/application"));
                host.AddServiceEndpoint(typeof(Turbine.Lite.Web.Resources.SimulationResource), new WebHttpBinding(), new Uri("http://localhost:8000/TurbineLite/simulation"));

                // Enable metadata publishing.
                //ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                //smb.HttpGetEnabled = true;
                //smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                //host.Description.Behaviors.Add(smb);

                // Open the ServiceHost to start listening for messages. Since
                // no endpoints are explicitly configured, the runtime will create
                // one endpoint per base address for each service contract implemented
                // by the service.
                host.Open();

                Console.WriteLine("The service is ready");
                Console.WriteLine("Press <Enter> to stop the service.");
                Console.ReadLine();

                // Close the ServiceHost.
                host.Close();
            }
             * */
        }
    }
}
