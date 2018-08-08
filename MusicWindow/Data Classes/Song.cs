using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Glaxion.Tools;
using System.Drawing;
using TagLib;
using System.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;

//id3 info class example
//https://www.codeproject.com/Articles/17890/Do-Anything-With-ID

//ID3 lib
//https://sourceforge.net/projects/csid3lib/
//saving pictures to ID3
//https://stackoverflow.com/questions/25944311/write-picture-to-mp3-file-id3-net-windows-store-apps?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa


//https://stackoverflow.com/questions/68283/view-edit-id3-data-for-mp3-files?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa
//https://github.com/crowell/Linebeck/tree/master/ultraID3lib

//true peak analysis for regain
//https://github.com/audionuma/libtruepeak/tree/master/src/truepeak
//https://toneprints.com/media/1017421/lundt013011.pdf
//http://www.speech.kth.se/prod/publications/files/3319.pdf
//http://www.speech.kth.se/prod/publications/files/3319.pdf


//mp3 info
//https://github.com/moumar/ruby-mp3info
//https://stackoverflow.com/questions/13404957/loading-album-art-with-taglib-sharp-and-then-saving-it-to-same-different-file-in/13612644?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa

namespace Glaxion.Music
{
    public enum SongState
    {
        None,
        IsPlaying,
        WasPlaying,
        HasPlayed,
        Invalid,
        MissingTags,
        InvalidTag
    }

    public class SongEventArgs : PropertyChangedEventArgs
    {
        public SongEventArgs(string propertyName)
        : base(propertyName)
        {
            //Set enum
        }
        public SongEventArgs(string propertyName,object propertyValue)
        : base(propertyName)
        {
            PropertyValue = propertyValue;
            //Set enum
        }
        public object PropertyValue { get; set; }

    }

    public class Song : INotifyPropertyChanged
    {
        class ThreadInfo
        {
            public string Filepath { get; set; }
            public TagLib.File File { get; set; }
            public Song song { get; set; }
        }
        public string Filepath { get; set; }
        public string[] Genres { get; set; }
        //public string Genre { get { return Genres[0]; } set { Genres[0] = value; } }
        public string Album { get; set; }
        public string Artist { get; set; }
        public string Year { get; set; }
        public string Title { get; set; }
        public int trackNo;
        public uint Track { get; set; }
        public string folderImage;
        public Image image;
        public string Name { get; set; }
        public string comment;
        public IPicture[] Pictures { get; set; }
        public string Lyrics { get; set; }
        public string Length { get; set; }
        public TagLib.File file; //Dispose
        public bool loaded;
        bool dirty;
        SongState _state;
        public SongState State
        {
            get { return _state; }
            set { _state = value; OnPropertyChanged(value); }
        }
        
        public static List<string> TagLoadingLog = new List<string>();

        public Song(string filePath)
        {
            Album = "Untitled";
            Artist = "Someone";
            Year = "Unknown";
            //Genre = "A genre";
            Genres = new string[] { "Music Genre" };
            loaded = false;
            Filepath = filePath;
            Title = Path.GetFileNameWithoutExtension(filePath);
            if (!System.IO.File.Exists(filePath))
                State = SongState.Invalid;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(object propertyValue,[CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new SongEventArgs(propertyName,propertyValue));
        }
        

        bool isMP3()
        {
            byte[] buf = System.IO.File.ReadAllBytes(Filepath);
            if (buf[0] == 0xFF && (buf[1] & 0xF6) > 0xF0 && (buf[2] & 0xF0) != 0xF0)
                return true;

            return false;
        }
        
        /*
        public void ReadID3Async()
        {
            Task.Run(() => ReadID3Info()).ConfigureAwait(false);
        }
        */
        
        void CreateFileAsync()
        {
            //send the process file task to a threadpool
            ThreadPool.QueueUserWorkItem( new WaitCallback(ProcessFile),null);
        }

        private void ProcessFile(object a)
        {
            //ThreadInfo ti = a as ThreadInfo;
            //ti.File = TagLib.File.Create(ti.Filepath);
            Title = Path.GetFileNameWithoutExtension(Filepath);
            try
            {
                file = TagLib.File.Create(Filepath);
                file.GetTag(TagTypes.AllTags);

                Album = file.Tag.Album;
                Artist = file.Tag.FirstAlbumArtist;
                Track = file.Tag.Track;
                Lyrics = file.Tag.Lyrics;
                Pictures = file.Tag.Pictures;
                Year = file.Tag.Year.ToString();
                Length = file.Length.ToString();
                if (file.Tag.Genres.Length > 0)
                    Genres = file.Tag.Genres; //genre loading appers tp be broken
                if (!string.IsNullOrEmpty(file.Tag.Title) && !string.IsNullOrWhiteSpace(file.Tag.Title))
                    Title = file.Tag.Title;

                LoadAlbumArt();
                dirty = false;
            }
            catch (Exception e)
            {
                TagLoadingLog.Add(string.Concat("--> Failed to Get All Tags: \n", e.Message, "\n", Filepath));
                State = SongState.MissingTags;
            }
            finally
            {
                //investigate:  we dispose of the taglib file here 
                //but in the save function we don't reopen it
                if (file != null) file.Dispose();
            }
            loaded = true;
        }

        public void ReadID3Info()
        {
            if (loaded)
                return;
            if (tool.IsAudioFile(Filepath))
            {
                if (System.IO.File.Exists(Filepath))
                    CreateFileAsync();
            }
        }
        
        public bool SaveInfo()
        {
            try
            {
                //investigate:  see comment in process file
                //how come we don't need to create a new file
                //even though we disposed of it in ProcessFile()
                file.Save();
            }
            catch(Exception e)
            {
                TagLoadingLog.Add(e.Message);
                return false;
            }
            return true;
            // tool.show(3, "ID Tag Saved: ",path);
        }

        private void Reset()
        {
            loaded = false;
        }

        public void Reload()
        {
            Reset();
            ReadID3Info();
           // ReadID3Async();
        }

        public void SetTitle(string text)
        {
            if (file == null)
                return;
            file.Tag.Title = text;
            dirty = true;
        }

        public void SetAlbum(string text)
        {
            if (file == null)
                return;
            file.Tag.Album = text;
            dirty = true;
        }

        public void SetArtist(string text)
        {
            if (file == null)
                return;
            List<string> alb = file.Tag.AlbumArtists.ToList();
            alb.Clear();
            alb.Add(text);
            file.Tag.AlbumArtists = alb.ToArray();
            dirty = true;
        }

        public void SetGenre(string text)
        {
            if (file == null)
                return;
            List<string> gen = file.Tag.Genres.ToList();
            gen.Insert(0,text);
            file.Tag.Genres = gen.ToArray();
            dirty = true;
        }

        public void SetYear(uint num)
        {
            if (file == null)
                return;
            file.Tag.Year = num;
            dirty = true;
        }

        public void SetYear(string text)
        {
            if (file == null)
                return;
            uint result = 0;
            if(uint.TryParse(text,out result))
            {
                if(result != 0)
                    file.Tag.Year = result;
            }
            dirty = true;
        }

        public void SetTrack(uint num)
        {
            if (file == null)
                return;
            file.Tag.Track = num;
            dirty = true;
        }
        public void SetLyrics(string text)
        {
            if (file == null)
                return;
            file.Tag.Lyrics = text;
            dirty = true;
        }

        private byte[] imageToByteArray(Image imageIn)
        {
            Image img;
            using (MemoryStream ms = new MemoryStream())
            {
                img = new Bitmap(imageIn);
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        public void SetPictureFromImage(Image i)
        {
            if (file == null)
                return;
            if (i == null)
                return;

            byte[] image_bytes = imageToByteArray(i);
            if (image_bytes == null)
                return;
            image = i;
            file.Tag.Pictures = new TagLib.IPicture[]
            {
                new TagLib.Picture(new TagLib.ByteVector(image_bytes))
            };
        }

        public string GetFolderImage()
        {
            return folderImage = tool.GetFolderJPGFile(Filepath);
        }

        public Task LoadAlbumArtAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                LoadAlbumArt();
            });
        }

        //using streamn method for getting image byte[]
        //https://stackoverflow.com/questions/3801275/how-to-convert-image-to-byte-array/16576471#16576471
        public void LoadAlbumArt()
        {
            image = null;
            if (loaded && Pictures != null && Pictures.Length > 0)
            {
                byte[] bytes = Pictures[0].Data.Data;
                ImageConverter ic = new ImageConverter();
                image = (Image)ic.ConvertFrom(bytes);
                return;
            }

            string pic = GetFolderImage();
            if (pic == null)
            {
                string defaultimage = @"Resources\music_gui_logo.png";
                if(System.IO.File.Exists(defaultimage))
                    image = Image.FromFile(defaultimage);  //no extension needed
            }
            else
                image = Image.FromFile(pic);
        }
    }
}
