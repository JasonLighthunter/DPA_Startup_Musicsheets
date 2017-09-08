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
			default:
				return SJNoteDurationEnum.Undefined;
			}
		}
	}
}
