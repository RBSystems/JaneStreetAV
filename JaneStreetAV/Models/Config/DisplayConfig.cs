using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UX.Lib2.Config;

namespace JaneStreetAV.Models.Config
{
    public class DisplayConfig : AConfig, IDeviceConfig
    {
        public uint DeviceAddressNumber { get; set; }
        public string DeviceAddressString { get; set; }
        public uint SwitcherOutputIndex { get; set; }
        public string PreSharedKey { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DeviceConnectionType DeviceConnectionType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DisplayType DisplayType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DisplayPosition Position { get; set; }

        public bool UsesRelaysForScreenControl { get; set; }
    }

    public enum DisplayType
    {
        Generic,
        Samsung,
        CrestronConnected
    }
}