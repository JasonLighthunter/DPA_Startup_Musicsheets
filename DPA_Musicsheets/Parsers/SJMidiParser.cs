using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPA_Musicsheets.Models;
using DPA_Musicsheets.Managers;
using DPA_Musicsheets.Utility;
using PSAMControlLibrary;
using DPA_Musicsheets.Builders;

namespace DPA_Musicsheets.Parsers
{
    public class SJMidiParser : ISJParser<Sequence>
    {
        private SJNoteBuilder _noteBuilder { get; set; }

        public SJMidiParser(SJNoteBuilder noteBuilder)
        {
            _noteBuilder = noteBuilder;
        }

        private List<string> notesOrderWithCrosses = new List<string>() { "c", "cis", "d", "dis", "e", "f", "fis", "g", "gis", "a", "ais", "b" };
        private int absoluteTicks = 0;
        public Sequence ParseFromSJSong(SJSong song)
        {
            Sequence midiSequence = new Sequence();

            Track metaTrack = new Track();
            midiSequence.Add(metaTrack);

            // Calculate tempo
            byte[] tempo = GetMidiTempo(song.Tempo);
            metaTrack.Insert(0 /* Insert at 0 ticks*/, new MetaMessage(MetaType.Tempo, tempo));


            byte[] timeSignature = GetMidiTimeSignature(song.TimeSignature);
            metaTrack.Insert(absoluteTicks, new MetaMessage(MetaType.TimeSignature, timeSignature));

            Track notesTrack = new Track();

            FillNotesTrackWithBars(song.Bars, song.TimeSignature.NoteValueOfBeat, midiSequence.Division, ref notesTrack);

            midiSequence.Add(notesTrack);

            notesTrack.Insert(absoluteTicks, MetaMessage.EndOfTrackMessage);
            metaTrack.Insert(absoluteTicks, MetaMessage.EndOfTrackMessage);
            return midiSequence;
        }

        public SJSong ParseToSJSong(Sequence data)
        {
            SJSongBuilder songBuilder = new SJSongBuilder();
            SJBarBuilder barBuilder = new SJBarBuilder();
            SJTimeSignatureBuilder timeSignatureBuilder = new SJTimeSignatureBuilder();

            songBuilder.Prepare();
            barBuilder.Prepare();

            SJTimeSignature timeSignature = null;

            int division = data.Division;

            int previousMidiKey = 60; // Central C;
            songBuilder.SetUnheardStartNote(SetUnheardStartNote(previousMidiKey));
            songBuilder.SetClefType(SJClefTypeEnum.Treble);
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
									SetTimeSignature(songBuilder, timeSignatureBuilder, metaMessage, ref timeSignature);
                                    break;
                                case MetaType.Tempo:
									SetTempo(songBuilder, metaMessage);
                                    break;
                                case MetaType.EndOfTrack:
                                    Console.WriteLine("=== Creating endOf Track");
                                    if (previousNoteAbsoluteTicks > 0)
                                    {
                                        // Finish the last notelength.
                                        AddNoteToBar(timeSignature, barBuilder, previousNoteAbsoluteTicks, midiEvent.AbsoluteTicks, division, ref percentageOfBarReached);
                                        AddBarIfFull(songBuilder, barBuilder, ref percentageOfBarReached);
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
                                    _noteBuilder.Prepare("N");
                                    SetPitchAndAlteration(channelMessage.Data1);
                                    SetOctave(previousMidiKey, channelMessage.Data1);

                                    previousMidiKey = channelMessage.Data1;
                                    startedNoteIsClosed = false;
                                }
                                else if (!startedNoteIsClosed)
                                {
                                    AddNoteToBar(timeSignature, barBuilder, previousNoteAbsoluteTicks, midiEvent.AbsoluteTicks, division, ref percentageOfBarReached);
                                    AddBarIfFull(songBuilder, barBuilder, ref percentageOfBarReached);

                                    previousNoteAbsoluteTicks = midiEvent.AbsoluteTicks;
                                    startedNoteIsClosed = true;
                                }
                                else
                                {
                                    Console.WriteLine("===@ Preparing Rest");
                                    _noteBuilder.Prepare("R");
                                }
                            }
                            break;
                    }
                }
            }
            return songBuilder.Build();
        }

		private void SetTimeSignature(SJSongBuilder songBuilder, SJTimeSignatureBuilder timeSignatureBuilder, MetaMessage metaMessage, ref SJTimeSignature timeSignature)
		{
			Console.WriteLine("=== Creating TimeSignature");
			byte[] timeSignatureBytes = metaMessage.GetBytes();
			uint _beatNote = timeSignatureBytes[0];
			uint _beatsPerBar = (uint)(1 / Math.Pow(timeSignatureBytes[1], -2));
			timeSignatureBuilder.Prepare();
			timeSignatureBuilder.SetNoteValueOfBeat(_beatNote);
			timeSignatureBuilder.SetNumberOfBeatsPerBar(_beatsPerBar);
			timeSignature = timeSignatureBuilder.Build();
			songBuilder.SetTimeSignature(timeSignature);
		}

		private void SetTempo(SJSongBuilder songBuilder, MetaMessage metaMessage)
		{
			Console.WriteLine("=== Creating Tempo");
			byte[] tempoBytes = metaMessage.GetBytes();
			long tempo = (tempoBytes[0] & 0xff) << 16 | (tempoBytes[1] & 0xff) << 8 | (tempoBytes[2] & 0xff);
			ulong _bpm = (ulong)(60000000 / tempo);
			songBuilder.SetTempo(_bpm);
		}

        private void SetPitchAndAlteration(int midiKey)
        {
            switch (midiKey % 12)
            {
                case 0:
                    _noteBuilder.SetPitch(SJPitchEnum.C);
                    break;
                case 1:
                    _noteBuilder.SetPitch(SJPitchEnum.C);
                    _noteBuilder.SetPitchAlteration(1);
                    break;
                case 2:
                    _noteBuilder.SetPitch(SJPitchEnum.D);
                    break;
                case 3:
                    _noteBuilder.SetPitch(SJPitchEnum.D);
                    _noteBuilder.SetPitchAlteration(1);
                    break;
                case 4:
                    _noteBuilder.SetPitch(SJPitchEnum.E);
                    break;
                case 5:
                    _noteBuilder.SetPitch(SJPitchEnum.F);
                    break;
                case 6:
                    _noteBuilder.SetPitch(SJPitchEnum.F);
                    _noteBuilder.SetPitchAlteration(1);
                    break;
                case 7:
                    _noteBuilder.SetPitch(SJPitchEnum.G);
                    break;
                case 8:
                    _noteBuilder.SetPitch(SJPitchEnum.G);
                    _noteBuilder.SetPitchAlteration(1);
                    break;
                case 9:
                    _noteBuilder.SetPitch(SJPitchEnum.A);
                    break;
                case 10:
                    _noteBuilder.SetPitch(SJPitchEnum.A);
                    _noteBuilder.SetPitchAlteration(1);
                    break;
                case 11:
                    _noteBuilder.SetPitch(SJPitchEnum.B);
                    break;
            }
        }

        private byte[] GetMidiTempo(ulong songTempo)
        {
            int speed = (int)(60000000 / songTempo);
            byte[] tempo = new byte[3];
            tempo[0] = (byte)((speed >> 16) & 0xff);
            tempo[1] = (byte)((speed >> 8) & 0xff);
            tempo[2] = (byte)(speed & 0xff);
            return tempo;
        }

        private byte[] GetMidiTimeSignature(SJTimeSignature songTimeSignature)
        {
            byte[] timeSignature = new byte[4];
            timeSignature[0] = (byte)songTimeSignature.NumberOfBeatsPerBar;
            timeSignature[1] = (byte)(Math.Log(songTimeSignature.NoteValueOfBeat) / Math.Log(2));
            return timeSignature;
        }

        private void FillNotesTrackWithBars(List<SJBar> bars, uint noteValueOfBeat, int midiDivision, ref Track notesTrack)
        {
            foreach(SJBar bar in bars)
            {
                FillNotesTrackWithNotes(bar.Notes, noteValueOfBeat, midiDivision, ref notesTrack);
            }
        }

        private void FillNotesTrackWithNotes(List<SJBaseNote> notes, uint noteValueOfBeat, int midiDivision, ref Track notesTrack)
        {
            foreach (SJBaseNote tempNote in notes)
            {

                // Calculate duration
                double absoluteLength = (EnumConverters.ConvertSJNoteDurationEnumToDouble(tempNote.Duration));
                absoluteLength += (absoluteLength / 2.0) * tempNote.NumberOfDots;

                double relationToQuartNote = noteValueOfBeat / 4.0;
                double percentageOfBeatNote = (1.0 / noteValueOfBeat) / absoluteLength;
                double deltaTicks = (midiDivision / relationToQuartNote) / percentageOfBeatNote;

                int noteHeight = 0;
                int volume = 0;
                if (tempNote is SJNote)
                {
                    // Calculate height
                    int octave = ((SJNote)tempNote).Octave + 1;
                    int pitchValue = notesOrderWithCrosses.IndexOf(((SJNote)tempNote).Pitch.ToString().ToLower());
                    noteHeight = pitchValue + octave * 12;
                    noteHeight += ((SJNote)tempNote).PitchAlteration;

                    volume = 90;
                }

                notesTrack.Insert(absoluteTicks, new ChannelMessage(ChannelCommand.NoteOn, 1, noteHeight, volume)); // Data2 = volume

                absoluteTicks += (int)deltaTicks;
                notesTrack.Insert(absoluteTicks, new ChannelMessage(ChannelCommand.NoteOn, 1, noteHeight, 0)); // Data2 = volume
            }
        }

        private void SetOctave(int previousMidiKey, int midiKey)
        {
            int octave = (midiKey / 12) - 1;

            _noteBuilder.SetOctave(octave);
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

            _noteBuilder.SetNumberOfDots(dots);
            _noteBuilder.SetDuration(EnumConverters.ConvertDoubleToSJNoteDurationEnum(1.0 / duration));
        }

        private SJUnheardNote SetUnheardStartNote(int midiPitchValue)
        {
            _noteBuilder.Prepare("U");
            SetOctave(midiPitchValue, midiPitchValue);
            SetPitchAndAlteration(midiPitchValue);
            return (SJUnheardNote)_noteBuilder.Build();
        }

        // AddNoteToBar and AddBarIfFull are seperated in favor of Modular Understandibility.
        private void AddNoteToBar(SJTimeSignature timeSignature, SJBarBuilder barBuilder, int previousNoteAbsoluteTicks, int currentNoteAbsoluteTicks, int division, ref double percentageOfBarReached)
        {
            double percentageOfBar;

            SetDotsAndDuration(timeSignature, previousNoteAbsoluteTicks, currentNoteAbsoluteTicks, division, out percentageOfBar);
            barBuilder.AddNote(_noteBuilder.Build());

            percentageOfBarReached += percentageOfBar;
        }

        private void AddBarIfFull(SJSongBuilder songBuilder, SJBarBuilder barBuilder, ref double percentageOfBarReached)
        {
            if (percentageOfBarReached >= 1)
            {
                //SJBar newBar = bar;
                //song.Bars.Add(newBar);
                songBuilder.AddBar(barBuilder.Build());
                barBuilder.Prepare();
                //bar.Notes.Clear();
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
            for (int i = 4; i > 0; i--)
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

