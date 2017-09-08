using DPA_Musicsheets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Managers
{
    public static class SJNoteBuilder
    {
//        private static SJNoteFactory factory;
        public static SJBaseNote note;

        public static void Prepare(string value)
        {
            note = SJNoteFactory.CreateNote(value);
        }

        public static void SetPitch(SJPitchEnum pitch)
        {
            if(note is SJNote)
            {
                ((SJNote)note).Pitch = pitch;
            }
        }

        public static void SetPitchAlteration(int pitchAlteration)
        {
            if (note is SJNote)
            {
                ((SJNote)note).PitchAlteration = pitchAlteration;
            }
        }

        public static void SetOctave(int octave)
        {
            if (note is SJNote)
            {
                ((SJNote)note).Octave = octave;
            }
        }

        public static void SetNumberOfDots(uint numberofDots)
        {
            note.NumberOfDots = numberofDots;
        }

        public static void SetDuration(SJNoteDurationEnum duration)
        {
            note.Duration = duration;
        }

        public static SJBaseNote Build()
        {
            if (note != null && note.Duration != SJNoteDurationEnum.Undefined)
            {
                if (note is SJNote && ((SJNote)note).Pitch == SJPitchEnum.Undefined)
                {
                    throw new ArgumentException();
                }
                return note;
            }
            throw new ArgumentException();
        }
    }
}
