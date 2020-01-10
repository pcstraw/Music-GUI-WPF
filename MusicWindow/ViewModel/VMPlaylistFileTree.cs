using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glaxion.Music;
using Glaxion.Tools;

namespace Glaxion.ViewModel
{
    public class VMPlaylistFileTree : VMTree
    {
        public VMPlaylistFileTree() : base()
        {
            fileLoader = PlaylistFileManager.Instance;
        }

        public override List<FileDirectory> SelectAndLoadDirectory(string dialogBrowserTile)
        {
            return base.SelectAndLoadDirectory(dialogBrowserTile);
        }

        public override FileDirectory AddDirectory(string dir)
        {
            if (!HasPlaylistFiles(dir))
                return null;
            FileDirectory fd = fileLoader.AddDirectory(dir);
            if (fileLoader.Directories.Count > 0)
                Playlist.DefaultDirectory = fileLoader.Directories[0].directory;
            return fd;
        }

        bool HasPlaylistFiles(string path)
        {
            try
            {
                foreach (string s in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
                {
                    if (tool.IsPlaylistFile(s))
                        return true;
                }
                return false;
            }
            catch(Exception e)
            {
                tool.show(2,e.Message);
                return false;
            }
        }
    }
}
