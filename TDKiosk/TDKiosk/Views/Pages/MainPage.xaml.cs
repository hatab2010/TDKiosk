using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using TDKiosk.Models;
using Xamarin.Forms;

namespace TDKiosk
{

    public enum PageType
    {
        [Index(0)]
        [Description("Поиск сервера")]
        FaindServer,

        [Index(2)]
        [Description("Главное меню")]
        Main,

        [Index(1)]
        [Description("Интро")]
        Intro,

        [Index(3)]
        [Description("Частицы")]
        Partical,

        [Index(4)]
        [Description("Фотоны")]
        Photons,

        [Index(5)]
        [Description("Портал")]
        RightPortal,

        [Index(5)]
        [Description("Портал")]
        LeftPortal
    }

    public interface IPage
    {
        //PageType Type { get; }
        Menu Page { get; }
    }

    public partial class MainPage : ContentPage
    {
        public static Dictionary<PageType, Menu> Menus = new Dictionary<PageType, Menu>
        {
            {
                PageType.Intro,
                null
            },
            {
                PageType.Main, 
                new Menu(
                        new Button(PageType.Partical, "Частицы"),
                        new Button(PageType.Photons, "Фотоны")
                )         
            },
            {
                PageType.Partical, 
                new Menu(
                        new Button(PageType.RightPortal, "Частицы", isActive: true),
                        new Button(PageType.RightPortal, "Открыть \nпортал")
                )
            },
            {
                PageType.Photons,
                new Menu(
                        new Button(PageType.LeftPortal, "Открыть \nпортал"),
                        new Button(PageType.LeftPortal, "Фотоны", isActive: true)
                )
            },
            {
                PageType.RightPortal,
                new Menu(
                        new Button(PageType.Main, "Главное \nменю"),
                        new Button(PageType.Main, "Продлить \nпортал", isActive: true)
                )
            },
            {
                PageType.LeftPortal,
                new Menu(
                        new Button(PageType.Main, "Продлить \nпортал", isActive: true),
                        new Button(PageType.Main, "Главное \nменю")
                )
            },
        };
        private bool _isIntro;
        private PageType CurrentPage = PageType.Main;
        private List<Menu> _pages;
        object _lock = new object();

        public MainPage()
        {
            InitializeComponent();
            SetPage(CurrentPage);

            Envirement.TDClient.Disconnected += OnDisconnected;
            Envirement.TDClient.Connected += OnConnected;
            Envirement.TDClient.IntroStateChanged += OnIntroStateChanged;
        }

        private async Task OnIntroStateChanged(bool arg)
        {
            bool isIntro;
            lock (_lock)
            {
                isIntro = arg;
                _isIntro = arg;
            }

            Dispatcher.BeginInvokeOnMainThread(() =>
            {
                if (isIntro)
                {
                    Intro.IsVisible = true;
                    _ = Envirement.TDClient.SendState(1);
                }
                else
                {
                    SetFirstPage();
                    Intro.IsVisible = false;
                }
            });            
                        
        }       

        private void SetFirstPage()
        {
            CurrentPage = PageType.Main;
            _ = Envirement.TDClient.SendState(CurrentPage.GetIndex());
            SetPage(CurrentPage);
        }

        private async Task OnConnected()
        {
            Dispatcher.BeginInvokeOnMainThread(() =>
            {
                FaindServer.IsVisible = false;
                SetFirstPage();
            });
        }

        private async Task OnDisconnected()
        {
            Dispatcher.BeginInvokeOnMainThread(() =>
            {
                FaindServer.IsVisible = true;
            });
        }

        private void RightButton_Relesed(string obj)
        {            
            if (Menus[CurrentPage] != null)
            {
                var nextPage = Menus[CurrentPage].rightButton.Link;
                _ = Envirement.TDClient.SendState(nextPage.GetIndex());
                SetPage(nextPage);
            }
        }

        private void LeftButton_Relesed(string obj)
        {
            if (Menus[CurrentPage] != null)
            {
                var nextPage = Menus[CurrentPage].leftButton.Link;
                _ = Envirement.TDClient.SendState(nextPage.GetIndex());
                SetPage(nextPage);
            }            
        }

        void SetPage(PageType pageType)
        {
            LeftButton.Stop();
            RightButton.Stop();

            if (Menus[pageType] != null)
            {
                LeftButton.Text = Menus[pageType].leftButton.Text;
                RightButton.Text = Menus[pageType].rightButton.Text;

                if (Menus[pageType].leftButton.IsActive)                
                    LeftButton.Restart();

                if (Menus[pageType].rightButton.IsActive)
                    RightButton.Restart();
            }

            CurrentPage = pageType;
        }
    }
}
