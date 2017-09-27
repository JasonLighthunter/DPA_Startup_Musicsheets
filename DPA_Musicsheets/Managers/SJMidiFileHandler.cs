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
        public Sequence MidiSequence { get; set; }
		public SJSong Song { get; set; }

        private SJMidiStateHandler midiStateHandler = new SJMidiStateHandler();
		private SJMidiParser _midiParser;

        public SJMidiFileHandler(SJMidiParser midiParser)
        {
            _midiParser = midiParser;
        }

        public SJSong LoadSong(string fileName)
        {
            MidiSequence = new Sequence();
            MidiSequence.Load(fileName);
            midiStateHandler.UpdateData(MidiSequence);
			Song = _midiParser.ParseToSJSong(MidiSequence);
            return Song;
        }
    }
}
