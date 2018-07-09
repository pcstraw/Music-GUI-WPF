using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glaxion.Music;
using Glaxion.Tools;

namespace Glaxion.ViewModel
{
    public class VMMusicFiles : VMTree
    {
        public VMMusicFiles() : base()
        {
            fileLoader = MusicFileManager.Instance;
        }
    }
}
