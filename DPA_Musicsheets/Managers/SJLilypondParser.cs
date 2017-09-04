using DPA_Musicsheets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Managers
{
    public class SJLilypondParser
    {
        private static SJNoteBuilder _noteBuilder { get; set; }

        //int absoluteTicks = 0 hardcoded
        int nextNoteAbsoluteTicks
        int division
        int beatNote
        int beatsPerBar
        out double percentageOfBar

        public static SJBaseNote ParseNote()
        {
            throw new NotImplementedException();
        }
    }
}
