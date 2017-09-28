﻿using DPA_Musicsheets.Managers;
using DPA_Musicsheets.Messages;
using DPA_Musicsheets.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using PSAMWPFControlLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        //private FileHandler _fileHandler;
        private SJFileReader _fileReader;

		//public MainViewModel(FileHandler fileHandler)
		//{ 
		//	_fileHandler = fileHandler;
		//	FileName = @"files/alle-eendjes-zwemmen-in-het-water.mid";

		//	MessengerInstance.Register<CurrentStateMessage>(this, (message) => CurrentState = message.State);
		//}

        public MainViewModel(SJFileReader fileReader)
        {
            _fileReader = fileReader;
            FileName = @"files/alle-eendjes-zwemmen-in-het-water.mid";

            MessengerInstance.Register<CurrentStateMessage>(this, (message) => CurrentState = message.State);
        }

        public ICommand OpenFileCommand => new RelayCommand(() =>
        {
            OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "Midi, LilyPond or MusicXML files (*.mid *.ly *.xml)|*.mid;*.ly;*.xml" };
            if (openFileDialog.ShowDialog() == true)
            {
                FileName = openFileDialog.FileName;
            }
        });
        //public ICommand LoadCommand => new RelayCommand(() =>
        //{
        //	_fileHandler.OpenFile(FileName);
        //});

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
        });

        public ICommand OnKeyUpCommand => new RelayCommand(() =>
        {
            Console.WriteLine("Key Up");
        });

        public ICommand OnWindowClosingCommand => new RelayCommand(() =>
        {
            ViewModelLocator.Cleanup();
        });
    }
}
