using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Managers
{
    public interface ISJStateHandler<T, A> where T : class where A : class
    {
        T StateData { get;}

        event EventHandler<A> StateDataChanged;

        void UpdateData(T data);
    }
}
