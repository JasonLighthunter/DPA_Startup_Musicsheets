using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DPA_Musicsheets.Models;
using DPA_Musicsheets.Utility;
using System.Text.RegularExpressions;
using DPA_Musicsheets.Builders;

namespace DPA_Musicsheets.Parsers
{
    public class SJLilypondParser : ISJParser<string>
    {
        #region instance variables
        private SJNoteBuilder _noteBuilder { get; set; }
        private SJBarBuilder _barBuilder { get; set; }
        private SJSongBuilder _songBuilder { get; set; }

        private SJPitchEnum _previousPitch { get; set; }
        private int _previousOctave { get; set; }

        private Dictionary<SJLilypondParserKeyEnum, Action<string>> _lilypondToSJSongMetaParserDictionary { get; set; }
        #endregion

        public SJLilypondParser(SJNoteBuilder noteBuilder)
        {
            _noteBuilder = noteBuilder;

            _lilypondToSJSongMetaParserDictionary = new Dictionary<SJLilypondParserKeyEnum, Action<string>>()
            {
                { SJLilypondParserKeyEnum.RelativeKey , SetSJUnheardStartNote },
                { SJLilypondParserKeyEnum.ClefKey, SetSJClefType },
                { SJLilypondParserKeyEnum.TimeSignatureKey, SetSJTimeSignature },
                { SJLilypondParserKeyEnum.TempoKey, SetSJSongTempo},
                { SJLilypondParserKeyEnum.Undefined, null }
            };
        }

        private static List<SJPitchEnum> notesorder = new List<SJPitchEnum> {
            SJPitchEnum.C, SJPitchEnum.D, SJPitchEnum.E, SJPitchEnum.F, SJPitchEnum.G, SJPitchEnum.A, SJPitchEnum.B
        };

        public string ParseFromSJSong(SJSong song)
        {
            _previousOctave = 3;
            _previousPitch = SJPitchEnum.C;

            StringBuilder lilypondContent = new StringBuilder();

            lilypondContent.AppendLine(GetOctaveEntry(song.UnheardStartNote));
            lilypondContent.AppendLine(GetClef(song.ClefType));
            lilypondContent.AppendLine(GetTimeSignature(song.TimeSignature));
            lilypondContent.AppendLine(GetTempo(song.Tempo));

            foreach (SJBar bar in song.Bars)
            {
                lilypondContent.AppendLine(GetBar(bar));
            }

            lilypondContent.AppendLine("}");

            return lilypondContent.ToString();
        }

        public SJSong ParseToSJSong(string data)
        {
            _songBuilder = new SJSongBuilder();
            _barBuilder = new SJBarBuilder();

            _songBuilder.Prepare();

            string content = data.Trim().ToLower().Replace("\r\n", " ").Replace("\n", " ").Replace("  ", " ");

            _previousOctave = 3;
            _previousPitch = SJPitchEnum.Undefined;

			string[] barStrings = content.Split("{|".ToCharArray());

			foreach (string lilypondBarString in barStrings)
			{
				AddSJBar(lilypondBarString);
			}

            return _songBuilder.Build();
        }

		#region ToSJSong
		private void AddSJBar(string lilypondBarString)
		{
			_barBuilder.Prepare();
			string previousLilypondItemString = "";
			bool isNote;
			bool isRest;
			bool barContainsNotes = false;

			foreach(string lilypondItemString in lilypondBarString.Split(' '))
			{
				isNote = false;
				isRest = false;

				SJLilypondParserKeyEnum key = EnumConverters.ConvertLilypondStringToKeyEnum(previousLilypondItemString);
				_lilypondToSJSongMetaParserDictionary[key]?.Invoke(lilypondItemString);

				isNote = new Regex(@"[a-g][,'eis]*[0-9]+[.]*").IsMatch(previousLilypondItemString);
				isRest = new Regex(@"r.*?[0-9][.]*").IsMatch(previousLilypondItemString);

				if(isNote)
				{
					_barBuilder.AddNote(GetSJNote(previousLilypondItemString));
					barContainsNotes = true;
				}
				else if(isRest)
				{
					_barBuilder.AddNote(GetSJRest(previousLilypondItemString));
					barContainsNotes = true;
				}

				previousLilypondItemString = lilypondItemString;
			}
			if(barContainsNotes)
			{
				_songBuilder.AddBar(_barBuilder.Build());
				barContainsNotes = false;
			}
		}
		private void SetSJTimeSignature(string lilypondItemString)
		{
			SJTimeSignatureBuilder timeSignatureBuilder = new SJTimeSignatureBuilder();
			timeSignatureBuilder.Prepare();
			var times = lilypondItemString.Split('/');
			timeSignatureBuilder.SetNumberOfBeatsPerBar(uint.Parse(times[0]));
			timeSignatureBuilder.SetNoteValueOfBeat(uint.Parse(times[1]));
			_songBuilder.SetTimeSignature(timeSignatureBuilder.Build());
		}

		private void SetSJSongTempo(string lilypondItemString)
		{
			_songBuilder.SetTempo(120);
		}

		private void SetSJClefType(string lilypondItemString)
		{
			SJClefTypeEnum clefTypeEnum = EnumConverters.ConvertStringToClefTypeEnum(lilypondItemString);
			_songBuilder.SetClefType(clefTypeEnum);
		}

		private SJRest GetSJRest(string lilypondItemString)
		{
			_noteBuilder.Prepare("R");
			_noteBuilder.SetDuration(GetSJDuration(lilypondItemString));
			_noteBuilder.SetNumberOfDots(GetSJNumberOfDots(lilypondItemString));
			return (SJRest)_noteBuilder.Build();
		}

		private SJNote GetSJNote(string lilypondItemString)
		{
			SJPitchEnum pitch = GetSJPitch(lilypondItemString);
			_noteBuilder.Prepare("N");
			_noteBuilder.SetPitch(pitch);
			_noteBuilder.SetPitchAlteration(GetSJPitchAlteration(lilypondItemString));
			_noteBuilder.SetOctave(GetSJOctave(lilypondItemString, pitch));
			_noteBuilder.SetDuration(GetSJDuration(lilypondItemString));
			_noteBuilder.SetNumberOfDots(GetSJNumberOfDots(lilypondItemString));
			var tempNote = _noteBuilder.Build();
			SJNote note = (SJNote)tempNote;
			_previousOctave = note.Octave;
			_previousPitch = pitch;
			return note;
		}

		private void SetSJUnheardStartNote(string lilypondItemString)
		{
			SJPitchEnum pitch = GetSJPitch(lilypondItemString);
			_noteBuilder.Prepare("U");
			_noteBuilder.SetPitch(pitch);
			_noteBuilder.SetPitchAlteration(GetSJPitchAlteration(lilypondItemString));
			_noteBuilder.SetOctave(GetSJOctave(lilypondItemString, pitch));
			SJUnheardNote note = (SJUnheardNote)_noteBuilder.Build();
			_previousOctave = note.Octave;
			_previousPitch = pitch;
			_songBuilder.SetUnheardStartNote(note);
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

		private int GetSJOctave(string lilypondItemString, SJPitchEnum currentPitch)
		{
			_previousOctave -= GetOctaveDifference(currentPitch);

			// Force up or down.
			_previousOctave += lilypondItemString.Count(c => c == '\'');
			_previousOctave -= lilypondItemString.Count(c => c == ',');
			return _previousOctave;
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
		#endregion
		#region FromSJSong
        private string GetOctaveEntry(SJNote unheardStartNote)
        {
            string octaveEntry = "\\relative ";

            string noteLetter = unheardStartNote.Pitch.ToString().ToLower();
            string octaveDifference = GetOctaveDifference(unheardStartNote);

            octaveEntry = octaveEntry + noteLetter + octaveDifference + " {";

            _previousPitch = unheardStartNote.Pitch;

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

        private string GetBar(SJBar bar)
        {
            string barString = "";
            foreach (var note in bar.Notes)
            {
                string noteString = (note is SJRest) ? "r" : GetNote((SJNote)note);
                string durationString = GetDuration(note.Duration, note.NumberOfDots);
                barString = barString + noteString + durationString + " ";
            }

            barString = barString + "|";
            return barString;
        }

        private string GetNote(SJNote note)
        {
            string noteLetter = note.Pitch.ToString().ToLower();
            string alterationString = GetAlteration(note);
            string octaveDifferenceString = GetOctaveDifference(note);

            string noteString = noteLetter + alterationString + octaveDifferenceString;

            _previousPitch = note.Pitch;

            return noteString;
        }

        private string GetOctaveDifference(SJNote note)
        {
            string octaveDifferenceString = "";
            int octaveDifference = note.Octave - _previousOctave;

            octaveDifference += GetOctaveDifference(note.Pitch);

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

            _previousOctave = note.Octave;

            return octaveDifferenceString;
        }

        private string GetAlteration(SJNote note)
        {
            string alterationString = "";
            int alteration = note.PitchAlteration;

            while (alteration > 0)
            {
                alterationString = alterationString + "is";
                alteration--;
            }
            while (alteration < 0)
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

            for (int i = 0; i < numberOfDots; i++)
            {
                durationString = durationString + ".";
            }

            return durationString;
        }

        private int GetOctaveDifference(SJPitchEnum currentPitch)
        {
            int distanceWithPreviousPitch = notesorder.IndexOf(currentPitch) - notesorder.IndexOf(_previousPitch);

            if (distanceWithPreviousPitch > 3) // Shorter path possible the other way around
            {
                distanceWithPreviousPitch -= 7; // The number of notes in an octave
            }
            if (distanceWithPreviousPitch < -3)
            {
                distanceWithPreviousPitch += 7; // The number of notes in an octave
            }

            if (distanceWithPreviousPitch + notesorder.IndexOf(_previousPitch) >= 7)
            {
                return -1;
            }
            if (distanceWithPreviousPitch + notesorder.IndexOf(_previousPitch) < 0)
            {
                return 1;
            }
            return 0;
        }
		#endregion
	}
}
