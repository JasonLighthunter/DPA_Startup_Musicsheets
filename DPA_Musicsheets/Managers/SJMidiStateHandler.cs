using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Managers
{
    class SJMidiStateHandler : ISJStateHandler<Sequence, MidiSequenceEventArgs>
    {
        public Sequence StateData { get; private set; }

        public event EventHandler<MidiSequenceEventArgs> StateDataChanged;

        public void UpdateData(Sequence data)
        {
            StateData = data;
            StateDataChanged?.Invoke(this, new MidiSequenceEventArgs() { MidiSequence = StateData });
        }
    }
}
