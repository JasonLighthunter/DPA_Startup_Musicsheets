using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPA_Musicsheets.Models;
using DPA_Musicsheets.Utility;
using System.Text.RegularExpressions;
using DPA_Musicsheets.Managers;
using DPA_Musicsheets.Builders;

namespace DPA_Musicsheets.Parsers
{
    public class SJLilypondParser : ISJParser<string>
    {
        private static List<SJPitchEnum> notesorder = new List<SJPitchEnum> {
            SJPitchEnum.C, SJPitchEnum.D, SJPitchEnum.E, SJPitchEnum.F, SJPitchEnum.G, SJPitchEnum.A, SJPitchEnum.B
        };

        public string ParseFromSJSong(SJSong song)
        {
            int previousOctave = 3;
            SJPitchEnum previousPitch = SJPitchEnum.C;
            StringBuilder lilypondContent = new StringBuilder();
            lilypondContent.AppendLine(GetOctaveEntry(song.UnheardStartNote, ref previousOctave, ref previousPitch));
            lilypondContent.AppendLine(GetClef(song.ClefType));
            lilypondContent.AppendLine(GetTimeSignature(song.TimeSignature));
            lilypondContent.AppendLine(GetTempo(song.Tempo));
            foreach (SJBar bar in song.Bars)
            {
                lilypondContent.AppendLine(GetBar(bar, ref previousOctave, ref previousPitch));
            }

            lilypondContent.AppendLine("}");

            return lilypondContent.ToString();
        }

        public SJSong ParseToSJSong(string data)
        {
            SJSongBuilder songBuilder = new SJSongBuilder();
            SJBarBuilder barBuilder = new SJBarBuilder();

            songBuilder.Prepare();
            barBuilder.Prepare();

            bool barContainsNotes = false;

            string content = data.Trim().ToLower().Replace("\r\n", " ").Replace("\n", " ").Replace("  ", " ");

            int previousOctave = 3;
            SJPitchEnum previousPitch = SJPitchEnum.Undefined;
            string previousLilypondItemString = "";
            bool isNote;
            bool isRest;

            foreach (string lilypondItemString in content.Split(' '))
            {
                isNote = false;
                isRest = false;
                LilypondToken token = new LilypondToken()
                {
                    Value = lilypondItemString
                };

                switch (previousLilypondItemString)
                {
                    case "\\relative":
                        songBuilder.SetUnheardStartNote(GetSJUnheardStartNote(lilypondItemString, ref previousOctave, ref previousPitch));
                        break;
                    case "\\clef":
                        songBuilder.SetClefType(GetSJClefType(lilypondItemString));
                        break;
                    case "\\time":
                        songBuilder.SetTimeSignature(GetSJTimeSignature(lilypondItemString));
                        break;
                    case "\\tempo":
                        // Tempo is not supported, therefore default is used.
                        songBuilder.SetTempo(120);
                        break;
                    case "|":
                        songBuilder.AddBar(barBuilder.Build());
                        barBuilder.Prepare();
                        break;
                    default:
                        isNote = new Regex(@"[a-g][,'eis]*[0-9]+[.]*").IsMatch(previousLilypondItemString);
                        isRest = new Regex(@"r.*?[0-9][.]*").IsMatch(previousLilypondItemString);
                        break;
                }

                //token.Value = lilypondItemString;

                if (isNote)
                {
                    barBuilder.AddNote(GetSJNote(previousLilypondItemString, ref previousOctave, ref previousPitch));
                    barContainsNotes = true;
                }
                else if (isRest)
                {
                    barBuilder.AddNote(GetSJRest(previousLilypondItemString));
                    barContainsNotes = true;
                }

                if(lilypondItemString == "}")
                {
                    if (barContainsNotes)
                    {
                        songBuilder.AddBar(barBuilder.Build());
                        barBuilder.Prepare();
                        barContainsNotes = false;
                    }
                }

                previousLilypondItemString = lilypondItemString;
            }

            return songBuilder.Build();
        }

        private SJTimeSignature GetSJTimeSignature(string lilypondItemString)
        {
            SJTimeSignatureBuilder timeSignatureBuilder = new SJTimeSignatureBuilder();
            timeSignatureBuilder.Prepare();
            var times = lilypondItemString.Split('/');
            timeSignatureBuilder.SetNumberOfBeatsPerBar(uint.Parse(times[0]));
            timeSignatureBuilder.SetNoteValueOfBeat(uint.Parse(times[1]));
            return timeSignatureBuilder.Build();
        }

        private SJClefTypeEnum GetSJClefType(string lilypondItemString)
        {
            SJClefTypeEnum cleftTypeEnum;
            cleftTypeEnum = EnumConverters.ConvertStringToClefTypeEnum(lilypondItemString);
            return cleftTypeEnum;
        }

        private SJRest GetSJRest(string lilypondItemString)
        {
            SJNoteBuilder.Prepare("R");
            SJNoteBuilder.SetDuration(GetSJDuration(lilypondItemString));
            SJNoteBuilder.SetNumberOfDots(GetSJNumberOfDots(lilypondItemString));
            return (SJRest)SJNoteBuilder.Build();
        }

        private SJNote GetSJNote(string lilypondItemString, ref int previousOctave, ref SJPitchEnum previousPitch)
        {
            SJPitchEnum pitch = GetSJPitch(lilypondItemString);
            SJNoteBuilder.Prepare("N");
            SJNoteBuilder.SetPitch(pitch);
            SJNoteBuilder.SetPitchAlteration(GetSJPitchAlteration(lilypondItemString));
            SJNoteBuilder.SetOctave(GetSJOctave(lilypondItemString, previousOctave, previousPitch, pitch));
            SJNoteBuilder.SetDuration(GetSJDuration(lilypondItemString));
            SJNoteBuilder.SetNumberOfDots(GetSJNumberOfDots(lilypondItemString));
            var tempNote = SJNoteBuilder.Build();
            SJNote note = (SJNote)tempNote;
            previousOctave = note.Octave;
            previousPitch = pitch;
            return note;
        }

        private SJUnheardNote GetSJUnheardStartNote(string lilypondItemString, ref int previousOctave, ref SJPitchEnum previousPitch)
        {
            SJPitchEnum pitch = GetSJPitch(lilypondItemString);
            SJNoteBuilder.Prepare("U");
            SJNoteBuilder.SetPitch(pitch);
            SJNoteBuilder.SetPitchAlteration(GetSJPitchAlteration(lilypondItemString));
            SJNoteBuilder.SetOctave(GetSJOctave(lilypondItemString, previousOctave, previousPitch, pitch));
            SJUnheardNote note = (SJUnheardNote)SJNoteBuilder.Build();
            previousOctave = note.Octave;
            previousPitch = pitch;
            return note;
        }

        private uint GetSJNumberOfDots(string lilypondItemString)
        {
            uint numberOfDots = (uint)lilypondItemString.Count(c => c.Equals('.'));
            return numberOfDots;
        }

        private SJNoteDurationEnum GetSJDuration(string lilypondItemString)
        {
            int noteLength = Int32.Parse(Regex.Match(lilypondItemString, @"\d+").Value);
            SJNoteDurationEnum duration = EnumConverters.ConvertDoubleToSJNoteDurationEnum(1.0 / noteLength);
            return duration;
        }

        private int GetSJOctave(string lilypondItemString, int previousOctave, SJPitchEnum previousPitch, SJPitchEnum currentPitch)
        {
            int octave = previousOctave;

            int distanceWithPreviousPitch = notesorder.IndexOf(currentPitch) - notesorder.IndexOf(previousPitch);
            if (distanceWithPreviousPitch > 3) // Shorter path possible the other way around
            {
                distanceWithPreviousPitch -= 7; // The number of notes in an octave
            }
            else if (distanceWithPreviousPitch < -3)
            {
                distanceWithPreviousPitch += 7; // The number of notes in an octave
            }

            if (distanceWithPreviousPitch + notesorder.IndexOf(previousPitch) >= 7)
            {
                octave++;
            }
            else if (distanceWithPreviousPitch + notesorder.IndexOf(previousPitch) < 0)
            {
                octave--;
            }

            // Force up or down.
            octave += lilypondItemString.Count(c => c == '\'');
            octave -= lilypondItemString.Count(c => c == ',');
            return octave;
        }

        private int GetSJPitchAlteration(string lilypondItemString)
        {
            int alter = 0;
            alter += Regex.Matches(lilypondItemString, "is").Count;
            alter -= Regex.Matches(lilypondItemString, "es|as").Count;
            return alter;
        }

        private SJPitchEnum GetSJPitch(string lilypondItemString)
        {
            char previousNoteChar = lilypondItemString.First();
            SJPitchEnum pitch = EnumConverters.ConvertCharToSJNotePitchEnum(previousNoteChar);
            return pitch;
        }

        private string GetOctaveEntry(SJNote unheardStartNote, ref int previousOctave, ref SJPitchEnum previousPitch)
        {
            string octaveEntry = "\\relative ";

            octaveEntry = octaveEntry + unheardStartNote.Pitch.ToString().ToLower();
            octaveEntry = octaveEntry + GetOctaveDifference(unheardStartNote, ref previousOctave, previousPitch);
            octaveEntry = octaveEntry + " {";

            previousPitch = unheardStartNote.Pitch;

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
            string tempoString = "\\tempo 4=";
            tempoString = tempoString + tempo.ToString();
            return tempoString;
        }

        private string GetBar(SJBar bar, ref int previousOctave, ref SJPitchEnum previousPitch)
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
                    barString = barString + GetNote((SJNote)note, ref previousOctave, ref previousPitch);
                }
                barString = barString + GetDuration(note.Duration, note.NumberOfDots);
                barString = barString + " ";
            }

            barString = barString + "|";
            return barString;
        }

        private string GetRest(SJRest rest)
        {
            return "r";
        }

        private string GetNote(SJNote note, ref int previousOctave, ref SJPitchEnum previousPitch)
        {
            string noteString;

            noteString = note.Pitch.ToString().ToLower();
            noteString = noteString + GetAlteration(note);
            noteString = noteString + GetOctaveDifference(note, ref previousOctave, previousPitch);
            previousPitch = note.Pitch;

            return noteString;
        }

        private string GetOctaveDifference(SJNote note, ref int previousOctave, SJPitchEnum previousPitch)
        {
            string octaveDifferenceString = "";
            int octaveDifference = note.Octave - previousOctave;
            int distanceWithPreviousPitch = notesorder.IndexOf(note.Pitch) - notesorder.IndexOf(previousPitch);
            if (distanceWithPreviousPitch > 3) // Shorter path possible the other way around
            {
                distanceWithPreviousPitch -= 7; // The number of notes in an octave
            }
            else if (distanceWithPreviousPitch < -3)
            {
                distanceWithPreviousPitch += 7; // The number of notes in an octave
            }

            if (distanceWithPreviousPitch + notesorder.IndexOf(previousPitch) >= 7)
            {
                octaveDifference--;
            }
            else if (distanceWithPreviousPitch + notesorder.IndexOf(previousPitch) < 0)
            {
                octaveDifference++;
            }

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
