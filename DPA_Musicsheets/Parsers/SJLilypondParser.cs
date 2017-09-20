using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPA_Musicsheets.Models;
using DPA_Musicsheets.Utility;

namespace DPA_Musicsheets.Parsers
{
    public class SJLilypondParser : ISJParser<string>
    {
        public string ParseFromSJSong(SJSong song)
        {
            StringBuilder lilypondContent = new StringBuilder();
            lilypondContent.AppendLine(GetOctaveEntry(song.UnheardStartNote));
            lilypondContent.AppendLine(GetClef(song.ClefType));
            lilypondContent.AppendLine(GetTimeSignature(song.TimeSignature));
            lilypondContent.AppendLine(GetTempo(song.Tempo));

            int previousOctave = song.UnheardStartNote.Octave;
            foreach (SJBar bar in song.Bars)
            {
                lilypondContent.AppendLine(GetBar(bar, ref previousOctave));
            }

            lilypondContent.AppendLine("}");

            return lilypondContent.ToString();
        }

        public SJSong ParseToSJSong(string data)
        {
            SJSong song = new SJSong();

            return song;
        }

        private string GetOctaveEntry(SJNote unheardStartNote)
        {
            string octaveEntry = "\\relative ";

            octaveEntry = octaveEntry + unheardStartNote.Pitch.ToString().ToLower();

            octaveEntry = octaveEntry + " {";
            return octaveEntry;

        }

        private string GetClef(SJClefTypeEnum clefType)
        {
            string clef = "\\clef ";
            clef = clef + clefType.ToString().ToLower();
            return clef;
        }

        private string GetTimeSignature(SJTimeSignature timeSignature)
        {
            string timeSignatureString = "\\time ";
            string beatsPerBar = timeSignature.NumberOfBeatsPerBar.ToString();
            string noteValueOfBeat = timeSignature.NoteValueOfBeat.ToString();

            timeSignatureString = timeSignatureString + beatsPerBar + "/" + noteValueOfBeat;

            return timeSignatureString;
        }

        private string GetTempo(ulong tempo)
        {
            string tempoString = "\\tempo 4 =";
            tempoString = tempoString + tempo.ToString();
            return tempoString;
        }

        private string GetBar(SJBar bar, ref int previousOctave)
        {
            string barString = "";
            foreach (var note in bar.Notes)
            {
                if (note is SJRest)
                {
                    barString = barString + GetRest((SJRest)note);
                }
                else
                {
                    barString = barString + GetNote((SJNote)note, ref previousOctave);
                }
                barString = barString + GetDuration(note.Duration, note.NumberOfDots);
                barString = barString + " ";
            }

            barString = barString + "|";
            return barString;
        }

        private string GetRest(SJRest rest)
        {
            string restString = "r";
            return restString;
        }

        private string GetNote(SJNote note, ref int previousOctave)
        {
            string noteString;
            noteString = note.Pitch.ToString().ToLower();
            noteString = noteString + GetAlteration(note);

            noteString = noteString + GetOctaveDifference(note, ref previousOctave);

            return noteString;
        }

        private string GetOctaveDifference(SJNote note, ref int previousOctave)
        {
            string octaveDifferenceString = "";
            int octaveDifference = previousOctave - note.Octave;

            while (octaveDifference > 0)
            {
                octaveDifferenceString = octaveDifferenceString + "'";
                octaveDifference--;
            }
            while (octaveDifference < 0)
            {
                octaveDifferenceString = octaveDifferenceString + ",";
                octaveDifference++;
            }

            previousOctave = note.Octave;

            return octaveDifferenceString;

        }

        private string GetAlteration(SJNote note)
        {
            string alterationString = "";
            int alteration = note.PitchAlteration;

            while ( alteration > 0 )
            {
                alterationString = alterationString + "is";
                alteration--;
            }
            while ( alteration < 0 )
            {
                alterationString = alterationString + "es";
                alteration++;
            }

            return alterationString;
        }

        private string GetDuration(SJNoteDurationEnum duration, uint numberOfDots)
        {
            string durationString;
            int durationInt = (int)(1 / EnumConverters.ConvertSJNoteDurationEnumToDouble(duration));
            durationString = durationInt.ToString();

            for(int i = 0; i < numberOfDots; i++)
            {
                durationString = durationString + ".";
            }

            return durationString;
        }
    }
}
