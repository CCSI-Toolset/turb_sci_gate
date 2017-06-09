using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceProcess;
using System.Text;

namespace SelfManagedWebApplicationWindowsService
{
    public partial class Service1 : ServiceBase
    {
        List<ServiceHost> hosts = new List<ServiceHost>();

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStop()
        {
            Debug.WriteLine("OnStop handler called", "SelfManagedWebApplicationWindowsService");
            try
            {
                foreach (var host in hosts)
                {
                    Debug.WriteLine("closing:  " + host.BaseAddresses, "SelfManagedWebApplicationWindowsService");
                    host.Close();
                }
            }
            finally
            {
                foreach (var host in hosts)
                    ((IDisposable)host).Dispose();
            }
        }

        protected override void OnStart(string[] args)
        {
            Debug.WriteLine("OnStart handler called", "SelfManagedWebApplicationWindowsService");
            try
            {
                using (var db = new Turbine.DataEF6.ProducerContext())
                {
                    Debug.WriteLine("Application ProducerContext", "SelfManagedWebApplicationWindowsService");
                    db.Applications.Add(new Turbine.Data.Entities.Application { Name = "ACM" });
                    db.SaveChanges();
                }
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
                Debug.WriteLine("Ignore Application ACM input entries already made", "SelfManagedWebApplicationWindowsService");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(String.Format("Exception {0}", ex), "SelfManagedWebApplicationWindowsService");
                throw;
            }
            try
            {
                using (var db = new Turbine.DataEF6.ProducerContext())
                {
                    Debug.WriteLine("Application ProducerContext", "SelfManagedWebApplicationWindowsService");
                    db.Applications.Add(new Turbine.Data.Entities.Application { Name = "AspenPlus" });
                    db.SaveChanges();
                }
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
                Debug.WriteLine("Ignore Application AspenPlus input entries already made", "SelfManagedWebApplicationWindowsService");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(String.Format("Exception {0}", ex), "SelfManagedWebApplicationWindowsService");
                throw;
            }
            try
            {
                using (var db = new Turbine.DataEF6.ProducerContext())
                {
                    Debug.WriteLine("Application ProducerContext", "SelfManagedWebApplicationWindowsService");
                    db.Applications.Add(new Turbine.Data.Entities.Application { Name = "GProms" });
                    db.SaveChanges();
                }
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
                Debug.WriteLine("Ignore Application GProms input entries already made", "SelfManagedWebApplicationWindowsService");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(String.Format("Exception {0}", ex), "SelfManagedWebApplicationWindowsService");
                throw;
            }
            try
            {
                using (var db = new Turbine.DataEF6.ProducerContext())
                {
                    Debug.WriteLine("Application ProducerContext", "SelfManagedWebApplicationWindowsService");
                    db.Applications.Add(new Turbine.Data.Entities.Application { Name = "Excel" });
                    db.SaveChanges();
                }
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
                Debug.WriteLine("Ignore Application Excel input entries already made", "SelfManagedWebApplicationWindowsService");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(String.Format("Exception {0}", ex), "SelfManagedWebApplicationWindowsService");
                throw;
            }
            try
            {
                string owner = Turbine.Producer.Container.GetAppContext().UserName;
                Debug.WriteLine("OWNER " + owner, "SelfManagedWebApplicationWindowsService");
                using (var db = new Turbine.DataEF6.ProducerContext())
                {
                    Debug.WriteLine("Application ProducerContext", "SelfManagedWebApplicationWindowsService");
                    db.Users.Add(new Turbine.Data.Entities.User { Name = owner });
                    db.SaveChanges();
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                Debug.WriteLine("Ignore User entries already made", "SelfManagedWebApplicationWindowsService");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(String.Format("Exception {0}", ex), "SelfManagedWebApplicationWindowsService");
                throw;
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
            }
            catch (Exception ex)
            {
                Debug.WriteLine(String.Format("Exception {0}", ex), "SelfManagedWebApplicationWindowsService");
                throw;
            }

            Debug.WriteLine("Service is ready", "SelfManagedWebApplicationWindowsService");
        }
    }
}
