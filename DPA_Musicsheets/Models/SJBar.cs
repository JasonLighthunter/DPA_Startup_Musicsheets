using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Models
{
    public class SJBar
    {
        private List<SJBaseNote> _notes = new List<SJBaseNote>();

        public List<SJBaseNote> Notes
        {
            get { return _notes; }
            set { _notes = value; }
        }
    }
}
