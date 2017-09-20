using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPA_Musicsheets.Models;
using DPA_Musicsheets.Utility;
using System.Text.RegularExpressions;

namespace DPA_Musicsheets.Parsers
{
    public class SJLilypondParser : ISJParser<string>
    {
        private static List<SJPitchEnum> notesorder = new List<SJPitchEnum> {
            SJPitchEnum.C, SJPitchEnum.D, SJPitchEnum.E, SJPitchEnum.F, SJPitchEnum.G, SJPitchEnum.A, SJPitchEnum.B
        };

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

            string content = data.Trim().ToLower().Replace("\r\n", " ").Replace("\n", " ").Replace("  ", " ");

            int previousOctave = 4;
            SJPitchEnum previousPitch = SJPitchEnum.Undefined;
            string previousLilypondItemString = "";

            foreach (string lilypondItemString in content.Split(' '))
            {
                LilypondToken token = new LilypondToken()
                {
                    Value = lilypondItemString
                };

                switch (previousLilypondItemString)
                {
                    case "\\relative":
                        song.UnheardStartNote = GetSJUnheardStartNote(lilypondItemString, ref previousOctave, ref previousPitch);
                        break;
                    case "\\clef":
                        song.ClefType = GetSJClefType(lilypondItemString);
                        break;
                    case "\\time": token.TokenKind = LilypondTokenKind.Time; break;
                    case "\\tempo": token.TokenKind = LilypondTokenKind.Tempo; break;
                    case "|": token.TokenKind = LilypondTokenKind.Bar; break;
                    default: token.TokenKind = LilypondTokenKind.Unknown; break;
                }

                //token.Value = lilypondItemString;

                if (token.TokenKind == LilypondTokenKind.Unknown && new Regex(@"[a-g][,'eis]*[0-9]+[.]*").IsMatch(s))
                {
                    token.TokenKind = LilypondTokenKind.Note;
                }
                else if (token.TokenKind == LilypondTokenKind.Unknown && new Regex(@"r.*?[0-9][.]*").IsMatch(s))
                {
                    token.TokenKind = LilypondTokenKind.Rest;
                }

                if (tokens.Last != null)
                {
                    tokens.Last.Value.NextToken = token;
                    token.PreviousToken = tokens.Last.Value;
                }

                tokens.AddLast(token);
                previousLilypondItemString = lilypondItemString;
            }

            return song;

//            WPFStaffs.AddRange(GetStaffsFromTokens(tokens, out message));
        }

        private SJClefTypeEnum GetSJClefType(string lilypondItemString)
        {
            SJClefTypeEnum cleftTypeEnum;
            cleftTypeEnum = EnumConverters.ConvertStringToClefTypeEnum(lilypondItemString);
            return cleftTypeEnum;
        }

        private SJNote GetSJUnheardStartNote(string lilypondItemString, ref int previousOctave, ref SJPitchEnum previousPitch)
        {
            SJUnheardNote unheardNote = new SJUnheardNote();
            unheardNote.Pitch = GetSJPitch(lilypondItemString);
            unheardNote.PitchAlteration = GetSJPitchAlteration(lilypondItemString);
            unheardNote.Octave = GetSJOctave(lilypondItemString, previousOctave, previousPitch, unheardNote.Pitch);
            previousOctave = unheardNote.Octave;
            previousPitch = unheardNote.Pitch;
            return unheardNote;
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
            throw new NotImplementedException();
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
            string tempoString = "\\tempo 4=";
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
