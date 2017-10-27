using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPA_Musicsheets.Managers;
using DPA_Musicsheets.ViewModels;

namespace DPA_Musicsheets.Commands
{
    class SaveAsLilypondCommand : SJMainViewModelCommand
    {
        public SaveAsLilypondCommand(MainViewModel viewModel) : base(viewModel)
        {
        }

        public override void Execute()
        {
            _viewModel.FileReader.SaveToLilypond(_viewModel.FileName);
        }
    }
}
