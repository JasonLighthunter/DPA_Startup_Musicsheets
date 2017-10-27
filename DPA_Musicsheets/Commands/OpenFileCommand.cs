using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPA_Musicsheets.ViewModels;
using Microsoft.Win32;

namespace DPA_Musicsheets.Commands
{
    public class OpenFileCommand : SJMainViewModelCommand
    {
        public OpenFileCommand(MainViewModel viewModel) : base(viewModel)
        {
        }

        public override void Execute()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "Midi, LilyPond or MusicXML files (*.mid *.ly *.xml)|*.mid;*.ly;*.xml" };
            if (openFileDialog.ShowDialog() == true)
            {
                _viewModel.FileName = openFileDialog.FileName;
            }
        }
    }
}
