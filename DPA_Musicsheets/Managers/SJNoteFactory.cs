using DPA_Musicsheets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Managers
{
    public class SJNoteFactory
    {
        private Dictionary<string, Type> _types;

        public SJNoteFactory()
        {
            _types = new Dictionary<string, Type>();
        }

        public void AddNoteType(string name, Type type)
        {
            _types[name] = type;
        }

        public SJBaseNote CreateNote(string type)
        {
            if (string.IsNullOrWhiteSpace(type) || !_types.ContainsKey(type))
            {
                throw new ArgumentException(type);
            }

            Type noteType = _types[type];
            SJBaseNote note = (SJBaseNote)Activator.CreateInstance(noteType);
            return note;
        }
    }
}
