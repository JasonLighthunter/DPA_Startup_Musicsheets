using DPA_Musicsheets.Managers;
using DPA_Musicsheets.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DPA_Musicsheets.ViewModels
{
    public class LilypondViewModel : ViewModelBase
    {
        private SJFileReader _fileReader;

        private string _text;
        private string _previousText;
        private string _nextText;

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

        private bool _textChangedByLoad = false;
        private DateTime _lastChange;
        private static int MILLISECONDS_BEFORE_CHANGE_HANDLED = 1500;
        private bool _waitingForRender = false;

        public LilypondViewModel(SJFileReader fileReader)
        {
            _fileReader = fileReader;

            SJLilypondStateHandler.StateDataChanged += (src, e) =>
            {
                _textChangedByLoad = true;
                LilypondText = _previousText = e.LilypondText;
                _textChangedByLoad = false;
            };

            _text = "Your lilypond text will appear here.";
        }

        public ICommand TextChangedCommand => new RelayCommand<TextChangedEventArgs>((args) =>
        {
            if (!_textChangedByLoad)
            {
                _waitingForRender = true;
                _lastChange = DateTime.Now;
                MessengerInstance.Send(new CurrentStateMessage() { State = "Rendering..." });

                Task.Delay(MILLISECONDS_BEFORE_CHANGE_HANDLED).ContinueWith((task) =>
                {
                    if ((DateTime.Now - _lastChange).TotalMilliseconds >= MILLISECONDS_BEFORE_CHANGE_HANDLED)
                    {
                        _waitingForRender = false;
                        UndoCommand.RaiseCanExecuteChanged();

                        _fileReader.RefreshLilypond(LilypondText);
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext()); // Request from main thread.
            }
        });

        public RelayCommand UndoCommand => new RelayCommand(() =>
        {
            _nextText = LilypondText;
            LilypondText = _previousText;
            _previousText = null;
        }, () => _previousText != LilypondText);

        public RelayCommand RedoCommand => new RelayCommand(() =>
        {
            _previousText = LilypondText;
            LilypondText = _nextText;
            _nextText = null;
            RedoCommand.RaiseCanExecuteChanged();
        }, () => _nextText != LilypondText);

        public ICommand SaveAsCommand => new RelayCommand(() =>
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog() { Filter = "Midi|*.mid|Lilypond|*.ly|PDF|*.pdf" };
            if (saveFileDialog.ShowDialog() == true)
            {
                _fileReader.SaveToFile(saveFileDialog.FileName);
                //string extension = Path.GetExtension(saveFileDialog.FileName);
                //if (extension.EndsWith(".mid"))
                //{
                //    _fileHandler.SaveToMidi(saveFileDialog.FileName);
                //}
                //else if (extension.EndsWith(".ly"))
                //{
                //    _fileHandler.SaveToLilypond(saveFileDialog.FileName);
                //}
                //else if (extension.EndsWith(".pdf"))
                //{
                //    _fileHandler.SaveToPDF(saveFileDialog.FileName);
                //}
                //else
                //{
                //    MessageBox.Show($"Extension {extension} is not supported.");
                //}
            }
        });
    }
}
