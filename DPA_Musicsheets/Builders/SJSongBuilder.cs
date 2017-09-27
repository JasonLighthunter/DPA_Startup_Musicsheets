using DPA_Musicsheets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Builders
{
    public class SJSongBuilder
    {
        private SJSong song;

        public void Prepare()
        {
            song = new SJSong();
        }

        public void SetUnheardStartNote(SJUnheardNote unheardStartNote)
        {
            song.UnheardStartNote = unheardStartNote;
        }

        public void SetClefType(SJClefTypeEnum clefType)
        {
            song.ClefType = clefType;
        }

        public void SetTimeSignature(SJTimeSignature timeSignature)
        {
            song.TimeSignature = timeSignature;
        }

        public void SetTempo(ulong tempo)
        {
            song.Tempo = tempo;
        }

        public void AddBar(SJBar bar)
        {
            song.Bars.Add(bar);
        }

        public SJSong Build()
        {
            return song;
        }
    }
}
