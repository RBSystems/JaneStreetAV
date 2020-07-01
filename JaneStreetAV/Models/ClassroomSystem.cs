using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using JaneStreetAV.Models.Config;
using UX.Lib2.Devices.Cisco;
using UX.Lib2.Devices.Extron;
using UX.Lib2.DeviceSupport;

namespace JaneStreetAV.Models
{
    public class ClassroomSystem : ASystem
    {
        private readonly Dictionary<uint, CiscoTelePresenceCodec> _codecs =
            new Dictionary<uint, CiscoTelePresenceCodec>();

        private readonly Dictionary<uint, ExtronSmp> _recorders =
            new Dictionary<uint, ExtronSmp>();

        public ClassroomSystem(CrestronControlSystem controlSystem)
            : base(controlSystem)
        {
            _codecs = new Dictionary<uint, CiscoTelePresenceCodec>();

            var addresses = ConfigManager.Config.CodecAddresses;

            if(addresses == null) return;

            foreach (var address in addresses)
            {
                _codecs[address.Key] = new CiscoTelePresenceCodec(address.Value, ConfigManager.Config.CodecUsername, ConfigManager.Config.CodecPassword);
                _codecs[address.Key].Calls.CallIncoming += CallsOnCallIncoming;
            }

            _recorders.Add(1, new ExtronSmp("30.93.1.142", "admin"));
            _recorders.Add(2, new ExtronSmp("30.93.1.141", "admin"));
        }

        private void CallsOnCallIncoming(Call call)
        {
            
        }

        public override ReadOnlyDictionary<uint, CiscoTelePresenceCodec> Codecs
        {
            get { return new ReadOnlyDictionary<uint, CiscoTelePresenceCodec>(_codecs); }
        }

        public override ReadOnlyDictionary<uint, ExtronSmp> Recorders
        {
            get { return new ReadOnlyDictionary<uint, ExtronSmp>(_recorders); }
        }

        public override ReadOnlyDictionary<uint, ICamera> Cameras
        {
            get { throw new System.NotImplementedException(); }
        }

        public override void Initialize()
        {
            base.Initialize();

            foreach (var extronSmp in _recorders.Values)
            {
                extronSmp.Initialize();
            }
        }
    }
}