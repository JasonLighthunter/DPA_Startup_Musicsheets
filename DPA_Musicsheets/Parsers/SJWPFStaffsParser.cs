using PSAMControlLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPA_Musicsheets.Models;
using DPA_Musicsheets.Utility;

namespace DPA_Musicsheets.Parsers
{
    public class SJWPFStaffsParser : ISJParser<IEnumerable<MusicalSymbol>>
    {
        private Dictionary<SJClefTypeEnum, Clef> _clefDictionary { get; set; }
        private List<MusicalSymbol> _symbols { get; set; }

        public SJWPFStaffsParser()
        {
            _clefDictionary = new Dictionary<SJClefTypeEnum, Clef>()
            {
                {SJClefTypeEnum.Alto, null },
                {SJClefTypeEnum.Bass, new Clef(ClefType.FClef, 4) },
                {SJClefTypeEnum.Tenor, null },
                {SJClefTypeEnum.Treble, new Clef(ClefType.GClef, 2) },
                {SJClefTypeEnum.Undefined, null }
            };
        }

        public IEnumerable<MusicalSymbol> ParseFromSJSong(SJSong song)
        {
            _symbols = new List<MusicalSymbol>();

            _symbols.Add(_clefDictionary[song.ClefType]);
            _symbols.Add( new TimeSignature(
                    TimeSignatureType.Numbers,
                    song.TimeSignature.NumberOfBeatsPerBar,
                    song.TimeSignature.NoteValueOfBeat
                ));

            AddBars(song.Bars);

            return _symbols;
        }


        public SJSong ParseToSJSong(IEnumerable<MusicalSymbol> data)
        {
            throw new NotImplementedException();
        }

        private void AddBars(List<SJBar> bars)
        {
            foreach (var bar in bars)
            {
                AddNotes(bar.Notes);
                _symbols.Add(new Barline());
            }
        }

        private void AddNotes(List<SJBaseNote> notes)
        {   
            foreach(var note in notes)
            {
                var newSymbol = (note is SJRest) ? GetRestSymbol((SJRest)note) : GetNoteSymbol((SJNote)note);
                _symbols.Add(newSymbol);
            }
        }

        private MusicalSymbol GetRestSymbol(SJRest note)
        {
            var restLength = getBaseNoteLength(note.Duration);
            return new Rest((MusicalSymbolDuration)restLength);
        }

        private MusicalSymbol GetNoteSymbol(SJNote note)
        {
            int noteLength = getBaseNoteLength(note.Duration);

            var noteSymbol = new Note(
                note.Pitch.ToString(),
                note.PitchAlteration,
                note.Octave,
                (MusicalSymbolDuration)noteLength,
                NoteStemDirection.Up,
                NoteTieType.None,
                new List<NoteBeamType>() { NoteBeamType.Single }
            );
            noteSymbol.NumberOfDots = (int)note.NumberOfDots;

            return noteSymbol;
        }

        private int getBaseNoteLength(SJNoteDurationEnum duration)
        {
            return (int)(1 / EnumConverters.ConvertSJNoteDurationEnumToDouble(duration));
        }
    }
}
