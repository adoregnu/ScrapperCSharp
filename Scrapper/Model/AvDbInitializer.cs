using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SQLite.CodeFirst;

namespace Scrapper.Model
{
    class AvDbInitializer : SqliteDropCreateDatabaseWhenModelChanges<AvDbContext>
    {
        public AvDbInitializer(DbModelBuilder modelBuilder)
            : base(modelBuilder)
        {
        }
    }
}
