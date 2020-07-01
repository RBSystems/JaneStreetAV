using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UX.Lib2.Config;

namespace JaneStreetAV.Models.Config
{
    public class SystemConfig : AConfig
    {
        public bool IsDefault { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public SystemType SystemType { get; set; }
        public string ConfigPath { get; set; }
        public string SystemId { get; set; }
        public List<UserInterface> UserInterfaces { get; set; }
        public List<RoomConfig> Rooms { get; set; }
        public List<SourceConfig> GlobalSources { get; set; }
        public SwitcherConfig SwitcherConfig { get; set; }
        public DspDeviceConfig DspConfig { get; set; }
        public string SwitcherAddress { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public SystemSwitcherType SwitcherType { get; set; }
        public Dictionary<string, object> ValueStore { get; set; }
        public string BackgroundImageUrl { get; set; }
        public string TriplePlayServerAddress { get; set; }
        public List<FaderItemConfig> DspFaderComponents { get; set; }
        public List<FaderItemColor> DspFaderColors { get; set; }
        public List<FaderItemFilter> DspFaderFilters { get; set; } 
        public Dictionary<uint, string> CodecAddresses { get; set; }
        public string CodecUsername { get; set; }
        public string CodecPassword { get; set; }
        public Dictionary<uint, string> OneBeyondAddresses { get; set; } 
        public string OneBeyondUsername { get; set; }
        public string OneBeyondPassword { get; set; }
        public OneBeyondConfig OneBeyondConfig { get; set; }

        public SystemConfig()
        {
            ConfigPath = "Not Defined / Self Generated";

            UserInterfaces = new List<UserInterface>();
            Rooms = new List<RoomConfig>();
            GlobalSources = new List<SourceConfig>();
            OneBeyondAddresses = new Dictionary<uint, string>();
            ValueStore = new Dictionary<string, object>();
        }
    }

    public enum SystemSwitcherType
    {
        NotInstalled,
        BigDmFrame,
        DmFrame
    }

    public class FaderItemConfig
    {
        public string Name { get; set; }
        public string ComponentName { get; set; }
        public string TrafficLightBaseName { get; set; }
        public string Color { get; set; }
        public uint Group { get; set; }
        public uint MixerIndex { get; set; }
    }

    public class FaderItemColor
    {
        public string Name { get; set; }
        public string ColorHex { get; set; }
        public int Priority { get; set; }
    }

    public class FaderItemFilter
    {
        public uint FilterButton { get; set; }
        public string FilterString { get; set; }
    }
}