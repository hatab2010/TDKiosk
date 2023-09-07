using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace TDKiosk.Models
{
    public class TDClientBase
    {
        public event Func<bool, Task> IntroStateChanged;

        public event Func<Task> Disconnected;
        public event Func<Task> Connected;

        private bool _isDataSynchrone;

        public bool IsDataSychrone
        {
            get { lock (_lock) return _isDataSynchrone; }
            set { lock (_lock) _isDataSynchrone = value; }
        }


        protected object _lock = new object();

        private bool _introState;
        protected bool IntroState
        {
            get
            {
                lock (_lock)
                    return _introState;
            }
            set
            {
                bool introState;
                lock (_lock)
                {
                    introState = _introState;
                    _introState = value;
                }

                if (introState != value || IsDataSychrone == false)
                {
                    _ = IntroStateChanged?.Invoke(value);
                    IsDataSychrone = true;
                }
            }
        }

        private bool _isConnect;
        protected bool IsConnect
        {
            get
            {
                lock (_lock)
                    return _isConnect;
            }
            set
            {
                bool isConnect;
                bool isNew;

                lock (_lock)
                {
                    isNew = _isConnect != value;
                    isConnect = value;
                    _isConnect = value;
                }

                if (isConnect == true && (isNew))
                    Connected?.Invoke();
                else if (isConnect == false && isNew)
                {
                    _ = Disconnected?.Invoke();
                    _ = OnServerDisconnected();
                }
            }
        }

        private bool _isPolling;

        public bool IsPolling
        {
            get { lock (_lock) return _isPolling; }
            set { lock (_lock) _isPolling = value; }
        }

        protected virtual async Task OnServerDisconnected() { }
    }

    public class TDClient : TDClientBase, IDisposable
    {
        private IpAddressManager _ipAddressManager = new IpAddressManager();
        private IPAddress _server;
        private HttpClient _httpClient;
        private int _reservedAdress = 60;
        private int port = 11859;
        private int _poolingInterval = 1000;

        private bool _isPolling = false;
        private Task _poolingProcess;

        public TDClient()
        {
            //_httpClient = new HttpClient();
            //_httpClient.Timeout = TimeSpan.FromSeconds(3);
        }

        public async Task Connect()
        {
            //await FindServer();
            //await StartPolling();
        }

        public async Task Disconnect()
        {
            //await StopPolling();
            //IsDataSychrone = false;
        }

        bool _dispoce;
        private async Task FindServer()
        {
            try
            {
                while (true)
                {
                    lock (_lock)
                        if (_dispoce)
                        break;

                    try
                    {
                        // получить имя хоста
                        //string hostName = Dns.GetHostName();

                        // получить ip-адрес
                        var myIP = _ipAddressManager.GetLocalIPAddress();
                        var segments = myIP.ToString().Split('.');
                        segments[3] = _reservedAdress.ToString();
                        var r = String.Join(".", segments);
                        _server = IPAddress.Parse(r);

                        await GetIntroState();
                        IsConnect = true;
                        break;
                    }
                    catch (Exception)
                    {
                        await Task.Delay(5000);
                    }
                }
            }
            catch
            {

            }
        }       

        private async Task StartPolling()
        {
            await StopPolling();
            IsPolling = true;
            _poolingProcess = Task.Run(Pooling);
        }

        private async Task StopPolling()
        {
            IsPolling = false;

            try
            {
                if (_poolingProcess != null)
                    await _poolingProcess;

                _poolingProcess = null;
            }
            catch (Exception)
            {
            }
        }

        private async Task Pooling()
        {
            if (!IsPolling)
                return;

            while (IsPolling)
            {
                try
                {
                    var introSTate = await GetIntroState();
                    IntroState = introSTate;
                    await Task.Delay(_poolingInterval);

                }
                catch (Exception) { IsConnect = false; IsPolling = false; break; }
            }
        }

        protected override async Task OnServerDisconnected()
        {
            IsDataSychrone = false;
            await StopPolling();
            await FindServer();
            await StartPolling();
        }

        public async Task SendState(int index)
        {
            //try
            //{
            //    await GetResponseString($"http://{_server.ToString()}:{port}/set_state?index={index}");
            //}
            //catch (Exception) { }
        }

        protected async Task<bool> GetIntroState()
        {
            return bool.Parse(await GetResponseString($"http://{_server.ToString()}:{port}/get_intro_state"));
        }

        protected async Task<string> GetResponseString(string uri)
        {
            try
            {
                using (var response = await _httpClient.GetAsync(uri))
                {
                    var contents = await response.Content.ReadAsStringAsync();
                    return contents;
                }
            }
            catch (Exception ex)
            {
                IsConnect = false;
                throw new Exception();
            }
        }

        public void Dispose()
        {
            Disconnect().Wait();
            _httpClient.Dispose();
        }
    }
}
