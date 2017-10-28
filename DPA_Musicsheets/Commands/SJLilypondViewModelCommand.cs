using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPA_Musicsheets.ViewModels;

namespace DPA_Musicsheets.Commands
{
    public abstract class SJLilypondViewModelCommand : ISJCommand
    {
        protected readonly LilypondViewModel _viewModel;

        public SJLilypondViewModelCommand(LilypondViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public abstract void Execute();
    }
}
