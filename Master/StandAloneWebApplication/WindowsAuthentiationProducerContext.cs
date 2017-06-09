using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbine.Data;
using Turbine.Producer.Contracts;
using System.Diagnostics;

namespace StandAloneWebApplication
{
    /// <summary>
    /// WindowsAuthentiationProducerContext creates a User row entry if 
    /// the authenticated username does not exist in the database table User.
    /// </summary>
    public class WindowsAuthentiationProducerContext : IProducerContext
    {
        public string UserName
        {
            get {
                var name = System.Web.HttpContext.Current.User.Identity.Name;
                using (TurbineModelContainer container = new TurbineModelContainer())
                {
                    var u = container.Users.SingleOrDefault<User>(s => s.Name == name);
                    if (u == null)
                    {
                        Debug.WriteLine("Add User {0} to Database", GetType());
                        container.Users.AddObject(new User()
                        {
                            Name = name,
                            Token = ""
                        });
                        container.SaveChanges();
                    }
                }
                return name;
            }
        }
    }
}