using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Managers
{
	public class SJLilypondStateHandler : ISJStateHandler<string, LilypondEventArgs>
	{
		public string StateData { get; private set; }

		public static event EventHandler<LilypondEventArgs> StateDataChanged;

		public void UpdateData(string data, string message = null)
		{
			StateData = data;
			StateDataChanged?.Invoke(this, new LilypondEventArgs() { LilypondText = StateData, Message = message });
		}
	}
}
