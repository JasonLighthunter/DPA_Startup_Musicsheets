using DPA_Musicsheets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Managers
{
    public class SJNoteBuilder
    {
        private SJNoteFactory factory;
        private SJBaseNote note;

        public SJNoteBuilder(SJNoteFactory factory)
        {
            this.factory = factory;
        }

        public void Prepare(string value)
        {
            note = factory.CreateNote(value);
        }

        public void SetPitch(SJPitchEnum pitch)
        {
            if(note is SJNote)
            {
                ((SJNote)note).Pitch = pitch;
            }
        }

        public void SetPitchAlteration(int pitchAlteration)
        {
            if (note is SJNote)
            {
                ((SJNote)note).PitchAlteration = pitchAlteration;
            }
        }

        public void setOctave(int octave)
        {
            if (note is SJNote)
            {
                ((SJNote)note).Octave = octave;
            }
        }

        public void SetNumberOfDots(uint numberofDots)
        {
            note.NumberOfDots = numberofDots;
        }

        public void setDuration(SJNoteDurationEnum duration)
        {
            note.Duration = duration;
        }

        public SJBaseNote Build()
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
