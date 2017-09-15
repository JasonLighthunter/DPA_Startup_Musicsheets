using DPA_Musicsheets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Managers
{
    public interface ISJFileHandler
    {
        SJSong LoadSong(string fileName);
    }
}
