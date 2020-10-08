using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using SQLite.CodeFirst;


namespace Scrapper.Model
{
    class MiscItem
    {
    }

    public class AvTorrent
    {
        [Key]
        [Autoincrement]
        public int Id { get; set; }

        public string Pid { get; set; }
    }
}
