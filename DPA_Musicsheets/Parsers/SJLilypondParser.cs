using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPA_Musicsheets.Models;

namespace DPA_Musicsheets.Parsers
{
    public class SJLilypondParser : ISJParser<string>
    {
        public string ParseFromSJSong(SJSong song)
        {
            throw new NotImplementedException();
        }

        public SJSong ParseToSJSong(string data)
        {
            SJSong song = new SJSong();

            return song;
        }
    }
}
