using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using static Android.Graphics.ColorSpace;

namespace TDKiosk.Models
{

    public interface ITDClient
    {
       event Func<bool, Task> IntroStateChanged;

       event Func<Task> Disconnected;
       event Func<Task> Connected;

       Task Connect();
       Task Disconnect();
       Task SendState(int index);
    }

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

        protected void JaUstal()
        {
            _ = Connected?.Invoke();
        }

        protected virtual async Task OnServerDisconnected() { }
    }

    public class TDDemo : ITDClient
    {
        public event Func<bool, Task> IntroStateChanged;
        public event Func<Task> Disconnected;
        public event Func<Task> Connected;

        public Task Connect()
        {
            Connected?.Invoke();
            IntroStateChanged?.Invoke(false);
            return Task.CompletedTask;
        }

        public Task Disconnect()
        {
            Disconnected?.Invoke();
            return Task.CompletedTask;
        }

        public Task SendState(int index)
        {
            return Task.CompletedTask;
        }
    }

    public class TDClient : TDClientBase, IDisposable, ITDClient
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
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(3);
        }

        public async Task Connect()
        {
            await FindServer();
            await StartPolling();           
        }

        public async Task Disconnect()
        {
            await StopPolling();
            IsDataSychrone = false;
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
                        // получить ip-адрес
                        var myIP = _ipAddressManager.GetLocalIPAddress();
                        var segments = myIP.ToString().Split('.');
                        segments[3] = _reservedAdress.ToString();
                        var r = String.Join(".", segments);
                        _server = IPAddress.Parse(r);

                        await GetIntroState();
                        JaUstal();
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
            try
            {
                await GetResponseString($"http://{_server.ToString()}:{port}/set_state?index={index}");
            }
            catch (Exception) { }
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
