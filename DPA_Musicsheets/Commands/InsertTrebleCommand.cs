using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPA_Musicsheets.ViewModels;

namespace DPA_Musicsheets.Commands
{
    class InsertTrebleCommand : SJLilypondViewModelCommand
    {
        public InsertTrebleCommand(LilypondViewModel viewModel) : base(viewModel)
        {
        }

        public override void Execute()
        {
            _viewModel.LilypondText = _viewModel.LilypondText.Insert(_viewModel.CaretPosition, "\\clef treble");
        }
    }
}
