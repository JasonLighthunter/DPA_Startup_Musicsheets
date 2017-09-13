using DPA_Musicsheets.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Managers
{
    public class SJLilypondFileHandler : ISJFileHandler
    {
        private SJLilypondStateHandler lilypondStateHandler = new SJLilypondStateHandler();
        // private SJLilypondParser lilypondParser = new SJLilypondParser();

        private string _lilypondText;
        public string LilypondText
        {
            get { return _lilypondText; }
            set
            {
                _lilypondText = value;

                lilypondStateHandler.UpdateData(_lilypondText);
            }
        }

        public void Load(string fileName)
        {
            if (Path.GetExtension(fileName).EndsWith(".ly"))
            {
                StringBuilder sb = new StringBuilder();
                foreach (var line in File.ReadAllLines(fileName))
                {
                    sb.AppendLine(line);
                }

                LilypondText = sb.ToString();

                LoadLilypond(sb.ToString());
            }
            else
            {
                throw new NotSupportedException($"File extension {Path.GetExtension(fileName)} is not supported.");
            }
        }

        public void LoadLilypond(string content)
        {
            //LilypondText = content;
            //content = content.Trim().ToLower().Replace("\r\n", " ").Replace("\n", " ").Replace("  ", " ");
            //LinkedList<LilypondToken> tokens = GetTokensFromLilypond(content);
            //WPFStaffs.Clear();
            //string message;
            //WPFStaffs.AddRange(GetStaffsFromTokens(tokens, out message));
            //WPFStaffsChanged?.Invoke(this, new WPFStaffsEventArgs() { Symbols = WPFStaffs, Message = message });

            //MidiSequence = GetSequenceFromWPFStaffs();
            //MidiSequenceChanged?.Invoke(this, new MidiSequenceEventArgs() { MidiSequence = MidiSequence });
        }

    }
}
