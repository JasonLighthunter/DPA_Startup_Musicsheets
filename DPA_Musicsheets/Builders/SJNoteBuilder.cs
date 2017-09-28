using DPA_Musicsheets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Builders
{
    public class SJNoteBuilder
    {
        private SJNoteFactory _noteFactory;
        public SJBaseNote note;

        public SJNoteBuilder(SJNoteFactory noteFactory)
        {
            _noteFactory = noteFactory;
            _noteFactory.AddNoteType("R", typeof(SJRest));
            _noteFactory.AddNoteType("N", typeof(SJNote));
            _noteFactory.AddNoteType("U", typeof(SJUnheardNote));
        }

        public void Prepare(string value)
        {
            note = _noteFactory.CreateNote(value);
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

        public void SetOctave(int octave)
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

        public void SetDuration(SJNoteDurationEnum duration)
        {
            note.Duration = duration;
        }

        public SJBaseNote Build()
        {
            if (note != null && (note.Duration != SJNoteDurationEnum.Undefined || note is SJUnheardNote))
            {
                if (note is SJNote && ((SJNote)note).Pitch == SJPitchEnum.Undefined)
                {
                    throw new ArgumentException();
                }
				Console.WriteLine("NoteMade");
                return note;
            }
            throw new ArgumentException();
        }
    }
}
