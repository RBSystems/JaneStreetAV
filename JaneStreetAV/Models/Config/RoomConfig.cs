using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using UX.Lib2.Config;
using UX.Lib2.Models;

namespace JaneStreetAV.Models.Config
{
    public class RoomConfig : AConfig
    {
        public uint PgmAudioSwitcherOutput { get; set; }
        public DspRoomConfig DspConfig { get; set; }
        public List<DisplayConfig> Displays { get; set; }
        public List<SourceConfig> Sources { get; set; }
        public string RoomType { get; set; }
        public FusionConfig Fusion { get; set; }
        public uint SwitcherOutputCodecContent { get; set; }
        public List<JToken> FusionMonitoringInfo { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DivisibleRoomType DivisibleRoomType { get; set; }
    }
}