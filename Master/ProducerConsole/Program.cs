using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Turbine.Data.Contract;
using Turbine.Data.Contract.Behaviors;
using Newtonsoft.Json;

namespace Turbine.ProducerConsole
{

    class Program
    {
        public static void Main(string[] args)
        {
            string line = null;
            while (line != "quit" && line != "exit")
            {
                Console.WriteLine("==============================");
                Console.WriteLine("  Producer Console");
                Console.WriteLine("      log --");
                Console.WriteLine("      consumers --");
                Console.WriteLine("      quit -- ");
                Console.WriteLine("==============================");
                Console.Write(">> ");
                line = Console.ReadLine();
                if (line == "log")
                {
                    Console.WriteLine("Enter Instance-ID");
                    PrintLog(Console.ReadLine());
                }
                else if (line == "consumers") PrintConsumers();
            }
        }

        private static void PrintConsumers()
        {
            string connectionString = "mongodb://coltrane.lbl.gov";
            var server = MongoDB.Driver.MongoServer.Create(connectionString);
            var db = server.GetDatabase("turbine");
            string msg = "";
            foreach (var i in db.GetCollectionNames())
                msg += i;
            Console.WriteLine(msg);
        }

        private static void PrintLog(string instanceID)
        {
            string connectionString = "mongodb://coltrane.lbl.gov";
            var server = MongoDB.Driver.MongoServer.Create(connectionString);
            var db = server.GetDatabase("turbine");
            var col = db.GetCollection(instanceID);
            Console.WriteLine("Log For " + instanceID + "(" + col.Count() + ")");
            foreach (MongoDB.Bson.BsonDocument bdoc in col.FindAll())
            {
                Console.WriteLine(bdoc.GetElement("message"));
            }
        }
    }
}
