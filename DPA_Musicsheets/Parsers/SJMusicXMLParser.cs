using System;
using System.Linq;
using DPA_Musicsheets.Models;
using DPA_Musicsheets.Builders;
using DPA_Musicsheets.Utility;
using System.Collections.Generic;

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

            string xmlOneLine = data.Trim().ToLower().Replace("\r\n", "").Replace("\n", "").Replace("  ", "");
            string[] separators = { ">", "<" };
            string[] content = xmlOneLine.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            string[][] measures = GetElementAsStringArrayArray(content, "measure");
            foreach (string[] measure in measures)
            {
                songBuilder.AddBar(GetBarFromBarArray(measure, barBuilder, songBuilder, timeSignatureBuilder));
            }

            return songBuilder.Build();
        }

        private SJBar GetBarFromBarArray(string[] barArray, SJBarBuilder barBuilder, SJSongBuilder songBuilder, SJTimeSignatureBuilder timeSignatureBuilder)
        {
            barBuilder.Prepare();

            string[][] attributes = GetElementAsStringArrayArray(barArray, "attributes");
            foreach (string[] attribute in attributes)
            {
                songBuilder.SetClefType(GetClefTypeFromAttributesArray(attribute));
                songBuilder.SetTimeSignature(GetTimeSignatureFromAttributesArray(attribute, timeSignatureBuilder));
            }

            string[][] directions = GetElementAsStringArrayArray(barArray, "direction");
            foreach (string[] direction in directions)
            {
                songBuilder.SetTempo(GetTempoFromDirectionArray(direction));
            }

            string[][] notes = GetElementAsStringArrayArray(barArray, "note");
            foreach (string[] note in notes)
            {
                barBuilder.AddNote(GetBaseNoteFromNoteArray(note));
            }

            return barBuilder.Build();
        }

        private SJClefTypeEnum GetClefTypeFromAttributesArray(string[] attributesArray)
        {
            string[][] signs = GetElementAsStringArrayArray(attributesArray, "sign");
            return EnumConverters.ConvertCharacterToClefTypeEnum(signs[0][0][0]);
        }

        private SJTimeSignature GetTimeSignatureFromAttributesArray(string[] attributesArray, SJTimeSignatureBuilder timeSignatureBuilder)
        {
            timeSignatureBuilder.Prepare();

            string[][] beatsPerbar = GetElementAsStringArrayArray(attributesArray, "beats");
            timeSignatureBuilder.SetNumberOfBeatsPerBar(uint.Parse(beatsPerbar[0][0]));

            string[][] noteValueOfBeat = GetElementAsStringArrayArray(attributesArray, "beat-type");
            timeSignatureBuilder.SetNoteValueOfBeat(uint.Parse(noteValueOfBeat[0][0]));

            return timeSignatureBuilder.Build();
        }

        private ulong GetTempoFromDirectionArray(string[] directionArray)
        {
            string elementString = "";

            elementString = directionArray.SingleOrDefault(e => e.Contains("sound"));

            string elementAttributesString = elementString.Substring(5);
            string tempoString = elementAttributesString.Split('=')[1];
            tempoString = tempoString.TrimEnd(" /".ToCharArray()).Trim("\\\"".ToCharArray());

            tempoString = tempoString.Replace('.', ',');

            return (ulong)double.Parse(tempoString);
        }

        private SJBaseNote GetBaseNoteFromNoteArray(string[] noteArray)
        {
            SJBaseNote returnValue = (noteArray[0].Trim() == "rest /") ? GetRestFromNoteArray(noteArray) : GetNoteFromNoteArray(noteArray);
            return returnValue;
        }

        private SJBaseNote GetRestFromNoteArray(string[] noteArray)
        {
            _noteBuilder.Prepare("R");

            string[][] durationString = GetElementAsStringArrayArray(noteArray, "type");
            _noteBuilder.SetDuration(EnumConverters.ConvertMusicXMLStringToSJNoteDurationEnum(durationString[0][0]));

            string[][] numberOfDots = GetElementAsStringArrayArray(noteArray, "dot");
            _noteBuilder.SetNumberOfDots((uint)(numberOfDots.Count()));

            return (SJRest)_noteBuilder.Build();
        }

        private SJBaseNote GetNoteFromNoteArray(string[] noteArray)
        {

            int alteration = 0;

            _noteBuilder.Prepare("N");

            string[][] pitches = GetElementAsStringArrayArray(noteArray, "pitch");
            foreach (string[] pitch in pitches)
            {
                _noteBuilder.SetOctave(GetOctaveFromPitchArray(pitch));
                _noteBuilder.SetPitch(GetPitchEnumFromPitchArray(pitch));
            }

            string[][] durationString = GetElementAsStringArrayArray(noteArray, "type");
            _noteBuilder.SetDuration(EnumConverters.ConvertMusicXMLStringToSJNoteDurationEnum(durationString[0][0]));

            string[][] numberOfDots = GetElementAsStringArrayArray(noteArray, "dot");
            _noteBuilder.SetNumberOfDots((uint)(numberOfDots.Count()));

            string[][] accidentals = GetElementAsStringArrayArray(noteArray, "accidental");
            foreach (var accidental in accidentals)
            {
                if (accidental[0] == "sharp")
                {
                    alteration += 1;
                }
                else if (accidental[0] == "flat")
                {
                    alteration -= 1;
                }
            }

            _noteBuilder.SetPitchAlteration(alteration);
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
            string[][] octave = GetElementAsStringArrayArray(pitchArray, "octave");
            return int.Parse(octave[0][0]);
        }

        private SJPitchEnum GetPitchEnumFromPitchArray(string[] pitchArray)
        {
            string[][] step = GetElementAsStringArrayArray(pitchArray, "step");
            return EnumConverters.ConvertCharToSJNotePitchEnum(step[0][0][0]);
        }

        //To Change name
        private string[][] GetElementAsStringArrayArray(string[] xmlStringArray, string elementName)
        {
            List<string[]> returnArray = new List<string[]>();

            int indexOfMeasureStart = 0;

            for (int i = 0; i < xmlStringArray.Length; i++)
            {
                string line = xmlStringArray[i];

                string elementOpeningTag = elementName;
                string elementClosingTag = "/" + elementName;

                if (line.Trim().Split(' ').First() == elementOpeningTag)
                {
                    indexOfMeasureStart = i;
                }
                if (line.Trim().Split(' ').First() == elementClosingTag)
                {
                    int indexOfMeasureEnd = i;
                    string[] elementStringArray = xmlStringArray.Skip(indexOfMeasureStart + 1).Take(indexOfMeasureEnd - indexOfMeasureStart - 1).ToArray();
                    returnArray.Add(elementStringArray);
                }
            }

            return returnArray.ToArray();
        }
    }
}
