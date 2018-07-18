using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWindow.ViewModel
{
    public class VMSongInfo
    {
        public VMSongInfo()
        {

        }

        public string Title {get; set;}
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Year { get; set; }
        public string Genre { get; set; }
        public byte[] Picture { get; set; }
    }
}
