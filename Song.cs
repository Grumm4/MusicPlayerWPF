using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayerWPF
{
    internal class Song
    {
        //public int Id { get; set; }

        private readonly string name;

        private readonly string artist;

        private readonly string title;

        private readonly string path;

        public Song(string path)
        {
            try
            {
                this.path = path;
                name = Path.GetFileName(this.path).Split('.')[0];

                using (var audioFile = new AudioFileReader(path))
                {
                    var tagFile = TagLib.File.Create(path);
                    artist = tagFile.Tag.FirstPerformer;
                    title = tagFile.Tag.Title;
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Song Constructor");
            }
        }

        public string GetTitleAndArtist()
        {
            try
            {
                if (!string.IsNullOrEmpty(title) & !string.IsNullOrEmpty(artist))
                    return $"{artist} -- {title}";
                else
                    return name;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return "";
            }
        }
    }
}
