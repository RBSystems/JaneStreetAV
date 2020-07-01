using Crestron.SimplSharpPro.DM;
using JaneStreetAV.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UX.Lib2.Config;

namespace JaneStreetAV.Models.Config
{
    public class SourceConfig : AConfig, IDeviceConfig
    {
        public string GroupName { get; set; }
        public string DeviceAddressString { get; set; }
        public uint DeviceAddressNumber { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public uint SwitcherInputIndex { get; set; }
        public uint SwitcherInputIndexSecondary { get; set; }
        public uint SwitcherOutputForContent { get; set; }
        public uint DmEndpointSourceSelect { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public JSIcons Icon { get; set; }
        public bool OnlyAvailableWhenRoomCombined { get; set; }
        public string NameForWhenRoomCombined { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public UX.Lib2.Models.SourceType SourceType { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public UX.Lib2.Models.SourceAvailabilityType AvailabilityType { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public UX.Lib2.DeviceSupport.DisplayDeviceInput DisplayInput { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public DeviceConnectionType DeviceConnectionType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public eDmps34KAudioOutSource DmpsAudioInput { get; set; }
    }
}