using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Models
{
    class SJNote : SJBaseNote
    {
        private SJPitchEnum _pitch;

        public SJPitchEnum Pitch
        {
            get { return _pitch; }
            set { _pitch = value; }
        }

        private int _pitchAlteration;

        public int PitchAlteration
        {
            get { return _pitchAlteration; }
            set { _pitchAlteration = value; }
        }

        private int _octave;

        public int Octave
        {
            get { return _octave; }
            set { _octave = value; }
        }

    }
}
