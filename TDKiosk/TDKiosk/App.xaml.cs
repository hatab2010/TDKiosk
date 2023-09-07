using System;
using TDKiosk.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TDKiosk
{

    public static class Envirement
    {
        public static TDClient TDClient = new TDClient();
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
            _ = Envirement.TDClient.Disconnect();
        }

        protected override void OnResume()
        {
            _ = Envirement.TDClient.Connect();
        }        
    }
}
