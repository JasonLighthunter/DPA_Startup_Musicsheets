using DPA_Musicsheets.Models;
using DPA_Musicsheets.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Managers
{
    public class SJLilypondFileHandler : ISJFileHandler
    {
        private SJLilypondParser _lilypondParser;

        public SJLilypondFileHandler(SJLilypondParser lilypondParser)
        {
            _lilypondParser = lilypondParser;
        }

        public SJSong LoadSongFromFile(string fileName)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var line in File.ReadAllLines(fileName))
            {
                sb.AppendLine(line);
            }

            var lilypondText = sb.ToString();

            return LoadSongFromString(lilypondText);
        }

        public SJSong LoadSongFromString(string lilypondText)
        {
            SJSong song = _lilypondParser.ParseToSJSong(lilypondText);
            return song;
        }

    }
}
