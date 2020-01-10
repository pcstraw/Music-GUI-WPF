using Glaxion.Music;

namespace Glaxion.ViewModel
{
    public class VMMusicFiles : VMTree
    {
        public VMMusicFiles() : base()
        {
            //move to parametre
            fileLoader = MusicFileManager.Instance;
        }
    }
}
