using System.Collections.Generic;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
using Crestron.SimplSharpPro.EthernetCommunication;
using Crestron.SimplSharpPro.UI;
using JaneStreetAV.UI;
using UX.Lib2.Config;
using UX.Lib2.Models;

namespace JaneStreetAV.Models.Config.Templates
{
    public class RecArea : TemplateBase
    {
        public RecArea()
        {
            SystemType = SystemType.RecArea;

            UserInterfaces = new List<UserInterface>
            {
                new UserInterface
                {
                    Enabled = true,
                    Id = 1,
                    DeviceAddressNumber = 0x0A,
                    DeviceAddressString = "30.93.1.155",
                    DeviceType = typeof (ThreeSeriesTcpIpEthernetIntersystemCommunications).Name,
                    Name = string.Format("MPC 1"),
                    UIControllerType = UIControllerType.RemoteMpc,
                    DefaultRoom = 2
                },
                new UserInterface
                {
                    Enabled = true,
                    Id = 2,
                    DeviceAddressNumber = 0x0B,
                    DeviceAddressString = "30.93.1.156",
                    DeviceType = typeof (ThreeSeriesTcpIpEthernetIntersystemCommunications).Name,
                    Name = string.Format("MPC 2"),
                    UIControllerType = UIControllerType.RemoteMpc,
                    DefaultRoom = 2
                },
                new UserInterface
                {
                    Enabled = true,
                    Id = 3,
                    DeviceAddressNumber = 0x0C,
                    DeviceAddressString = "30.93.1.157",
                    DeviceType = typeof (ThreeSeriesTcpIpEthernetIntersystemCommunications).Name,
                    Name = string.Format("MPC 3"),
                    UIControllerType = UIControllerType.RemoteMpc,
                    DefaultRoom = 1
                },
                new UserInterface
                {
                    Enabled = true,
                    Id = 4,
                    DeviceAddressNumber = 0x04,
                    DeviceAddressString = "JaneStreet_UI_CrestronApp_MJ_v1_03",
                    DeviceType = typeof (CrestronApp).Name,
                    Name = string.Format("MPC 3"),
                    UIControllerType = UIControllerType.TechPanel,
                    DefaultRoom = 1
                }
            };

            Rooms = new List<RoomConfig>()
            {
                new RoomConfig()
                {
                    Enabled = true,
                    Id = 1,
                    RoomType = typeof (Rooms.RecRoom).FullName,
                    DspConfig = new DspRoomConfig()
                    {
                        Enabled = true,
                        PgmVolControlName = "pgm.level"
                    },
                    Displays = new List<DisplayConfig>
                    {
                        new DisplayConfig
                        {
                            Enabled = true,
                            Id = 1,
                            Name = "Display 1",
                            SwitcherOutputIndex = 1,
                            DisplayType = DisplayType.CrestronConnected,
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
                            SwitcherInputIndex = 1,
                            SourceType = SourceType.Laptop,
                            Enabled = true
                        },
                        new SourceConfig
                        {
                            Id = 2,
                            Name = "IPTV",
                            Icon = JSIcons.TV,
                            SwitcherInputIndex = 4,
                            DeviceAddressNumber = 10,
                            SourceType = SourceType.IPTV,
                            Enabled = true
                        }
                    },
                    PgmAudioSwitcherOutput = 3,
                    Fusion = new FusionConfig
                    {
                        Enabled = true,
                        DeviceAddressNumber = 0xF1
                    }
                },
                new RoomConfig()
                {
                    Enabled = true,
                    Id = 2,
                    RoomType = typeof (Rooms.RecRoom).FullName,
                    DspConfig = new DspRoomConfig()
                    {
                        Enabled = false,
                        PgmVolControlName = string.Empty
                    },
                    Displays = new List<DisplayConfig>
                    {
                        new DisplayConfig
                        {
                            Enabled = true,
                            Id = 1,
                            Name = "Display 1",
                            SwitcherOutputIndex = 2,
                            DisplayType = DisplayType.Samsung
                        }
                    },
                    Sources = new List<SourceConfig>
                    {
                        new SourceConfig
                        {
                            Id = 1,
                            Name = "Laptop",
                            Icon = JSIcons.Laptop,
                            SwitcherInputIndex = 2,
                            SourceType = SourceType.Laptop,
                            Enabled = true
                        },
                        new SourceConfig
                        {
                            Id = 2,
                            Name = "IPTV",
                            Icon = JSIcons.TV,
                            SwitcherInputIndex = 6,
                            DeviceAddressNumber = 12,
                            SourceType = SourceType.IPTV,
                            Enabled = true
                        }
                    },
                    Fusion = new FusionConfig
                    {
                        Enabled = true,
                        DeviceAddressNumber = 0xF2
                    }
                }
            };

            GlobalSources = new List<SourceConfig>();

            SwitcherType = SystemSwitcherType.DmFrame;

            CodecUsername = "admin";
            CodecPassword = "";

            DspConfig = new DspDeviceConfig
            {
                Enabled = true,
                DeviceAddressString = "30.93.1.217",
                DeviceConnectionType = DeviceConnectionType.Network,
                Name = "Q-Sys Core"
            };

            SwitcherConfig = new SwitcherConfig
            {
                Enabled = true,
                FrameType = typeof (DmMd8x8Cpu3).FullName,
                Id = 0x80,
                Name = "DM Switcher Config",
                InputCards = new List<SwitcherCardConfig>(),
                OutputCards = new List<SwitcherCardConfig>(),
                Inputs = new List<SwitcherInputConfig>(),
                Outputs = new List<SwitcherOutputConfig>()
            };

            for (uint nCard = 1; nCard <= 2; nCard++)
            {
                SwitcherConfig.InputCards.Add(new SwitcherCardConfig
                {
                    Number = nCard,
                    Type = typeof(Dmc4kzC).FullName
                });
            }

            for (uint nCard = 3; nCard <= 6; nCard++)
            {
                SwitcherConfig.InputCards.Add(new SwitcherCardConfig
                {
                    Number = nCard,
                    Type = typeof(Dmc4kzHd).FullName
                });
            }

            SwitcherConfig.OutputCards.Add(new SwitcherCardConfig
            {
                Number = 1,
                Type = typeof(Dmc4kzCoHdSingle).FullName
            });

            SwitcherConfig.OutputCards.Add(new SwitcherCardConfig
            {
                Number = 2,
                Type = typeof(Dmc4kzHdoSingle).FullName
            });

            for (var input = 1U; input <= 2; input ++)
            {
                SwitcherConfig.Inputs.Add(new SwitcherInputConfig
                {
                    Number = input,
                    Name = "Input " + input,
                    EndpointType = typeof(DmTx4k202C).FullName
                });
            }

            for (var input = 3U; input <= 6; input++)
            {
                SwitcherConfig.Inputs.Add(new SwitcherInputConfig
                {
                    Number = input,
                    Name = "Input " + input,
                    EndpointType = string.Empty
                });
            }

            foreach (var inputConfig in SwitcherConfig.Inputs)
            {
                switch (inputConfig.Number)
                {
                    case 1: inputConfig.Name = "Rec Pres Laptop"; break;
                    case 2: inputConfig.Name = "Gaming Area Laptop"; break;
                    case 3: inputConfig.Name = "Spare"; break;
                    case 4: inputConfig.Name = "IPTV 1"; break;
                    case 5: inputConfig.Name = "Spare"; break;
                    case 6: inputConfig.Name = "IPTV 2"; break;
                }
            }

            for (var output = 1U; output <= 2; output++)
            {
                SwitcherConfig.Outputs.Add(new SwitcherOutputConfig
                {
                    Number = output,
                    Name = "Output " + output,
                    EndpointType = typeof (DmRmc4kzScalerC).FullName
                });
            }

            for (var output = 3U; output <= 4; output++)
            {
                var outputConfig = new SwitcherOutputConfig
                {
                    Number = output,
                    Name = "Output " + output,
                    EndpointType = string.Empty
                };

                SwitcherConfig.Outputs.Add(outputConfig);
            }

            foreach (var outputConfig in SwitcherConfig.Outputs)
            {
                switch (outputConfig.Number)
                {
                    case 1: outputConfig.Name = "R1 Display"; break;
                    case 2: outputConfig.Name = "R2 Display"; break;
                    case 3: outputConfig.Name = "R1 Audio"; break;
                    case 4: outputConfig.Name = "R2 Audio"; break;
                }
            }

            DspFaderColors = new List<FaderItemColor>
            {
                new FaderItemColor
                {
                    ColorHex = "#ff8000",
                    Name = "Orange",
                    Priority = 1
                },
                new FaderItemColor
                {
                    ColorHex = "#0080FF",
                    Name = "Cyan",
                    Priority = 2
                },
                new FaderItemColor
                {
                    ColorHex = "#ff0000",
                    Name = "Red",
                    Priority = 3
                },
                new FaderItemColor
                {
                    ColorHex = "#ff00ff",
                    Name = "Pink",
                    Priority = 4
                },
                new FaderItemColor
                {
                    ColorHex = "#ffff00",
                    Name = "Yellow",
                    Priority = 5
                },
                new FaderItemColor
                {
                    ColorHex = "#00ff00",
                    Name = "Green",
                    Priority = 6
                },
                new FaderItemColor
                {
                    ColorHex = "#8000ff",
                    Name = "Purple",
                    Priority = 7
                },
                new FaderItemColor
                {
                    ColorHex = "#0000ff",
                    Name = "Blue",
                    Priority = 8
                },
            };

            DspFaderComponents = new List<FaderItemConfig>
            {
                new FaderItemConfig
                {
                    Name = "Rec PGM Mix",
                    ComponentName = "Rec.pgmmix.level",
                    Color = "Blue",
                    Group = 3,
                    MixerIndex = 0
                },
                new FaderItemConfig
                {
                    Name = "Mic Mix",
                    ComponentName = "Rec.srmix.level",
                    Color = "Blue",
                    Group = 3,
                    MixerIndex = 0
                },
                new FaderItemConfig
                {
                    Name = "Rec PGM",
                    ComponentName = "pgm.level",
                    Color = "Blue",
                    Group = 1,
                    MixerIndex = 0
                },
                new FaderItemConfig
                {
                    Name = "Aux",
                    ComponentName = "aux.level",
                    Color = "Blue",
                    Group = 1,
                    MixerIndex = 0
                },
                new FaderItemConfig
                {
                    Name = "Handheld",
                    ComponentName = "hh.boost",
                    Color = "Blue",
                    Group = 2,
                    MixerIndex = 0
                },
                new FaderItemConfig
                {
                    Name = "Lapel",
                    ComponentName = "lapel.boost",
                    Color = "Blue",
                    Group = 2,
                    MixerIndex = 0
                }
            };

            TriplePlayServerAddress = "30.93.1.126";
        }
    }
}