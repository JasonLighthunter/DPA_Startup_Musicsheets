using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPA_Musicsheets.Parsers;
using DPA_Musicsheets.Models;
using PSAMControlLibrary;

namespace DPA_Musicsheets.Managers
{
    public class SJMidiFileHandler : ISJFileHandler
    {
        private SJMidiParser _midiParser;

        public SJMidiFileHandler(SJMidiParser midiParser)
        {
            _midiParser = midiParser;
        }

        public SJSong LoadSongFromFile(string fileName)
        {
            Sequence midiSequence = new Sequence();
            midiSequence.Load(fileName);
            SJSong song = _midiParser.ParseToSJSong(midiSequence);
            return song;
        }
    }
}
