using DPA_Musicsheets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Parsers
{
    interface ISJParser<T> where T : class
    {
        SJSong ParseToSJSong(T data);
        T ParseFromSJSong(SJSong song);
    }
}
