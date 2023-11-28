using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MusicPlayerWPF
{
    internal class Song
    {
        //public int Id { get; set; }

        private readonly string _name;

        public string artist = "Unknown Artist";

        public string album = "Unknown Album";

        private readonly string _path;

        private readonly TimeSpan _totalduration;

        private int minutes;
        
        private int seconds;

        
        public Song(string path)
        {

            try
            {
                this._path = path;
                _name = Path.GetFileName(this._path).Split('.')[0];

                using (var audioFile = new AudioFileReader(path))
                {
                    var tagFile = TagLib.File.Create(path);
                    if (!string.IsNullOrEmpty(tagFile.Tag.FirstPerformer))
                    {
                        artist = tagFile.Tag.FirstPerformer;
                    }
                    if (!string.IsNullOrEmpty(tagFile.Tag.Album))
                    {
                        album = tagFile.Tag.Album;
                    }
                    minutes = tagFile.Properties.Duration.Minutes;
                    seconds = tagFile.Properties.Duration.Seconds;
                }
                //ParseSongDuration(out minutes, out seconds, _totalduration);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Song Constructor");
            }
        }

        private void ParseSongDuration(out int min, out int sec, TimeSpan totalduration)
        {
            sec = totalduration.Seconds;
            min = totalduration.Minutes;
        }

        //public string GetTitle()
        //{
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(album) & !string.IsNullOrEmpty(artist))
        //            return $"{artist} -- {album}";
        //        else
        //        {
        //            artist = "Unknown Artist";
        //            album = "Unknown Album";
        //            return _name;
        //        }    
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Windows.Forms.MessageBox.Show(ex.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
        //        return "";
        //    }
        //}

        public string GetSongName()
        {
            return _name;
        }

        public string GetMinSec() 
        {
            return $"{minutes}:{seconds}";
        }
    }
}
