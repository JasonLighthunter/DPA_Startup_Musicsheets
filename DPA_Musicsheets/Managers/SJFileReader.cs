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

        private SJLilypondParser lilypondParser;
        private SJWPFStaffsParser staffsParser;

        private SJLilypondStateHandler lilypondStateHandler;
        private SJWPFStaffStateHandler staffsStateHandler;
        

        public SJFileReader(SJFileHandlerFactory fileHandlerFactory)
        {
            _fileHandlerFactory = fileHandlerFactory;
			_fileHandlerFactory.AddFileHandlerType(".mid", typeof(SJMidiFileHandler));
            _fileHandlerFactory.AddFileHandlerType(".ly", typeof(SJLilypondFileHandler));

            lilypondParser = new SJLilypondParser();
            staffsParser = new SJWPFStaffsParser();

            lilypondStateHandler = new SJLilypondStateHandler();
            staffsStateHandler = new SJWPFStaffStateHandler();
        }

        public void ReadFile(string fileName)
        {
            string fileExtension = Path.GetExtension(fileName);
            try
            {
                ISJFileHandler fileHandler = _fileHandlerFactory.CreateFileHandler(fileExtension);
                SJSong song = fileHandler.LoadSong(fileName);

                string lilypondContent = lilypondParser.ParseFromSJSong(song);
                IEnumerable<MusicalSymbol> symbols = staffsParser.ParseFromSJSong(song);

                lilypondStateHandler.UpdateData(lilypondContent);
                staffsStateHandler.UpdateData(symbols);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.StackTrace);
                throw new NotSupportedException($"File extension {Path.GetExtension(fileName)} is not supported.");
            }
        }
    }
}
