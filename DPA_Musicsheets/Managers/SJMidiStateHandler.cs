using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Managers
{
    public class SJMidiStateHandler : ISJStateHandler<Sequence, MidiSequenceEventArgs>
    {
        public Sequence StateData { get; private set; }

        public static event EventHandler<MidiSequenceEventArgs> StateDataChanged;

        public void UpdateData(Sequence data, string message = null)
        {
            StateData = data;
            StateDataChanged?.Invoke(this, new MidiSequenceEventArgs() { MidiSequence = StateData, Message = message });
        }
    }
}
