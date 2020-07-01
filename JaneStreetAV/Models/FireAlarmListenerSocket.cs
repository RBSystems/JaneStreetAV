using System;
using System.Text;
using Crestron.SimplSharp;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.Sockets;

namespace JaneStreetAV.Models
{
    public class FireAlarmListenerSocket : TCPClientSocketBase
    {
        private CTimer _timer;

        public FireAlarmListenerSocket(string address) : base(address, 9002, 1000)
        {
        }

        public event FireAlarmSocketReceivedDataEventHandler ReceivedData;

        protected override void OnConnect()
        {
            _timer = new CTimer(specific =>
            {
                if (Connected)
                {
                    Send("\r");
                }
            }, null, 60000, 60000);
        }

        protected override void OnDisconnect()
        {
            if (_timer != null && !_timer.Disposed)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        protected override void OnReceive(byte[] buffer, int count)
        {
            var data = Encoding.ASCII.GetString(buffer, 0, count);
            if(ReceivedData == null) return;

            try
            {
                ReceivedData(data);
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }
    }

    public delegate void FireAlarmSocketReceivedDataEventHandler(string data);
}