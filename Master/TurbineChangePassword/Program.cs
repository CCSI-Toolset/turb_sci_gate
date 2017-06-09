using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using Turbine.Data;
using Turbine.DataEF6;

namespace TurbineChangePassword
{
    class Program
    {
        static Boolean verbose = false;
        class Options
        {
            [Option('u', "username", Required = true,
              HelpText = "Username")]
            public string Username { get; set; }

            [Option('p', "password", Required = true,
              HelpText = "Password")]
            public string Password { get; set; }

            [Option('v', "verbose", DefaultValue = true,
              HelpText = "Prints all messages to standard output.")]
            public bool Verbose { get; set; }

            [ParserState]
            public IParserState LastParserState { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                return HelpText.AutoBuild(this,
                  (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
            }
        }

        static bool UpdateUser(string username, string token)
        {
            using (ProducerContext container = new ProducerContext())
            {
                Turbine.Data.Entities.User user = container.Users.SingleOrDefault<Turbine.Data.Entities.User>(s => s.Name == username);
                if (user == null) return false;
                user.Token = token;
                container.SaveChanges();
            }
            return true;
        }

        static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                // Values are available here
                verbose = options.Verbose;
                if (verbose) Console.WriteLine("Changing Password for User: {0}", options.Username);
                int saltSize = 12;
                byte[] data = new byte[saltSize];
                var provider = new System.Security.Cryptography.RNGCryptoServiceProvider();
                provider.GetBytes(data);
                string salt = System.Text.UTF8Encoding.UTF8.GetString(data);                
                string token = Turbine.Security.AuthenticateCredentials.
                    CreateFormattedToken("pbkdf2_sha256", 12000, salt, options.Password);
                if (verbose) Console.WriteLine("Create Security Token: {0}", token);

                if (UpdateUser(options.Username, token) == false)
                {
                    Console.WriteLine("User {0} does not exist", options.Username);
                    Environment.ExitCode = 1;
                }
            }
        }
    }
}
