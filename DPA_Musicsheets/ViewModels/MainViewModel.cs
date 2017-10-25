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

        public MainViewModel(SJFileReader fileReader)
        {
            _fileReader = fileReader;
            FileName = @"files/alle-eendjes-zwemmen-in-het-water.mid";

            MessengerInstance.Register<CurrentStateMessage>(this, (message) => CurrentState = message.State);
		}

		private void OpenFileDialogAction()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "Midi, LilyPond or MusicXML files (*.mid *.ly *.xml)|*.mid;*.ly;*.xml" };
			if(openFileDialog.ShowDialog() == true)
			{
				FileName = openFileDialog.FileName;
			}
		}

        public ICommand OpenFileCommand => new RelayCommand(() => { OpenFileDialogAction(); });

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
            Console.WriteLine($"Key down: {e.Key}");
			pressedKeys.Add(e.Key.ToString());
			CheckPressedKeysForCombination();
		});

		private void CheckPressedKeysForCombination()
		{
			HashSet<string> saveLilypondSet = new HashSet<string>{ "S", "LeftCtrl" };
			HashSet<string> openFileSet = new HashSet<string> { "LeftCtrl", "O" };

			if(pressedKeys.SetEquals(saveLilypondSet))
			{
				SaveAsLilypondAction();
			}
			else if(pressedKeys.SetEquals(openFileSet))
			{
				OpenFileDialogAction();
			}
		}

		private void SaveAsLilypondAction() => _fileReader.SaveToLilypond(_fileName);

		public ICommand OnKeyUpCommand => new RelayCommand<KeyEventArgs>((e) =>
        {
            Console.WriteLine("Key Up");
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
