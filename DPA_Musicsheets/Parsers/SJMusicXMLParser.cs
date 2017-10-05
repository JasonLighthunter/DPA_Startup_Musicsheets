using System;
using System.Linq;
using DPA_Musicsheets.Models;
using DPA_Musicsheets.Builders;
using DPA_Musicsheets.Utility;

namespace DPA_Musicsheets.Parsers
{
	public class SJMusicXMLParser : ISJParser<String>
    {
		private SJNoteBuilder _noteBuilder { get; set; }

		public SJMusicXMLParser(SJNoteBuilder noteBuilder)
		{
			_noteBuilder = noteBuilder;
		}

		public string ParseFromSJSong(SJSong song)
        {
            throw new NotImplementedException();
        }

        public SJSong ParseToSJSong(string data)
        {
			SJSongBuilder songBuilder = new SJSongBuilder();
			SJBarBuilder barBuilder = new SJBarBuilder();
			SJTimeSignatureBuilder timeSignatureBuilder = new SJTimeSignatureBuilder();

			songBuilder.Prepare();

			songBuilder.SetUnheardStartNote(GetCentralCUnheardNote());

			string delimiter = ">";
			string[] content = data.Trim().ToLower().Replace("\r\n", "").Replace("\n", "").Replace("  ", " ").Split(delimiter.ToCharArray());

			int indexOfMeasureStart = 0;
			int indexOfMeasureEnd = 0;

			for(int i = 0; i < content.Length; i++)
			{
				string line = content[i];

				switch(line.Trim().Split(' ').First())
				{					
					case "<measure":
						barBuilder.Prepare();
						indexOfMeasureStart = i;
						break;
					case "</measure":
						indexOfMeasureEnd = i;
						var barArray = content.Skip(indexOfMeasureStart).Take(indexOfMeasureEnd - indexOfMeasureStart + 1).ToArray();
						songBuilder.AddBar(GetBarFromBarArray(barArray, barBuilder, songBuilder, timeSignatureBuilder));
						break;
					default:
						break;
				}
			}
			
			return songBuilder.Build();
		}

		private SJBar GetBarFromBarArray(string[] barArray, SJBarBuilder barBuilder, SJSongBuilder songBuilder, SJTimeSignatureBuilder timeSignatureBuilder)
		{
			int indexOfAttributesStart = 0;
			int indexOfAttributesEnd = 0;

			int indexOfDirectionStart = 0;
			int indexOfDirectionEnd = 0;

			int indexOfNoteStart = 0;
			int indexOfNoteEnd = 0;

			barBuilder.Prepare();

			for(int i = 0; i < barArray.Length; i++)
			{
				string line = barArray[i];

				switch(line.Trim().Split(' ').First())
				{
					case "<attributes":
						indexOfAttributesStart = i;
						break;
					case "</attributes":
						indexOfAttributesEnd = i;
						string[] attributesArray = barArray.Skip(indexOfAttributesStart).Take(indexOfAttributesEnd - indexOfAttributesStart + 1).ToArray();
						songBuilder.SetClefType(GetClefTypeFromAttributesArray(attributesArray));
						songBuilder.SetTimeSignature(GetTimeSignatureFromAttributesArray(attributesArray, timeSignatureBuilder));
						break;
					case "<direction":
						indexOfDirectionStart = i;
						break;
					case "</direction":
						indexOfDirectionEnd = i;
						string[] directionArray = barArray.Skip(indexOfDirectionStart).Take(indexOfDirectionEnd - indexOfDirectionStart + 1).ToArray();
						songBuilder.SetTempo(GetTempoFromDirectionArray(directionArray));
						break;
					case "<note":
						indexOfNoteStart = i;
						break;
					case "</note":
						indexOfNoteEnd = i;
						string[] noteArray = barArray.Skip(indexOfNoteStart).Take(indexOfNoteEnd - indexOfNoteStart + 1).ToArray();
						barBuilder.AddNote(GetBaseNoteFromNoteArray(noteArray));
						break;
					default:
						break;
				}

			}
			return barBuilder.Build();
		}

		private SJClefTypeEnum GetClefTypeFromAttributesArray(string[] attributesArray)
		{
			for(int i = 1; i < attributesArray.Length; i++)
			{
				string previousLine = attributesArray[i-1];

				if(previousLine.Trim().Split(' ').First() == "<sign")
				{
					char clefTypeCharacter = attributesArray[i].Trim().First();
					return EnumConverters.ConvertCharacterToClefTypeEnum(clefTypeCharacter);
				}
			}
			return SJClefTypeEnum.Undefined;
		}

		private SJTimeSignature GetTimeSignatureFromAttributesArray(string[] attributesArray, SJTimeSignatureBuilder timeSignatureBuilder)
		{
			timeSignatureBuilder.Prepare();
			
			for(int i = 1; i < attributesArray.Length; i++)
			{
				string previousLine = attributesArray[i - 1];

				switch(previousLine.Trim().Split(' ').First())
				{
					case "<beats":
						uint beatsPerbar = uint.Parse(attributesArray[i].Split('<').First());
						timeSignatureBuilder.SetNumberOfBeatsPerBar(beatsPerbar);
						break;
					case "<beat-type":
						uint noteValueOfBeat = uint.Parse(attributesArray[i].Split('<').First());
						timeSignatureBuilder.SetNoteValueOfBeat(noteValueOfBeat);
						break;
					default:
						break;
				}
			}

			return timeSignatureBuilder.Build();
		}

		private ulong GetTempoFromDirectionArray(string[] directionArray)
		{
			for(int i = 0; i < directionArray.Length; i++)
			{
				string line = directionArray[i];

				if(line.Trim().Split(' ').First() == "<sound")
				{
					string tempoString = line.Trim().Split(' ')[1].Split('=')[1].Replace("\"", "");
					tempoString = tempoString.Replace('.', ',');
					return (ulong)double.Parse(tempoString);
				}
			}
			return 0;
		}

		private SJBaseNote GetBaseNoteFromNoteArray(string[] noteArray)
		{
			string string1 = noteArray[1].Trim();
			string string2 = "<rest/";

			Console.WriteLine(string1 + " == " + string2);

			SJBaseNote returnValue = (noteArray[1].Trim() == "<rest /") ? GetRestFromNoteArray(noteArray) : GetNoteFromNoteArray(noteArray));
			return returnValue;
		}

		private SJBaseNote GetRestFromNoteArray(string[] noteArray)
		{
			uint numberOfDots = 0;

			_noteBuilder.Prepare("R");

			for(int i = 1; i < noteArray.Length; i++)
			{
				string previousLine = noteArray[i - 1];

				switch(previousLine.Trim().Split(' ').First())
				{
					case "<type":
						string durationString = noteArray[i].Split('<').First();
						_noteBuilder.SetDuration(EnumConverters.ConvertMusicXMLStringToSJNoteDurationEnum(durationString));
						break;
					case "<dot":
						numberOfDots++;
						break;
					default:
						break;
				}
			}

			_noteBuilder.SetNumberOfDots(numberOfDots);
			return (SJRest)_noteBuilder.Build();
		}

		private SJBaseNote GetNoteFromNoteArray(string[] noteArray)
		{
			int indexOfPitchStart = 0;
			int indexOfPitchEnd = 0;

			uint numberOfDots = 0;
			int alteration = 0;

			_noteBuilder.Prepare("N");

			for(int i = 1; i < noteArray.Length; i++)
			{
				string previousLine = noteArray[i - 1];

				switch(previousLine.Trim().Split(' ').First())
				{
					case "<pitch":
						indexOfPitchStart = i - 1;
						break;
					case "</pitch":
						indexOfPitchEnd = i - 1;
						string[] pitchArray = noteArray.Skip(indexOfPitchStart).Take(indexOfPitchEnd - indexOfPitchStart + 1).ToArray();
						_noteBuilder.SetOctave(GetOctaveFromPitchArray(pitchArray));
						_noteBuilder.SetPitch(GetPitchEnumFromPitchArray(pitchArray));
						break;
					case "<type":
						string durationString = noteArray[i].Trim().Split('<').First().Trim();
						_noteBuilder.SetDuration(EnumConverters.ConvertMusicXMLStringToSJNoteDurationEnum(durationString));
						break;
					case "<dot":
						numberOfDots++;
						break;
					case "<accidental":
						string accidentalString = noteArray[i].Split('<').First();
						if(accidentalString == "sharp")
						{
							alteration += 1;
						}
						else if(accidentalString == "flat")
						{
							alteration -= 1;
						}
						break;
					default:
						break;
				}
			}

			_noteBuilder.SetPitchAlteration(alteration);
			_noteBuilder.SetNumberOfDots(numberOfDots);
			return (SJNote)_noteBuilder.Build();
		}

		private SJUnheardNote GetCentralCUnheardNote()
		{
			_noteBuilder.Prepare("U");

			_noteBuilder.SetPitch(SJPitchEnum.C);
			_noteBuilder.SetOctave(4);
			_noteBuilder.SetPitchAlteration(0);

			return (SJUnheardNote)_noteBuilder.Build();
		}

		private int GetOctaveFromPitchArray(string[] pitchArray)
		{
			for(int i = 1; i < pitchArray.Length; i++)
			{
				string previousLine = pitchArray[i - 1];

				if(previousLine.Trim().Split(' ').First() == "<octave")
				{
					int octave = int.Parse(pitchArray[i].Trim().First().ToString());
					return octave;
				}
			}
			return 0;
		}

		private SJPitchEnum GetPitchEnumFromPitchArray(string[] pitchArray)
		{
			for(int i = 1; i < pitchArray.Length; i++)
			{
				string previousLine = pitchArray[i - 1];

				if(previousLine.Trim().Split(' ').First() == "<step")
				{
					char pitchCharacter = pitchArray[i].Trim().First();
					return EnumConverters.ConvertCharToSJNotePitchEnum(pitchCharacter);
				}
			}
			return SJPitchEnum.Undefined;
		}
	}
}
