using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPA_Musicsheets.ViewModels;

namespace DPA_Musicsheets.Commands
{
    class InsertTimeSignatureCommand : SJLilypondViewModelCommand
    {
        private int _numberOfBeatsPerBar;
        private int _noteValueOfBeat;
        public InsertTimeSignatureCommand(LilypondViewModel viewModel, int numberOfBeatsPerBar, int noteValueOfBeat) : base(viewModel)
        {
            _numberOfBeatsPerBar = numberOfBeatsPerBar;
            _noteValueOfBeat = noteValueOfBeat;
        }

        public override void Execute()
        {
            _viewModel.LilypondText = _viewModel.LilypondText.Insert(_viewModel.CaretPosition, String.Format("\\time {0}/{1}", _numberOfBeatsPerBar, _noteValueOfBeat));
        }
    }
}
