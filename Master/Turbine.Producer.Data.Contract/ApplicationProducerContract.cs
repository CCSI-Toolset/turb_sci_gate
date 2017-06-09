using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Producer.Data.Contract.Behaviors;
using Turbine.Data;
using Turbine.Data.Contract;


namespace Turbine.Producer.Data.Contract
{
    public class ApplicationProducerContract : IApplicationProducerContract
    {
        public static IApplicationProducerContract Create(string applicationName, string version)
        {
            string owner = Container.GetAppContext().UserName;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                var user = container.Users.Single<User>(s => s.Name == owner);
                var obj = container.Applications.SingleOrDefault<Application>(s => s.Name == applicationName);
                if (obj != null)
                    throw new InvalidStateChangeException(String.Format("Application with Name {0} already exists", applicationName));

                container.Applications.AddObject(new Application() { Name = applicationName, User = user, Version = version });
                container.SaveChanges();
            }
            var contract = new ApplicationProducerContract();
            contract.applicationName = applicationName;
            return contract;
        }

        public static IApplicationProducerContract Get(string applicationName)
        {
            string owner = Container.GetAppContext().UserName;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                var user = container.Users.Single<User>(s => s.Name == owner);
                var obj = container.Applications.
                    SingleOrDefault<Application>(s => s.Name == applicationName);
                if (obj == null)
                    return null;
            }
            var contract = new ApplicationProducerContract();
            contract.applicationName = applicationName;
            return contract;
        }
        private string applicationName;
        private ApplicationProducerContract() { }

        public bool UpdateInputFileType(string name, bool required, string type)
        {
            string owner = Container.GetAppContext().UserName;
            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                var user = container.Users.Single<User>(s => s.Name == owner);
                var obj = container.Applications.
                    Single<Application>(s => s.Name == applicationName);

                if (user != obj.User)
                {
                    throw new AuthorizationError(String.Format("Only owner {0} of application {1} can update it", owner, name));
                }

                var fileType = container.InputFileTypes.SingleOrDefault<InputFileType>(s => s.ApplicationName == applicationName & s.Name == name);
                if (fileType == null)
                    fileType = new InputFileType() { Name = name, Required = required, Type = type, Id = Guid.NewGuid(), ApplicationName = applicationName };
                fileType.Required = required;
                fileType.Type = type;
                obj.InputFileTypes.Add(fileType);
                container.SaveChanges();
            }
            return true;
        }
    }
}
