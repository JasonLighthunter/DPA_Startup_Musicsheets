using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Models
{
    public abstract class SJBaseNote
    {
        private uint _numberOfDots;

        public uint NumberOfDots
        {
            get { return _numberOfDots; }
            set { _numberOfDots = value; }
        }

        private SJNoteDurationEnum _duration;

        public SJNoteDurationEnum Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }
    }
}
