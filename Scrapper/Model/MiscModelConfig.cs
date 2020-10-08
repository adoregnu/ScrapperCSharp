using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Scrapper.Model
{
    class MiscModelConfig
    {
        public static void Config(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AvTorrent>();
        } 
    }
}
