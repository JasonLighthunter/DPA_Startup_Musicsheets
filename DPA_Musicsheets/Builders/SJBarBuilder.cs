using DPA_Musicsheets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Builders
{
    public class SJBarBuilder
    {
        private SJBar bar;

        public void Prepare()
        {
            bar = new SJBar();
        }

        public void AddNote(SJBaseNote note)
        {
            bar.Notes.Add(note);
        }

        public SJBar Build()
        {
            return bar;
        }
    }
}
