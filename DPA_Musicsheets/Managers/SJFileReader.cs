using DPA_Musicsheets.Models;
using DPA_Musicsheets.Parsers;
using PSAMControlLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Managers
{
    public class SJFileReader
    {
        private SJFileHandlerFactory _fileHandlerFactory { get; set; }

        private SJWPFStaffsParser staffsParser;
        private SJWPFStaffStateHandler staffsStateHandler;

        public SJFileReader(SJFileHandlerFactory fileHandlerFactory)
        {
            _fileHandlerFactory = fileHandlerFactory;
			_fileHandlerFactory.AddFileHandlerType(".mid", typeof(SJMidiFileHandler));
        }

        public void ReadFile(string fileName)
        {
            string fileExtension = Path.GetExtension(fileName);
            try
            {
                ISJFileHandler fileHandler = _fileHandlerFactory.CreateFileHandler(fileExtension);
                SJSong song = fileHandler.LoadSong(fileName);
                IEnumerable<MusicalSymbol> symbols = staffsParser.ParseFromSJSong(song);
                staffsStateHandler.UpdateData(symbols);
            }
            catch (ArgumentException e)
            {
                throw new NotSupportedException($"File extension {Path.GetExtension(fileName)} is not supported.");
            }
        }
    }
}
