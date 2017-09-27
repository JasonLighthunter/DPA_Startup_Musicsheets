using DPA_Musicsheets.Models;
using DPA_Musicsheets.Parsers;
using PSAMControlLibrary;
using Sanford.Multimedia.Midi;
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
        //private SJFileHandlerFactory _fileHandlerFactory { get; set; }
        private ISJFileHandler _midiFileHandler { get; set; }
        private ISJFileHandler _lilypondFileHandler { get; set; }

        private SJLilypondParser _lilypondParser;
        private SJMidiParser _midiParser;
        private SJWPFStaffsParser _staffsParser;

        private SJLilypondStateHandler lilypondStateHandler;
        private SJMidiStateHandler midiStateHandler;
        private SJWPFStaffStateHandler staffsStateHandler;

        public SJFileReader(SJMidiFileHandler midiFileHandler, SJLilypondFileHandler lilypondFileHandler, SJLilypondParser lilypondParser, SJMidiParser midiParser, SJWPFStaffsParser staffsParser)
        {
            //         _fileHandlerFactory = fileHandlerFactory;
            //_fileHandlerFactory.AddFileHandlerType(".mid", typeof(SJMidiFileHandler));
            //         _fileHandlerFactory.AddFileHandlerType(".ly", typeof(SJLilypondFileHandler));

            _midiFileHandler = midiFileHandler;
            _lilypondFileHandler = lilypondFileHandler;

            _lilypondParser = lilypondParser;
            _midiParser = midiParser;
            _staffsParser = staffsParser;

            lilypondStateHandler = new SJLilypondStateHandler();
            midiStateHandler = new SJMidiStateHandler();
            staffsStateHandler = new SJWPFStaffStateHandler();
        }

        public void ReadFile(string fileName)
        {
            string fileExtension = Path.GetExtension(fileName);
            try
            {
                //ISJFileHandler fileHandler = _fileHandlerFactory.CreateFileHandler(fileExtension);
                SJSong song = new SJSong();
                switch (fileExtension)
                {
                    case ".mid":
                        song = _midiFileHandler.LoadSong(fileName);
                        break;
                    case ".ly":
                        song = _lilypondFileHandler.LoadSong(fileName);
                        break;
                    default:
                        break;
                }
                 

                Sequence midiSequence = _midiParser.ParseFromSJSong(song);
                string lilypondContent = _lilypondParser.ParseFromSJSong(song);
                IEnumerable<MusicalSymbol> symbols = _staffsParser.ParseFromSJSong(song);

                lilypondStateHandler.UpdateData(lilypondContent);
                midiStateHandler.UpdateData(midiSequence);
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
