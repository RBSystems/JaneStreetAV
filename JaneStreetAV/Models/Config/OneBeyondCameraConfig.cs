using System.Collections.Generic;

namespace JaneStreetAV.Models.Config
{
    public class OneBeyondConfig
    {
        public Dictionary<uint, OneBeyondProcessor> ProcessorConfigs { get; set; }
        public List<OneBeyondCameraConfig> Cameras { get; set; } 
    }

    public class OneBeyondProcessor
    {
        public List<OneBeyondProcessorConfig> CameraConfigs { get; set; } 
    }

    public class OneBeyondProcessorConfig
    {
        public uint UsedWithConfigValue { get; set; }
        public List<uint> CameraIds { get; set; }
    }

    public class OneBeyondCameraConfig
    {
        public uint Id { get; set; }
        public string IpAddress { get; set; }
        public string Name { get; set; }
    }
}