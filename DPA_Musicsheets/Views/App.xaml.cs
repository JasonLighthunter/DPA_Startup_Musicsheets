using DPA_Musicsheets.Managers;
using DPA_Musicsheets.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MidiPlayerTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            SJNoteFactory.AddNoteType("R", typeof(SJRest));
            SJNoteFactory.AddNoteType("N", typeof(SJNote));
            base.OnStartup(e);
        }
    }
}
