using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrapper.Model
{
    class AvModelConfig
    {
        public static void Config(DbModelBuilder modelBuilder)
        {
            ConfigActorName(modelBuilder);
            ConfigActor(modelBuilder);
            ConfigStudio(modelBuilder);
            ConfigGenre(modelBuilder);
            ConfigItem(modelBuilder);
        }

        static void ConfigActorName(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AvActorName>();
        }

        static void ConfigActor(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AvActor>();
        }
        static void ConfigStudio(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AvStudio>();
        }
        static void ConfigGenre(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AvGenre>();
        }
        static void ConfigItem(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AvItem>();
        }
    }
}
