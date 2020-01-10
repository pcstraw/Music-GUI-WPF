using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Glaxion.Tools;

namespace Glaxion.Music
{
    public class MusicFileManager : FileManager
    {
        private MusicFileManager() : base()
        {
        }

        public static MusicFileManager Instance { get { return Nested.instance; } }
        private class Nested
        {
            static Nested() { }//lazy singleton
            internal static readonly MusicFileManager instance = new MusicFileManager();
        }

        public override List<string> LoadFiles(string directory)
        {
            List<string> list = tool.LoadAudioFiles(directory, SearchOption.AllDirectories);
            AddFiles(list);
            return list;
        }

        public void AddFiles(IEnumerable<string> files)
        {
            
            Files.AddRange(files);
        }
    }
}
