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
        public string LilypondText;
        public SJSong Song;

        private SJLilypondStateHandler lilypondStateHandler = new SJLilypondStateHandler();
        private SJLilypondParser _lilypondParser;

        public SJLilypondFileHandler(SJLilypondParser lilypondParser)
        {
            _lilypondParser = lilypondParser;
        }

        public SJSong LoadSong(string fileName)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var line in File.ReadAllLines(fileName))
            {
                sb.AppendLine(line);
            }

            LilypondText = sb.ToString();

            
            lilypondStateHandler.UpdateData(LilypondText);
            Song = _lilypondParser.ParseToSJSong(LilypondText);
            return Song;
        }

    }
}
