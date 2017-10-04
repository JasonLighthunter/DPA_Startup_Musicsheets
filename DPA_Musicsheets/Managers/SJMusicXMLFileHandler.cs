using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPA_Musicsheets.Models;
using DPA_Musicsheets.Parsers;
using System.IO;

namespace DPA_Musicsheets.Managers
{
	public class SJMusicXMLFileHandler : ISJFileHandler
	{
		public string MusicXMLText;
		public SJSong Song;

		private SJMusicXMLParser _musicXMLParser;

		public SJMusicXMLFileHandler(SJMusicXMLParser musicXMLParser)
		{
			_musicXMLParser = musicXMLParser;
		}

		public SJSong LoadSong(string fileName)
		{
			StringBuilder sb = new StringBuilder();
			foreach(var line in File.ReadAllLines(fileName))
			{
				sb.AppendLine(line);
			}

			MusicXMLText = sb.ToString();

			Song = _musicXMLParser.ParseToSJSong(MusicXMLText);
			return Song;
		}
	}
}
