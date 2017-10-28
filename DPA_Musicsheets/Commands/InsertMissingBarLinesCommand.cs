using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DPA_Musicsheets.Models;
using DPA_Musicsheets.ViewModels;

namespace DPA_Musicsheets.Commands
{
    class InsertMissingBarLinesCommand : SJLilypondViewModelCommand
    {
        public InsertMissingBarLinesCommand(LilypondViewModel viewModel) : base(viewModel)
        {
        }

        public override void Execute()
        {
            int startPosition = _viewModel.CaretPosition;
            int selectionLength = _viewModel.SelectionLength;
            string selection = _viewModel.LilypondText.Substring(startPosition, selectionLength);
            string[] selectionSets = selection.Trim().ToLower().Replace("\r\n", " ").Replace("\n", " ").Replace("  ", " ").Replace("|", " ").Replace("  ", " ").Split(' ');
            SJTimeSignature timeSignature = _viewModel.FileReader.GetTimeSignatue();

            selectionSets = selectionSets.Where(e => e != "").ToArray();

            int iterator = 0;
            double currentNotesValue = 0;
            string resultString = "";

            while (iterator < selectionSets.Length)
            {
                int noteLength = Int32.Parse(Regex.Match(selectionSets[iterator], @"\d+").Value);
                int numberOfDots = selectionSets[iterator].Count(c => c.Equals('.'));
                double noteValue = noteLength * Math.Pow(1.5, numberOfDots);
                currentNotesValue += timeSignature.NoteValueOfBeat / noteValue;

                resultString += selectionSets[iterator] + " ";

                if (currentNotesValue >= timeSignature.NumberOfBeatsPerBar)
                {
                    resultString += "|\r\n";
                    currentNotesValue = 0;
                }
                iterator++;
            }

            string text = _viewModel.LilypondText;
            text = text.Remove(startPosition, selectionLength);
            text = text.Insert(startPosition, resultString);
            _viewModel.LilypondText = text;
        }
    }
}