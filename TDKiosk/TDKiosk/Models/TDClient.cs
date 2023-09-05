using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WebSocket4Net;

namespace TDKiosk.Models
{
    public class WebSocketClient
    {
        private WebSocket _webSocket;

        private string _url;

        public WebSocketClient(string url)
        {
            _url = url;
            InitializeWebSocket();
        }

        private void InitializeWebSocket()
        {
            _webSocket = new WebSocket(_url);

            _webSocket.Opened += WebSocketOpened;
            _webSocket.Closed += WebSocketClosed;
            _webSocket.MessageReceived += WebSocketMessageReceived;
            _webSocket.Error += WebSocketError;

            _webSocket.Open();
        }

        private void WebSocketError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Debug.WriteLine("WebSocket Error: " + e.Exception.Message);
        }

        private void WebSocketMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Debug.WriteLine("WebSocket Message Received: " + e.Message);
        }

        private void WebSocketClosed(object sender, EventArgs e)
        {
            Debug.WriteLine("WebSocket Connection Closed");
            System.Threading.Thread.Sleep(5000);
            InitializeWebSocket();
        }

        private void WebSocketOpened(object sender, EventArgs e)
        {
            Debug.WriteLine("WebSocket Connection Opened");
        }

        public void SendMessage(string message)
        {
            if (_webSocket.State == WebSocketState.Open)
                _webSocket.Send(message);
        }
    }

    internal class TDClient
    {
        public event Action Disconnected;
        public event Action Connected;

        private object _lock = new object();
        private IPAddress _server;
        private HttpClient _httpClient;
        private int _reservedAdress = 73;
        private int port = 9980;
        private int _poolingInterval = 2000;
        private bool _isConnect;

        private Task _poolingProcess;

        public TDClient()
        {
            _httpClient = new HttpClient();
            //_client.GetAsync
        }

        public bool IsConnect
        {
            get
            {
                lock (_lock)
                    return _isConnect;
            } 
        }

        private IPAddress GetNetworkRang()
        {
            throw new NotImplementedException();
        }

        private void FindServer()
        {
            var isFinded = false;

            while(!isFinded)
                try 
                {
                    
                } catch (Exception) 
                {
                    OnServerDisconnect();
                }
        }

        private void OnServerDisconnect()
        {
            _isConnect = false;

        }

        private void StartPolling()
        {


        }

        private void StopPolling()
        {

        }

        private void GetEnableSatet()
        {
            try
            {
                _httpClient.GetAsync($"http://{_server.ToString()}:{port}/set_state?index={index}");
            }
            catch (Exception)
            {

            }
        }

        private void SendState(int index)
        {
                _httpClient.GetAsync($"http://{_server.ToString()}:{port}/set_state?index={index}");
        }

        private async Task<HttpResponseMessage> Get(string url)
        {
            try
            {
            }
            catch (Exception)
            {
            }
        }
    }
}
