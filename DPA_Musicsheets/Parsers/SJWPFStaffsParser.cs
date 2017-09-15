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
            AddNotes(song.Notes, song.UnheardStartNote.Pitch, song.UnheardStartNote.Octave, ref symbols);

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

        private void AddNotes(List<SJBaseNote> notes, SJPitchEnum unheardStartNotePitch, int unheardStartNoteOctave, ref List<MusicalSymbol> symbols)
        {
            SJPitchEnum previousPitch = unheardStartNotePitch;
            int previousOctave = unheardStartNoteOctave;
            foreach(var note in notes)
            {
                if (note is SJRest)
                {
                    symbols.Add(GetRestSymbol((SJRest)note));
                }
                else
                {
                    symbols.Add(GetNoteSymbol((SJNote)note, previousPitch, previousOctave));
                    previousPitch = ((SJNote)note).Pitch;
                    previousOctave = ((SJNote)note).Octave;
                }
            }
        }

        private MusicalSymbol GetRestSymbol(SJRest note)
        {
            var restLength = getBaseNoteLength(note.Duration);
            return new Rest((MusicalSymbolDuration)restLength);
            
        }

        private MusicalSymbol GetNoteSymbol(SJNote note, SJPitchEnum previousPitch, int previousOctave)
        {
            // Length
            int noteLength = getBaseNoteLength(note.Duration);
            // Crosses and Moles
            int alter = note.PitchAlteration;
            // Octaves
            int distanceWithPreviousNote = notesorder.IndexOf(note.Pitch) - notesorder.IndexOf(previousPitch);
            if (distanceWithPreviousNote > 3) // Shorter path possible the other way around
            {
                distanceWithPreviousNote -= 7; // The number of notes in an octave
            }
            else if (distanceWithPreviousNote < -3)
            {
                distanceWithPreviousNote += 7; // The number of notes in an octave
            }

            if (distanceWithPreviousNote + notesorder.IndexOf(previousPitch) >= 7)
            {
                previousOctave++;
            }
            else if (distanceWithPreviousNote + notesorder.IndexOf(previousPitch) < 0)
            {
                previousOctave--;
            }

            // Force up or down.

            var noteSymbol = new Note(
                note.Pitch.ToString(), alter, previousOctave,
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
