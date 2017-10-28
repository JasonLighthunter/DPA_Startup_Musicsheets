using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPA_Musicsheets.Managers;
using DPA_Musicsheets.ViewModels;

namespace DPA_Musicsheets.Commands
{
    class SaveAsPDFCommand : SJMainViewModelCommand
    {
        public SaveAsPDFCommand(MainViewModel viewModel) : base(viewModel)
        {
        }

        public override void Execute()
        {
            _viewModel.FileReader.SaveToPDF(_viewModel.FileName);
        }
    }
}
