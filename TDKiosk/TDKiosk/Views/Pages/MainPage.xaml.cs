using Java.Lang;
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
    public class HybridWebView : WebView
    {
        Action<string> action;

        public static readonly BindableProperty UriProperty = BindableProperty.Create(
            propertyName: "Uri",
            returnType: typeof(string),
            declaringType: typeof(HybridWebView),
            defaultValue: default(string));

        public string Uri
        {
            get { return (string)GetValue(UriProperty); }
            set { SetValue(UriProperty, value); }
        }

        public void RegisterAction(Action<string> callback)
        {
            action = callback;
        }

        public void Cleanup()
        {
            action = null;
        }

        public void InvokeAction(string data)
        {
            if (action == null || data == null)
            {
                return;
            }
            action.Invoke(data);
        }
    }


    public partial class MainPage : ContentPage
    {
        WebView _webView;
        int currentState = 0;
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
            //Envirement.TDClient.SceneStateChanged += OnSceneStateChanged;

            _webView = new HybridWebView();
            UrlWebViewSource urlSource = new UrlWebViewSource();
            urlSource.Url = System.IO.Path.Combine(DependencyService.Get<IBaseUrl>().GetUrl(), "index.html");
            _webView.Source = urlSource;
            this.Content = _webView;

            Task.Run(async () =>
            {
                await Task.Delay(3000);
                var exCpunt = 0;
                while (true || exCpunt > 10)
                {
                    try
                    {
                        Dispatcher.BeginInvokeOnMainThread(async () =>
                        {
                            var result = await _webView
                                .EvaluateJavaScriptAsync("getState()");
                            var panelState = int.Parse(result);

                            if (panelState != _currentState)
                            {
                                _currentState = panelState;
                                await Envirement.TDClient.SendState(panelState);
                            }

                           
                        });
                        await Task.Delay(100);
                    }
                    catch (System.Exception)
                    {
                        exCpunt++;
                    }
                }
            });
        }

        private async Task OnSceneStateChanged(int sceneIndex)
        {
            Dispatcher.BeginInvokeOnMainThread(() =>
            {
                SetScene(sceneIndex);
            });
        }

        private void SetScene(int index)
        {
            try
            {
                Dispatcher.BeginInvokeOnMainThread(async () =>
                {
                    //var result = await _webView.EvaluateJavaScriptAsync($"setState({index})");
                });

            }
            catch (System.Exception)
            {

            }
        }

        private async Task OnConnected()
        {
            Dispatcher.BeginInvokeOnMainThread(async () =>
            {
                await _webView.EvaluateJavaScriptAsync($"showPopup(false)");
            });
        }

        private async Task OnDisconnected()
        {
            Dispatcher.BeginInvokeOnMainThread(async () =>
            {
                await _webView.EvaluateJavaScriptAsync($"showPopup(true)");
            });
        }
    }
}
