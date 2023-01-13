using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer.Classes
{
    public class Playlist
    {
        public string Name { get; set; }
        public List<MediaItem> Items { get; set; }

        public Playlist()
        {
            Items = new List<MediaItem>();
        }
    }
}
