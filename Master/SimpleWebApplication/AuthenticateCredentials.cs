using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Turbine.DataEF6;
using Turbine.Data.Entities;

namespace SimpleWebApplication
{
    public class AuthenticateCredentials
    {
        static private Dictionary<String,DateTime> userCache = new Dictionary<String,DateTime>();
        static private int cacheRefreshSec = 60;

        static public bool Check(string username, string password) 
        {
            bool is_authorized = false;
            DateTime now = DateTime.UtcNow;    
            string key = String.Format("{0}:{1}", username, password);
            if (userCache.ContainsKey(key) && userCache[key] > now)
            {
                Debug.WriteLine("user cache accessed: " + username, "AuthenticateCredentials");
                return true;
            }
            using (var db = new ProducerContext())
            {
                // Assuming User.Name is unique
                // Unfortunately cannot specify unique in EF
                //User user = container.Users.FirstOrDefault<User>(
                //    s => s.Name == username & s.Token == password);
                //User user = container.Users.SingleOrDefault<User>(s => s.Name == username);
                User user = db.Users.SingleOrDefault<User>(s => s.Name == username);
                if (user != null)
                {
                    Debug.WriteLine("check token: " + username, "AuthenticateCredentials");
                    is_authorized = Turbine.Security.AuthenticateCredentials.CheckToken(password, user.Token);
                    if (is_authorized) userCache[key] = now.AddSeconds(cacheRefreshSec);
                }
                return is_authorized;
            }
        }
   
    }
}
