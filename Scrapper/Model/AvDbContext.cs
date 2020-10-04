using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrapper.Model
{
    public class AvDbContext : DbContext
    {
        public AvDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            Configuration.ProxyCreationEnabled = true;
            Configuration.LazyLoadingEnabled = true;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            AvModelConfig.Config(modelBuilder);

            var initilizer = new AvDbInitializer(modelBuilder);
            Database.SetInitializer(initilizer);
        }

        public DbSet<AvActorName> ActorNames { get; set; }
        public DbSet<AvActor> Actors { get; set; }
        public DbSet<AvGenre> Genres { get; set; }
        public DbSet<AvStudio> Studios { get; set; }
        public DbSet<AvItem> Items { get; set; }
    }
}
