using DPA_Musicsheets.Models;
using DPA_Musicsheets.Parsers;
using PSAMControlLibrary;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DPA_Musicsheets.Managers
{
    public class SJFileReader
    {
        private bool _isSavedSinceChange;

        public bool IsSavedSinceChange
        {
            get { return _isSavedSinceChange; }
            set { _isSavedSinceChange = value; }
        }


        //private SJFileHandlerFactory _fileHandlerFactory { get; set; 
        private SJSong _currentSong { get; set; }
        private ISJFileHandler _midiFileHandler { get; set; }
        private ISJFileHandler _lilypondFileHandler { get; set; }
        private ISJFileHandler _musicXMLFileHandler { get; set; }

        private SJLilypondParser _lilypondParser;
        private SJMidiParser _midiParser;
        private SJWPFStaffsParser _staffsParser;

        private SJLilypondStateHandler lilypondStateHandler;
        private SJMidiStateHandler midiStateHandler;
        private SJWPFStaffStateHandler staffsStateHandler;

        public SJFileReader(SJMidiFileHandler midiFileHandler, SJLilypondFileHandler lilypondFileHandler, SJMusicXMLFileHandler musicXMLFileHandler, SJLilypondParser lilypondParser, SJMidiParser midiParser, SJWPFStaffsParser staffsParser)
        {
            //_fileHandlerFactory = fileHandlerFactory;
            //_fileHandlerFactory.AddFileHandlerType(".mid", typeof(SJMidiFileHandler));
            //_fileHandlerFactory.AddFileHandlerType(".ly", typeof(SJLilypondFileHandler));

            _midiFileHandler = midiFileHandler;
            _lilypondFileHandler = lilypondFileHandler;
            _musicXMLFileHandler = musicXMLFileHandler;

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
                        song = _midiFileHandler.LoadSongFromFile(fileName);
                        break;
                    case ".ly":
                        song = _lilypondFileHandler.LoadSongFromFile(fileName);
                        break;
                    case ".xml":
                        song = _musicXMLFileHandler.LoadSongFromFile(fileName);
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

                _currentSong = song;
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.StackTrace);
                throw new NotSupportedException($"File extension {Path.GetExtension(fileName)} is not supported.");
            }
        }

        public void RefreshLilypond(string lilypondText)
        {
            try
            {
                var song = ((SJLilypondFileHandler)_lilypondFileHandler).LoadSongFromString(lilypondText);

                Sequence midiSequence = _midiParser.ParseFromSJSong(song);
                IEnumerable<MusicalSymbol> symbols = _staffsParser.ParseFromSJSong(song);

                midiStateHandler.UpdateData(midiSequence);
                staffsStateHandler.UpdateData(symbols);
            }
            catch (ArgumentException e)
            {
                //Console.WriteLine(e.StackTrace);
                //throw e;
            }
        }

        #region Saving to files
        public void SaveToFile(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            if (extension.EndsWith(".mid"))
            {
                SaveToMidi(fileName);
                _isSavedSinceChange = true;
            }
            else if (extension.EndsWith(".ly"))
            {
                SaveToLilypond(fileName);
                _isSavedSinceChange = true;
            }
            else if (extension.EndsWith(".pdf"))
            {
                SaveToPDF(fileName);
                _isSavedSinceChange = true;
            }
        }

        internal void SaveToMidi(string fileName)
        {
            Sequence sequence = _midiParser.ParseFromSJSong(_currentSong);

            sequence.Save(fileName);
        }

        internal void SaveToPDF(string fileName)
        {
            string tmpFileName = $"{fileName}-tmp.ly";
            SaveToLilypond(tmpFileName);

            string lilypondLocation = @"C:\Program Files (x86)\LilyPond\usr\bin\lilypond.exe";
            string sourceFolder = Path.GetDirectoryName(tmpFileName);
            string sourceFileName = Path.GetFileNameWithoutExtension(tmpFileName);
            string targetFolder = Path.GetDirectoryName(fileName);
            string targetFileName = Path.GetFileNameWithoutExtension(fileName);

            var process = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = sourceFolder,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = String.Format("--pdf \"{0}\\{1}.ly\"", sourceFolder, sourceFileName),
                    FileName = lilypondLocation
                }
            };

            process.Start();
            while (!process.HasExited)
            { /* Wait for exit */
            }
            if (sourceFolder != targetFolder || sourceFileName != targetFileName)
            {
                File.Move(sourceFolder + "\\" + sourceFileName + ".pdf", targetFolder + "\\" + targetFileName + ".pdf");
                File.Delete(tmpFileName);
            }
        }

        internal void SaveToLilypond(string fileName)
        {
            var lilypondContent = _lilypondParser.ParseFromSJSong(_currentSong);
            using (StreamWriter outputFile = new StreamWriter(fileName))
            {
                outputFile.Write(lilypondContent);
                outputFile.Close();
            }
        }

        #endregion Saving to files

    }
}
