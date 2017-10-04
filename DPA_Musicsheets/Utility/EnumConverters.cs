using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPA_Musicsheets.Models;

namespace DPA_Musicsheets.Utility
{
	public static class EnumConverters
	{
		public static SJNoteDurationEnum ConvertDoubleToSJNoteDurationEnum(double doubleValue)
		{
			switch(doubleValue)
			{
                case 4.0:
                    return SJNoteDurationEnum.Long;
                case 2.0:
				    return SJNoteDurationEnum.Double;
			    case 1.0:
				    return SJNoteDurationEnum.Whole;
			    case 0.5:
				    return SJNoteDurationEnum.Half;
			    case 0.25:
				    return SJNoteDurationEnum.Quarter;
			    case 0.125:
				    return SJNoteDurationEnum.Eighth;
			    case 0.0625:
				    return SJNoteDurationEnum.Sixteenth;
                case 0.03125:
                    return SJNoteDurationEnum.ThirtySecond;
                default:
				    return SJNoteDurationEnum.Undefined;
			}
		}

		public static SJNoteDurationEnum ConvertMusicXMLStringToSJNoteDurationEnum(string stringValue)
		{
			switch(stringValue)
			{
				case "long":
					return SJNoteDurationEnum.Long;
				case "double":
					return SJNoteDurationEnum.Double;
				case "whole":
					return SJNoteDurationEnum.Whole;
				case "half":
					return SJNoteDurationEnum.Half;
				case "quarter":
					return SJNoteDurationEnum.Quarter;
				case "eighth":
					return SJNoteDurationEnum.Eighth;
				case "16th":
					return SJNoteDurationEnum.Sixteenth;
				case "32th":
					return SJNoteDurationEnum.ThirtySecond;
				default:
					return SJNoteDurationEnum.Undefined;
			}
		}

		public static double ConvertSJNoteDurationEnumToDouble(SJNoteDurationEnum enumValue)
        {
            switch (enumValue)
            {
                case SJNoteDurationEnum.Long:
                    return 4.0;
                case SJNoteDurationEnum.Double:
                    return 2.0;
                case SJNoteDurationEnum.Whole:
                    return 1.0;
                case SJNoteDurationEnum.Half:
                    return 0.5;
                case SJNoteDurationEnum.Quarter:
                    return 0.25;
                case SJNoteDurationEnum.Eighth:
                    return 0.125;
                case SJNoteDurationEnum.Sixteenth:
                    return 0.0625;
                case SJNoteDurationEnum.ThirtySecond:
                    return 0.03125;
                default:
                    throw new ArgumentException("recieved Enumvalue was undefined");

            }
        }

        public static SJPitchEnum ConvertCharToSJNotePitchEnum(char pitchCharacter)
        {
            switch (pitchCharacter)
            {
                case 'a':
                    return SJPitchEnum.A;
                case 'b':
                    return SJPitchEnum.B;
                case 'c':
                    return SJPitchEnum.C;
                case 'd':
                    return SJPitchEnum.D;
                case 'e':
                    return SJPitchEnum.E;
                case 'f':
                    return SJPitchEnum.F;
                case 'g':
                    return SJPitchEnum.G;
                default:
                    return SJPitchEnum.Undefined;
            }
        }

        public static SJPitchEnum ConvertMidiKeyToSJNotePitchEnum(int midiKey)
        {
            switch (midiKey % 12)
            {
                case 0:
                case 1:
                    return SJPitchEnum.C;
                case 2:
                case 3:
                    return SJPitchEnum.D;
                case 4:
                    return SJPitchEnum.E;
                case 5:
                case 6:
                    return SJPitchEnum.F;
                case 7:
                case 8:
                    return SJPitchEnum.G;
                case 9:
                case 10:
                    return SJPitchEnum.A;
                case 11:
                    return SJPitchEnum.B;
                default:
                    return SJPitchEnum.Undefined;
            }
        }

        public static SJClefTypeEnum ConvertStringToClefTypeEnum(string clefTypeString)
        {
            switch (clefTypeString.ToLower())
            {
                case "treble":
                    return SJClefTypeEnum.Treble;
                case "bass":
                    return SJClefTypeEnum.Bass;
                case "tenor":
                    return SJClefTypeEnum.Tenor;
                case "alto":
                    return SJClefTypeEnum.Alto;
                default:
                    return SJClefTypeEnum.Undefined;
            }
        }

		public static SJClefTypeEnum ConvertCharacterToClefTypeEnum(char clefTypeCharacter)
		{
			switch(clefTypeCharacter.ToString().ToLower())
			{
				case "g":
					return SJClefTypeEnum.Treble;
				default:
					return SJClefTypeEnum.Undefined;
			}
		}
    }
}
