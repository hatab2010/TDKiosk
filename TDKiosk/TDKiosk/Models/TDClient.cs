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
       bool IsConnect { get; }
       Task<int> GetState();
       Task Connect();
       Task Disconnect();
       Task SendState(int index);
    }

    public class ConnectException : Exception
    {

    }

    public class TDClientBase
    {
        public event Func<int, Task> SceneStateChanged;

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
        public bool IsConnect
        {
            get
            {
                lock (_lock)
                    return _isConnect;
            }
            protected set
            {

                lock (_lock)
                {
                    _isConnect = value;
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

    public class TDDemo : ITDClient
    {
        public bool IsConnect => throw new NotImplementedException();

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

        public Task<int> GetState()
        {
            throw new NotImplementedException();
        }

        public Task SendState(int index)
        {
            return Task.CompletedTask;
        }
    }

    public class TDClient : TDClientBase, IDisposable, ITDClient
    {
        public event Func<Task> Disconnected;
        public event Func<Task> Connected;
        public event Action<int> StateRecived;

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
        }

        public async Task Disconnect()
        {
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

                        IsConnect = await GetState() != -1;
                        Connected?.Invoke();
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

        protected override async Task OnServerDisconnected()
        {
            IsDataSychrone = false;
            await FindServer();
        }

        public async Task SendState(int index)
        {

            await GetResponseString($"http://{_server}:{port}/set_state?index={index}");
        }

        public async Task<int> GetState()
        {

            return int.Parse(await GetResponseString($"http://{_server}:{port}/get_state"));
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
                if (IsConnect == true)
                {
                    IsConnect = false;
                    Disconnected?.Invoke();
                }                

                throw new ConnectException();
            }
        }

        public void Dispose()
        {
            Disconnect().Wait();
            _httpClient.Dispose();
        }
    }
}
