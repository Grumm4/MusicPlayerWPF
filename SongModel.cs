using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayerWPF
{
    internal class SongModel
    {
        private string _title;
        private string _artist;
        private string _album;
        private string _duration;

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        public string Artist
        {
            get { return _artist; }
            set { _artist = value; }
        }

        public string Album
        {
            get { return _album; }
            set { _album = value; }
        }

        public string Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }
    }
}
