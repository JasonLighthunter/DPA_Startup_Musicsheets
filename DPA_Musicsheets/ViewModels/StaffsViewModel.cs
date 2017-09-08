using DPA_Musicsheets.Managers;
using DPA_Musicsheets.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using PSAMControlLibrary;
using PSAMWPFControlLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace DPA_Musicsheets.ViewModels
{
    public class StaffsViewModel : ViewModelBase
    {
		public ObservableCollection<MusicalSymbol> Staffs { get; set; }

		public StaffsViewModel()
		{
			Staffs = new ObservableCollection<MusicalSymbol>();

			SJWPFStaffStateHandler.StateDataChanged += (src, args) =>
			{
				Staffs.Clear();
				foreach(var symbol in args.Symbols)
				{
					Staffs.Add(symbol);
				}

				MessengerInstance.Send<CurrentStateMessage>(new CurrentStateMessage() { State = args.Message });
			};
		}
	}
}
