using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Data.Entity;

namespace TheWorld.Models
{
    // The starting point for data access and data storage for EF
    public class WorldContext : IdentityDbContext<WorldUser>
    {
        public WorldContext()
        {
            Database.EnsureCreated();
        }

        public DbSet<Trip> Trips { get; set; }
        public DbSet<Stop> Stops { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = Startup.Configuration["Data:WorldContextConnection"];
            optionsBuilder.UseSqlServer(connectionString);

            base.OnConfiguring(optionsBuilder);
        }
    }
}
