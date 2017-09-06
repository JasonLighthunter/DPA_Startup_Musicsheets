using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPA_Musicsheets.Models;
using DPA_Musicsheets.Managers;

namespace DPA_Musicsheets.Parsers
{
    class SJMidiParser : ISJParser<Sequence>
    {
        public Sequence ParseFromSJSong(SJSong song)
        {
            throw new NotImplementedException();
        }

        public SJSong ParseToSJSong(Sequence data)
        {
            SJSong song = new SJSong();

            int division = data.Division;
            int previousMidiKey = 60; // Central C;
            int previousNoteAbsoluteTicks = 0;
            double percentageOfBarReached = 0;
            bool startedNoteIsClosed = true;

            for (int i = 0; i < data.Count(); i++) //voor elke track in de sequence
            {
                Track track = data[i];//selecteer een track

                foreach (var midiEvent in track.Iterator())//loop door elk event per track
                {
                    IMidiMessage midiMessage = midiEvent.MidiMessage;
                    switch (midiMessage.MessageType)
                    {
                        case MessageType.Meta:
                            var metaMessage = midiMessage as MetaMessage;
                            switch (metaMessage.MetaType)
                            {
                                case MetaType.TimeSignature:
                                    byte[] timeSignatureBytes = metaMessage.GetBytes();
                                    uint _beatNote = timeSignatureBytes[0];
                                    uint _beatsPerBar = (uint)(1 / Math.Pow(timeSignatureBytes[1], -2));
                                    song.TimeSignature = new SJTimeSignature { NoteValueOfBeat = _beatNote, NumberOfBeatsPerBar = _beatsPerBar };
                                    break;
                                case MetaType.Tempo:
                                    byte[] tempoBytes = metaMessage.GetBytes();
                                    long tempo = (tempoBytes[0] & 0xff) << 16 | (tempoBytes[1] & 0xff) << 8 | (tempoBytes[2] & 0xff);
                                    ulong _bpm = (ulong) (60000000 / tempo);
                                    song.Tempo = _bpm;
                                    break;
                                case MetaType.EndOfTrack: //magic
                                    //if (previousNoteAbsoluteTicks > 0)
                                    //{
                                    //    // Finish the last notelength.
                                    //    double percentageOfBar;
                                    //    lilypondContent.Append(GetNoteLength(previousNoteAbsoluteTicks, midiEvent.AbsoluteTicks, division, _beatNote, _beatsPerBar, out percentageOfBar));
                                    //    lilypondContent.Append(" ");

                                    //    percentageOfBarReached += percentageOfBar;
                                    //    if (percentageOfBarReached >= 1)
                                    //    {
                                    //        lilypondContent.AppendLine("|");
                                    //        percentageOfBar = percentageOfBar - 1;
                                    //    }
                                    //}
                                    break;
                                default: break;
                            }
                            break;
                        case MessageType.Channel:
                            var channelMessage = midiEvent.MidiMessage as ChannelMessage;
                            song.Notes.
                            if (channelMessage.Command == ChannelCommand.NoteOn)
                            {
                                if (channelMessage.Data2 > 0) // Data2 = loudness
                                {
                                    // Append the new note.
                                    lilypondContent.Append(GetNoteName(previousMidiKey, channelMessage.Data1));

                                    previousMidiKey = channelMessage.Data1;
                                    startedNoteIsClosed = false;
                                }
                                else if (!startedNoteIsClosed)
                                {
                                    // Finish the previous note with the length.
                                    double percentageOfBar;
                                    lilypondContent.Append(GetNoteLength(previousNoteAbsoluteTicks, midiEvent.AbsoluteTicks, division, _beatNote, _beatsPerBar, out percentageOfBar));
                                    previousNoteAbsoluteTicks = midiEvent.AbsoluteTicks;
                                    lilypondContent.Append(" ");

                                    percentageOfBarReached += percentageOfBar;
                                    if (percentageOfBarReached >= 1)
                                    {
                                        lilypondContent.AppendLine("|");
                                        percentageOfBarReached -= 1;
                                    }
                                    startedNoteIsClosed = true;
                                }
                                else
                                {
                                    lilypondContent.Append("r");
                                }
                            }
                            break;
                    }
                }
            }
            throw new NotImplementedException();
        }

        private SJBaseNote tempFunction()
        {
            int octave = (channelMessage.Data1 / 12) - 1;

            SJNoteBuilder.Prepare("N");
            SJNoteBuilder.setOctave(octave);
            return SJNoteBuilder.Build();
            //GetNoteName(previousMidiKey, channelMessage.Data1)
            //GetNoteLength(previousNoteAbsoluteTicks, midiEvent.AbsoluteTicks, division, _beatNote, _beatsPerBar, out percentageOfBar)

                int duration = 0;
                int dots = 0;

                double deltaTicks = nextNoteAbsoluteTicks - absoluteTicks;

                if (deltaTicks <= 0)
                {
                    percentageOfBar = 0;
                    return String.Empty;
                }

                double percentageOfBeatNote = deltaTicks / division;
                percentageOfBar = (1.0 / beatsPerBar) * percentageOfBeatNote;

                for (int noteLength = 32; noteLength >= 1; noteLength -= 1)
                {
                    double absoluteNoteLength = (1.0 / noteLength);

                    if (percentageOfBar <= absoluteNoteLength)
                    {
                        if (noteLength < 2)
                            noteLength = 2;

                        int subtractDuration;

                        if (noteLength == 32)
                            subtractDuration = 32;
                        else if (noteLength >= 16)
                            subtractDuration = 16;
                        else if (noteLength >= 8)
                            subtractDuration = 8;
                        else if (noteLength >= 4)
                            subtractDuration = 4;
                        else
                            subtractDuration = 2;

                        if (noteLength >= 17)
                            duration = 32;
                        else if (noteLength >= 9)
                            duration = 16;
                        else if (noteLength >= 5)
                            duration = 8;
                        else if (noteLength >= 3)
                            duration = 4;
                        else
                            duration = 2;

                        double currentTime = 0;

                        while (currentTime < (noteLength - subtractDuration))
                        {
                            var addtime = 1 / ((subtractDuration / beatNote) * Math.Pow(2, dots));
                            if (addtime <= 0) break;
                            currentTime += addtime;
                            if (currentTime <= (noteLength - subtractDuration))
                            {
                                dots++;
                            }
                            if (dots >= 4) break;
                        }

                        break;
                    }
                }

                //return duration + new String('.', dots);

                
                string name = "";
                switch (midiKey % 12)
                {
                    case 0:
                        name = "c";
                        break;
                    case 1:
                        name = "cis";
                        break;
                    case 2:
                        name = "d";
                        break;
                    case 3:
                        name = "dis";
                        break;
                    case 4:
                        name = "e";
                        break;
                    case 5:
                        name = "f";
                        break;
                    case 6:
                        name = "fis";
                        break;
                    case 7:
                        name = "g";
                        break;
                    case 8:
                        name = "gis";
                        break;
                    case 9:
                        name = "a";
                        break;
                    case 10:
                        name = "ais";
                        break;
                    case 11:
                        name = "b";
                        break;
                }

                int distance = midiKey - previousMidiKey;
                while (distance < -6)
                {
                    name += ",";
                    distance += 8;
                }

                while (distance > 6)
                {
                    name += "'";
                    distance -= 8;
                }

                //return name;
        }
    }
}
