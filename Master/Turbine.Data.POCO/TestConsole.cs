using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Npgsql;
using Turbine.Data.POCO.Domain;
using NHibernate.Criterion;


namespace Turbine.Data.POCO
{
    public class TestConsole
    {
        static void TestConnection()
        {
            //var conn = new NpgsqlConnection("Server=50.18.153.83;Port=5432;User Id=uturbine;Password=X3n0F0b3;Database=turbine");
            var conn = new NpgsqlConnection("Server=dudley.lbl.gov;Port=5432;User Id=uturbine;Password=X3n0F0b3;Database=turbineMS10");
            conn.Open();
            var command = new NpgsqlCommand("select * from Jobs WHERE ID=1", conn);
            try
            {
                var dr = command.ExecuteReader();
                while (dr.Read())
                {
                    for (var i = 0; i < dr.FieldCount; i++)
                    {
                        Console.WriteLine("{0}", dr[i]);
                    }
                }
            }
            finally
            {
                conn.Close();
            }
        }
        static void TestNHibernate()
        {
            //var dialect = new NHibernate.Dialect.PostgreSQLDialect();
            //var driver = new NHibernate.Driver.NpgsqlDriver();
            //var factory = System.Data.Common.DbProviderFactories.GetFactory("Npsql");

            var config = new NHibernate.Cfg.Configuration();
            config.Configure();
            config.AddAssembly(typeof(Job).Assembly);

            var sessionFactory = config.BuildSessionFactory();

            using (NHibernate.ISession session = sessionFactory.OpenSession())
            {
                using (NHibernate.ITransaction trans = session.BeginTransaction())
                {
                    //Job job = session.Get<Job>(1);
                    var jobs = session
                                       .CreateCriteria<Job>()
                                       .Add(Restrictions.Eq("state", "success"))
                                       .SetMaxResults(5)
                                       .List<Job>();
                    System.Console.WriteLine("======================================");
                    System.Console.WriteLine("Jobs");
                    foreach (Job job in jobs)
                    {
                        System.Console.WriteLine(job);
                        System.Console.WriteLine("Id -- {0}", job.Id);

                        System.Console.WriteLine("User -- {0}", job.user.name);
                        System.Console.WriteLine("Session -- {0}", job.session.Id);
                        System.Console.WriteLine("Simulation -- {0}", job.simulation.name);
                        if (job.state != "create")
                            System.Console.WriteLine("Consumer -- {0}", job.consumer.Id);
                        if (job.state == "success")
                            System.Console.WriteLine("Process -- {0}", job.process.Id);

                        System.Console.WriteLine("state -- {0}", job.state);
                        System.Console.WriteLine("create -- {0}", job.create);
                        System.Console.WriteLine("submit -- {0}", job.submit);
                        System.Console.WriteLine("setup -- {0}", job.setup);
                        System.Console.WriteLine("running -- {0}", job.running);
                        System.Console.WriteLine("finished -- {0}", job.finished);
                        System.Console.WriteLine("initialize -- {0}", job.initialize);
                    }

                    foreach (Simulation sim in session.CreateCriteria<Simulation>().List<Simulation>()) 
                    {
                        System.Console.WriteLine("{0}) {1}", sim.Id, sim.name);
                        System.Console.WriteLine("configuration -- {0}", sim.configuration);
                        System.Console.WriteLine("backup -- {0}", sim.backup);
                        System.Console.WriteLine("defaults -- {0}", System.Text.Encoding.ASCII.GetString(sim.defaults));
                    }
                }
            }
        }

        static void Generate()
        {
            var cfg = new NHibernate.Cfg.Configuration();
            cfg.Configure();
            cfg.AddAssembly(typeof(Job).Assembly);
            //var n = new NHibernate.Tool.hbm2ddl.SchemaExport(cfg).Execute(false, true, false, false);
            var export = new NHibernate.Tool.hbm2ddl.SchemaExport(cfg);
            export.Create(false, true);
        }
        static void Main(string[] args)
        {

            //TestConnection();
            //TestNHibernate();
            Generate();
            //System.Console.Out.Write("<Return>");
            //System.Console.In.ReadLine();
        }
    }
}
