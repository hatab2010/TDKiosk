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

    public partial class MainPage : ContentPage
    {
        private int _currentState;
        private List<Menu> _pages;
        object _lock = new object();
        private List<Card> _cards = new List<Card>();
        private List<string> _descriptions = new List<string>
        {
            "Описание для интро",
            "Текст подсказка для ART1",
            "Текст подсказка для ART2",
            "Текст подсказка для ART3"
        };

        public MainPage()
        {
            InitializeComponent();

            Envirement.TDClient.Disconnected += OnDisconnected;
            Envirement.TDClient.Connected += OnConnected;
            Envirement.TDClient.SceneStateChanged += OnIntroStateChanged;

            _cards.Add(Btn_intercome);
            _cards.Add(Btn_art1);
            _cards.Add(Btn_art2);
            _cards.Add(Btn_art3);

            foreach (var btn in _cards)
            {
                btn.OnPressed += BtnOnPressed;
            }
        }

        private void BtnOnPressed(Card card)
        {
            var sceneIndex = _cards.IndexOf(card) + 2;
            Envirement.TDClient.SendState(sceneIndex);

            Dispatcher.BeginInvokeOnMainThread(() =>
            {
                SetScene(sceneIndex);
            });
        }

        private async Task OnIntroStateChanged(int sceneIndex)
        {
            Dispatcher.BeginInvokeOnMainThread(() =>
            {
                SetScene(sceneIndex);
            });
        }

        private void SetScene(int index)
        {
            if (index == _currentState)
                return;

            if (index == 0)
                Intro.IsVisible = true;
            else
                Intro.IsVisible = false;


            foreach (var card in _cards) { card.FlashEnable = false; }

            if (index >= 2 && index <= 5)
            {
                var buttonIndex = index - 2;
                _cards[buttonIndex].FlashEnable = true;
                Infobar.Text = _descriptions[buttonIndex];
            }

            lock (_lock)
            {
                _currentState = index;
            }
        }

        private async Task OnConnected()
        {
            Dispatcher.BeginInvokeOnMainThread(() =>
            {
                FaindServer.IsVisible = false;
            });
        }

        private async Task OnDisconnected()
        {
            Dispatcher.BeginInvokeOnMainThread(() =>
            {
                FaindServer.IsVisible = true;
            });
        }
    }
}
