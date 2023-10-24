using System;
using TDKiosk.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: ExportFont("BlenderPro-Medium.ttf", Alias = "Blender")]
namespace TDKiosk
{

    public static class Envirement
    {
        public static ITDClient TDClient;

        static Envirement()
        {
#if DEBUG
            TDClient = new TDClient();
#else
            TDClient = new TDClient();
#endif
        }

    }

    public partial class App : Application
    {
        private TDClient _tdClient;
        private MainPage _page;

        public App()
        {
            InitializeComponent();
            _page = new MainPage();
            MainPage = _page;            
        }

        protected override void OnStart()
        {
            _ = Envirement.TDClient.Connect();
        }

        protected override void OnSleep()
        {

        }

        protected override void OnResume()
        {

        }        
    }
}
