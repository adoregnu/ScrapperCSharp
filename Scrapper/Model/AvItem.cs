using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

using SQLite.CodeFirst;

namespace Scrapper.Model
{
    public class AvGenre
    {
        [Key]
        [Autoincrement]
        public int Id { get; set; }

        public string Name { get; set; }
        public virtual ICollection<AvItem> Items { get; set; }
    }

    public class AvActorName
    { 
        [Key]
        [Autoincrement]
        public int Id { get; set; }

        public string Name { get; set; }
        public AvActor Actor { get; set; }
    }

    public class AvActor
    { 
        [Key]
        [Autoincrement]
        public int Id { get; set; }

        public string PicturePath { get; set; }
        public ICollection<AvActorName> Names { get; set; }
        public virtual ICollection<AvItem> Items { get; set; }
    }

    public class AvStudio
    { 
        [Key]
        [Autoincrement]
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class AvItem
    {
        [Key]
        [Autoincrement]
        public int Id { get; set; }

        [Required]
        public string Pid { get; set; }

        [MaxLength(512)]
        public string Title { get; set; }

        public bool Sensored { get; set; }
        public DateTime ReleaseDate { get; set; }
        public AvStudio Studio { get; set; }

        [Required]
        public string Path { get; set; }

        public string Set { get; set; }

        [MaxLength(1024)]
        public string Plot { get; set; }
        //public string Poster { get; set; }
        //public string Screenshots { get; set; }
        public float Rating { get; set; }
        public virtual ICollection<AvGenre> Genres { get; set; }
        public virtual ICollection<AvActor> Actors { get; set; }
    }
}
