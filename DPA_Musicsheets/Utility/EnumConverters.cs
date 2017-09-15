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
    }
}
