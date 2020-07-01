using System;
using System.Text.RegularExpressions;
using UX.Lib2;
using UX.Lib2.Cloud.Logger;

namespace JaneStreetAV.Models
{
    public class FireAlarmListener
    {
        private FireAlarmStatus _status;
        private readonly FireAlarmListenerSocket _socket;

        public FireAlarmListener(string interfaceIpAddress)
        {
            _socket = new FireAlarmListenerSocket(interfaceIpAddress);
        }

        public event FireAlarmStatusChangedEventHandler StatusChanged;

        public void Start()
        {
            _socket.ReceivedData += SocketOnReceivedData;
            _socket.Connect();
        }

        private void SocketOnReceivedData(string data)
        {
#if DEBUG
            Debug.WriteInfo("FireListener Rx: {0}", data);
#endif
            var match = Regex.Match(data, @"firestate\[(\w+)\]");

            if (match.Success)
            {
                Status = (FireAlarmStatus)Enum.Parse(typeof(FireAlarmStatus), match.Groups[1].Value, true);
            }
        }

        public FireAlarmStatus Status
        {
            get { return _status; }
            set
            {
                if (_status == value) return;

                _status = value;
                
                if(StatusChanged == null) return;

                try
                {
                    StatusChanged(this, _status);
                }
                catch (Exception e)
                {
                    CloudLog.Exception(e);
                }
            }
        }
    }

    public enum FireAlarmStatus
    {
        Normal,
        Alert
    }

    public delegate void FireAlarmStatusChangedEventHandler(FireAlarmListener listener, FireAlarmStatus status);
}