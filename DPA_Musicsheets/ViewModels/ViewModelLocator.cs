using DPA_Musicsheets.Builders;
using DPA_Musicsheets.Managers;
using DPA_Musicsheets.Models;
using DPA_Musicsheets.Parsers;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using System;

namespace DPA_Musicsheets.ViewModels
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<SJNoteFactory>();
            SimpleIoc.Default.Register<SJNoteBuilder>();

            SimpleIoc.Default.Register<SJLilypondFileHandler>();
            SimpleIoc.Default.Register<SJMidiFileHandler>();
            SimpleIoc.Default.Register<SJMusicXMLFileHandler>();

            //noteFactory.AddNoteType("R", typeof(SJRest));
            //noteFactory.AddNoteType("N", typeof(SJNote));
            //noteFactory.AddNoteType("U", typeof(SJUnheardNote));

            SimpleIoc.Default.Register<SJLilypondParser>();
            SimpleIoc.Default.Register<SJMidiParser>();
            SimpleIoc.Default.Register<SJWPFStaffsParser>();
            SimpleIoc.Default.Register<SJMusicXMLParser>();

            SimpleIoc.Default.Register<SJFileHandlerFactory>();

            SimpleIoc.Default.Register<SJFileReader>();

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<LilypondViewModel>();
            SimpleIoc.Default.Register<StaffsViewModel>();
            SimpleIoc.Default.Register<MidiPlayerViewModel>();
        }

        public MainViewModel MainViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public LilypondViewModel LilypondViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<LilypondViewModel>();
            }
        }

        public StaffsViewModel StaffsViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<StaffsViewModel>();
            }
        }

        public MidiPlayerViewModel MidiPlayerViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MidiPlayerViewModel>();
            }
        }

        public static void Cleanup()
        {
            ServiceLocator.Current.GetInstance<MidiPlayerViewModel>().Cleanup();
        }
    }
}