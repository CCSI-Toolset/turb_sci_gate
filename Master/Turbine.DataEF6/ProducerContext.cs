using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Turbine.DataEF6
{
    public class ProducerContext : DbContext
    {
        public DbSet<Turbine.Data.Entities.Application> Applications { get; set; }
        public DbSet<Turbine.Data.Entities.Simulation> Simulations { get; set; }
        public DbSet<Turbine.Data.Entities.Job> Jobs { get; set; }
        public DbSet<Turbine.Data.Entities.JobConsumer> Consumers { get; set; }
        public DbSet<Turbine.Data.Entities.Session> Sessions { get; set; }
        public DbSet<Turbine.Data.Entities.User> Users { get; set; }
        public DbSet<Turbine.Data.Entities.Message> Messages { get; set; }
        public DbSet<Turbine.Data.Entities.Process> Processes { get; set; }
        public DbSet<Turbine.Data.Entities.StagedInputFile> StagedInputFiles { get; set;  }
        public DbSet<Turbine.Data.Entities.StagedOutputFile> StagedOutputFiles { get; set;  }
        public DbSet<Turbine.Data.Entities.Generator> Generators { get; set; }
        public DbSet<Turbine.Data.Entities.GeneratorJob> GeneratorJobs { get; set; }

        public ProducerContext()
            : base("name=TurbineCompactDatabase")
        {
            bool success = this.Database.CreateIfNotExists();
            //Debug.WriteLine(String.Format("Database Created: {0}", success), this.GetType().Name);
        }
        /// <summary>
        /// Fluent API
        /// referencial relationship will result in a cyclical reference
        /// http://stackoverflow.com/questions/9859127/ef-code-first-table-relationships
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Turbine.Data.Entities.Simulation>().HasRequired(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserName).WillCascadeOnDelete(false);
            /*
            modelBuilder.Entity<Turbine.Data.Entities.Simulation>().HasRequired(x => x.Application)
               .WithMany()
               .HasForeignKey(x => x.Application).WillCascadeOnDelete(false);
            */

            /*
            modelBuilder.Entity<Turbine.Data.Entities.Session>()
                .HasOptional(s => s.Jobs)
                .WithMany()
                .WillCascadeOnDelete();
            */
            /*
            // job cascade delete
            var mb = modelBuilder.Entity<Turbine.Data.Entities.Job>();
            var option = mb.HasOptional(a => a.Process);
            var config = option.WithOptionalDependent();
            config.WillCascadeOnDelete(true);
            */
            /*
            var optionMessages = mb.HasMany(j => j.Messages);
            var configMessages = optionMessages.WithOptional();
            configMessages.WillCascadeOnDelete(true);
            */

            base.OnModelCreating(modelBuilder);
        }
    }
}
