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
        private static List<SJPitchEnum> notesorder = new List<SJPitchEnum> {
            SJPitchEnum.C, SJPitchEnum.D, SJPitchEnum.E, SJPitchEnum.F, SJPitchEnum.G, SJPitchEnum.A, SJPitchEnum.B
        };

        public IEnumerable<MusicalSymbol> ParseFromSJSong(SJSong song)
        {
            List<MusicalSymbol> symbols = new List<MusicalSymbol>();

            symbols.Add(GetClefSymbol(song.ClefType));
            symbols.Add(new TimeSignature(TimeSignatureType.Numbers, song.TimeSignature.NumberOfBeatsPerBar, song.TimeSignature.NoteValueOfBeat));
            AddBars(song.Bars, ref symbols);

            return symbols;
        }


        public SJSong ParseToSJSong(IEnumerable<MusicalSymbol> data)
        {
            throw new NotImplementedException();
        }

        private MusicalSymbol GetClefSymbol(SJClefTypeEnum clefValue)
        {
            switch (clefValue)
            {
                case SJClefTypeEnum.Treble:
                    return new Clef(ClefType.GClef, 2);
                case SJClefTypeEnum.Bass:
                    return new Clef(ClefType.FClef, 4);
                case SJClefTypeEnum.Alto:
                case SJClefTypeEnum.Tenor:
                default:
                    throw new NotSupportedException($"Clef {clefValue.ToString()} is not supported.");
            }
        }

        private void AddBars(List<SJBar> bars, ref List<MusicalSymbol> symbols)
        {
            foreach (var bar in bars)
            {
                AddNotes(bar.Notes, ref symbols);
                symbols.Add(new Barline());
            }
        }

        private void AddNotes(List<SJBaseNote> notes, ref List<MusicalSymbol> symbols)
        {   
            foreach(var note in notes)
            {
                if (note is SJRest)
                {
                    symbols.Add(GetRestSymbol((SJRest)note));
                }
                else
                {
                    symbols.Add(GetNoteSymbol((SJNote)note));
                }
            }
        }

        private MusicalSymbol GetRestSymbol(SJRest note)
        {
            var restLength = getBaseNoteLength(note.Duration);
            return new Rest((MusicalSymbolDuration)restLength);
            
        }

        private MusicalSymbol GetNoteSymbol(SJNote note)
        {
            // Length
            int noteLength = getBaseNoteLength(note.Duration);
            // Crosses and Moles
            int alter = note.PitchAlteration;

            var noteSymbol = new Note(
                note.Pitch.ToString(), alter, note.Octave,
                (MusicalSymbolDuration)noteLength, NoteStemDirection.Up, NoteTieType.None,
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
