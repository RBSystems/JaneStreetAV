using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.Devices.Cisco;
using UX.Lib2.Devices.Extron;
using UX.Lib2.Devices.FireInterfaces;
using UX.Lib2.DeviceSupport;

namespace JaneStreetAV.Models
{
    public class FireAlarmMonitorSystem : ASystem
    {
        private readonly FireAlarmMonitor _fireAlarmMonitor;

        public FireAlarmMonitorSystem(CrestronControlSystem controlSystem) : base(controlSystem)
        {
            try
            {
                if (controlSystem.NumberOfVersiPorts > 0)
                {
                    controlSystem.VersiPorts[1].Register();
                    _fireAlarmMonitor = new FireAlarmMonitor(9002, 20, controlSystem.VersiPorts[1]);
                    return;
                }

                if(controlSystem.NumberOfDigitalInputPorts > 0)
                {
                    controlSystem.DigitalInputPorts[1].Register();
                    _fireAlarmMonitor = new FireAlarmMonitor(9002, 20, controlSystem.DigitalInputPorts[1]);
                    return;
                }

                CloudLog.Error("Could not setup fire alarm interface! No io ports available");
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }

        public override ReadOnlyDictionary<uint, ExtronSmp> Recorders
        {
            get { return null; }
        }

        public override ReadOnlyDictionary<uint, ICamera> Cameras
        {
            get { throw new NotImplementedException(); }
        }

        public override ReadOnlyDictionary<uint, CiscoTelePresenceCodec> Codecs
        {
            get { return null; }
        }

        public override void Initialize()
        {
            base.Initialize();

            if (_fireAlarmMonitor != null)
            {
                _fireAlarmMonitor.Start(true);
            }
        }
    }
}