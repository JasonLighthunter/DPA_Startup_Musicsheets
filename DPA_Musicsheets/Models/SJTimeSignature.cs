using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Models
{
    public class SJTimeSignature
    {
        private uint _numberOfBeatsPerBar;

        public uint NumberOfBeatsPerBar
        {
            get { return _numberOfBeatsPerBar; }
            set { _numberOfBeatsPerBar = value; }
        }

        private uint _noteValueOfBeat;

        public uint NoteValueOfBeat
        {
            get { return _noteValueOfBeat; }
            set { _noteValueOfBeat = value; }
        }

    }
}
