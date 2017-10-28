using PSAMControlLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Managers
{
	public class SJWPFStaffStateHandler : ISJStateHandler<IEnumerable<MusicalSymbol>, WPFStaffsEventArgs>
	{
		public IEnumerable<MusicalSymbol> StateData { get; private set; }

		public static event EventHandler<WPFStaffsEventArgs> StateDataChanged;

		public void UpdateData(IEnumerable<MusicalSymbol> data, string message = null)
		{
			StateData = data;
			StateDataChanged?.Invoke(this, new WPFStaffsEventArgs() { Symbols = StateData, Message = message });
		}
	}
}
