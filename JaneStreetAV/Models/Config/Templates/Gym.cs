using System.Collections.Generic;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
using Crestron.SimplSharpPro.EthernetCommunication;
using Crestron.SimplSharpPro.UI;
using JaneStreetAV.UI;
using UX.Lib2.Config;
using UX.Lib2.DeviceSupport;
using UX.Lib2.Models;

namespace JaneStreetAV.Models.Config.Templates
{
    public class Gym : TemplateBase
    {
        public Gym()
        {
            SystemType = SystemType.RecArea;

            UserInterfaces = new List<UserInterface>
            {
                new UserInterface
                {
                    Enabled = true,
                    Id = 1,
                    Name = string.Format("MPC"),
                    UIControllerType = UIControllerType.Mpc,
                    DefaultRoom = 1
                }
            };

            Rooms = new List<RoomConfig>()
            {
                new RoomConfig()
                {
                    Enabled = true,
                    Id = 1,
                    RoomType = typeof (Rooms.Gym).FullName,
                    DspConfig = new DspRoomConfig()
                    {
                        Enabled = true,
                        PgmVolControlName = "gym.level",
                        OtherComponents = new List<string>
                        {
                            "gym.select"
                        }
                    },
                    Displays = new List<DisplayConfig>
                    {
                        new DisplayConfig
                        {
                            Enabled = true,
                            Id = 1,
                            Name = "Display",
                            SwitcherOutputIndex = 1,
                            DisplayType = DisplayType.Samsung,
                            DeviceAddressString = "30.92.1.93",
                            DeviceAddressNumber = IpIdFactory.Create(IpIdFactory.DeviceType.Other),
                            UsesRelaysForScreenControl = true
                        }
                    },
                    Sources = new List<SourceConfig>
                    {
                        new SourceConfig
                        {
                            Id = 1,
                            Name = "Laptop",
                            Icon = JSIcons.Laptop,
                            SourceType = SourceType.Laptop,
                            DisplayInput = DisplayDeviceInput.HDMI1,
                            Enabled = true
                        },
                        new SourceConfig
                        {
                            Id = 2,
                            Name = "IPTV",
                            Icon = JSIcons.TV,
                            DeviceAddressNumber = 26,
                            SourceType = SourceType.IPTV,
                            DisplayInput = DisplayDeviceInput.HDMI2,
                            Enabled = true
                        }
                    },
                    Fusion = new FusionConfig
                    {
                        Enabled = true,
                        DeviceAddressNumber = 0xF1
                    }
                }
            };

            GlobalSources = new List<SourceConfig>();

            SwitcherType = SystemSwitcherType.DmFrame;

            DspConfig = new DspDeviceConfig
            {
                Enabled = true,
                DeviceAddressString = "30.92.1.163",
                DeviceConnectionType = DeviceConnectionType.Network,
                Name = "Q-Sys Core"
            };

            TriplePlayServerAddress = "30.93.1.126";
        }
    }
}