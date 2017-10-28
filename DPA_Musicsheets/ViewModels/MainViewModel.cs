using DPA_Musicsheets.Managers;
using DPA_Musicsheets.Messages;
using DPA_Musicsheets.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using PSAMWPFControlLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DPA_Musicsheets.ChainOfResponsibility;
using DPA_Musicsheets.Commands;

namespace DPA_Musicsheets.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private string _fileName;
        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                RaisePropertyChanged(() => FileName);
            }
        }

        private string _currentState;
        public string CurrentState
        {
            get { return _currentState; }
            set
            {
                _currentState = value;
                RaisePropertyChanged(() => CurrentState);
            }
        }

        private HashSet<string> pressedKeys = new HashSet<string>();

        private SJFileReader _fileReader;

        public SJFileReader FileReader
        {
            get { return _fileReader; }
            private set { _fileReader = value; }
        }

        private ISJHandler _baseHandler;

        public MainViewModel(SJFileReader fileReader)
        {
            _fileReader = fileReader;
            FileName = @"files/alle-eendjes-zwemmen-in-het-water.mid";


            _baseHandler = new ConcreteHandler(new SaveAsPDFCommand(this), new HashSet<string>() { "LeftCtrl", "S", "P" });
            ISJHandler handler = new ConcreteHandler(new SaveAsLilypondCommand(this), new HashSet<string> { "S", "LeftCtrl" });
            handler.SetNext(new ConcreteHandler(new OpenFileCommand(this), new HashSet<string>() { "LeftCtrl", "O" }));
            _baseHandler.SetNext(handler);

            MessengerInstance.Register<CurrentStateMessage>(this, (message) => CurrentState = message.State);
        }

        public ICommand OpenFileCommand => new RelayCommand(() =>
        {
            ISJCommand command = new OpenFileCommand(this);
            command.Execute();
        });

        public ICommand LoadCommand => new RelayCommand(() =>
        {
            _fileReader.ReadFile(FileName);
        });

        public ICommand OnLostFocusCommand => new RelayCommand(() =>
        {
            Console.WriteLine("Maingrid Lost focus");
        });

        public ICommand OnKeyDownCommand => new RelayCommand<KeyEventArgs>((e) =>
        {
            //Console.WriteLine($"Key down: {e.Key}");
            pressedKeys.Add(e.Key.ToString());
            CheckPressedKeysForCombination();
        });

        private void CheckPressedKeysForCombination()
        {
            if (_baseHandler.Handle(pressedKeys))
            {
                pressedKeys.Clear();
            }
        }

        public ICommand OnKeyUpCommand => new RelayCommand<KeyEventArgs>((e) =>
        {
            //Console.WriteLine($"Key Up: {e.Key}");
            pressedKeys.Remove(e.Key.ToString());
        });

        public ICommand OnWindowClosingCommand => new RelayCommand(() =>
        {
            if (!_fileReader.IsSavedSinceChange)
            {
                MessageBoxButton button = MessageBoxButton.YesNo;
                var result = MessageBox.Show("Veranderingen zijn nog niet opgeslagen. Wilt u dit alsnog doen?", "Veranderingen zijn nog niet opgeslagen.", button);
                if (result == MessageBoxResult.Yes)
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
                }
            }
            ViewModelLocator.Cleanup();
        });
    }
}
