using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Models
{
    public class SJSong
    {
        private SJTimeSignature _timeSignature;

        public SJTimeSignature TimeSignature
        {
            get { return _timeSignature; }
            set { _timeSignature = value; }
        }

        private List<SJBar> _bars;

        public List<SJBar> Bars
        {
            get { return _bars; }
            set { _bars = value; }
        }


        private ulong _tempo;

        public ulong Tempo
        {
            get { return _tempo; }
            set { _tempo = value; }
        }

        private SJNote _unheardStartNote;

        public SJNote UnheardStartNote
        {
            get { return _unheardStartNote; }
            set { _unheardStartNote = value; }
        }

        private SJClefTypeEnum _clefType;

        public SJClefTypeEnum ClefType
        {
            get { return _clefType; }
            set { _clefType = value; }
        }

    }
}
