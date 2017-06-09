using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Turbine.Data;

namespace Turbine.Console.Admin
{
    /*
     * Console appliation for creating and updating users.
     */
    class Program
    {
        // create a string identifier for the cache key
        // format: prefix + Base64(Hash(username+password))
        static private string GetIdentifier(string username, string password)
        {
            // use default hash algorithm configured on this machine via CryptoMappings
            // usually SHA1CryptoServiceProvider
            HashAlgorithm hash = HashAlgorithm.Create();

            string identifier = username + password;
            byte[] identifierBytes = Encoding.UTF8.GetBytes(identifier);
            byte[] identifierHash = hash.ComputeHash(identifierBytes);

            return Convert.ToBase64String(identifierHash);
        }

        // extracts and decodes username and password from the auth header
        static private string[] ExtractCredentials(string authHeader)
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

        static void Main(string[] args)
        {
            System.Console.Out.Write("Username: ");
            String username = System.Console.In.ReadLine();
            System.Console.Out.Write("Password: ");
            String password = System.Console.In.ReadLine();

            using (TurbineModelContainer container = new TurbineModelContainer())
            {
                User user = container.Users.FirstOrDefault<User>(s => s.Name == username);
                if (user != null)
                {
                    System.Console.Out.Write("Updating Password");
                    user.Token = password;
                }
                else
                {
                    System.Console.Out.Write("Creating new user");
                    user = new User();
                    //user.Id = Guid.NewGuid();
                    user.Name = username;
                    user.Token = password;
                    container.Users.AddObject(user);
                }
                container.SaveChanges();
            }
            System.Console.In.ReadLine();
        }
    }
}
