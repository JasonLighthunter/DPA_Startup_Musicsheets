using PSAMControlLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPA_Musicsheets.Models;

namespace DPA_Musicsheets.Parsers
{
    public class SJWPFStaffsParser : ISJParser<IEnumerable<MusicalSymbol>>
    {
        public IEnumerable<MusicalSymbol> ParseFromSJSong(SJSong song)
        {
            List<MusicalSymbol> symbols = new List<MusicalSymbol>();

            symbols.Add(GetClefSymbol(song.ClefType));
            symbols.Add(new TimeSignature(TimeSignatureType.Numbers, song.TimeSignature.NumberOfBeatsPerBar, song.TimeSignature.NoteValueOfBeat));

            AddNotes(song.Notes, out List<MusicalSymbol> symbols);

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

        private void AddNotes(List<SJBaseNote> notes, out List<MusicalSymbol> symbols)
        {
            throw new NotImplementedException();
        }
    }
}
