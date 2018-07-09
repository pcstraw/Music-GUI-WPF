using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Glaxion.Tools;
using System.Drawing;
using TagLib;

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
    public class Song
    {
        public string path;
        public string Filepath { get { return path; } set { path = value; } }
        public string genres;
        public string genre;
        public string album;
        public string artist;
        public string year;
        public string title;
        public int trackNo;
        public uint track;
        public string folderImage;
        public Image image;
        public string name;
        public string Name { get { return name; } set { name = value; } }
        public string comment;
        public IPicture[] pictures;
        public string lyrics;
        public string length;
        public TagLib.File file; //Dispose
        public bool loaded;
        public bool invalid;
        public static List<string> TagLoadingLog = new List<string>();
        
        public Song(string filePath)
        {
            album = "Untitled";
            artist = "Someone";
            year = "Unknown";
            genres = "A genre";
            loaded = false;
            invalid = false;
            path = filePath;
            name = Path.GetFileNameWithoutExtension(filePath);
            title = name;
        }

        private void Reset()
        {
            loaded = false;
            invalid = false;
        }

        //taglib will fail if it attempts to read a corrupted mp3 file
        bool isMP3()
        {
            byte[] buf = System.IO.File.ReadAllBytes(path);
            if (buf[0] == 0xFF && (buf[1] & 0xF6) > 0xF0 && (buf[2] & 0xF0) != 0xF0)
                return true;

            return false;
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
            byte[] image_bytes = imageToByteArray(i);
            if (image_bytes == null)
                return;
            image = i;
            file.Tag.Pictures = new TagLib.IPicture[]
            {
                new TagLib.Picture(new TagLib.ByteVector(image_bytes))
            };
        }

        private void TestTagLibLoading()
        {
            TagLib.File file = null;
            
            if (!System.IO.File.Exists(path))
            {
                tool.show(3, "Invalid path", path);
                return;
            }
            try
            {
                //if(isMP3())
                file = TagLib.File.Create(path);
            }
            catch(TagLib.CorruptFileException e)
            {
                tool.show(2,e.Message);
                return;
            }
            if (file == null)
                return;
            file.Mode = TagLib.File.AccessMode.Read;
            file.GetTag(TagTypes.AllTags);
        }

        public bool ReadID3Info()
        {
            if (loaded)
                return true;
            if (tool.IsAudioFile(path))
            {
               title = Path.GetFileNameWithoutExtension(path);
               try
               {
                    file = TagLib.File.Create(path);
                    file.GetTag(TagTypes.AllTags);
                    album = file.Tag.Album;
                    artist = file.Tag.FirstAlbumArtist;
                    track = file.Tag.Track;
                    lyrics = file.Tag.Lyrics;
                    pictures = file.Tag.Pictures;
                    year = file.Tag.Year.ToString();
                    length = file.Length.ToString();
                    if (!string.IsNullOrEmpty(file.Tag.FirstGenre))
                        genres = file.Tag.FirstGenre; //genre loading appers tp be broken
                    if (!string.IsNullOrEmpty(file.Tag.Title))
                        title = file.Tag.Title;
                    
                    return loaded = true;
                }
                catch (Exception e)
                {
                    TagLoadingLog.Add(string.Concat("--> Failed to Get All Tags: \n", e.Message, "\n", path));
                    invalid = true;
                    return loaded = false;
                }
                finally
                {
                    if (file != null) file.Dispose();
                }
            }
            invalid = true;
            return false;
        }

        public string GetFolderImage()
        {
            return folderImage = tool.GetFolderJPGFile(path);
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
            if (!loaded)
                return;

            image = null;
            if (pictures.Length > 0)
            {
                byte[] bytes = pictures[0].Data.Data;
                ImageConverter ic = new ImageConverter();
                image = (Image)ic.ConvertFrom(bytes);
            }
            if (image == null)
            {
                string pic = GetFolderImage();
                if (System.IO.File.Exists(pic))
                    image = Image.FromFile(pic);
                return;
            }
        }

        public void SaveInfo()
        {
            try
            {
                file.Save();
            }
            catch(Exception e)
            {
                TagLoadingLog.Add(e.Message);
                return;
            }
            Reset();
            ReadID3Info();
            // tool.show(3, "ID Tag Saved: ",path);
        }

        public void SetTitle(string text)
        {
            file.Tag.Title = text;
        }
        public void SetAlbum(string text)
        {
            file.Tag.Album = text;
        }
        public void SetArtist(string text)
        {
            List<string> alb = file.Tag.AlbumArtists.ToList();
            alb.Clear();
            alb.Add(text);
            file.Tag.AlbumArtists = alb.ToArray();
        }
        public void SetGenre(string text)
        {
            List<string> gen = file.Tag.Genres.ToList();
            gen.Clear(); //force the array to reset
            gen.Add(text);
            file.Tag.Genres = gen.ToArray();
            //genres = text;
        }
        public void SetYear(uint num)
        {
            file.Tag.Year = num;   
        }
        public void SetYear(string text)
        {
            uint result = 0;
            if(uint.TryParse(text,out result))
            {
                if(result != 0)
                {
                    file.Tag.Year = result;
                }
            }
        }
        public void SetTrack(uint num)
        {
            file.Tag.Track = num;
        }
        public void SetLyrics(string text)
        {
            file.Tag.Lyrics = text;
        }
    }
}
