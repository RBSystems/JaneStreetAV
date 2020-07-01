using System.Collections.Generic;

namespace JaneStreetAV.Models.Config.Templates
{
    public class RoomMonitoring : TemplateBase
    {
        public RoomMonitoring()
        {
            SystemType = SystemType.RoomMonitoring;

            Rooms = new List<RoomConfig>()
            {
                new RoomConfig()
                {
                    Enabled = true,
                    Id = 1,
                    RoomType = typeof (Rooms.MonitoredRoom).FullName,
                    Displays = new List<DisplayConfig>
                    {
                        new DisplayConfig
                        {
                            Enabled = true,
                            Id = 1,
                            Name = "Display 1",
                            DisplayType = DisplayType.Generic
                        }
                    },
                    Fusion = new FusionConfig
                    {
                        Enabled = true,
                        DeviceAddressNumber = 0xF1
                    }
                }
            };

            CodecAddresses = new Dictionary<uint, string>
            {
                {1, "172.16.200.15"},
            };

            CodecUsername = "crestron";
            CodecPassword = "crestron";
        }
    }
}