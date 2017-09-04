using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Managers
{
    public class SJMidiFileHandler : ISJFileHandler
    {
        public Sequence MidiSequence { get; set; }
        public event EventHandler<MidiSequenceEventArgs> MidiSequenceChanged;

        public void Load(string fileName)
        {
            MidiSequence = new Sequence();
            MidiSequence.Load(fileName);
            //MidiSequenceChanged?.Invoke(this, new MidiSequenceEventArgs() { MidiSequence = MidiSequence });
            //LoadMidi(MidiSequence);
        }

        //public void LoadMidi(Sequence sequence)
        //{
        //    StringBuilder lilypondContent = new StringBuilder();
        //    lilypondContent.AppendLine("\\relative c' {");
        //    lilypondContent.AppendLine("\\clef treble");

        //    int division = sequence.Division;
        //    int previousMidiKey = 60; // Central C;
        //    int previousNoteAbsoluteTicks = 0;
        //    double percentageOfBarReached = 0;
        //    bool startedNoteIsClosed = true;

        //    for (int i = 0; i < sequence.Count(); i++)
        //    {
        //        Track track = sequence[i];

        //        foreach (var midiEvent in track.Iterator())
        //        {
        //            IMidiMessage midiMessage = midiEvent.MidiMessage;
        //            switch (midiMessage.MessageType)
        //            {
        //                case MessageType.Meta:
        //                    var metaMessage = midiMessage as MetaMessage;
        //                    switch (metaMessage.MetaType)
        //                    {
        //                        case MetaType.TimeSignature:
        //                            byte[] timeSignatureBytes = metaMessage.GetBytes();
        //                            _beatNote = timeSignatureBytes[0];
        //                            _beatsPerBar = (int)(1 / Math.Pow(timeSignatureBytes[1], -2));
        //                            lilypondContent.AppendLine($"\\time {_beatNote}/{_beatsPerBar}");
        //                            break;
        //                        case MetaType.Tempo:
        //                            byte[] tempoBytes = metaMessage.GetBytes();
        //                            int tempo = (tempoBytes[0] & 0xff) << 16 | (tempoBytes[1] & 0xff) << 8 | (tempoBytes[2] & 0xff);
        //                            _bpm = 60000000 / tempo;
        //                            lilypondContent.AppendLine($"\\tempo 4={_bpm}");
        //                            break;
        //                        case MetaType.EndOfTrack:
        //                            if (previousNoteAbsoluteTicks > 0)
        //                            {
        //                                // Finish the last notelength.
        //                                double percentageOfBar;
        //                                lilypondContent.Append(GetNoteLength(previousNoteAbsoluteTicks, midiEvent.AbsoluteTicks, division, _beatNote, _beatsPerBar, out percentageOfBar));
        //                                lilypondContent.Append(" ");

        //                                percentageOfBarReached += percentageOfBar;
        //                                if (percentageOfBarReached >= 1)
        //                                {
        //                                    lilypondContent.AppendLine("|");
        //                                    percentageOfBar = percentageOfBar - 1;
        //                                }
        //                            }
        //                            break;
        //                        default: break;
        //                    }
        //                    break;
        //                case MessageType.Channel:
        //                    var channelMessage = midiEvent.MidiMessage as ChannelMessage;
        //                    if (channelMessage.Command == ChannelCommand.NoteOn)
        //                    {
        //                        if (channelMessage.Data2 > 0) // Data2 = loudness
        //                        {
        //                            // Append the new note.
        //                            lilypondContent.Append(GetNoteName(previousMidiKey, channelMessage.Data1));

        //                            previousMidiKey = channelMessage.Data1;
        //                            startedNoteIsClosed = false;
        //                        }
        //                        else if (!startedNoteIsClosed)
        //                        {
        //                            // Finish the previous note with the length.
        //                            double percentageOfBar;
        //                            lilypondContent.Append(GetNoteLength(previousNoteAbsoluteTicks, midiEvent.AbsoluteTicks, division, _beatNote, _beatsPerBar, out percentageOfBar));
        //                            previousNoteAbsoluteTicks = midiEvent.AbsoluteTicks;
        //                            lilypondContent.Append(" ");

        //                            percentageOfBarReached += percentageOfBar;
        //                            if (percentageOfBarReached >= 1)
        //                            {
        //                                lilypondContent.AppendLine("|");
        //                                percentageOfBarReached -= 1;
        //                            }
        //                            startedNoteIsClosed = true;
        //                        }
        //                        else
        //                        {
        //                            lilypondContent.Append("r");
        //                        }
        //                    }
        //                    break;
        //            }
        //        }
        //    }

        //    lilypondContent.Append("}");

        //    LoadLilypond(lilypondContent.ToString());
        //}
    }
}
