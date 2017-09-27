using DPA_Musicsheets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Builders
{
    class SJTimeSignatureBuilder
    {
        private SJTimeSignature timeSignature;

        public void Prepare()
        {
            timeSignature = new SJTimeSignature();
        }

        public void SetNumberOfBeatsPerBar(uint numberOfBeatsPerBar)
        {
            timeSignature.NumberOfBeatsPerBar = numberOfBeatsPerBar;
        }

        public void SetNoteValueOfBeat(uint noteValueOfBeat)
        {
            timeSignature.NoteValueOfBeat = noteValueOfBeat;
        }

        public SJTimeSignature Build()
        {
            return timeSignature;
        }
    }
}
