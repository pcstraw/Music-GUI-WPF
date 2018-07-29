﻿using System;
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

        public override void SelectAndLoadDirectory(string dialogBrowserTile)
        {
            base.SelectAndLoadDirectory(dialogBrowserTile);
        }

        public override bool AddDirectory(string dir)
        {
            if (!HasPlaylistFiles(dir))
                return false;
            if (!fileLoader.AddDirectory(dir))
                return false;
            if (fileLoader.Directories.Count > 0)
                Playlist.DefaultDirectory = fileLoader.Directories[0];
            return true;
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
