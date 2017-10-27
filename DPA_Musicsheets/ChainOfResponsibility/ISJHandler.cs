using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.ChainOfResponsibility
{
    public interface ISJHandler
    {
        void SetNext(ISJHandler handler);

        bool Handle(HashSet<string> pressedKeys);
    }
}
