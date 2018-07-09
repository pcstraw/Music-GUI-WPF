using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glaxion.Music
{
    public class Album
    {
        Album()
        {

        }

        void AddSong(Song s)
        {
            tracks.Add(s);
        }

        public List<Song> tracks = new List<Song>();
    }
}
