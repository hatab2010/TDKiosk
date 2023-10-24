using Android.App;
using Android.Content;
using Android.Net.Wifi;
using Java.Lang;
using Java.Nio;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using TDKiosk;

namespace TDKiosk
{
    public class IpAddressManager
    {
        public string GetLocalIPAddress()
        {
            var wifiManager = (WifiManager)Application.Context.GetSystemService(Context.WifiService);
            int ipAddress = wifiManager.ConnectionInfo.IpAddress;

            //// Convert little-endian to big-endian if needed
            //if (ByteOrder.NativeOrder().Equals(ByteOrder.LittleEndian))
            //{
            //    ipAddress = Integer.ReverseBytes(ipAddress);
            //}

            byte[] ipBytes = BitConverter.GetBytes(ipAddress);
            return new IPAddress(ipBytes).ToString();
        }
    }


    public interface IBaseUrl
    {
        string GetUrl();
    }
}
