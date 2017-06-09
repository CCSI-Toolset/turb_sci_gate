using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Collections.Specialized;
using Turbine.Consumer.AWS;
using System.Diagnostics;

namespace AWSTestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            NameValueCollection appConfig = ConfigurationManager.AppSettings;
            /*
            SQSContext.AccessKey = appConfig["AWSAccessKey"];
            SQSContext.Secretkey = appConfig["AWSSecretKey"];
            SQSContext.Region = appConfig["Region"];
            SQSContext.InstanceID = "awstestconsole";
             */
            string xml = "<bla xmlns=\"https://whatever\">hi</bla>";
            Console.WriteLine("testing");

            for (var i = 0; i < 3; i++)
            {
                var d1 = DateTime.Now;
                Debug.WriteLine("HI");
                var d2 = DateTime.Now;
                Console.WriteLine("Time: " + (d2 - d1));
                d1 = DateTime.Now;
                Debug.WriteLine(xml);
                d2 = DateTime.Now;
                Console.WriteLine("Time: " + (d2 - d1));
                
            }
            Console.ReadLine();
        }
    }
}
