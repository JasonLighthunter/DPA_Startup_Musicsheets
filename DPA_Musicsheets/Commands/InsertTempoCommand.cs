using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPA_Musicsheets.ViewModels;

namespace DPA_Musicsheets.Commands
{
    class InsertTempoCommand : SJLilypondViewModelCommand
    {
        private int _tempo;
        private int _noteValueOfBeat;

        public InsertTempoCommand(LilypondViewModel viewModel, int noteValueOfBeat, int tempo) : base(viewModel)
        {
            _tempo = tempo;
            _noteValueOfBeat = noteValueOfBeat;
        }

        public override void Execute()
        {
            _viewModel.LilypondText = _viewModel.LilypondText.Insert(_viewModel.CaretPosition, String.Format("\\tempo {0}={1}", _noteValueOfBeat, _tempo));
        }
    }
}
