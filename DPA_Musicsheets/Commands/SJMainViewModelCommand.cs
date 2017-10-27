using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPA_Musicsheets.Managers;
using DPA_Musicsheets.ViewModels;

namespace DPA_Musicsheets.Commands
{
    public abstract class SJMainViewModelCommand : ISJCommand
    {
        protected readonly MainViewModel _viewModel;

        public SJMainViewModelCommand(MainViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public abstract void Execute();
    }
}
