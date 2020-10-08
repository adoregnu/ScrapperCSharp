using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

using SQLite.CodeFirst;
namespace Scrapper.Model
{
    using  DbInitializer = SqliteDropCreateDatabaseWhenModelChanges<MiscDbContext>;
    class MiscDbContext : DbContext
    {
        public MiscDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            MiscModelConfig.Config(modelBuilder);

            var initilizer = new DbInitializer(modelBuilder);
            Database.SetInitializer(initilizer);
        }

        public DbSet<AvTorrent> Torrents { get; set; }
    }
}
