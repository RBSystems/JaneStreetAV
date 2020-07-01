using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.EthernetCommunication;
using JaneStreetAV.Models.Config;
using UX.Lib2.Devices.Cameras.Sony;
using UX.Lib2.Devices.Cisco;
using UX.Lib2.Devices.Extron;
using UX.Lib2.DeviceSupport;

namespace JaneStreetAV.Models
{
    public class AuditoriumSystem : ASystem
    {
        private readonly Dictionary<uint, CiscoTelePresenceCodec> _codecs =
            new Dictionary<uint, CiscoTelePresenceCodec>();

        private readonly Dictionary<uint, ExtronSmp> _recorders =
            new Dictionary<uint, ExtronSmp>();

        private readonly Dictionary<uint, ICamera> _cameras = new Dictionary<uint, ICamera>();

        public AuditoriumSystem(CrestronControlSystem controlSystem) : base(controlSystem)
        {
            _codecs = new Dictionary<uint, CiscoTelePresenceCodec>();

            var addresses = ConfigManager.Config.CodecAddresses;

            if(addresses == null) return;

            foreach (var address in addresses)
            {
                _codecs[address.Key] = new CiscoTelePresenceCodec(address.Value, ConfigManager.Config.CodecUsername, ConfigManager.Config.CodecPassword);
                _codecs[address.Key].Calls.CallIncoming += CallsOnCallIncoming;
            }

            _recorders.Add(1, new ExtronSmp("30.92.1.167", "admin"));
            _recorders.Add(2, new ExtronSmp("30.92.1.166", "admin"));

            var camConf = ConfigManager.Config.OneBeyondConfig.Cameras;
            if (camConf != null)
            {
                foreach (var cameraConfig in camConf)
                {
                    _cameras[cameraConfig.Id] = new ViscaCamera(1, cameraConfig.IpAddress, 5500);
                }
            }
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
            get { return new ReadOnlyDictionary<uint, ICamera>(_cameras); }
        }

        public ThreeSeriesTcpIpEthernetIntersystemCommunications BlindsProcessorEIC { get; set; }

        public override void Initialize()
        {
            base.Initialize();

            foreach (var extronSmp in _recorders.Values)
            {
                extronSmp.Initialize();
            }

            foreach (var cam in _cameras.Values.OfType<ViscaCamera>())
            {
                cam.Initialize();
            }
        }
    }
}