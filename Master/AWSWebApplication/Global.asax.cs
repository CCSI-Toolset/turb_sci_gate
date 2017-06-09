using System;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Routing;
using System.Diagnostics;
using System.Security.Principal;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Practices.Unity;
using Turbine.Web;


namespace AWSWebApplication
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            Debug.WriteLine("Application_Start", GetType());
            RouteTable.Routes.Add(new ServiceRoute("application", new WebServiceHostFactory(), typeof(Turbine.Web.Resources.ApplicationResource)));
            RouteTable.Routes.Add(new ServiceRoute("simulation", new WebServiceHostFactory(), typeof(Turbine.Web.Resources.SimulationResource)));
            RouteTable.Routes.Add(new ServiceRoute("job", new WebServiceHostFactory(), typeof(Turbine.Web.Resources.JobResource)));
            RouteTable.Routes.Add(new ServiceRoute("session", new WebServiceHostFactory(), typeof(Turbine.Web.Resources.SessionResource)));
            RouteTable.Routes.Add(new ServiceRoute("consumer", new WebServiceHostFactory(), typeof(Turbine.Web.Resources.ConsumerResource)));
        }

        void Application_AuthenticateRequest(Object sender, EventArgs e)
        {
            Debug.WriteLine("Application_AuthenticateRequest", GetType());
            if (! AuthenticateUser())
            {
                SendAuthenticationHeader();
                HttpContext.Current.Response.End();
            }
        }

        //void Application_EndRequest(Object sender, EventArgs e)
        //{
        //}

        // create GenericPrincipal and set it on Context.User
        private static void SetPrincipal(string username)
        {
            // create principal and set Context.User
            GenericIdentity id = new GenericIdentity(username);
            GenericPrincipal p = new GenericPrincipal(id, null);
            HttpContext.Current.User = p;
            Debug.WriteLine("GenericIdentity: " + id.GetHashCode(), "Turbine.Web.Global");
        }

        // send header to start Basic Authentication handshake
        private void SendAuthenticationHeader()
        {
            HttpContext context = HttpContext.Current;
            String realm = "turbineRealm";
            //Debug.WriteLine(">> SendAuthenticationHeader", GetType());
            context.Response.StatusCode = 401;
            context.Response.AddHeader(
                "WWW-Authenticate",
                String.Format("Basic realm=\"{0}\"", realm));
        }

        private bool AuthenticateUser()
        {
            string username = "", password = "";
            string authHeader = HttpContext.Current.Request.Headers["Authorization"];
            //Debug.WriteLine(">> AuthenticateUser", GetType());
            if (authHeader != null && authHeader.StartsWith("Basic"))
            {
                // extract credentials from header
                string[] credentials = ExtractCredentials(authHeader);
                username = credentials[0];
                password = credentials[1];
                //Debug.WriteLine(">> tokens: " + username + ":" + password, GetType());
                if (Turbine.Producer.Data.Contract.AuthenticateCredentials.Check(username, password))
                {
                    SetPrincipal(username);
                    return true;
                }
            }

            return false;
        }

        // extracts and decodes username and password from the auth header
        private string[] ExtractCredentials(string authHeader)
        {
            // strip out the "basic"
            string encodedUserPass = authHeader.Substring(6).Trim();

            // that's the right encoding
            Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            string userPass = encoding.GetString(Convert.FromBase64String(encodedUserPass));
            int separator = userPass.IndexOf(':');

            string[] credentials = new string[2];
            credentials[0] = userPass.Substring(0, separator);
            credentials[1] = userPass.Substring(separator + 1);

            return credentials;
        }

        // create a string identifier for the cache key
        // format: prefix + Base64(Hash(username+password))
        private string GetIdentifier(string username, string password)
        {
            // use default hash algorithm configured on this machine via CryptoMappings
            // usually SHA1CryptoServiceProvider
            HashAlgorithm hash = HashAlgorithm.Create();

            string identifier = username + password;
            byte[] identifierBytes = Encoding.UTF8.GetBytes(identifier);
            byte[] identifierHash = hash.ComputeHash(identifierBytes);

            return Convert.ToBase64String(identifierHash);
        }
    }
}
