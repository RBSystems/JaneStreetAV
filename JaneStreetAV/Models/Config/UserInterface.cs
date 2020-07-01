using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UX.Lib2.Config;

namespace JaneStreetAV.Models.Config
{
    public class UserInterface : AConfig, IDeviceConfig
    {
        public string DeviceType { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public UIControllerType UIControllerType { get; set; }
        public uint DefaultRoom { get; set; }
        public string DeviceAddressString { get; set; }
        public uint DeviceAddressNumber { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public DeviceConnectionType DeviceConnectionType { get; set; }
        public List<uint> AllowedRoomIds { get; set; }

        public UserInterface()
        {
            AllowedRoomIds = new List<uint>();
        }
    }

    public enum UIControllerType
    {
        UserPanel,
        ControlRoom,
        TechPanel,
        RemoteMpc,
        Mpc
    }
}