using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Managers
{
    public class SJFileHandlerFactory
    {
        private Dictionary<string, Type> _types;

        public SJFileHandlerFactory()
        {
            _types = new Dictionary<string, Type>();
        }

        public void AddFileHandlerType(string name, Type type)
        {
            _types[name] = type;
        }

        public ISJFileHandler CreateFileHandler(string type)
        {
            if (string.IsNullOrWhiteSpace(type) || !_types.ContainsKey(type))
            {
                throw new ArgumentException(type);
            }

            Type fileHandlerType = _types[type];
            ISJFileHandler fileHandler = (ISJFileHandler)Activator.CreateInstance(fileHandlerType);
            return fileHandler;
        }
    }
}
