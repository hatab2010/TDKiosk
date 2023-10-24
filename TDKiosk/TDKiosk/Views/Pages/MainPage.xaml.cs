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

        int _currentServerState = 0;
        public MainPage()
        {
            InitializeComponent();

            Envirement.TDClient.Disconnected += OnDisconnected;
            Envirement.TDClient.Connected += OnConnected;
            //((TDClient)Envirement.TDClient).StateRecived += OnStateRecived;
            //Envirement.TDClient.SceneStateChanged += OnSceneStateChanged;

            _webView = new HybridWebView();
            UrlWebViewSource urlSource = new UrlWebViewSource();
            urlSource.Url = System.IO.Path.Combine(DependencyService.Get<IBaseUrl>().GetUrl(), "index.html");
            _webView.Source = urlSource;
            this.Content = _webView;

            Task.Run((Func<Task>)(async () =>
            {
                await Task.Delay(5000);
                var exCpunt = 0;
                while (true || exCpunt > 10)
                {
                    try
                    {
                        Dispatcher.BeginInvokeOnMainThread((Action)(async () =>
                        {

                            if (Envirement.TDClient.IsConnect == false)
                                return;

                            try
                            {
                                var panelState = int.Parse(await _webView.EvaluateJavaScriptAsync("getState()"));
                                if (panelState != _currentState)
                                {
                                    _currentState = panelState;
                                    await Envirement.TDClient.SendState(panelState);
                                }

                                var serverState = await Envirement.TDClient.GetState();
                                if (serverState != _currentState)
                                {
                                    await _webView.EvaluateJavaScriptAsync($"setState({serverState})");
                                    _currentState = serverState;
                                }
                            }
                            catch (System.Exception)
                            {
                                await _webView.EvaluateJavaScriptAsync($"showPopup(true)");
                                await Envirement.TDClient.Connect();
                            }
                        }));

                        await Task.Delay(300);
                    }
                    catch (System.Exception)
                    {
                        exCpunt++;
                    }
                }
            }));
        }

        private void OnStateRecived(int stateIndex)
        {
            var t = stateIndex;
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
                // Envirement.TDClient.Connect();
            });
        }
    }
}
