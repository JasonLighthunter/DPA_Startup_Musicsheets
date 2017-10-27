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
        private Dictionary<SJFormatTypeEnum, Action> _updateActionDictionary;
        private bool _isSavedSinceChange = true;
        public bool IsSavedSinceChange
        {
            get { return _isSavedSinceChange; }
            set { _isSavedSinceChange = value; }
        }

        private SJSong _currentSong;
        //private SJFileHandlerFactory _fileHandlerFactory { get; set; 
        private SJSong currentSong
        {
            get { return _currentSong; }
            set
            {
                _previousSongs.Push(_currentSong);
                _nextSongs.Clear();
                _currentSong = value;
            }

        }

        private Stack<SJSong> _previousSongs { get; set; }
        private Stack<SJSong> _nextSongs { get; set; }


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

            _previousSongs = new Stack<SJSong>();
            _nextSongs = new Stack<SJSong>();

            _updateActionDictionary = new Dictionary<SJFormatTypeEnum, Action>()
            {
                {SJFormatTypeEnum.Midi, ParseAndUpdateMidi},
                {SJFormatTypeEnum.LilyPond, ParseAndUpdateLilypond},
                {SJFormatTypeEnum.WPFStaffs, ParseAndUpdateWPFStaffs},
                {SJFormatTypeEnum.None, null }
            };
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

                _nextSongs.Clear();
                _previousSongs.Clear();
                _currentSong = song;

                UpdateDataExeptFormat(SJFormatTypeEnum.None);
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

                currentSong = song;

                UpdateDataExeptFormat(SJFormatTypeEnum.LilyPond);
            }
            catch (ArgumentException e)
            {
                //Console.WriteLine(e.StackTrace);
                //throw e;
            }
        }

        private void UpdateDataExeptFormat(SJFormatTypeEnum formatTypeEnum)
        {
            foreach (var c in _updateActionDictionary.Where(e => e.Key != formatTypeEnum).Select(e => e.Value))
            {
                c?.Invoke();
            }
        }

        private void ParseAndUpdateMidi()
        {
            Sequence midiSequence = _midiParser.ParseFromSJSong(_currentSong);
            midiStateHandler.UpdateData(midiSequence);
        }

        private void ParseAndUpdateLilypond()
        {
            string lilypondContent = _lilypondParser.ParseFromSJSong(_currentSong);
            lilypondStateHandler.UpdateData(lilypondContent);
        }

        private void ParseAndUpdateWPFStaffs()
        {
            IEnumerable<MusicalSymbol> symbols = _staffsParser.ParseFromSJSong(_currentSong);
            staffsStateHandler.UpdateData(symbols);
        }


        public void UndoSong()
        {
            _nextSongs.Push(_currentSong);
            _currentSong = _previousSongs.Pop();
            UpdateDataExeptFormat(SJFormatTypeEnum.None);
        }

        public void RedoSong()
        {
            _previousSongs.Push(_currentSong);
            _currentSong = _nextSongs.Pop();
            UpdateDataExeptFormat(SJFormatTypeEnum.None);
        }

        public bool CanUndo()
        {
            return _previousSongs.Any();
        }

        public bool CanRedo()
        {
            return _nextSongs.Any();
        }

        #region Saving to files
        public void SaveToFile(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            if (extension.EndsWith(".mid"))
            {
                SaveToMidi(fileName);
            }
            else if (extension.EndsWith(".ly"))
            {
                SaveToLilypond(fileName);
            }
            else if (extension.EndsWith(".pdf"))
            {
                SaveToPDF(fileName);
            }
        }

        internal void SaveToMidi(string fileName)
        {
            Sequence sequence = _midiParser.ParseFromSJSong(_currentSong);

            sequence.Save(fileName);
            _isSavedSinceChange = true;
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
            _isSavedSinceChange = true;
        }

        internal void SaveToLilypond(string fileName)
        {
            var lilypondContent = _lilypondParser.ParseFromSJSong(_currentSong);
            using (StreamWriter outputFile = new StreamWriter(fileName))
            {
                outputFile.Write(lilypondContent);
                outputFile.Close();
            }
            _isSavedSinceChange = true;
        }

        #endregion Saving to files

    }
}
