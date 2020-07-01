using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using UX.Lib2.Devices.Cisco;
using UX.Lib2.Devices.Extron;
using UX.Lib2.DeviceSupport;

namespace JaneStreetAV.Models
{
    public class MeetingSpaceSystem : ASystem
    {
        public MeetingSpaceSystem(CrestronControlSystem controlSystem)
            : base(controlSystem)
        {

        }

        public override ReadOnlyDictionary<uint, ExtronSmp> Recorders
        {
            get { return null; }
        }

        public override ReadOnlyDictionary<uint, ICamera> Cameras
        {
            get { throw new System.NotImplementedException(); }
        }

        public override ReadOnlyDictionary<uint, CiscoTelePresenceCodec> Codecs
        {
            get { return null; }
        }
    }
}