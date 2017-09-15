using DPA_Musicsheets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Managers
{
    public static class SJNoteFactory
    {
        private static Dictionary<string, Type> _types = new Dictionary<string, Type>();

        public static void AddNoteType(string name, Type type)
        {
            _types[name] = type;
        }

        public static SJBaseNote CreateNote(string type)
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
