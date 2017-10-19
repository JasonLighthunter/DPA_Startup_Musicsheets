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
        #region instanceVariables
        private SJTimeSignature _timeSignature { get; set; }
        private double _percentageOfBarReached { get; set; }
        private int _division { get; set; }
        private int _previousNoteAbsoluteTicks { get; set; }
        private SJBarBuilder _barBuilder { get; set; }
        private SJSongBuilder _songBuilder { get; set; }
        private bool _startedNoteIsClosed { get; set; }

        private Dictionary<MetaType, Action<MidiEvent>> _midiToSJSongMetaParserDictionary { get; set; }
        private Dictionary<MessageType, Action<MidiEvent>> _midiToSJSongMessageParserDicionary { get; set; }

        private SJNoteBuilder _noteBuilder { get; set; }
        
        private int absoluteTicks = 0;
        #endregion

        public SJMidiParser(SJNoteBuilder noteBuilder)
        {
            _noteBuilder = noteBuilder;

            _midiToSJSongMetaParserDictionary = new Dictionary<MetaType, Action<MidiEvent>>()
            {
                { MetaType.Copyright, null },
                { MetaType.CuePoint, null },
                { MetaType.DeviceName, null },
                { MetaType.EndOfTrack, SetEndOfTrack },
                { MetaType.InstrumentName, null },
                { MetaType.KeySignature, null },
                { MetaType.Lyric, null },
                { MetaType.Marker, null },
                { MetaType.ProgramName, null },
                { MetaType.ProprietaryEvent, null },
                { MetaType.SequenceNumber, null },
                { MetaType.SmpteOffset, null },
                { MetaType.Tempo, SetTempo },
                { MetaType.Text, null },
                { MetaType.TimeSignature, SetTimeSignature },
                { MetaType.TrackName, null }
            };
            _midiToSJSongMessageParserDicionary = new Dictionary<MessageType, Action<MidiEvent>>()
            {
                { MessageType.Channel, ParseFromChannelData },
                { MessageType.Meta, ParseFromMetaData },
                { MessageType.Short, null },
                { MessageType.SystemCommon, null },
                { MessageType.SystemExclusive, null },
                { MessageType.SystemRealtime, null }
            };
        }

        #region publicMethods
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
            //start tempcode?
            _songBuilder = new SJSongBuilder();
            _barBuilder = new SJBarBuilder();
            //end tempcode?

            _songBuilder.Prepare();
            _barBuilder.Prepare();

            _timeSignature = null;

            _division = data.Division;

            int previousMidiKey = 60; // Central C; //vervangen door constante
            _songBuilder.SetUnheardStartNote(SetUnheardStartNote(previousMidiKey));
            _songBuilder.SetClefType(SJClefTypeEnum.Treble);
            _previousNoteAbsoluteTicks = 0;
            _percentageOfBarReached = 0;
            _startedNoteIsClosed = true;

            for (int i = 0; i < data.Count(); i++) //voor elke track in de sequence
            {
                Track track = data[i];//selecteer een track
                
                foreach (var midiEvent in track.Iterator())//loop door elk event per track
                {
                    IMidiMessage midiMessage = midiEvent.MidiMessage;

                    _midiToSJSongMessageParserDicionary[midiMessage.MessageType]?.Invoke(midiEvent);
                }
            }
            return _songBuilder.Build();
        }
        #endregion

        #region privateMethods
        #region privateMethodsFromSJSong
        
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
            foreach (SJBar bar in bars)
            {
                FillNotesTrackWithNotes(bar.Notes, noteValueOfBeat, midiDivision, ref notesTrack);
            }
        }

        private void FillNotesTrackWithNotes(List<SJBaseNote> notes, uint noteValueOfBeat, int midiDivision, ref Track notesTrack)
        {
            List<string> notesOrderWithCrosses = new List<string>() { "c", "cis", "d", "dis", "e", "f", "fis", "g", "gis", "a", "ais", "b" };
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
        #endregion
        #region privateMethodsToSJSong
        private void ParseFromChannelData(MidiEvent midiEvent)
        {
            var channelMessage = midiEvent.MidiMessage as ChannelMessage;
            if (channelMessage.Command == ChannelCommand.NoteOn)
            {
                if (channelMessage.Data2 > 0) // Data2 = loudness
                {
                    _noteBuilder.Prepare("N");
                    SetPitchAndAlteration(channelMessage.Data1);
                    SetOctave(channelMessage.Data1);
                    _startedNoteIsClosed = false;
                }
                else if (!_startedNoteIsClosed)
                {
                    AddNoteToBar(midiEvent.AbsoluteTicks);
                    AddBarIfFull();

                    _previousNoteAbsoluteTicks = midiEvent.AbsoluteTicks;
                    _startedNoteIsClosed = true;
                }
                else
                {
                    _noteBuilder.Prepare("R");
                }
            }
        }

        private void ParseFromMetaData(MidiEvent midiEvent)
        {
            var metaMessage = midiEvent.MidiMessage as MetaMessage;
            _midiToSJSongMetaParserDictionary[metaMessage.MetaType]?.Invoke(midiEvent);
        }

        private void SetEndOfTrack(MidiEvent midiEvent)
        {
            if (_previousNoteAbsoluteTicks > 0)
            {
                int absoluteTicks = midiEvent.AbsoluteTicks;
                AddNoteToBar(absoluteTicks);
                AddBarIfFull();
            }
        }

        private void SetTimeSignature(MidiEvent midiEvent)
        {
            MetaMessage metaMessage = midiEvent.MidiMessage as MetaMessage;
            SJTimeSignatureBuilder timeSignatureBuilder = new SJTimeSignatureBuilder();

            byte[] timeSignatureBytes = metaMessage.GetBytes();
            uint _beatNote = timeSignatureBytes[0];
            uint _beatsPerBar = (uint)(1 / Math.Pow(timeSignatureBytes[1], -2));

            timeSignatureBuilder.Prepare();
            timeSignatureBuilder.SetNoteValueOfBeat(_beatNote);
            timeSignatureBuilder.SetNumberOfBeatsPerBar(_beatsPerBar);

            _timeSignature = timeSignatureBuilder.Build();

            _songBuilder.SetTimeSignature(_timeSignature);
        }

        private void SetTempo(MidiEvent midiEvent)
        {
            MetaMessage metaMessage = midiEvent.MidiMessage as MetaMessage;
            byte[] tempoBytes = metaMessage.GetBytes();

            long tempo = (tempoBytes[0] & 0xff) << 16 | (tempoBytes[1] & 0xff) << 8 | (tempoBytes[2] & 0xff);
            ulong _bpm = (ulong)(60000000 / tempo);

            _songBuilder.SetTempo(_bpm);
        }

        private void SetOctave(int midiKey)
        {
            int octave = (midiKey / 12) - 1;

            _noteBuilder.SetOctave(octave);
        }

        private void SetPitchAndAlteration(int midiKey)
        {
            List<int> crossedMidiKeyValues = new List<int>() { 1, 3, 6, 8, 10 };

            _noteBuilder.SetPitch(EnumConverters.ConvertMidiKeyToSJNotePitchEnum(midiKey));

            if (crossedMidiKeyValues.Contains(midiKey % 12))
            {
                _noteBuilder.SetPitchAlteration(1);
            }
        }

        private void SetDotsAndDuration(int nextNoteAbsoluteTicks, out double percentageOfBar)
        {
            int duration = 0;
            uint dots = 0;

            double deltaTicks = nextNoteAbsoluteTicks - _previousNoteAbsoluteTicks;

            double percentageOfBeatNote = deltaTicks / _division;
            percentageOfBar = (1.0 / _timeSignature.NumberOfBeatsPerBar) * percentageOfBeatNote;

            for (int noteLength = 32; noteLength >= 1; noteLength -= 1)
            {
                double absoluteNoteLength = (1.0 / noteLength);

                if (percentageOfBar <= absoluteNoteLength)
                {
                    noteLength = (noteLength < 2) ? 2 : noteLength;

                    //TODO: duration declaration verplaatsen
                    uint subtractDuration = GetSubtractDuration(noteLength);
                    duration = GetDuration(noteLength);

                    double currentTime = 0;

                    while (currentTime < (noteLength - subtractDuration))
                    {
                        double addtime = 1 / ((subtractDuration / _timeSignature.NoteValueOfBeat) * Math.Pow(2, dots));

                        currentTime += addtime;

                        if (currentTime <= (noteLength - subtractDuration))
                        {
                            dots++;
                        }
                    }
                    break;
                }
            }

            _noteBuilder.SetNumberOfDots(dots);
            _noteBuilder.SetDuration(EnumConverters.ConvertDoubleToSJNoteDurationEnum(1.0 / duration));
        }

        private uint GetSubtractDuration(int noteLength)
        {
            for (int i = 1; i < 6; i++)
            {
                if (noteLength < Math.Pow(2, i))
                {
                    return (uint)Math.Pow(2, i - 1);
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

        private SJUnheardNote SetUnheardStartNote(int previousMidiKey)
        {
            _noteBuilder.Prepare("U");
            SetOctave(previousMidiKey);
            SetPitchAndAlteration(previousMidiKey);

            return (SJUnheardNote)_noteBuilder.Build();
        }

        // AddNoteToBar and AddBarIfFull are seperated in favor of Modular Understandibility.
        private void AddNoteToBar(int currentNoteAbsoluteTicks)
        {
            double percentageOfBar;

            SetDotsAndDuration(currentNoteAbsoluteTicks, out percentageOfBar);
            _barBuilder.AddNote(_noteBuilder.Build());

            _percentageOfBarReached += percentageOfBar;
        }

        private void AddBarIfFull()
        {
            if (_percentageOfBarReached >= 1)
            {
                _songBuilder.AddBar(_barBuilder.Build());
                _barBuilder.Prepare();
                _percentageOfBarReached -= 1;
            }
        }
        #endregion
        #endregion
    }
}

