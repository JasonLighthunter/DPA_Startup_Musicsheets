using DPA_Musicsheets.Managers;
using DPA_Musicsheets.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DPA_Musicsheets.ChainOfResponsibility;
using DPA_Musicsheets.Commands;

namespace DPA_Musicsheets.ViewModels
{
    public class LilypondViewModel : ViewModelBase
    {
        private SJFileReader _fileReader;
        public SJFileReader FileReader
        {
            get { return _fileReader; }
            private set { _fileReader = value; }
        }

        private ISJHandler _baseHandler;

        private string _text;
        private string _previousText;
        private string _nextText;

        private HashSet<string> pressedKeys = new HashSet<string>();

        public string LilypondText
        {
            get { return _text; }
            set
            {
                if (!_waitingForRender && !_textChangedByLoad)
                {
                    _previousText = _text;
                }
                _text = value;
                RaisePropertyChanged(() => LilypondText);
            }
        }

        private int _caretPosition;

        public int CaretPosition
        {
            get { return _caretPosition; }
            set { _caretPosition = value; }
        }

        private int _selectionLength;

        public int SelectionLength
        {
            get { return _selectionLength; }
            set { _selectionLength = value; }
        }

        private bool _textChangedByLoad = false;
        private DateTime _lastChange;
        private static int MILLISECONDS_BEFORE_CHANGE_HANDLED = 1500;
        private bool _waitingForRender = false;

        public LilypondViewModel(SJFileReader fileReader)
        {
            _fileReader = fileReader;

            SJLilypondStateHandler.StateDataChanged += (src, e) =>
            {
                _fileReader.IsSavedSinceChange = true;
                _textChangedByLoad = true;
                LilypondText = _previousText = e.LilypondText;
                _textChangedByLoad = false;
            };

            ISJHandler handler1;
            ISJHandler handler2;



            handler1 = new ConcreteHandler(new InsertTempoCommand(this, 4, 120), new HashSet<string> { "LeftAlt", "S" });
            handler2 = new ConcreteHandler(new InsertTimeSignatureCommand(this, 4, 4), new HashSet<string> { "LeftAlt", "T" });
            handler2.SetNext(handler1);
            handler1 = new ConcreteHandler(new InsertTimeSignatureCommand(this, 4, 4), new HashSet<string> { "LeftAlt", "T", "4" });
            handler1.SetNext(handler2);
            handler2 = new ConcreteHandler(new InsertTimeSignatureCommand(this, 3, 4), new HashSet<string> { "LeftAlt", "T", "3" });
            handler2.SetNext(handler1);
            handler1 = new ConcreteHandler(new InsertTimeSignatureCommand(this, 6, 8), new HashSet<string> { "LeftAlt", "T", "6" });
            handler1.SetNext(handler2);
            handler2 = new ConcreteHandler(new InsertMissingBarLinesCommand(this), new HashSet<string> { "LeftAlt", "B" });
            handler2.SetNext(handler1);

            _baseHandler = new ConcreteHandler(new InsertTrebleCommand(this), new HashSet<string>() { "LeftAlt", "C" });
            _baseHandler.SetNext(handler2);

            _text = "Your lilypond text will appear here.";
        }

        public ICommand TextChangedCommand => new RelayCommand<TextChangedEventArgs>((args) =>
        {
            if (!_textChangedByLoad)
            {
                _fileReader.IsSavedSinceChange = false;
                _waitingForRender = true;
                _lastChange = DateTime.Now;
                MessengerInstance.Send(new CurrentStateMessage() { State = "Rendering..." });

                Task.Delay(MILLISECONDS_BEFORE_CHANGE_HANDLED).ContinueWith((task) =>
                {
                    if ((DateTime.Now - _lastChange).TotalMilliseconds >= MILLISECONDS_BEFORE_CHANGE_HANDLED)
                    {
                        _waitingForRender = false;
                        UndoCommand.RaiseCanExecuteChanged();

                        try
                        {
                            _fileReader.RefreshLilypond(LilypondText);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext()); // Request from main thread.
            }
        });

        public RelayCommand UndoCommand => new RelayCommand(() =>
        {
            _fileReader.UndoSong();
        }, () => _fileReader.CanUndo());

        public RelayCommand RedoCommand => new RelayCommand(() =>
        {
            _fileReader.RedoSong();
        }, () => _fileReader.CanRedo());

        public ICommand SaveAsCommand => new RelayCommand(() =>
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog() { Filter = "Midi|*.mid|Lilypond|*.ly|PDF|*.pdf" };
            if (saveFileDialog.ShowDialog() == true)
            {
                _fileReader.SaveToFile(saveFileDialog.FileName);
                if (!_fileReader.IsSavedSinceChange)
                {
                    MessageBox.Show($"Extension {Path.GetExtension(saveFileDialog.FileName)} is not supported.");
                }
            }
        });

        public ICommand OnKeyDownCommand => new RelayCommand<KeyEventArgs>((e) =>
        {
            var a = e.Key == Key.System ? e.SystemKey : e.Key;
            Console.WriteLine("Lilypond: Key down: {0}", a);
            pressedKeys.Add((e.Key == Key.System ? e.SystemKey : e.Key).ToString());
            CheckPressedKeysForCombination();
        });

        public ICommand OnKeyUpCommand => new RelayCommand<KeyEventArgs>((e) =>
        {
            Console.WriteLine("Lilypond: Key Up");
            pressedKeys.Remove(e.Key.ToString());
        });

        private void CheckPressedKeysForCombination()
        {
            if (_baseHandler.Handle(pressedKeys))
            {
                pressedKeys.Clear();
            }
        }
    }
}
