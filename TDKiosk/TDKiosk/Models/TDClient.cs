using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using static Android.Graphics.ColorSpace;

namespace TDKiosk.Models
{

    public interface ITDClient
    {
       event Func<int, Task> SceneStateChanged;

       event Func<Task> Disconnected;
       event Func<Task> Connected;

       Task Connect();
       Task Disconnect();
       Task SendState(int index);
    }

    public class TDClientBase
    {
        public event Func<int, Task> SceneStateChanged;

        public event Func<Task> Disconnected;
        public event Func<Task> Connected;

        private bool _isDataSynchrone;

        public bool IsDataSychrone
        {
            get { lock (_lock) return _isDataSynchrone; }
            set { lock (_lock) _isDataSynchrone = value; }
        }

        protected object _lock = new object();

        private int _sceneState;
        protected int SceneState
        {
            get
            {
                lock (_lock)
                    return _sceneState;
            }
            set
            {
                int introState;
                lock (_lock)
                {
                    introState = _sceneState;
                    _sceneState = value;
                }

                if (introState != value || IsDataSychrone == false)
                {
                    _ = SceneStateChanged?.Invoke(value);
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
                else if (isConnect == false)
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
            //_ = Connected?.Invoke();
        }

        protected virtual async Task OnServerDisconnected() { }
    }

    public class TDDemo : ITDClient
    {
        public event Func<int, Task> SceneStateChanged;
        public event Func<Task> Disconnected;
        public event Func<Task> Connected;

        public Task Connect()
        {
            Connected?.Invoke();
            SceneStateChanged?.Invoke(1);
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

                        IsConnect = await GetState() != 0;
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
                    var introSTate = await GetState();
                    SceneState = introSTate;
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
                await GetResponseString($"http://{_server}:{port}/set_state?index={index}");
            }
            catch (Exception) 
            {
                IsConnect = false;
            }
        }

        protected async Task<int> GetState()
        {
            try
            {
                return int.Parse(await GetResponseString($"http://{_server}:{port}/get_state"));
            }
            catch
            {
                IsConnect = false;
                return 0;
            }
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
