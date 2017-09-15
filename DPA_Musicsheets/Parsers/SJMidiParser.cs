using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPA_Musicsheets.Models;
using DPA_Musicsheets.Managers;
using DPA_Musicsheets.Utility;

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
            SJBar bar = new SJBar();

            int division = data.Division;

            int previousMidiKey = 60; // Central C;
            song.UnheardStartNote = (SJNote)SetUnheardStartNote(previousMidiKey);
            song.ClefType = SJClefTypeEnum.Treble;
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
                                    Console.WriteLine("=== Creating TimeSignature");
                                    byte[] timeSignatureBytes = metaMessage.GetBytes();
                                    uint _beatNote = timeSignatureBytes[0];
                                    uint _beatsPerBar = (uint)(1 / Math.Pow(timeSignatureBytes[1], -2));
                                    song.TimeSignature = new SJTimeSignature { NoteValueOfBeat = _beatNote, NumberOfBeatsPerBar = _beatsPerBar };
                                    break;
                                case MetaType.Tempo:
                                    Console.WriteLine("=== Creating Tempo");
                                    byte[] tempoBytes = metaMessage.GetBytes();
                                    long tempo = (tempoBytes[0] & 0xff) << 16 | (tempoBytes[1] & 0xff) << 8 | (tempoBytes[2] & 0xff);
                                    ulong _bpm = (ulong)(60000000 / tempo);
                                    song.Tempo = _bpm;
                                    break;
                                case MetaType.EndOfTrack: //magic
                                    Console.WriteLine("=== Creating endOf Track");
                                    if (previousNoteAbsoluteTicks > 0)
                                    {
                                        // Finish the last notelength.
                                        AddNoteToBar(ref song, ref bar, previousNoteAbsoluteTicks, midiEvent.AbsoluteTicks, division, ref percentageOfBarReached);
                                        AddBarIfFull(ref song, ref bar, ref percentageOfBarReached);
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case MessageType.Channel:
                            Console.WriteLine("=== CreatingNote");
                            var channelMessage = midiEvent.MidiMessage as ChannelMessage;
                            if (channelMessage.Command == ChannelCommand.NoteOn)
                            {
                                if (channelMessage.Data2 > 0) // Data2 = loudness
                                {
                                    Console.WriteLine("===@ Creating Octave and Pitch");
                                    SJNoteBuilder.Prepare("N");
                                    SetPitchAndAlteration(channelMessage.Data1);
                                    SetOctave(previousMidiKey, channelMessage.Data1);

                                    previousMidiKey = channelMessage.Data1;
                                    startedNoteIsClosed = false;
                                }
                                else if (!startedNoteIsClosed)
                                {
                                    AddNoteToBar(ref song, ref bar, previousNoteAbsoluteTicks, midiEvent.AbsoluteTicks, division, ref percentageOfBarReached);
                                    AddBarIfFull(ref song, ref bar, ref percentageOfBarReached);

                                    previousNoteAbsoluteTicks = midiEvent.AbsoluteTicks;
                                    startedNoteIsClosed = true;
                                }
                                else
                                {
                                    Console.WriteLine("===@ Preparing Rest");
                                    SJNoteBuilder.Prepare("R");
                                }
                            }
                            break;
                    }
                }
            }
            //throw new NotImplementedException();
            return song;
        }

        private void SetPitchAndAlteration(int midiKey)
        {
            switch (midiKey % 12)
            {
                case 0:
                    SJNoteBuilder.SetPitch(SJPitchEnum.C);
                    break;
                case 1:
                    SJNoteBuilder.SetPitch(SJPitchEnum.C);
                    SJNoteBuilder.SetPitchAlteration(1);
                    break;
                case 2:
                    SJNoteBuilder.SetPitch(SJPitchEnum.D);
                    break;
                case 3:
                    SJNoteBuilder.SetPitch(SJPitchEnum.D);
                    SJNoteBuilder.SetPitchAlteration(1);
                    break;
                case 4:
                    SJNoteBuilder.SetPitch(SJPitchEnum.E);
                    break;
                case 5:
                    SJNoteBuilder.SetPitch(SJPitchEnum.F);
                    break;
                case 6:
                    SJNoteBuilder.SetPitch(SJPitchEnum.F);
                    SJNoteBuilder.SetPitchAlteration(1);
                    break;
                case 7:
                    SJNoteBuilder.SetPitch(SJPitchEnum.G);
                    break;
                case 8:
                    SJNoteBuilder.SetPitch(SJPitchEnum.G);
                    SJNoteBuilder.SetPitchAlteration(1);
                    break;
                case 9:
                    SJNoteBuilder.SetPitch(SJPitchEnum.A);
                    break;
                case 10:
                    SJNoteBuilder.SetPitch(SJPitchEnum.A);
                    SJNoteBuilder.SetPitchAlteration(1);
                    break;
                case 11:
                    SJNoteBuilder.SetPitch(SJPitchEnum.B);
                    break;
            }
        }

        private void SetOctave(int previousMidiKey, int midiKey)
        {
            int octave = (midiKey / 12) - 1;
            int distance = midiKey - previousMidiKey;

            while (distance < -6)
            {
                octave--;
                distance += 8;
            }

            while (distance > 6)
            {
                octave++;
                distance -= 8;
            }

            SJNoteBuilder.SetOctave(octave);
        }

        private void SetDotsAndDuration(SJTimeSignature timeSignature, int absoluteTicks, int nextNoteAbsoluteTicks, int division, out double percentageOfBar)
        {
            int duration = 0;
            uint dots = 0;

            double deltaTicks = nextNoteAbsoluteTicks - absoluteTicks;

            if (deltaTicks <= 0)
            {
                percentageOfBar = 0;
            }

            double percentageOfBeatNote = deltaTicks / division;
            percentageOfBar = (1.0 / timeSignature.NumberOfBeatsPerBar) * percentageOfBeatNote;

            for (int noteLength = 32; noteLength >= 1; noteLength -= 1)
            {
                double absoluteNoteLength = (1.0 / noteLength);

                if (percentageOfBar <= absoluteNoteLength)
                {
                    if (noteLength < 2)
                    {
                        noteLength = 2;
                    }

                    //TODO: duration declaration verplatsen
                    int subtractDuration = this.GetSubtractDuration(noteLength);
                    duration = this.GetDuration(noteLength);

                    double currentTime = 0;

                    while (currentTime < (noteLength - subtractDuration))
                    {
                        double addtime = 1 / ((subtractDuration / timeSignature.NoteValueOfBeat) * Math.Pow(2, dots));

                        if (addtime <= 0)
                        {
                            break;
                        }

                        currentTime += addtime;

                        if (currentTime <= (noteLength - subtractDuration))
                        {
                            dots++;
                        }

                        if (dots >= 4)
                        {
                            break;
                        }
                    }
                    break;
                }
            }

            SJNoteBuilder.SetNumberOfDots(dots);
            SJNoteBuilder.SetDuration(EnumConverters.ConvertDoubleToSJNoteDurationEnum(1.0 / duration));
        }

        private SJBaseNote SetUnheardStartNote(int midiPitchValue)
        {
            SJNoteBuilder.Prepare("N");
            SetOctave(midiPitchValue, midiPitchValue);
            SetPitchAndAlteration(midiPitchValue);
            return SJNoteBuilder.Build();
        }

        // AddNoteToBar and AddBarIfFull are seperated in favor of Modular Understandibility.
        private void AddNoteToBar(ref SJSong song, ref SJBar bar, int previousNoteAbsoluteTicks, int currentNoteAbsoluteTicks, int division, ref double percentageOfBarReached)
        {
            double percentageOfBar;

            SetDotsAndDuration(song.TimeSignature, previousNoteAbsoluteTicks, currentNoteAbsoluteTicks, division, out percentageOfBar);
            bar.Notes.Add(SJNoteBuilder.Build());

            percentageOfBarReached += percentageOfBar;
        }

        private void AddBarIfFull(ref SJSong song, ref SJBar bar, ref double percentageOfBarReached)
        {
            if (percentageOfBarReached >= 1)
            {
                song.Bars.Add(bar);
                bar.Notes.Clear();
                percentageOfBarReached -= 1;
            }
        }

        private int GetSubtractDuration(int noteLength)
        {
            for (int i = 1; i < 6; i++)
            {
                if (noteLength < Math.Pow(2, i))
                {
                    return (int)Math.Pow(2, i - 1);
                }
            }
            return 32;
        }
        private int GetDuration(int noteLength)
        {
            for (int i = 4; i > 0; i++)
            {
                if (noteLength > Math.Pow(2, i))
                {
                    return (int)Math.Pow(2, i + 1);
                }
            }
            return 2;
        }
    }
}

